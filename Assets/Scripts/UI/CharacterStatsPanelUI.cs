using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsPanelUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Main Tabs")]
    public Button characterTabButton;
    public Button talentsTabButton;
    public Button skillsTabButton;

    public GameObject characterTabPanel;
    public GameObject talentsTabPanel;
    public GameObject skillsTabPanel;

    [Header("Base Stats Dropdown")]
    public GameObject baseStatsDropdownPanel;
    public Button baseStatsDropdownButton;
    public Button baseStatsCategoryButton;
    public Button extraStatsCategoryButton;
    public TMP_Text baseStatsHeaderText;

    [Header("Extra Stats Window")]
    public GameObject characterExtraStatsWindow;

    [Header("Base Stat Labels")]
    public TMP_Text strengthLabel;
    public TMP_Text agilityLabel;
    public TMP_Text staminaLabel;
    public TMP_Text novaLabel;
    public TMP_Text spiritLabel;
    public TMP_Text armorLabel;

    [Header("Base Stat Values")]
    public TMP_Text strengthValue;
    public TMP_Text agilityValue;
    public TMP_Text staminaValue;
    public TMP_Text novaValue;
    public TMP_Text spiritValue;
    public TMP_Text armorValue;

    [Header("Extra Stats - Melee")]
    public TMP_Text meleeHitChanceValue;
    public TMP_Text meleeMissChanceValue;
    public TMP_Text meleeCritChanceValue;
    public TMP_Text meleeCritRatingValue;
    public TMP_Text meleeExpertiseValue;
    public TMP_Text meleeArmorPenetrationValue;

    [Header("Extra Stats - Ranged")]
    public TMP_Text rangedHitChanceValue;
    public TMP_Text rangedCritChanceValue;
    public TMP_Text rangedCritRateValue;
    public TMP_Text rangedHasteRatingValue;
    public TMP_Text rangedArmorPenetrationValue;

    [Header("Extra Stats - Spell")]
    public TMP_Text spellHitChanceValue;
    public TMP_Text spellCritChanceValue;
    public TMP_Text spellCritRateValue;
    public TMP_Text spellHasteRatingValue;
    public TMP_Text spellPenetrationValue;

    [Header("Extra Stats - Defense")]
    public TMP_Text defenseRatingValue;
    public TMP_Text defenseToCapValue;
    public TMP_Text defenseResilienceValue;
    public TMP_Text defenseDodgeValue;
    public TMP_Text defenseParryValue;
    public TMP_Text defenseBlockValue;

    [Header("Stat Caps")]
    public int maxStrength = 1000;
    public int maxAgility = 1000;
    public int maxStamina = 1000;
    public int maxNova = 20000;
    public int maxSpirit = 1000;
    public int maxArmor = 10000;

    [Header("Extra Stat Rules")]
    public float defaultMissChance = 5f;
    public int defenseCap = 540;

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            FindPlayerStats();
            SetupButtons();
            ShowCharacterTab();
        }
    }

    void Start()
    {
        FindPlayerStats();
        SetupButtons();
        ShowCharacterTab();
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (playerStats == null)
            FindPlayerStats();

        UpdateBaseStats();
        UpdateExtraStats();
    }

    void SetupButtons()
    {
        if (characterTabButton != null)
        {
            characterTabButton.onClick.RemoveAllListeners();
            characterTabButton.onClick.AddListener(ShowCharacterTab);
        }

        if (talentsTabButton != null)
        {
            talentsTabButton.onClick.RemoveAllListeners();
            talentsTabButton.onClick.AddListener(ShowTalentsTab);
        }

        if (skillsTabButton != null)
        {
            skillsTabButton.onClick.RemoveAllListeners();
            skillsTabButton.onClick.AddListener(ShowSkillsTab);
        }

        if (baseStatsDropdownButton != null)
        {
            baseStatsDropdownButton.onClick.RemoveAllListeners();
            baseStatsDropdownButton.onClick.AddListener(ToggleDropdown);
        }

        if (baseStatsCategoryButton != null)
        {
            baseStatsCategoryButton.onClick.RemoveAllListeners();
            baseStatsCategoryButton.onClick.AddListener(ShowBaseStats);
        }

        if (extraStatsCategoryButton != null)
        {
            extraStatsCategoryButton.onClick.RemoveAllListeners();
            extraStatsCategoryButton.onClick.AddListener(OpenExtraStatsWindow);
        }

        if (baseStatsDropdownPanel != null)
            baseStatsDropdownPanel.SetActive(false);

        if (characterExtraStatsWindow != null)
            characterExtraStatsWindow.SetActive(false);
    }

    public void ShowCharacterTab()
    {
        SetMainTab(characterTabPanel);
        ShowBaseStats();
    }

    public void ShowTalentsTab()
    {
        SetMainTab(talentsTabPanel);
    }

    public void ShowSkillsTab()
    {
        SetMainTab(skillsTabPanel);
    }

    void SetMainTab(GameObject panelToShow)
    {
        if (characterTabPanel != null)
            characterTabPanel.SetActive(panelToShow == characterTabPanel);

        if (talentsTabPanel != null)
            talentsTabPanel.SetActive(panelToShow == talentsTabPanel);

        if (skillsTabPanel != null)
            skillsTabPanel.SetActive(panelToShow == skillsTabPanel);

        if (baseStatsDropdownPanel != null)
            baseStatsDropdownPanel.SetActive(false);

        if (characterExtraStatsWindow != null)
            characterExtraStatsWindow.SetActive(false);
    }

    void ToggleDropdown()
    {
        if (baseStatsDropdownPanel == null)
            return;

        baseStatsDropdownPanel.SetActive(!baseStatsDropdownPanel.activeSelf);
    }

    void ShowBaseStats()
    {
        SetText(baseStatsHeaderText, "Base Stats");

        SetText(strengthLabel, "Strength:");
        SetText(agilityLabel, "Agility:");
        SetText(staminaLabel, "Stamina:");
        SetText(novaLabel, "Nova:");
        SetText(spiritLabel, "Spirit:");
        SetText(armorLabel, "Armor:");

        if (baseStatsDropdownPanel != null)
            baseStatsDropdownPanel.SetActive(false);

        if (characterExtraStatsWindow != null)
            characterExtraStatsWindow.SetActive(false);

        UpdateBaseStats();
    }

    void OpenExtraStatsWindow()
    {
        SetText(baseStatsHeaderText, "Base Stats");

        if (baseStatsDropdownPanel != null)
            baseStatsDropdownPanel.SetActive(false);

        if (characterExtraStatsWindow != null)
            characterExtraStatsWindow.SetActive(true);

        UpdateExtraStats();
    }

    void UpdateBaseStats()
    {
        if (playerStats == null)
            return;

        UpdateValueText(strengthValue, playerStats.strength, maxStrength);
        UpdateValueText(agilityValue, playerStats.agility, maxAgility);
        UpdateValueText(staminaValue, playerStats.stamina, maxStamina);
        UpdateValueText(novaValue, playerStats.nova, maxNova);
        UpdateValueText(spiritValue, playerStats.spirit, maxSpirit);
        UpdateValueText(armorValue, playerStats.armor, maxArmor);
    }

    void UpdateExtraStats()
    {
        if (playerStats == null)
            return;

        float hitChance = playerStats.GetHitRate();
        float missChance = Mathf.Max(0f, defaultMissChance - hitChance);
        float critChance = playerStats.GetCritRate();
        float haste = Mathf.Max(0f, playerStats.hasteRating);
        float armorPenetration = Mathf.Max(0f, playerStats.armorPenetration);

        SetText(meleeHitChanceValue, FormatPercent(hitChance));
        SetText(meleeMissChanceValue, FormatPercent(missChance));
        SetText(meleeCritChanceValue, FormatPercent(critChance));
        SetText(meleeCritRatingValue, Mathf.RoundToInt(critChance).ToString());
        SetText(meleeExpertiseValue, playerStats.expertise.ToString());
        SetText(meleeArmorPenetrationValue, FormatPercent(armorPenetration));

        SetText(rangedHitChanceValue, FormatPercent(hitChance));
        SetText(rangedCritChanceValue, FormatPercent(critChance));
        SetText(rangedCritRateValue, Mathf.RoundToInt(critChance).ToString());
        SetText(rangedHasteRatingValue, FormatPercent(haste));
        SetText(rangedArmorPenetrationValue, FormatPercent(armorPenetration));

        SetText(spellHitChanceValue, FormatPercent(hitChance));
        SetText(spellCritChanceValue, FormatPercent(critChance));
        SetText(spellCritRateValue, Mathf.RoundToInt(critChance).ToString());
        SetText(spellHasteRatingValue, FormatPercent(haste));
        SetText(spellPenetrationValue, "0");

        SetText(defenseRatingValue, playerStats.defense.ToString());
        SetText(defenseToCapValue, Mathf.Max(0, defenseCap - playerStats.defense).ToString());
        SetText(defenseResilienceValue, "0");
        SetText(defenseDodgeValue, FormatPercent(playerStats.dodge));
        SetText(defenseParryValue, FormatPercent(0f));
        SetText(defenseBlockValue, FormatPercent(playerStats.block));
    }

    void FindPlayerStats()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            playerStats = playerObject.GetComponent<PlayerStats>();
    }

    string FormatPercent(float value)
    {
        return value.ToString("0.00") + " %";
    }

    void SetText(TMP_Text textObject, string value)
    {
        if (textObject != null)
            textObject.text = value;
    }

    void UpdateValueText(TMP_Text textObject, int value, int maxValue)
    {
        if (textObject == null)
            return;

        textObject.text = value.ToString();
        textObject.color = GetStatColor(value, maxValue);
    }

    Color GetStatColor(int value, int maxValue)
    {
        float percent = (float)value / maxValue;

        if (percent >= 1f) return HexToColor("#FF3333");
        if (percent >= 0.91f) return HexToColor("#FF8800");
        if (percent >= 0.81f) return HexToColor("#A64DFF");
        if (percent >= 0.66f) return HexToColor("#FFD700");
        if (percent >= 0.46f) return HexToColor("#3399FF");
        if (percent >= 0.26f) return HexToColor("#33CC66");
        if (percent >= 0.11f) return HexToColor("#9E9E9E");

        return HexToColor("#FFFFFF");
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}