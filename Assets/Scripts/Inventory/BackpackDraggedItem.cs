using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackDraggedItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Slot Info")]
    public int originalSlotIndex = -1;

    [Header("Owner UI")]
    public BackpackUI backpackUI;

    [Header("Drag Icon")]
    public Image dragIcon;

    private Canvas rootCanvas;
    private Transform originalIconParent;
    private Vector3 originalIconPosition;

    private bool isDragging;
    private bool droppedOnValidSlot;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (dragIcon != null)
        {
            originalIconParent = dragIcon.transform.parent;
            originalIconPosition = dragIcon.transform.position;
        }
    }

    public void Setup(BackpackUI owner, int slotIndex)
    {
        backpackUI = owner;
        originalSlotIndex = slotIndex;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (BackpackInventorySystem.Instance == null)
            return;

        BackpackSlotData slot = BackpackInventorySystem.Instance.GetSlot(originalSlotIndex);

        if (slot == null || slot.IsEmpty())
            return;

        isDragging = true;
        droppedOnValidSlot = false;

        if (dragIcon != null)
        {
            originalIconParent = dragIcon.transform.parent;
            originalIconPosition = dragIcon.transform.position;

            if (rootCanvas != null)
                dragIcon.transform.SetParent(rootCanvas.transform, true);

            dragIcon.transform.SetAsLastSibling();
            dragIcon.raycastTarget = false;
            dragIcon.enabled = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragIcon == null)
            return;

        dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        droppedOnValidSlot = false;

        if (eventData.pointerEnter != null)
        {
            BackpackSlotUI backpackSlotUI = eventData.pointerEnter.GetComponentInParent<BackpackSlotUI>();
            BagEquipSlotUI bagEquipSlotUI = eventData.pointerEnter.GetComponentInParent<BagEquipSlotUI>();
            BagSlotUI bagSlotUI = eventData.pointerEnter.GetComponentInParent<BagSlotUI>();

            if (backpackSlotUI != null || bagEquipSlotUI != null || bagSlotUI != null)
                droppedOnValidSlot = true;
        }

        if (!droppedOnValidSlot && IsMouseOverDeleteZone(eventData.position))
        {
            if (DeleteItemPopupUI.Instance != null)
                DeleteItemPopupUI.Instance.Open(originalSlotIndex, backpackUI);
        }

        if (dragIcon != null)
        {
            dragIcon.transform.SetParent(originalIconParent, true);
            dragIcon.transform.position = originalIconPosition;
            dragIcon.raycastTarget = true;
        }

        isDragging = false;

        if (backpackUI != null)
            backpackUI.Refresh();
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
}