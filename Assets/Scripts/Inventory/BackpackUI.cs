using TMPro;
using UnityEngine;

public class BackpackUI : MonoBehaviour
{
    public static BackpackUI Instance;

    [Header("Main Window")]
    public GameObject backpackWindow;

    [Header("Slots")]
    public BackpackSlotUI[] backpackSlots;

    [Header("Money Text")]
    public TMP_Text goldText;
    public TMP_Text silverText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupSlots();
        Refresh();
    }

    private void SetupSlots()
    {
        if (backpackSlots == null)
            return;

        for (int i = 0; i < backpackSlots.Length; i++)
        {
            if (backpackSlots[i] != null)
                backpackSlots[i].Setup(this, i);
        }
    }

    public void ToggleBackpack()
    {
        if (backpackWindow == null)
            return;

        backpackWindow.SetActive(!backpackWindow.activeSelf);
        Refresh();
    }

    public void OpenBackpack()
    {
        if (backpackWindow != null)
            backpackWindow.SetActive(true);

        Refresh();
    }

    public void CloseBackpack()
    {
        if (backpackWindow != null)
            backpackWindow.SetActive(false);
    }

    public void ToggleAllInventoryWindows()
    {
        BagEquipSlotUI[] allBagSlots = FindObjectsByType<BagEquipSlotUI>(FindObjectsSortMode.None);

        bool allOpen = backpackWindow != null && backpackWindow.activeSelf;

        for (int i = 0; i < allBagSlots.Length; i++)
        {
            if (allBagSlots[i] == null || allBagSlots[i].bagWindow == null)
                continue;

            if (!allBagSlots[i].HasEquippedBag())
                continue;

            if (!allBagSlots[i].bagWindow.activeSelf)
            {
                allOpen = false;
                break;
            }
        }

        bool shouldOpen = !allOpen;

        if (backpackWindow != null)
            backpackWindow.SetActive(shouldOpen);

        for (int i = 0; i < allBagSlots.Length; i++)
        {
            if (allBagSlots[i] == null || allBagSlots[i].bagWindow == null)
                continue;

            if (!allBagSlots[i].HasEquippedBag())
                continue;

            allBagSlots[i].bagWindow.SetActive(shouldOpen);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (BackpackInventorySystem.Instance == null)
            return;

        if (goldText != null)
            goldText.text = BackpackInventorySystem.Instance.gold.ToString() + "g";

        if (silverText != null)
            silverText.text = BackpackInventorySystem.Instance.silver.ToString() + "s";

        if (backpackSlots != null)
        {
            for (int i = 0; i < backpackSlots.Length; i++)
            {
                if (backpackSlots[i] != null)
                    backpackSlots[i].RefreshVisual();
            }
        }
    }
}