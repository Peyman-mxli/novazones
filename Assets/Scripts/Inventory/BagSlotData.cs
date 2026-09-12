using System;
using UnityEngine;

[Serializable]
public class BagSlotData
{
    [Header("Item")]
    public ItemData item;

    [Header("Stack")]
    public int amount;

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }

    // 🔥 FIX: renamed from Set → SetItem
    public void SetItem(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = Mathf.Max(0, newAmount);

        if (amount <= 0)
            Clear();
    }
}