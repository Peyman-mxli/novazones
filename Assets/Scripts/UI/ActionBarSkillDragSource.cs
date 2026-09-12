using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBarSkillDragSource : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Skill Data")]
    public string skillName;
    public Sprite skillIcon;

    [Header("Skill Cooldown")]
    public float cooldownDuration = 0f;

    [Header("Global Cooldown")]
    public bool usesGlobalCooldown = true;

    private Canvas parentCanvas;
    private GameObject dragIconObject;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        if (skillIcon == null)
        {
            Image iconImage = GetComponentInChildren<Image>();

            if (iconImage != null)
                skillIcon = iconImage.sprite;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillIcon == null)
            return;

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        dragIconObject = new GameObject("DraggedSkillFromSkillsPanel");
        dragIconObject.transform.SetParent(parentCanvas.transform, false);
        dragIconObject.transform.SetAsLastSibling();

        Image dragImage = dragIconObject.AddComponent<Image>();
        dragImage.sprite = skillIcon;
        dragImage.raycastTarget = false;
        dragImage.preserveAspect = true;

        RectTransform dragRect = dragIconObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = GetDragIconSize();
        dragRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconObject != null)
            dragIconObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconObject != null)
            Destroy(dragIconObject);
    }

    private Vector2 GetDragIconSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null && rectTransform.sizeDelta != Vector2.zero)
            return rectTransform.sizeDelta;

        return new Vector2(40f, 40f);
    }
}