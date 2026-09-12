using UnityEngine;
using UnityEngine.EventSystems;

public class BagWindowDragUI : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Allowed Window")]
    public bool canMoveThisWindow = true;

    [Header("Return Position")]
    public bool returnToStartPositionOnEnable = true;

    private RectTransform windowRect;
    private Canvas rootCanvas;
    private Vector2 startAnchoredPosition;
    private Vector2 pointerOffset;

    private void Awake()
    {
        windowRect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (windowRect != null)
            startAnchoredPosition = windowRect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (returnToStartPositionOnEnable && windowRect != null)
            windowRect.anchoredPosition = startAnchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canMoveThisWindow)
            return;

        // ✅ NOW USING LEFT CLICK
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (windowRect == null)
            return;

        Camera uiCamera = null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowRect,
            eventData.position,
            uiCamera,
            out pointerOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canMoveThisWindow)
            return;

        // ✅ NOW USING LEFT CLICK
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (windowRect == null)
            return;

        RectTransform parentRect = windowRect.parent as RectTransform;

        if (parentRect == null)
            return;

        Camera uiCamera = null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            uiCamera,
            out Vector2 localPointerPosition
        );

        windowRect.anchoredPosition = localPointerPosition - pointerOffset;
    }
}