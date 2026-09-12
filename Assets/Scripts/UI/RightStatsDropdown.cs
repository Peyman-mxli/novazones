using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RightStatsDropdown : MonoBehaviour
{
    [Header("Header")]
    public Button headerButton;
    public TMP_Text headerText;

    [Header("Dropdown Panel")]
    public GameObject dropdownPanel;

    [Header("Category Buttons")]
    public Button meleeButton;
    public Button rangedButton;
    public Button spellButton;
    public Button defenseButton;

    [Header("Content Panels")]
    public GameObject meleePanel;
    public GameObject rangedPanel;
    public GameObject spellPanel;
    public GameObject defensePanel;

    void Start()
    {
        SetupButtons();
        CloseDropdown();
        ShowMelee();
    }

    void SetupButtons()
    {
        if (headerButton != null)
        {
            headerButton.onClick.RemoveAllListeners();
            headerButton.onClick.AddListener(ToggleDropdown);
        }

        if (meleeButton != null)
        {
            meleeButton.onClick.RemoveAllListeners();
            meleeButton.onClick.AddListener(ShowMelee);
        }

        if (rangedButton != null)
        {
            rangedButton.onClick.RemoveAllListeners();
            rangedButton.onClick.AddListener(ShowRanged);
        }

        if (spellButton != null)
        {
            spellButton.onClick.RemoveAllListeners();
            spellButton.onClick.AddListener(ShowSpell);
        }

        if (defenseButton != null)
        {
            defenseButton.onClick.RemoveAllListeners();
            defenseButton.onClick.AddListener(ShowDefense);
        }
    }

    public void ToggleDropdown()
    {
        if (dropdownPanel == null)
            return;

        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }

    public void ShowMelee()
    {
        UpdatePanels(meleePanel);
        UpdateHeader("Melee");
    }

    public void ShowRanged()
    {
        UpdatePanels(rangedPanel);
        UpdateHeader("Ranged");
    }

    public void ShowSpell()
    {
        UpdatePanels(spellPanel);
        UpdateHeader("Spell");
    }

    public void ShowDefense()
    {
        UpdatePanels(defensePanel);
        UpdateHeader("Defense");
    }

    void UpdatePanels(GameObject activePanel)
    {
        if (meleePanel != null)
            meleePanel.SetActive(activePanel == meleePanel);

        if (rangedPanel != null)
            rangedPanel.SetActive(activePanel == rangedPanel);

        if (spellPanel != null)
            spellPanel.SetActive(activePanel == spellPanel);

        if (defensePanel != null)
            defensePanel.SetActive(activePanel == defensePanel);

        CloseDropdown();
    }

    void UpdateHeader(string categoryName)
    {
        if (headerText != null)
            headerText.text = categoryName;
    }

    void CloseDropdown()
    {
        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }
}