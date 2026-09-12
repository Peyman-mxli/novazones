using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LootSlotHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Scale")]
    public float hoverScale = 1.05f;
    public float scaleSpeed = 12f;

    [Header("Glow")]
    public Image glowImage;
    public float glowFadeSpeed = 12f;
    public float glowMaxAlpha = 0.35f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isHovering;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (glowImage != null)
        {
            Color color = glowImage.color;
            color.a = 0f;
            glowImage.color = color;
            glowImage.enabled = true;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );

        UpdateGlowFade();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
    }

    private void UpdateGlowFade()
    {
        if (glowImage == null)
            return;

        float targetAlpha = isHovering ? glowMaxAlpha : 0f;

        Color color = glowImage.color;
        color.a = Mathf.Lerp(
            color.a,
            targetAlpha,
            Time.unscaledDeltaTime * glowFadeSpeed
        );

        glowImage.color = color;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovering = false;

        if (glowImage != null)
        {
            Color color = glowImage.color;
            color.a = 0f;
            glowImage.color = color;
        }
    }
}