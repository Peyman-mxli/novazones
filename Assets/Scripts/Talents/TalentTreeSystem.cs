using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalentTreeSystem : MonoBehaviour
{
    [Header("Links")]
    public PlayerStats playerStats;

    [Header("UI")]
    public TMP_Text totalTalentPointsText;
    public TMP_Text availableTalentPointsText;

    [Header("Tree Info")]
    public string treeName = "Nova Pahlavan Tank Tree";

    [Header("Talents")]
    public TalentData[] talents;

    [Header("Talent Slot Buttons")]
    public Button[] talentButtons;

    [Header("Talent Rank Texts")]
    public TMP_Text[] talentRankTexts;

    [Header("Talent Info Texts")]
    public TMP_Text[] talentInfoTexts;

    [Header("Talent Icons")]
    public GameObject[] talentIcons;

    [Header("Talent Name Texts")]
    public TMP_Text[] talentNameTexts;

    [Header("Locked Colors")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.gray;

    [Header("Runtime")]
    public int pointsSpentInTree = 0;
    public int selectedTalentIndex = -1;

    private void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        ConnectTalentButtons();

        RecalculatePointsSpent();
        RefreshAllRankTexts();
        RefreshTalentPointsUI();
        RefreshTalentLocks();

        HideAllRankTexts();
        HideAllTalentInfo();

        if (talents != null && talents.Length > 0 && talents[0] != null)
            SelectTalent(0);
    }

    private void Update()
    {
        RefreshTalentPointsUI();
    }

    private void RefreshTalentPointsUI()
    {
        if (playerStats == null)
            return;

        if (totalTalentPointsText != null)
            totalTalentPointsText.text = playerStats.totalTalentPointsEarned.ToString();

        if (availableTalentPointsText != null)
            availableTalentPointsText.text = playerStats.availableTalentPoints.ToString();
    }

    private void ConnectTalentButtons()
    {
        if (talentButtons == null)
            return;

        for (int i = 0; i < talentButtons.Length; i++)
        {
            int index = i;

            if (talentButtons[i] != null)
            {
                talentButtons[i].onClick.RemoveAllListeners();
                talentButtons[i].onClick.AddListener(() => SelectTalent(index));
            }
        }
    }

    public void SelectTalent(int talentIndex)
    {
        if (talents == null)
            return;

        if (talentIndex < 0 || talentIndex >= talents.Length)
            return;

        TalentData talent = talents[talentIndex];

        if (talent == null)
            return;

        RecalculatePointsSpent();

        if (pointsSpentInTree < talent.requiredPointsInTree)
        {
            Debug.Log("Talent Locked. Requires " + talent.requiredPointsInTree + " points in tree.");
            return;
        }

        selectedTalentIndex = talentIndex;

        RefreshAllRankTexts();
        ShowOnlyRankText(talentIndex);
        ShowOnlyTalentInfo(talentIndex);

        Debug.Log("Selected Talent Slot: " + talentIndex);
    }

    public void AddPointToSelectedTalent()
    {
        if (selectedTalentIndex < 0)
        {
            Debug.LogWarning("No talent selected.");
            return;
        }

        TryUpgradeTalent(selectedTalentIndex);
    }

    public bool TryUpgradeTalent(int talentIndex)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats missing.");
            return false;
        }

        if (talents == null)
        {
            Debug.LogWarning("Talents array missing.");
            return false;
        }

        if (talentIndex < 0 || talentIndex >= talents.Length)
        {
            Debug.LogWarning("Invalid talent index.");
            return false;
        }

        TalentData talent = talents[talentIndex];

        if (talent == null)
        {
            Debug.LogWarning("Talent is NULL.");
            return false;
        }

        RecalculatePointsSpent();

        if (!talent.CanUpgrade(playerStats.availableTalentPoints, pointsSpentInTree))
        {
            Debug.LogWarning("Cannot upgrade talent. Required Points: " + talent.requiredPointsInTree);
            return false;
        }

        if (!playerStats.SpendTalentPoint(1))
        {
            Debug.LogWarning("Not enough points.");
            return false;
        }

        talent.Upgrade();

        RecalculatePointsSpent();
        ApplyTalentEffect(talent);

        RefreshAllRankTexts();
        RefreshTalentLocks();
        ShowOnlyRankText(talentIndex);
        ShowOnlyTalentInfo(talentIndex);
        RefreshTalentPointsUI();

        Debug.Log("Talent Upgraded: " + talent.talentName + " | Rank: " + talent.currentRank + "/" + talent.maxRank);

        return true;
    }

    public void ResetTree()
    {
        if (talents == null)
            return;

        RecalculatePointsSpent();

        int refundAmount = pointsSpentInTree;

        for (int i = 0; i < talents.Length; i++)
        {
            if (talents[i] != null)
                talents[i].ResetTalent();
        }

        pointsSpentInTree = 0;

        if (playerStats != null)
            playerStats.RefundTalentPoints(refundAmount);

        RefreshAllRankTexts();
        RefreshTalentLocks();
        RefreshTalentPointsUI();

        selectedTalentIndex = -1;

        HideAllRankTexts();
        HideAllTalentInfo();

        if (talents.Length > 0 && talents[0] != null)
            SelectTalent(0);

        Debug.Log("Talent Tree Reset.");
    }

    private void RefreshTalentLocks()
    {
        if (talents == null || talentButtons == null)
            return;

        RecalculatePointsSpent();

        for (int i = 0; i < talentButtons.Length; i++)
        {
            if (i >= talents.Length)
                continue;

            if (talentButtons[i] == null || talents[i] == null)
                continue;

            bool unlocked = pointsSpentInTree >= talents[i].requiredPointsInTree;

            Image buttonImage = talentButtons[i].GetComponent<Image>();

            if (buttonImage != null)
                buttonImage.color = unlocked ? unlockedColor : lockedColor;

            talentButtons[i].interactable = unlocked;
        }
    }

    private void HideAllRankTexts()
    {
        if (talentRankTexts == null)
            return;

        for (int i = 0; i < talentRankTexts.Length; i++)
        {
            if (talentRankTexts[i] != null)
                talentRankTexts[i].gameObject.SetActive(false);
        }
    }

    private void ShowOnlyRankText(int talentIndex)
    {
        if (talentRankTexts == null)
            return;

        for (int i = 0; i < talentRankTexts.Length; i++)
        {
            if (talentRankTexts[i] != null)
                talentRankTexts[i].gameObject.SetActive(i == talentIndex);
        }
    }

    private void HideAllTalentInfo()
    {
        if (talentInfoTexts != null)
        {
            for (int i = 0; i < talentInfoTexts.Length; i++)
            {
                if (talentInfoTexts[i] != null)
                    talentInfoTexts[i].gameObject.SetActive(false);
            }
        }

        if (talentIcons != null)
        {
            for (int i = 0; i < talentIcons.Length; i++)
            {
                if (talentIcons[i] != null)
                    talentIcons[i].SetActive(false);
            }
        }

        if (talentNameTexts != null)
        {
            for (int i = 0; i < talentNameTexts.Length; i++)
            {
                if (talentNameTexts[i] != null)
                    talentNameTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private void ShowOnlyTalentInfo(int talentIndex)
    {
        HideAllTalentInfo();

        if (talentInfoTexts != null &&
            talentIndex >= 0 &&
            talentIndex < talentInfoTexts.Length &&
            talentInfoTexts[talentIndex] != null)
        {
            talentInfoTexts[talentIndex].gameObject.SetActive(true);
        }

        if (talentIcons != null &&
            talentIndex >= 0 &&
            talentIndex < talentIcons.Length &&
            talentIcons[talentIndex] != null)
        {
            talentIcons[talentIndex].SetActive(true);
        }

        if (talentNameTexts != null &&
            talentIndex >= 0 &&
            talentIndex < talentNameTexts.Length &&
            talentNameTexts[talentIndex] != null)
        {
            talentNameTexts[talentIndex].gameObject.SetActive(true);
        }
    }

    private void RefreshAllRankTexts()
    {
        if (talentRankTexts == null || talents == null)
            return;

        for (int i = 0; i < talentRankTexts.Length; i++)
        {
            if (talentRankTexts[i] == null)
                continue;

            if (i < talents.Length && talents[i] != null)
                talentRankTexts[i].text = talents[i].currentRank + "/" + talents[i].maxRank;
            else
                talentRankTexts[i].text = "0/0";
        }
    }

    public void RecalculatePointsSpent()
    {
        pointsSpentInTree = 0;

        if (talents == null)
            return;

        for (int i = 0; i < talents.Length; i++)
        {
            if (talents[i] != null)
                pointsSpentInTree += talents[i].currentRank;
        }
    }

    private void ApplyTalentEffect(TalentData talent)
    {
        if (talent == null)
            return;

        Debug.Log("Applied Talent Effect: " + talent.talentName);
    }
}