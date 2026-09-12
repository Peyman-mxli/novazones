using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterWindowDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Optional Linked Window")]
    public RectTransform extraStatsWindow;

    private RectTransform windowRect;
    private Canvas canvas;

    void Awake()
    {
        windowRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();

        if (extraStatsWindow != null)
            extraStatsWindow.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowRect == null || canvas == null)
            return;

        Vector2 moveAmount = eventData.delta / canvas.scaleFactor;

        windowRect.anchoredPosition += moveAmount;

        if (extraStatsWindow != null)
            extraStatsWindow.anchoredPosition += moveAmount;
    }
}