using UnityEngine;

public enum TalentType
{
    Passive,
    SkillUpgrade,
    UnlockSkill
}

[CreateAssetMenu(menuName = "NovaZone/Talent")]
public class TalentData : ScriptableObject
{
    [Header("Basic Info")]
    public string talentName;
    [TextArea] public string description;

    [Header("Ranks")]
    public int currentRank = 0;
    public int maxRank = 10;

    [Header("Requirements")]
    public int requiredPointsInTree = 0;
    public TalentData requiredTalent; // parent

    [Header("Type")]
    public TalentType talentType;

    [Header("Skill Link (optional)")]
    public string linkedSkillName;

    public bool CanUpgrade(int availablePoints, int pointsSpentInTree)
    {
        if (currentRank >= maxRank)
            return false;

        if (availablePoints <= 0)
            return false;

        if (pointsSpentInTree < requiredPointsInTree)
            return false;

        if (requiredTalent != null && requiredTalent.currentRank < requiredTalent.maxRank)
            return false;

        return true;
    }

    public void Upgrade()
    {
        if (currentRank < maxRank)
        {
            currentRank++;
            Debug.Log("Talent upgraded: " + talentName + " (" + currentRank + "/" + maxRank + ")");
        }
    }

    public void ResetTalent()
    {
        currentRank = 0;
    }
}