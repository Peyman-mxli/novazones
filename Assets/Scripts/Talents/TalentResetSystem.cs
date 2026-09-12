using UnityEngine;
using TMPro;

public class TalentResetSystem : MonoBehaviour
{
    [Header("Links")]
    public PlayerStats playerStats;
    public TalentTreeSystem talentTreeSystem;

    [Header("UI")]
    public TMP_Text freeResetLeftText;
    public TMP_Text resetInfoNoteText;

    [Header("Reset Rules")]
    public int freeResetLimit = 20;
    public int resetCostGold = 250;
    public int resetTimesUsed = 0;

    private void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (talentTreeSystem == null)
            talentTreeSystem = GetComponent<TalentTreeSystem>();

        RefreshResetUI();
    }

    public void ResetTalentsButton()
    {
        TryResetTalents();
    }

    public bool CanResetForFree()
    {
        return resetTimesUsed < freeResetLimit;
    }

    public int GetFreeResetsLeft()
    {
        int left = freeResetLimit - resetTimesUsed;

        if (left < 0)
            left = 0;

        return left;
    }

    public int GetCurrentResetCost()
    {
        if (CanResetForFree())
            return 0;

        return resetCostGold;
    }

    public bool TryResetTalents()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("TalentResetSystem: PlayerStats is missing.");
            return false;
        }

        if (talentTreeSystem == null)
        {
            Debug.LogWarning("TalentResetSystem: TalentTreeSystem is missing.");
            return false;
        }

        talentTreeSystem.RecalculatePointsSpent();

        if (talentTreeSystem.pointsSpentInTree <= 0)
        {
            Debug.Log("Talent reset ignored. No talent points were spent.");
            RefreshResetUI();
            return false;
        }

        int cost = GetCurrentResetCost();

        if (cost > 0)
        {
            if (BackpackInventorySystem.Instance == null)
            {
                Debug.LogWarning("TalentResetSystem: BackpackInventorySystem is missing.");
                return false;
            }

            if (!BackpackInventorySystem.Instance.SpendGold(cost))
            {
                Debug.LogWarning("Talent reset failed. Not enough gold.");
                return false;
            }
        }

        talentTreeSystem.ResetTree();

        resetTimesUsed++;

        RefreshResetUI();

        Debug.Log("Talents reset. Reset used: " + resetTimesUsed +
                  " | Free left: " + GetFreeResetsLeft() +
                  " | Cost: " + cost +
                  " | Available Talent Points: " + playerStats.availableTalentPoints);

        return true;
    }

    public void RefreshResetUI()
    {
        if (freeResetLeftText != null)
            freeResetLeftText.text = GetFreeResetsLeft().ToString("00");

        if (resetInfoNoteText != null)
        {
            resetInfoNoteText.text =
                "You have 20 free talent resets.\n" +
                "After all free resets are used,\n" +
                "each reset will cost 250 Gold.";
        }
    }

    public string GetResetNoticeText()
    {
        return "First " + freeResetLimit +
               " talent resets are free. After that, each reset costs " +
               resetCostGold + " gold.";
    }
}