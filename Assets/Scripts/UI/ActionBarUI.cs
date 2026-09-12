using UnityEngine;
using TMPro;

public class ActionBarUI : MonoBehaviour
{
    [Header("Slot Labels")]
    public TMP_Text[] slotTexts = new TMP_Text[12];

    [Header("Optional Manual Slots")]
    public ActionBarSlotUI[] actionSlots = new ActionBarSlotUI[12];

    [Header("Slot Layout")]
    public float startX = 100f;
    public float startY = 100f;
    public float slotSize = 45f;
    public float spacing = 8f;

    private string[] keyLabels =
    {
        "1","2","3","4","5","6",
        "7","8","9","0","-","="
    };

    private KeyCode[] keyCodes =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
        KeyCode.Minus,
        KeyCode.Equals
    };

    private void Start()
    {
        SetupBar();
        AutoFindSlotsFromKeyTexts();
    }

    private void Update()
    {
        HandleHotkeys();
    }

    private void SetupBar()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] == null)
                continue;

            slotTexts[i].text = keyLabels[i];

            RectTransform keyTextRect = slotTexts[i].GetComponent<RectTransform>();
            RectTransform slotRect = keyTextRect.parent.GetComponent<RectTransform>();

            if (slotRect != null)
            {
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.pivot = new Vector2(0.5f, 0.5f);

                slotRect.sizeDelta = new Vector2(slotSize, slotSize);
                slotRect.anchoredPosition = new Vector2(startX + i * (slotSize + spacing), startY);
            }

            keyTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            keyTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            keyTextRect.pivot = new Vector2(0.5f, 0.5f);
            keyTextRect.anchoredPosition = Vector2.zero;
        }
    }

    private void AutoFindSlotsFromKeyTexts()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (i >= actionSlots.Length)
                continue;

            if (actionSlots[i] != null)
                continue;

            if (slotTexts[i] == null)
                continue;

            ActionBarSlotUI slot = slotTexts[i].GetComponentInParent<ActionBarSlotUI>();

            if (slot != null)
                actionSlots[i] = slot;
        }
    }

    private void HandleHotkeys()
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (!Input.GetKeyDown(keyCodes[i]))
                continue;

            if (i >= actionSlots.Length)
                continue;

            if (actionSlots[i] == null)
                AutoFindSlotsFromKeyTexts();

            if (actionSlots[i] == null)
                continue;

            actionSlots[i].ExecuteAssignedSkill();
        }
    }
}