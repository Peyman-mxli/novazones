using UnityEngine;

public class ItemDatabaseManager : MonoBehaviour
{
    public static ItemDatabaseManager Instance;

    [Header("All Game Items")]
    public ItemData[] allItems;

    private void Awake()
    {
        Instance = this;
    }

    public ItemData GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || allItems == null)
            return null;

        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].itemId == itemId)
                return allItems[i];
        }

        return null;
    }

    public string GetItemName(string itemId)
    {
        ItemData itemData = GetItemById(itemId);

        if (itemData == null)
            return itemId;

        return itemData.itemName;
    }

    public Sprite GetItemIcon(string itemId)
    {
        ItemData itemData = GetItemById(itemId);

        if (itemData == null)
            return null;

        return itemData.itemIcon;
    }
}