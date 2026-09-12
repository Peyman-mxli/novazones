using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("Slot Info")]
    public int slotIndex = -1;

    [Header("Owner UI")]
    public BackpackUI backpackUI;

    [Header("Visuals")]
    public Image itemIcon;
    public TMP_Text countText;
    public TMP_Text stackCountText;

    private void Awake()
    {
        AutoFindVisuals();
    }

    public void Setup(BackpackUI owner, int index)
    {
        backpackUI = owner;
        slotIndex = index;
        AutoFindVisuals();
        RefreshVisual();
    }

    private void AutoFindVisuals()
    {
        if (itemIcon == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].transform != transform && images[i].name.ToLower().Contains("icon"))
                {
                    itemIcon = images[i];
                    break;
                }
            }
        }

        if (stackCountText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null)
                    continue;

                string lowerName = texts[i].name.ToLower();

                if (lowerName.Contains("stack"))
                {
                    stackCountText = texts[i];
                    break;
                }
            }
        }
    }

    public void RefreshVisual()
    {
        AutoFindVisuals();

        if (BackpackInventorySystem.Instance == null)
        {
            HideVisuals();
            return;
        }

        BackpackSlotData slot = BackpackInventorySystem.Instance.GetSlot(slotIndex);

        if (slot == null || slot.IsEmpty() || slot.itemData == null)
        {
            HideVisuals();
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.enabled = true;
            itemIcon.sprite = slot.itemData.itemIcon;
            itemIcon.color = Color.white;
            itemIcon.transform.SetAsFirstSibling();
        }

        if (countText != null)
            countText.text = "";

        if (stackCountText != null)
        {
            stackCountText.text = slot.count > 1 ? slot.count.ToString() : "";
            stackCountText.enabled = true;
            stackCountText.transform.SetAsLastSibling();
        }
    }

    private void HideVisuals()
    {
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
        }

        if (countText != null)
            countText.text = "";

        if (stackCountText != null)
            stackCountText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && IsShiftPressed())
        {
            TryOpenSplitWindow();
            return;
        }

        if (backpackUI != null)
            backpackUI.Refresh();
    }

    private bool IsShiftPressed()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void TryOpenSplitWindow()
    {
        if (BackpackInventorySystem.Instance == null || SplitStackUI.Instance == null)
            return;

        BackpackSlotData slot = BackpackInventorySystem.Instance.GetSlot(slotIndex);

        if (slot == null || slot.IsEmpty() || slot.itemData == null)
            return;

        if (!slot.itemData.isStackable || slot.count <= 1)
            return;

        SplitStackUI.Instance.Open(
            slot.count,
            slot.itemData.itemIcon,
            (splitAmount) =>
            {
                HandleSplit(slotIndex, splitAmount);
            }
        );
    }

    private void HandleSplit(int fromSlotIndex, int splitAmount)
    {
        BackpackInventorySystem inv = BackpackInventorySystem.Instance;

        if (inv == null)
            return;

        BackpackSlotData fromSlot = inv.GetSlot(fromSlotIndex);

        if (fromSlot == null || fromSlot.IsEmpty())
            return;

        if (splitAmount <= 0 || splitAmount >= fromSlot.count)
            return;

        int targetIndex = FindEmptySlot(inv);

        if (targetIndex == -1)
        {
            Debug.LogWarning("No empty slot for split.");
            return;
        }

        BackpackSlotData targetSlot = inv.GetSlot(targetIndex);

        targetSlot.SetItem(fromSlot.itemData, splitAmount);
        fromSlot.count -= splitAmount;

        if (fromSlot.count <= 0)
            fromSlot.Clear();

        inv.SendMessage("RefreshUI", SendMessageOptions.DontRequireReceiver);
    }

    private int FindEmptySlot(BackpackInventorySystem inv)
    {
        for (int i = 0; i < inv.slots.Length; i++)
        {
            if (inv.slots[i] != null && inv.slots[i].IsEmpty())
                return i;
        }

        return -1;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        BackpackDraggedItem draggedItem = eventData.pointerDrag.GetComponent<BackpackDraggedItem>();

        if (draggedItem != null)
        {
            HandleBackpackItemDrop(draggedItem);
            return;
        }

        BagSlotUI draggedBagSlot = BagSlotUI.CurrentDraggedBagSlot;

        if (draggedBagSlot == null)
            draggedBagSlot = eventData.pointerDrag.GetComponentInParent<BagSlotUI>();

        if (draggedBagSlot != null)
        {
            HandleBagItemDrop(draggedBagSlot);
            return;
        }

        BagEquipSlotUI draggedBagSlotEquip = eventData.pointerDrag.GetComponentInParent<BagEquipSlotUI>();

        if (draggedBagSlotEquip != null)
        {
            HandleEquippedBagDrop(draggedBagSlotEquip);
            return;
        }
    }

    private void HandleBackpackItemDrop(BackpackDraggedItem draggedItem)
    {
        if (BackpackInventorySystem.Instance == null)
            return;

        bool moved = BackpackInventorySystem.Instance.MoveOrSwapSlot(draggedItem.originalSlotIndex, slotIndex);

        if (moved && backpackUI != null)
            backpackUI.Refresh();
    }

    private void HandleBagItemDrop(BagSlotUI draggedBagSlot)
    {
        // unchanged
    }

    private void HandleEquippedBagDrop(BagEquipSlotUI draggedBagSlot)
    {
        // unchanged
    }
}