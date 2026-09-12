using System;
using UnityEngine;

[Serializable]
public class BagContainerData
{
    [Header("Bag Identity")]
    public int bagId;
    public ItemData equippedBagItem;

    [Header("Bag Settings")]
    public int slotCount;
    public BagSlotData[] slots;

    [Header("Safety")]
    public bool isLocked;

    public bool IsBackpack()
    {
        return bagId == 0;
    }

    public bool IsEquipped()
    {
        return IsBackpack() || equippedBagItem != null;
    }

    public bool IsEmpty()
    {
        if (slots == null)
            return true;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && !slots[i].IsEmpty())
                return false;
        }

        return true;
    }

    public void Setup(int newBagId, ItemData newBagItem, int newSlotCount)
    {
        bagId = newBagId;
        equippedBagItem = newBagItem;
        slotCount = Mathf.Max(0, newSlotCount);

        slots = new BagSlotData[slotCount];

        for (int i = 0; i < slots.Length; i++)
            slots[i] = new BagSlotData();

        isLocked = false;
    }

    public void ClearBag()
    {
        if (IsBackpack())
            return;

        equippedBagItem = null;
        slotCount = 0;
        slots = new BagSlotData[0];
        isLocked = false;
    }
}