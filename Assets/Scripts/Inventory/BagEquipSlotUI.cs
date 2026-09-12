using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagEquipSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("Bag Slot Identity")]
    public int bagId = 1;

    [Header("UI")]
    public Image bagIcon;

    [Header("Bag Window")]
    public GameObject bagWindow;

    private Canvas rootCanvas;
    private Transform originalIconParent;
    private Vector3 originalIconPosition;

    private bool isDragging;
    private static int lastToggleFrame = -1;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (bagIcon != null)
        {
            originalIconParent = bagIcon.transform.parent;
            originalIconPosition = bagIcon.transform.position;
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
        CheckCtrlRightClickGlobal();
    }

    private void CheckCtrlRightClickGlobal()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            return;

        if (Time.frameCount == lastToggleFrame)
            return;

        lastToggleFrame = Time.frameCount;

        BackpackUI backpackUI = BackpackUI.Instance;

        if (backpackUI != null)
            backpackUI.ToggleAllInventoryWindows();
    }

    public bool HasEquippedBag()
    {
        if (BagInventorySystem.Instance == null)
            return false;

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);
        return bag != null && bag.equippedBagItem != null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BagInventorySystem.Instance == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Right &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

            if (bag == null || bag.equippedBagItem == null)
                return;

            if (bagWindow != null)
                bagWindow.SetActive(!bagWindow.activeSelf);

            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            bool returned = BagInventorySystem.Instance.TryReturnBagToBackpack(bagId);

            if (returned)
            {
                if (bagWindow != null)
                    bagWindow.SetActive(false);

                Refresh();
            }

            return;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (BagInventorySystem.Instance == null)
            return;

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

        if (bag == null || bag.equippedBagItem == null)
            return;

        isDragging = true;

        if (bagWindow != null)
            bagWindow.SetActive(false);

        if (bagIcon != null)
        {
            originalIconParent = bagIcon.transform.parent;
            originalIconPosition = bagIcon.transform.position;

            if (rootCanvas != null)
                bagIcon.transform.SetParent(rootCanvas.transform, true);

            bagIcon.transform.SetAsLastSibling();
            bagIcon.raycastTarget = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || bagIcon == null)
            return;

        bagIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
            TryDropEquippedBagIntoStorage(eventData);

        if (bagIcon != null)
        {
            bagIcon.transform.SetParent(originalIconParent, true);
            bagIcon.transform.position = originalIconPosition;
            bagIcon.raycastTarget = true;
        }

        isDragging = false;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (BagInventorySystem.Instance == null)
            return;

        BackpackDraggedItem backpackDraggedItem = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<BackpackDraggedItem>()
            : null;

        if (backpackDraggedItem != null)
        {
            HandleDropFromBackpack(backpackDraggedItem);
            return;
        }

        BagSlotUI sourceBagSlot = BagSlotUI.CurrentDraggedBagSlot;

        if (sourceBagSlot == null && eventData.pointerDrag != null)
            sourceBagSlot = eventData.pointerDrag.GetComponentInParent<BagSlotUI>();

        if (sourceBagSlot != null)
        {
            TryPlaceBagFromBagSlot(sourceBagSlot);
            return;
        }

        BagEquipSlotUI sourceBagEquipSlot = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponentInParent<BagEquipSlotUI>()
            : null;

        if (sourceBagEquipSlot != null)
            HandleDropFromBagSlot(sourceBagEquipSlot);
    }

    private void TryDropEquippedBagIntoStorage(PointerEventData eventData)
    {
        if (BagInventorySystem.Instance == null)
            return;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            BagSlotUI targetBagSlot = results[i].gameObject.GetComponentInParent<BagSlotUI>();

            if (targetBagSlot != null)
            {
                targetBagSlot.TryPlaceEquippedBagHere(this);
                return;
            }
        }
    }

    public bool TryPlaceBagFromBagSlot(BagSlotUI sourceBagSlot)
    {
        if (sourceBagSlot == null)
            return false;

        if (BagInventorySystem.Instance == null)
            return false;

        BagSlotData source = sourceBagSlot.GetSlotDataForExternalUse();

        if (source == null || source.IsEmpty() || source.item == null)
            return false;

        ItemData sourceItem = source.item;

        if (!BagInventorySystem.Instance.CanItemGoIntoBagSlot(sourceItem))
            return false;

        if (source.amount > 1)
            return false;

        BagContainerData targetBag = BagInventorySystem.Instance.GetBag(bagId);

        if (targetBag == null || targetBag.isLocked)
            return false;

        if (targetBag.equippedBagItem == null)
        {
            targetBag.Setup(bagId, sourceItem, sourceItem.bagSlotCount);
            source.Clear();

            sourceBagSlot.Refresh();
            Refresh();
            return true;
        }

        if (!targetBag.IsEmpty())
        {
            Debug.LogWarning("Cannot replace this bag because it has items inside.");
            return false;
        }

        ItemData oldEquippedBagItem = targetBag.equippedBagItem;

        targetBag.Setup(bagId, sourceItem, sourceItem.bagSlotCount);
        source.SetItem(oldEquippedBagItem, 1);

        if (bagWindow != null)
            bagWindow.SetActive(false);

        sourceBagSlot.Refresh();
        Refresh();
        return true;
    }

    private void HandleDropFromBackpack(BackpackDraggedItem draggedItem)
    {
        if (BackpackInventorySystem.Instance == null)
            return;

        BackpackSlotData backpackSlot = BackpackInventorySystem.Instance.GetSlot(draggedItem.originalSlotIndex);

        if (backpackSlot == null || backpackSlot.IsEmpty())
            return;

        ItemData item = backpackSlot.itemData;

        if (!BagInventorySystem.Instance.CanItemGoIntoBagSlot(item))
            return;

        bool equipped = BagInventorySystem.Instance.TryEquipBagToSlot(item, bagId);

        if (!equipped)
            return;

        backpackSlot.Clear();

        if (draggedItem.backpackUI != null)
            draggedItem.backpackUI.Refresh();

        Refresh();
    }

    private void HandleDropFromBagSlot(BagEquipSlotUI sourceBagSlot)
    {
        if (sourceBagSlot == null)
            return;

        bool moved = BagInventorySystem.Instance.TryMoveOrSwapEquippedBag(sourceBagSlot.bagId, bagId);

        if (!moved)
            return;

        if (sourceBagSlot.bagWindow != null)
            sourceBagSlot.bagWindow.SetActive(false);

        if (bagWindow != null)
            bagWindow.SetActive(false);

        sourceBagSlot.Refresh();
        Refresh();
    }

    public void Refresh()
    {
        if (bagIcon == null)
            return;

        if (BagInventorySystem.Instance == null)
        {
            bagIcon.enabled = false;
            bagIcon.sprite = null;
            return;
        }

        BagContainerData bag = BagInventorySystem.Instance.GetBag(bagId);

        if (bag == null || bag.equippedBagItem == null || bag.equippedBagItem.itemIcon == null)
        {
            bagIcon.enabled = false;
            bagIcon.sprite = null;
            return;
        }

        bagIcon.enabled = true;
        bagIcon.sprite = bag.equippedBagItem.itemIcon;
    }
}