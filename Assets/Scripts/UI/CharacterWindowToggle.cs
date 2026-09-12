using UnityEngine;

public class CharacterWindowToggle : MonoBehaviour
{
    [Header("Windows")]
    public GameObject characterWindow;
    public GameObject extraStatsWindow;

    [Header("Keyboard")]
    public KeyCode toggleKey = KeyCode.C;

    [Header("Reset Position")]
    public Vector2 characterOpenPosition = Vector2.zero;
    public Vector2 extraStatsOpenPosition = new Vector2(-360f, 0f);

    private RectTransform characterRect;
    private RectTransform extraStatsRect;

    void Awake()
    {
        AutoFindWindows();
        CacheRects();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleWindow();
    }

    void AutoFindWindows()
    {
        if (characterWindow == null)
            characterWindow = GameObject.Find("CharacterStatusWindow");

        if (extraStatsWindow == null)
            extraStatsWindow = GameObject.Find("CharacterExtraStatsWindow");
    }

    void CacheRects()
    {
        if (characterWindow != null)
            characterRect = characterWindow.GetComponent<RectTransform>();

        if (extraStatsWindow != null)
            extraStatsRect = extraStatsWindow.GetComponent<RectTransform>();
    }

    public void ToggleWindow()
    {
        if (characterWindow == null)
            return;

        if (characterWindow.activeSelf)
            CloseWindow();
        else
            OpenWindow();
    }

    public void OpenWindow()
    {
        ResetWindowPositions();

        if (characterWindow != null)
            characterWindow.SetActive(true);

        if (extraStatsWindow != null)
            extraStatsWindow.SetActive(false);
    }

    public void CloseWindow()
    {
        if (characterWindow != null)
            characterWindow.SetActive(false);

        if (extraStatsWindow != null)
            extraStatsWindow.SetActive(false);
    }

    void ResetWindowPositions()
    {
        if (characterRect == null || extraStatsRect == null)
            CacheRects();

        if (characterRect != null)
            characterRect.anchoredPosition = characterOpenPosition;

        if (extraStatsRect != null)
            extraStatsRect.anchoredPosition = characterOpenPosition + extraStatsOpenPosition;
    }
}