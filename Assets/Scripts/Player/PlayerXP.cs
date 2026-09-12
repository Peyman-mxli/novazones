using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXP : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI")]
    public Slider xpSlider;
    public TMP_Text xpText;

    void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
            playerStats.OnStatsChanged += UpdateXPUI;

        UpdateXPUI();
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= UpdateXPUI;
    }

    public void GainXP(int amount)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerXP: PlayerStats is missing.");
            return;
        }

        playerStats.AddXP(amount);
        UpdateXPUI();
    }

    public void UpdateXPUI()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerXP: PlayerStats is missing.");
            return;
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = playerStats.GetXPPercent();
        }

        if (xpText != null)
        {
            int currentXP = playerStats.GetCurrentXP();
            int totalXP = playerStats.GetXPToNextLevel();

            int percent = 0;
            if (totalXP > 0)
                percent = Mathf.RoundToInt(((float)currentXP / totalXP) * 100f);

            xpText.text = currentXP + " / " + totalXP + " (" + percent + "%)";
        }
    }
}