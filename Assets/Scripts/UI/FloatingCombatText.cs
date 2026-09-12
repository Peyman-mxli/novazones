using UnityEngine;
using TMPro;

public class FloatingCombatText : MonoBehaviour
{
    [Header("Display Option")]
    public bool combatTextEnabled = true;

    [Header("Enemy / Outgoing Text Sizes")]
    public float normalHitSize = 1.8f;
    public float critHitSize = 2.8f;
    public float statusTextSize = 2.1f;

    [Header("Animation")]
    public float moveSpeed = 1.5f;
    public float fadeSpeed = 2f;
    public float visibleTime = 0.5f;

    [Header("Pop Effect")]
    public float startScale = 1.15f;
    public float normalScale = 1f;
    public float critStartScale = 1.35f;
    public float scaleSpeed = 8f;

    private TextMeshPro textMesh;
    private Color startColor;
    private float activeFontSize;
    private float activeStartScale;
    private float timer;
    private bool isShowing;
    private Camera mainCamera;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        mainCamera = Camera.main;

        if (textMesh != null)
        {
            textMesh.fontSize = normalHitSize;
            textMesh.alignment = TextAlignmentOptions.Center;
        }

        activeFontSize = normalHitSize;
        activeStartScale = startScale;

        if (textMesh != null)
            startColor = textMesh.color;
    }

    void OnEnable()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        ResetText();
    }

    void Update()
    {
        if (!isShowing || textMesh == null)
            return;

        FaceCamera();

        timer += Time.deltaTime;
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * normalScale,
            scaleSpeed * Time.deltaTime
        );

        if (timer >= visibleTime)
        {
            Color c = textMesh.color;
            c.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = c;

            if (c.a <= 0f)
                Destroy(gameObject);
        }
    }

    void FaceCamera()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        transform.rotation = mainCamera.transform.rotation;
    }

    public void ShowText(string message, Color color)
    {
        ShowText(message, color, normalHitSize, startScale);
    }

    public void ShowNormalHit(string message)
    {
        ShowText(message, Color.white, normalHitSize, startScale);
    }

    public void ShowCritHit(string message)
    {
        ShowText(message, HexToColor("#FFD600"), critHitSize, critStartScale);
    }

    public void ShowStatusText(string message, Color color)
    {
        ShowText(message, color, statusTextSize, startScale);
    }

    public void ShowText(string message, Color color, float customFontSize, float customStartScale)
    {
        if (!combatTextEnabled)
        {
            Destroy(gameObject);
            return;
        }

        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (textMesh == null)
            return;

        activeFontSize = customFontSize;
        activeStartScale = customStartScale;

        textMesh.text = message;
        textMesh.color = color;
        textMesh.fontSize = activeFontSize;

        startColor = color;

        ResetText();
    }

    void ResetText()
    {
        timer = 0f;
        isShowing = true;

        transform.localScale = Vector3.one * activeStartScale;

        if (textMesh != null)
        {
            textMesh.color = startColor;
            textMesh.fontSize = activeFontSize;
        }

        FaceCamera();
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}