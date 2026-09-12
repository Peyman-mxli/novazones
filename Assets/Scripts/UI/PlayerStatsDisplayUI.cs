using UnityEngine;
using TMPro;

public class PlayerStatsDisplayUI : MonoBehaviour
{
    [Header("Settings")]
    public bool useColoredStats = true;

    [Header("References")]
    public PlayerStats playerStats;

    [Header("Combat Stat Texts")]
    public TMP_Text critRateText;
    public TMP_Text critDamageText;
    public TMP_Text critHealText;
    public TMP_Text hitRateText;
    public TMP_Text dodgeText;
    public TMP_Text blockText;
    public TMP_Text novaRegenText;
    public TMP_Text healthRegenText;
    public TMP_Text hasteRatingText;
    public TMP_Text attackPowerText;
    public TMP_Text attackSpeedText;
    public TMP_Text defenseText;
    public TMP_Text armorText;
    public TMP_Text armorPenetrationText;
    public TMP_Text threatMultiplierText;
    public TMP_Text expertiseText;

    [Header("Stat Cap Texts")]
    public TMP_Text maxDefenseText;
    public TMP_Text maxDodgeText;
    public TMP_Text maxCritRateText;
    public TMP_Text maxAttackSpeedText;
    public TMP_Text maxNovaText;
    public TMP_Text maxCritHealText;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        RefreshStats();
    }

    public void SetColoredStats(bool enabled)
    {
        useColoredStats = enabled;
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (playerStats == null)
            return;

        SetText(critRateText, "Crit Rate", playerStats.critRate.ToString("0.##"), "#FF3B3B");
        SetText(critDamageText, "Crit Damage", playerStats.critDamage.ToString("0.##"), "#B30000");
        SetText(critHealText, "Crit Heal", playerStats.critHeal.ToString("0.##"), "#D94A64");

        SetText(hitRateText, "Hit Rate", playerStats.hitRate.ToString("0.##"), "#FF9F1C");
        SetText(dodgeText, "Dodge", playerStats.dodge.ToString("0.##"), "#00C853");
        SetText(blockText, "Block", playerStats.block.ToString("0.##"), "#8E24AA");
        SetText(novaRegenText, "Nova Regen", playerStats.novaRegen.ToString("0.##"), "#FF1493");
        SetText(healthRegenText, "Health Regen", playerStats.healthRegen.ToString("0.##"), "#64DD17");
        SetText(hasteRatingText, "Haste Rating", playerStats.hasteRating.ToString("0.##"), "#FF6F00");
        SetText(attackPowerText, "Attack Power", playerStats.attackPower.ToString(), "#FF5722");
        SetText(attackSpeedText, "Attack Speed", playerStats.attackSpeed.ToString("0.##"), "#FFD600");
        SetText(defenseText, "Defense", playerStats.defense.ToString(), "#78909C");
        SetText(armorText, "Armor", playerStats.armor.ToString(), "#C49A00");
        SetText(armorPenetrationText, "Armor Penetration", playerStats.armorPenetration.ToString("0.##"), "#651FFF");
        SetText(threatMultiplierText, "Threat Multiplier", playerStats.threatMultiplier.ToString("0.##"), "#7A1F1F");
        SetText(expertiseText, "Expertise", playerStats.expertise.ToString(), "#00B8D4");

        SetText(maxDefenseText, "Max Defense", playerStats.maxDefense.ToString(), "#1B5E20");
        SetText(maxDodgeText, "Max Dodge", playerStats.maxDodge.ToString("0.##"), "#9C27B0");
        SetText(maxCritRateText, "Max Crit Rate", playerStats.maxCritRate.ToString("0.##"), "#F44336");
        SetText(maxAttackSpeedText, "Max Attack Speed", playerStats.maxAttackSpeed.ToString("0.##"), "#FB8C00");
        SetText(maxNovaText, "Max Nova", playerStats.maxNova.ToString(), "#E91E63");
        SetText(maxCritHealText, "Max Crit Heal", playerStats.maxCritHeal.ToString("0.##"), "#C2185B");
    }

    void SetText(TMP_Text textField, string statName, string value, string hexColor)
    {
        if (textField == null)
            return;

        if (useColoredStats)
            textField.text = "<color=" + hexColor + ">" + statName + "</color>: " + value;
        else
            textField.text = statName + ": " + value;
    }
}