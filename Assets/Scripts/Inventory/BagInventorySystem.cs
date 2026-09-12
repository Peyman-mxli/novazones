using UnityEngine;

public class BagInventorySystem : MonoBehaviour
{
    public static BagInventorySystem Instance;

    [Header("Bag 0 - Backpack")]
    public int backpackSlotCount = 16;

    [Header("Bag 1-4 - Equipped Bags")]
    public BagContainerData[] bags = new BagContainerData[5];

    private const int TotalBagCount = 5;
    private const int BackpackBagId = 0;
    private const int FirstEquippedBagId = 1;
    private const int LastEquippedBagId = 4;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateBagStructure();
    }

    private void CreateBagStructure()
    {
        if (bags == null || bags.Length != TotalBagCount)
            bags = new BagContainerData[TotalBagCount];

        for (int i = 0; i < bags.Length; i++)
        {
            if (bags[i] == null)
                bags[i] = new BagContainerData();
        }

        bags[BackpackBagId].bagId = BackpackBagId;
        bags[BackpackBagId].equippedBagItem = null;
        bags[BackpackBagId].slotCount = backpackSlotCount;
        bags[BackpackBagId].isLocked = true;

        for (int i = FirstEquippedBagId; i <= LastEquippedBagId; i++)
        {
            bags[i].bagId = i;

            if (bags[i].slots == null)
            {
                bags[i].equippedBagItem = null;
                bags[i].slotCount = 0;
                bags[i].slots = new BagSlotData[0];
                bags[i].isLocked = false;
            }
        }
    }

    public BagContainerData GetBag(int bagId)
    {
        if (bagId < 0 || bagId >= bags.Length)
            return null;

        return bags[bagId];
    }

    public bool IsEquippedBagSlot(int bagId)
    {
        return bagId >= FirstEquippedBagId && bagId <= LastEquippedBagId;
    }

    public bool CanItemGoIntoBagSlot(ItemData item)
    {
        return item != null && item.itemType == ItemType.Bag && item.bagSlotCount > 0;
    }

    public bool TryEquipBagToSlot(ItemData bagItem, int bagId)
    {
        if (!IsEquippedBagSlot(bagId))
            return false;

        if (!CanItemGoIntoBagSlot(bagItem))
            return false;

        BagContainerData targetBag = GetBag(bagId);

        if (targetBag == null || targetBag.isLocked)
            return false;

        if (targetBag.equippedBagItem != null)
            return false;

        targetBag.isLocked = true;
        targetBag.Setup(bagId, bagItem, bagItem.bagSlotCount);
        targetBag.isLocked = false;

        return true;
    }

    public bool TryMoveOrSwapEquippedBag(int fromBagId, int toBagId)
    {
        if (!IsEquippedBagSlot(fromBagId) || !IsEquippedBagSlot(toBagId))
            return false;

        if (fromBagId == toBagId)
            return false;

        BagContainerData fromBag = GetBag(fromBagId);
        BagContainerData toBag = GetBag(toBagId);

        if (fromBag == null || toBag == null)
            return false;

        if (fromBag.isLocked || toBag.isLocked)
            return false;

        if (fromBag.equippedBagItem == null)
            return false;

        // 🔥 IMPORTANT CHANGE:
        // REMOVE ALL "IsEmpty()" RESTRICTIONS FOR SLOT SWAP

        fromBag.isLocked = true;
        toBag.isLocked = true;

        ItemData fromItem = fromBag.equippedBagItem;
        int fromSlotCount = fromBag.slotCount;
        BagSlotData[] fromSlots = fromBag.slots;

        ItemData toItem = toBag.equippedBagItem;
        int toSlotCount = toBag.slotCount;
        BagSlotData[] toSlots = toBag.slots;

        fromBag.equippedBagItem = toItem;
        fromBag.slotCount = toSlotCount;
        fromBag.slots = toSlots != null ? toSlots : new BagSlotData[0];

        toBag.equippedBagItem = fromItem;
        toBag.slotCount = fromSlotCount;
        toBag.slots = fromSlots != null ? fromSlots : new BagSlotData[0];

        fromBag.isLocked = false;
        toBag.isLocked = false;

        return true;
    }

    public bool TryReturnBagToBackpack(int bagId)
    {
        if (!IsEquippedBagSlot(bagId))
            return false;

        if (BackpackInventorySystem.Instance == null)
            return false;

        BagContainerData bag = GetBag(bagId);

        if (bag == null || bag.equippedBagItem == null)
            return false;

        // 🔥 KEEP THIS RULE (correct)
        if (!bag.IsEmpty())
        {
            Debug.LogWarning("Cannot remove bag because it has items inside.");
            return false;
        }

        ItemData bagItem = bag.equippedBagItem;

        bool added = BackpackInventorySystem.Instance.AddItem(bagItem, 1);

        if (!added)
        {
            Debug.LogWarning("Backpack is full.");
            return false;
        }

        bag.ClearBag();
        return true;
    }
}