using UnityEngine;

public class BackpackInventorySystem : MonoBehaviour
{
    public static BackpackInventorySystem Instance;

    [Header("BackPack Settings")]
    public int backpackSlotCount = 16;
    public BackpackSlotData[] slots;

    [Header("UI")]
    public BackpackUI backpackUI;

    [Header("Money")]
    public int gold = 0;
    public int silver = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateSlotsIfNeeded();
        NormalizeMoney();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void CreateSlotsIfNeeded()
    {
        if (backpackSlotCount < 1)
            backpackSlotCount = 16;

        if (slots == null || slots.Length != backpackSlotCount)
        {
            slots = new BackpackSlotData[backpackSlotCount];

            for (int i = 0; i < slots.Length; i++)
                slots[i] = new BackpackSlotData();
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new BackpackSlotData();
        }
    }

    // =========================
    // 💰 NEW — GOLD FUNCTIONS
    // =========================

    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (gold < amount)
        {
            Debug.LogWarning("Not enough gold.");
            return false;
        }

        gold -= amount;
        NormalizeMoney();
        RefreshUI();

        Debug.Log("Gold spent: " + amount + " | Remaining Gold: " + gold);

        return true;
    }

    // =========================

    public bool TryMoveItemToFirstAvailableBag(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return false;

        if (BagInventorySystem.Instance == null)
            return false;

        BackpackSlotData fromSlot = slots[slotIndex];

        if (fromSlot == null || fromSlot.IsEmpty())
            return false;

        ItemData item = fromSlot.itemData;

        if (item != null && item.itemType == ItemType.Bag)
        {
            Debug.LogWarning("Bag items cannot go inside bags.");
            return false;
        }

        int count = fromSlot.count;

        for (int i = 1; i <= 4; i++)
        {
            BagContainerData bag = BagInventorySystem.Instance.GetBag(i);

            if (bag == null || bag.equippedBagItem == null)
                continue;

            for (int j = 0; j < bag.slots.Length; j++)
            {
                if (bag.slots[j] == null)
                    bag.slots[j] = new BagSlotData();

                if (bag.slots[j].IsEmpty())
                {
                    bag.slots[j].SetItem(item, count);
                    fromSlot.Clear();

                    RefreshUI();
                    return true;
                }
            }
        }

        Debug.LogWarning("No space in any equipped bag.");
        return false;
    }

    public void AddMoney(int goldAmount, int silverAmount)
    {
        gold += Mathf.Max(0, goldAmount);
        silver += Mathf.Max(0, silverAmount);

        NormalizeMoney();
        RefreshUI();
    }

    private void NormalizeMoney()
    {
        if (silver >= 100)
        {
            gold += silver / 100;
            silver %= 100;
        }

        if (silver < 0)
            silver = 0;

        if (gold < 0)
            gold = 0;
    }

    public bool AddItem(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
            return false;

        if (itemData.itemType == ItemType.Money)
        {
            AddMoney(0, amount);
            return true;
        }

        int remainingAmount = amount;

        if (itemData.isStackable)
        {
            remainingAmount = StackIntoBackpack(itemData, remainingAmount);

            if (remainingAmount <= 0)
            {
                RefreshUI();
                return true;
            }

            if (itemData.itemType != ItemType.Bag)
            {
                remainingAmount = StackIntoEquippedBags(itemData, remainingAmount);

                if (remainingAmount <= 0)
                {
                    RefreshUI();
                    return true;
                }
            }
        }

        remainingAmount = AddToEmptyBackpackSlots(itemData, remainingAmount);

        if (remainingAmount <= 0)
        {
            RefreshUI();
            return true;
        }

        if (itemData.itemType != ItemType.Bag)
        {
            remainingAmount = AddToEmptyEquippedBagSlots(itemData, remainingAmount);

            if (remainingAmount <= 0)
            {
                RefreshUI();
                return true;
            }
        }

        RefreshUI();
        Debug.LogWarning("Inventory full. Could not add all items.");
        return false;
    }

    private int StackIntoBackpack(ItemData itemData, int remainingAmount)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].CanStackWith(itemData))
            {
                int freeSpace = slots[i].GetFreeStackSpace();

                if (freeSpace <= 0)
                    continue;

                int addAmount = Mathf.Min(freeSpace, remainingAmount);
                slots[i].count += addAmount;
                remainingAmount -= addAmount;

                if (remainingAmount <= 0)
                    return 0;
            }
        }

        return remainingAmount;
    }

    private int AddToEmptyBackpackSlots(ItemData itemData, int remainingAmount)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].IsEmpty())
            {
                int addAmount = itemData.isStackable
                    ? Mathf.Min(itemData.maxStackSize, remainingAmount)
                    : 1;

                slots[i].SetItem(itemData, addAmount);
                remainingAmount -= addAmount;

                if (remainingAmount <= 0)
                    return 0;
            }
        }

        return remainingAmount;
    }

    private int StackIntoEquippedBags(ItemData itemData, int remainingAmount)
    {
        if (BagInventorySystem.Instance == null)
            return remainingAmount;

        for (int bagId = 1; bagId <= 4; bagId++)
        {
            BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

            if (bag == null || bag.equippedBagItem == null || bag.slots == null)
                continue;

            for (int slotIndex = 0; slotIndex < bag.slots.Length; slotIndex++)
            {
                if (bag.slots[slotIndex] == null)
                    bag.slots[slotIndex] = new BagSlotData();

                BagSlotData bagSlot = bag.slots[slotIndex];

                if (CanStackBagSlotWith(bagSlot, itemData))
                {
                    int freeSpace = itemData.maxStackSize - bagSlot.amount;

                    if (freeSpace <= 0)
                        continue;

                    int addAmount = Mathf.Min(freeSpace, remainingAmount);
                    bagSlot.amount += addAmount;
                    remainingAmount -= addAmount;

                    if (remainingAmount <= 0)
                        return 0;
                }
            }
        }

        return remainingAmount;
    }

    private int AddToEmptyEquippedBagSlots(ItemData itemData, int remainingAmount)
    {
        if (BagInventorySystem.Instance == null)
            return remainingAmount;

        for (int bagId = 1; bagId <= 4; bagId++)
        {
            BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

            if (bag == null || bag.equippedBagItem == null || bag.slots == null)
                continue;

            for (int slotIndex = 0; slotIndex < bag.slots.Length; slotIndex++)
            {
                if (bag.slots[slotIndex] == null)
                    bag.slots[slotIndex] = new BagSlotData();

                if (bag.slots[slotIndex].IsEmpty())
                {
                    int addAmount = itemData.isStackable
                        ? Mathf.Min(itemData.maxStackSize, remainingAmount)
                        : 1;

                    bag.slots[slotIndex].SetItem(itemData, addAmount);
                    remainingAmount -= addAmount;

                    if (remainingAmount <= 0)
                        return 0;
                }
            }
        }

        return remainingAmount;
    }

    private bool CanStackBagSlotWith(BagSlotData bagSlot, ItemData itemData)
    {
        if (bagSlot == null || itemData == null)
            return false;

        if (bagSlot.IsEmpty())
            return false;

        if (!itemData.isStackable)
            return false;

        if (bagSlot.item == null)
            return false;

        if (bagSlot.item.itemId != itemData.itemId)
            return false;

        return bagSlot.amount < itemData.maxStackSize;
    }

    public bool MoveOrSwapSlot(int fromIndex, int toIndex)
    {
        if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex))
            return false;

        if (fromIndex == toIndex)
            return true;

        BackpackSlotData fromSlot = slots[fromIndex];
        BackpackSlotData toSlot = slots[toIndex];

        if (fromSlot == null || toSlot == null)
            return false;

        if (fromSlot.IsEmpty())
            return false;

        if (toSlot.IsEmpty())
        {
            toSlot.SetItem(fromSlot.itemData, fromSlot.count);
            fromSlot.Clear();

            RefreshUI();
            return true;
        }

        bool sameItem =
            fromSlot.itemData != null &&
            toSlot.itemData != null &&
            fromSlot.itemData.itemId == toSlot.itemData.itemId;

        if (sameItem && toSlot.CanStackWith(fromSlot.itemData))
        {
            int freeSpace = toSlot.GetFreeStackSpace();

            if (freeSpace > 0)
            {
                int moveAmount = Mathf.Min(freeSpace, fromSlot.count);

                toSlot.count += moveAmount;
                fromSlot.count -= moveAmount;

                if (fromSlot.count <= 0)
                    fromSlot.Clear();

                RefreshUI();
                return true;
            }
        }

        ItemData tempItem = toSlot.itemData;
        int tempCount = toSlot.count;

        toSlot.SetItem(fromSlot.itemData, fromSlot.count);
        fromSlot.SetItem(tempItem, tempCount);

        RefreshUI();
        return true;
    }

    public bool RemoveItemFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return false;

        if (slots[slotIndex].IsEmpty())
            return false;

        slots[slotIndex].Clear();

        RefreshUI();
        return true;
    }

    public BackpackSlotData GetSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;

        return slots[slotIndex];
    }

    public bool IsValidSlotIndex(int slotIndex)
    {
        return slots != null && slotIndex >= 0 && slotIndex < slots.Length;
    }

    private void RefreshUI()
    {
        if (backpackUI != null)
            backpackUI.Refresh();
    }
}