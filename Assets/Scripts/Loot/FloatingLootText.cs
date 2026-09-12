using UnityEngine;
using TMPro;

public class FloatingLootText : MonoBehaviour
{
    [Header("Animation")]
    public float moveSpeed = 45f;
    public float fadeSpeed = 2f;
    public float visibleTime = 0.6f;

    [Header("Pop Effect")]
    public float startScale = 1.35f;
    public float normalScale = 1f;
    public float scaleSpeed = 8f;

    [Header("Spawn Offset (EDIT IN INSPECTOR)")]
    public Vector2 spawnOffset = new Vector2(80f, 60f);
    // X = right (+) / left (-)
    // Y = up (+) / down (-)

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;

    private Color startColor;
    private float timer;
    private bool isShowing;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        startColor = textMesh.color;

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        textMesh.color = startColor;
        rectTransform.localScale = Vector3.one * startScale;

        timer = 0f;
        isShowing = true;
    }

    void Update()
    {
        if (!isShowing)
            return;

        timer += Time.deltaTime;

        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            Vector3.one * normalScale,
            scaleSpeed * Time.deltaTime
        );

        if (timer >= visibleTime)
        {
            Color c = textMesh.color;
            c.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = c;

            if (c.a <= 0f)
            {
                isShowing = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowText(string message)
    {
        gameObject.SetActive(true);

        textMesh.text = message;

        Vector2 screenPos = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPoint
        );

        // ✅ APPLY OFFSET HERE
        rectTransform.anchoredPosition = localPoint + spawnOffset;

        rectTransform.localScale = Vector3.one * startScale;

        textMesh.color = startColor;

        timer = 0f;
        isShowing = true;
    }
}