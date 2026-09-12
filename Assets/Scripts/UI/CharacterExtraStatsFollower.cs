using UnityEngine;

public class CharacterExtraStatsFollower : MonoBehaviour
{
    [Header("Main Character Window")]
    public RectTransform characterStatusWindow;

    [Header("Extra Stats Position")]
    public float leftGap = 10f;

    private RectTransform extraStatsWindow;

    void Awake()
    {
        extraStatsWindow = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (extraStatsWindow == null || characterStatusWindow == null)
            return;

        Vector3[] corners = new Vector3[4];
        characterStatusWindow.GetWorldCorners(corners);

        Vector3 leftMiddle = (corners[0] + corners[1]) * 0.5f;

        float extraHalfWidth = extraStatsWindow.rect.width * 0.5f;
        Vector3 targetPosition = leftMiddle + new Vector3(-extraHalfWidth - leftGap, 0f, 0f);

        extraStatsWindow.position = targetPosition;
    }
}