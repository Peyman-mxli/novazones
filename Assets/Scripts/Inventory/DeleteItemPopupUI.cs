using UnityEngine;

public class DeleteItemPopupUI : MonoBehaviour
{
    public static DeleteItemPopupUI Instance;

    private enum DeleteSourceType
    {
        None,
        Backpack,
        BagSlot
    }

    [Header("Popup")]
    public GameObject popupWindow;

    private DeleteSourceType pendingSourceType = DeleteSourceType.None;

    private int pendingBackpackSlotIndex = -1;
    private BackpackUI pendingBackpackUI;

    private int pendingBagId = -1;
    private int pendingBagSlotIndex = -1;
    private BagSlotUI pendingBagSlotUI;

    private void Awake()
    {
        Instance = this;

        if (popupWindow != null)
            popupWindow.SetActive(false);
    }

    public void Open(int slotIndex, BackpackUI backpackUI)
    {
        pendingSourceType = DeleteSourceType.Backpack;

        pendingBackpackSlotIndex = slotIndex;
        pendingBackpackUI = backpackUI;

        pendingBagId = -1;
        pendingBagSlotIndex = -1;
        pendingBagSlotUI = null;

        if (popupWindow != null)
            popupWindow.SetActive(true);
    }

    public void OpenBagSlot(int bagId, int slotIndex, BagSlotUI bagSlotUI)
    {
        pendingSourceType = DeleteSourceType.BagSlot;

        pendingBagId = bagId;
        pendingBagSlotIndex = slotIndex;
        pendingBagSlotUI = bagSlotUI;

        pendingBackpackSlotIndex = -1;
        pendingBackpackUI = null;

        if (popupWindow != null)
            popupWindow.SetActive(true);
    }

    public void ConfirmDelete()
    {
        if (pendingSourceType == DeleteSourceType.Backpack)
        {
            if (BackpackInventorySystem.Instance != null && pendingBackpackSlotIndex >= 0)
                BackpackInventorySystem.Instance.RemoveItemFromSlot(pendingBackpackSlotIndex);
        }
        else if (pendingSourceType == DeleteSourceType.BagSlot)
        {
            DeleteFromBagSlot();
        }

        Close();
    }

    private void DeleteFromBagSlot()
    {
        if (BagInventorySystem.Instance == null)
            return;

        BagContainerData bag = BagInventorySystem.Instance.GetBag(pendingBagId);

        if (bag == null || bag.slots == null)
            return;

        if (pendingBagSlotIndex < 0 || pendingBagSlotIndex >= bag.slots.Length)
            return;

        if (bag.slots[pendingBagSlotIndex] == null)
            return;

        bag.slots[pendingBagSlotIndex].Clear();

        if (pendingBagSlotUI != null)
            pendingBagSlotUI.Refresh();
    }

    public void CancelDelete()
    {
        Close();
    }

    private void Close()
    {
        BackpackUI refreshBackpackUI = pendingBackpackUI;
        BagSlotUI refreshBagSlotUI = pendingBagSlotUI;

        pendingSourceType = DeleteSourceType.None;

        pendingBackpackSlotIndex = -1;
        pendingBackpackUI = null;

        pendingBagId = -1;
        pendingBagSlotIndex = -1;
        pendingBagSlotUI = null;

        if (popupWindow != null)
            popupWindow.SetActive(false);

        if (refreshBackpackUI != null)
            refreshBackpackUI.Refresh();

        if (refreshBagSlotUI != null)
            refreshBagSlotUI.Refresh();
    }
}