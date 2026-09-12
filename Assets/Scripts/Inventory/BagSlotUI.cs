using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Bag Info")]
    public int bagId;
    public int slotIndex;

    [Header("UI")]
    public Image slotBackground;
    public Image itemIcon;
    public TMP_Text stackText;

    private Canvas rootCanvas;
    private Transform originalIconParent;
    private Vector3 originalIconPosition;
    private bool isDragging;

    private static BagSlotUI draggedBagSlot;

    public static BagSlotUI CurrentDraggedBagSlot => draggedBagSlot;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        AutoFindVisuals();
        MakeSlotDroppable();
    }

    public void Setup(int newBagId, int newSlotIndex)
    {
        bagId = newBagId;
        slotIndex = newSlotIndex;
        AutoFindVisuals();
        MakeSlotDroppable();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void AutoFindVisuals()
    {
        if (slotBackground == null)
            slotBackground = GetComponent<Image>();

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

        if (stackText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null)
                    continue;

                string textName = texts[i].name.ToLower();

                if (textName.Contains("stack") || textName.Contains("count"))
                {
                    stackText = texts[i];
                    break;
                }
            }
        }
    }

    private void MakeSlotDroppable()
    {
        if (slotBackground != null)
        {
            slotBackground.enabled = true;
            slotBackground.raycastTarget = true;
        }

        if (itemIcon != null)
            itemIcon.raycastTarget = true;

        if (stackText != null)
            stackText.raycastTarget = false;
    }

    public void Refresh()
    {
        AutoFindVisuals();
        MakeSlotDroppable();

        if (BagInventorySystem.Instance == null)
        {
            ClearVisual();
            return;
        }

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

        if (bag == null || bag.slots == null || slotIndex < 0 || slotIndex >= bag.slots.Length)
        {
            ClearVisual();
            return;
        }

        BagSlotData slot = bag.slots[slotIndex];

        if (slot == null || slot.IsEmpty() || slot.item == null)
        {
            ClearVisual();
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.enabled = true;
            itemIcon.sprite = slot.item.itemIcon;
            itemIcon.color = Color.white;
            itemIcon.raycastTarget = true;
        }

        if (stackText != null)
        {
            stackText.enabled = true;
            stackText.text = slot.amount > 1 ? slot.amount.ToString() : "";
            stackText.raycastTarget = false;
            stackText.transform.SetAsLastSibling();
        }
    }

    private void ClearVisual()
    {
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
            itemIcon.raycastTarget = false;
        }

        if (stackText != null)
        {
            stackText.enabled = true;
            stackText.text = "";
            stackText.raycastTarget = false;
        }

        if (slotBackground != null)
        {
            slotBackground.enabled = true;
            slotBackground.raycastTarget = true;
        }
    }

    public BagSlotData GetSlotDataForExternalUse()
    {
        return GetThisSlot();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        BagSlotData slot = GetThisSlot();

        if (slot == null || slot.IsEmpty())
            return;

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && slot.amount > 1)
        {
            OpenSplitPanel(slot);
            return;
        }

        isDragging = true;
        draggedBagSlot = this;

        if (itemIcon != null)
        {
            originalIconParent = itemIcon.transform.parent;
            originalIconPosition = itemIcon.transform.position;

            if (rootCanvas != null)
                itemIcon.transform.SetParent(rootCanvas.transform, true);

            itemIcon.transform.SetAsLastSibling();
            itemIcon.raycastTarget = false;
        }
    }

    private void OpenSplitPanel(BagSlotData sourceSlot)
    {
        if (sourceSlot == null || sourceSlot.IsEmpty() || sourceSlot.item == null)
            return;

        if (SplitStackUI.Instance == null)
        {
            Debug.LogWarning("SplitStackUI is missing in the scene.");
            return;
        }

        SplitStackUI.Instance.Open(
            sourceSlot.amount,
            sourceSlot.item.itemIcon,
            splitAmount =>
            {
                SplitIntoFirstEmptySlot(sourceSlot, splitAmount);
            }
        );
    }

    private void SplitIntoFirstEmptySlot(BagSlotData sourceSlot, int splitAmount)
    {
        if (sourceSlot == null || sourceSlot.IsEmpty() || sourceSlot.item == null)
            return;

        if (splitAmount < 1 || splitAmount >= sourceSlot.amount)
            return;

        if (BagInventorySystem.Instance == null)
            return;

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

        if (bag == null || bag.slots == null)
            return;

        for (int i = 0; i < bag.slots.Length; i++)
        {
            if (i == slotIndex)
                continue;

            if (bag.slots[i] == null)
                bag.slots[i] = new BagSlotData();

            if (!bag.slots[i].IsEmpty())
                continue;

            bag.slots[i].SetItem(sourceSlot.item, splitAmount);
            sourceSlot.amount -= splitAmount;

            if (sourceSlot.amount <= 0)
                sourceSlot.Clear();

            Refresh();
            return;
        }

        Debug.LogWarning("No empty slot available for split.");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || itemIcon == null)
            return;

        itemIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool handledDrop = false;

        if (isDragging)
            handledDrop = TryDropByRaycast(eventData);

        if (!handledDrop && isDragging && IsMouseOverDeleteZone(eventData.position))
        {
            if (DeleteItemPopupUI.Instance != null)
                DeleteItemPopupUI.Instance.OpenBagSlot(bagId, slotIndex, this);
        }

        if (itemIcon != null && originalIconParent != null)
        {
            itemIcon.transform.SetParent(originalIconParent, true);
            itemIcon.transform.position = originalIconPosition;
            itemIcon.raycastTarget = true;
        }

        isDragging = false;
        draggedBagSlot = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        BackpackDraggedItem backpackItem = eventData.pointerDrag.GetComponent<BackpackDraggedItem>();

        if (backpackItem == null)
            backpackItem = eventData.pointerDrag.GetComponentInParent<BackpackDraggedItem>();

        if (backpackItem != null)
        {
            HandleBackpackToBag(backpackItem);
            return;
        }

        BagEquipSlotUI equippedBagSlot = eventData.pointerDrag.GetComponentInParent<BagEquipSlotUI>();

        if (equippedBagSlot != null)
        {
            TryPlaceEquippedBagHere(equippedBagSlot);
            return;
        }

        BagSlotUI sourceBagSlot = draggedBagSlot;

        if (sourceBagSlot == null)
            sourceBagSlot = eventData.pointerDrag.GetComponentInParent<BagSlotUI>();

        if (sourceBagSlot != null)
            HandleBagToBag(sourceBagSlot);
    }

    private bool TryDropByRaycast(PointerEventData eventData)
    {
        if (draggedBagSlot == null)
            return false;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            BagEquipSlotUI targetEquipSlot = results[i].gameObject.GetComponentInParent<BagEquipSlotUI>();

            if (targetEquipSlot != null)
            {
                targetEquipSlot.TryPlaceBagFromBagSlot(draggedBagSlot);
                return true;
            }
        }

        for (int i = 0; i < results.Count; i++)
        {
            BagSlotUI targetBagSlot = results[i].gameObject.GetComponentInParent<BagSlotUI>();

            if (targetBagSlot != null && targetBagSlot != draggedBagSlot)
            {
                targetBagSlot.HandleBagToBag(draggedBagSlot);
                return true;
            }
        }

        for (int i = 0; i < results.Count; i++)
        {
            BackpackSlotUI targetBackpackSlot = results[i].gameObject.GetComponentInParent<BackpackSlotUI>();

            if (targetBackpackSlot != null)
            {
                HandleBagToBackpack(targetBackpackSlot);
                return true;
            }
        }

        return false;
    }

    private bool IsMouseOverDeleteZone(Vector2 screenPosition)
    {
        DeleteItemDropZone deleteZone = FindFirstObjectByType<DeleteItemDropZone>();

        if (deleteZone == null)
            return false;

        RectTransform zoneRect = deleteZone.GetComponent<RectTransform>();

        if (zoneRect == null)
            return false;

        Camera uiCamera = null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(zoneRect, screenPosition, uiCamera);
    }

    public bool TryPlaceEquippedBagHere(BagEquipSlotUI sourceEquipSlot)
    {
        if (sourceEquipSlot == null)
            return false;

        if (BagInventorySystem.Instance == null)
            return false;

        BagContainerData sourceBag = BagInventorySystem.Instance.GetBag(sourceEquipSlot.bagId);

        if (sourceBag == null || sourceBag.equippedBagItem == null)
            return false;

        if (!sourceBag.IsEmpty())
        {
            Debug.LogWarning("Cannot move this bag into storage because it has items inside.");
            return false;
        }

        BagSlotData target = GetThisSlot();

        if (target == null)
            return false;

        ItemData bagItem = sourceBag.equippedBagItem;

        if (!BagInventorySystem.Instance.CanItemGoIntoBagSlot(bagItem))
            return false;

        if (target.IsEmpty())
        {
            target.SetItem(bagItem, 1);
            sourceBag.ClearBag();

            if (sourceEquipSlot.bagWindow != null)
                sourceEquipSlot.bagWindow.SetActive(false);

            sourceEquipSlot.Refresh();
            Refresh();
            return true;
        }

        ItemData targetItem = target.item;

        if (!BagInventorySystem.Instance.CanItemGoIntoBagSlot(targetItem))
            return false;

        target.SetItem(bagItem, 1);
        sourceBag.Setup(sourceEquipSlot.bagId, targetItem, targetItem.bagSlotCount);

        if (sourceEquipSlot.bagWindow != null)
            sourceEquipSlot.bagWindow.SetActive(false);

        sourceEquipSlot.Refresh();
        Refresh();
        return true;
    }

    private void HandleBackpackToBag(BackpackDraggedItem dragged)
    {
        if (BackpackInventorySystem.Instance == null || BagInventorySystem.Instance == null)
            return;

        BackpackSlotData source = BackpackInventorySystem.Instance.GetSlot(dragged.originalSlotIndex);

        if (source == null || source.IsEmpty())
            return;

        BagSlotData target = GetThisSlot();

        if (target == null)
            return;

        if (TryStackBackpackIntoBag(source, target))
        {
            RefreshAfterBackpackMove(dragged);
            return;
        }

        if (target.IsEmpty())
        {
            target.SetItem(source.itemData, source.count);
            source.Clear();
            RefreshAfterBackpackMove(dragged);
            return;
        }

        ItemData targetItem = target.item;
        int targetAmount = target.amount;

        target.SetItem(source.itemData, source.count);
        source.SetItem(targetItem, targetAmount);

        RefreshAfterBackpackMove(dragged);
    }

    private bool TryStackBackpackIntoBag(BackpackSlotData source, BagSlotData target)
    {
        if (source == null || target == null)
            return false;

        if (source.IsEmpty() || target.IsEmpty())
            return false;

        if (source.itemData == null || target.item == null)
            return false;

        if (!source.itemData.isStackable)
            return false;

        if (source.itemData.itemId != target.item.itemId)
            return false;

        int maxStack = Mathf.Max(1, source.itemData.maxStackSize);
        int freeSpace = maxStack - target.amount;

        if (freeSpace <= 0)
            return false;

        int moveAmount = Mathf.Min(freeSpace, source.count);

        target.amount += moveAmount;
        source.count -= moveAmount;

        if (source.count <= 0)
            source.Clear();

        return true;
    }

    private void RefreshAfterBackpackMove(BackpackDraggedItem dragged)
    {
        Refresh();

        if (dragged != null && dragged.backpackUI != null)
            dragged.backpackUI.Refresh();
    }

    private void HandleBagToBag(BagSlotUI sourceUI)
    {
        if (sourceUI == null)
            return;

        if (sourceUI.bagId == bagId && sourceUI.slotIndex == slotIndex)
            return;

        BagSlotData source = sourceUI.GetThisSlot();
        BagSlotData target = GetThisSlot();

        if (source == null || target == null || source.IsEmpty())
            return;

        if (TryStackBagIntoBag(source, target))
        {
            sourceUI.Refresh();
            Refresh();
            return;
        }

        ItemData sourceItem = source.item;
        int sourceAmount = source.amount;

        if (sourceItem != null && sourceItem.itemType == ItemType.Bag && sourceAmount > 1)
            return;

        if (target.IsEmpty())
        {
            target.SetItem(sourceItem, sourceAmount);
            source.Clear();
        }
        else
        {
            ItemData targetItem = target.item;
            int targetAmount = target.amount;

            target.SetItem(sourceItem, sourceAmount);
            source.SetItem(targetItem, targetAmount);
        }

        sourceUI.Refresh();
        Refresh();
    }

    private bool TryStackBagIntoBag(BagSlotData source, BagSlotData target)
    {
        if (source == null || target == null)
            return false;

        if (source.IsEmpty() || target.IsEmpty())
            return false;

        if (source.item == null || target.item == null)
            return false;

        if (!source.item.isStackable)
            return false;

        if (source.item.itemId != target.item.itemId)
            return false;

        int maxStack = Mathf.Max(1, source.item.maxStackSize);
        int freeSpace = maxStack - target.amount;

        if (freeSpace <= 0)
            return false;

        int moveAmount = Mathf.Min(freeSpace, source.amount);

        target.amount += moveAmount;
        source.amount -= moveAmount;

        if (source.amount <= 0)
            source.Clear();

        return true;
    }

    private void HandleBagToBackpack(BackpackSlotUI targetBackpackSlot)
    {
        if (targetBackpackSlot == null)
            return;

        if (BackpackInventorySystem.Instance == null)
            return;

        BagSlotData source = GetThisSlot();
        BackpackSlotData target = BackpackInventorySystem.Instance.GetSlot(targetBackpackSlot.slotIndex);

        if (source == null || target == null || source.IsEmpty())
            return;

        if (TryStackBagIntoBackpack(source, target))
        {
            targetBackpackSlot.RefreshVisual();
            Refresh();
            return;
        }

        ItemData sourceItem = source.item;
        int sourceAmount = source.amount;

        if (sourceItem != null && sourceItem.itemType == ItemType.Bag && sourceAmount > 1)
            return;

        if (target.IsEmpty())
        {
            target.SetItem(sourceItem, sourceAmount);
            source.Clear();
        }
        else
        {
            ItemData targetItem = target.itemData;
            int targetAmount = target.count;

            target.SetItem(sourceItem, sourceAmount);
            source.SetItem(targetItem, targetAmount);
        }

        targetBackpackSlot.RefreshVisual();
        Refresh();
    }

    private bool TryStackBagIntoBackpack(BagSlotData source, BackpackSlotData target)
    {
        if (source == null || target == null)
            return false;

        if (source.IsEmpty() || target.IsEmpty())
            return false;

        if (source.item == null || target.itemData == null)
            return false;

        if (!source.item.isStackable)
            return false;

        if (source.item.itemId != target.itemData.itemId)
            return false;

        int maxStack = Mathf.Max(1, source.item.maxStackSize);
        int freeSpace = maxStack - target.count;

        if (freeSpace <= 0)
            return false;

        int moveAmount = Mathf.Min(freeSpace, source.amount);

        target.count += moveAmount;
        source.amount -= moveAmount;

        if (source.amount <= 0)
            source.Clear();

        return true;
    }

    private BagSlotData GetThisSlot()
    {
        if (BagInventorySystem.Instance == null)
            return null;

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

        if (bag == null || bag.slots == null || slotIndex < 0 || slotIndex >= bag.slots.Length)
            return null;

        if (bag.slots[slotIndex] == null)
            bag.slots[slotIndex] = new BagSlotData();

        return bag.slots[slotIndex];
    }
}