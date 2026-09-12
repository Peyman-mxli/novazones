using UnityEngine;

[System.Serializable]
public class LootDrop
{
    public string itemId = "";
    public int minAmount = 1;
    public int maxAmount = 1;

    [Range(0, 100)]
    public float dropChancePercent = 100f;

    public int rolledAmount = 0;
    public bool wasDropped = false;
    public bool isLooted = false;
}

public class EnemyLoot : MonoBehaviour
{
    public LootDrop[] itemDrops;

    public bool hasLoot = true;
    public bool lootRolled = false;

    private void OnEnable()
    {
        ResetLoot();
    }

    // ✅ NEW — LOOT ALL
    public void TakeAllItems()
    {
        RollLootIfNeeded();

        if (itemDrops == null)
            return;

        for (int i = 0; i < itemDrops.Length; i++)
        {
            LootDrop drop = itemDrops[i];

            if (drop == null || !drop.wasDropped || drop.isLooted)
                continue;

            ItemData itemData = GetLootItemData(i);

            if (itemData == null)
                continue;

            bool added = false;

            if (itemData.itemType == ItemType.Money)
            {
                BackpackInventorySystem.Instance.AddMoney(
                    itemData.goldValue * drop.rolledAmount,
                    itemData.silverValue * drop.rolledAmount
                );
                added = true;
            }
            else
            {
                added = BackpackInventorySystem.Instance.AddItem(itemData, drop.rolledAmount);
            }

            if (added)
                drop.isLooted = true;
        }

        RefreshHasLoot();
    }

    public bool HasAnyLoot()
    {
        RollLootIfNeeded();

        if (!hasLoot || itemDrops == null)
            return false;

        for (int i = 0; i < itemDrops.Length; i++)
        {
            if (itemDrops[i] != null && itemDrops[i].wasDropped && !itemDrops[i].isLooted)
                return true;
        }

        return false;
    }

    public string GetLootDisplayText(int itemIndex)
    {
        LootDrop drop = GetDrop(itemIndex);

        if (drop == null || !drop.wasDropped || drop.isLooted)
            return "";

        ItemData itemData = GetLootItemData(itemIndex);
        string itemName = itemData != null ? itemData.itemName : drop.itemId;

        return drop.rolledAmount > 1 ? itemName + " x" + drop.rolledAmount : itemName;
    }

    public ItemData GetLootItemData(int itemIndex)
    {
        LootDrop drop = GetDrop(itemIndex);

        if (drop == null || string.IsNullOrEmpty(drop.itemId))
            return null;

        ItemData[] allItems = Resources.FindObjectsOfTypeAll<ItemData>();

        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].itemId == drop.itemId)
                return allItems[i];
        }

        return null;
    }

    public int GetLootAmount(int itemIndex)
    {
        LootDrop drop = GetDrop(itemIndex);

        if (drop == null || !drop.wasDropped || drop.isLooted)
            return 0;

        return drop.rolledAmount;
    }

    public bool TakeItem(int itemIndex)
    {
        RollLootIfNeeded();

        LootDrop drop = GetDrop(itemIndex);

        if (drop == null || !drop.wasDropped || drop.isLooted)
            return false;

        ItemData itemData = GetLootItemData(itemIndex);

        if (itemData == null)
            return false;

        bool added = false;

        if (itemData.itemType == ItemType.Money)
        {
            BackpackInventorySystem.Instance.AddMoney(
                itemData.goldValue * drop.rolledAmount,
                itemData.silverValue * drop.rolledAmount
            );
            added = true;
        }
        else
        {
            added = BackpackInventorySystem.Instance.AddItem(itemData, drop.rolledAmount);
        }

        if (!added)
            return false;

        drop.isLooted = true;
        RefreshHasLoot();
        return true;
    }

    public void RollLootIfNeeded()
    {
        if (lootRolled)
            return;

        lootRolled = true;

        if (itemDrops == null)
        {
            hasLoot = false;
            return;
        }

        for (int i = 0; i < itemDrops.Length; i++)
        {
            LootDrop drop = itemDrops[i];

            if (drop == null)
                continue;

            drop.isLooted = false;

            float roll = Random.Range(0f, 100f);
            drop.wasDropped = roll <= drop.dropChancePercent;

            if (drop.wasDropped)
                drop.rolledAmount = Random.Range(drop.minAmount, drop.maxAmount + 1);
            else
                drop.rolledAmount = 0;
        }

        RefreshHasLoot();
    }

    public void ResetLoot()
    {
        lootRolled = false;
        hasLoot = true;

        if (itemDrops == null)
            return;

        for (int i = 0; i < itemDrops.Length; i++)
        {
            itemDrops[i].rolledAmount = 0;
            itemDrops[i].wasDropped = false;
            itemDrops[i].isLooted = false;
        }
    }

    private LootDrop GetDrop(int index)
    {
        RollLootIfNeeded();

        if (itemDrops == null || index < 0 || index >= itemDrops.Length)
            return null;

        return itemDrops[index];
    }

    private void RefreshHasLoot()
    {
        hasLoot = false;

        if (itemDrops == null)
            return;

        for (int i = 0; i < itemDrops.Length; i++)
        {
            if (itemDrops[i] != null && itemDrops[i].wasDropped && !itemDrops[i].isLooted)
            {
                hasLoot = true;
                return;
            }
        }
    }
}