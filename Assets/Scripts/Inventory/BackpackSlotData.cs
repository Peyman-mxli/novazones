using System;
using UnityEngine;

[Serializable]
public class BackpackSlotData
{
    public ItemData itemData;
    public int count;

    public bool IsEmpty()
    {
        return itemData == null || count <= 0;
    }

    public bool CanStackWith(ItemData newItem)
    {
        if (IsEmpty())
            return false;

        if (newItem == null)
            return false;

        if (!itemData.isStackable)
            return false;

        if (itemData.itemId != newItem.itemId)
            return false;

        return count < itemData.maxStackSize;
    }

    public int GetFreeStackSpace()
    {
        if (IsEmpty())
            return 0;

        if (!itemData.isStackable)
            return 0;

        return itemData.maxStackSize - count;
    }

    public void Clear()
    {
        itemData = null;
        count = 0;
    }

    public void SetItem(ItemData newItem, int newCount)
    {
        if (newItem == null || newCount <= 0)
        {
            Clear();
            return;
        }

        itemData = newItem;

        if (itemData.isStackable)
        {
            count = Mathf.Clamp(newCount, 1, itemData.maxStackSize);
        }
        else
        {
            count = 1;
        }
    }
}