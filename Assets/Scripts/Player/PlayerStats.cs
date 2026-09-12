using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    public int currentLevel = 1;
    public int maxLevel = 1000;

    [Header("XP")]
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public float xpGrowthMultiplier = 1.35f;

    [Header("Talent Points")]
    public int availableTalentPoints = 0;
    public int totalTalentPointsEarned = 0;
    public int talentPointLevelInterval = 5;
    public int talentPointsPerInterval = 5;

    [Header("Primary Stats")]
    public int strength = 5;
    public int agility = 0;
    public int stamina = 0;
    public int intelligence = 0;
    public int nova = 0;
    public int spirit = 0;
    public int armor = 5;

    [Header("Spirit Bonuses")]
    public float novaRegenPerSpirit = 0.2f;
    public float healthRegenPerSpirit = 0.1f;

    [Header("Combat Stats")]
    public float critRate = 0f;
    public float critDamage = 100f;
    public float critHeal = 0f;
    public float hitRate = 0f;
    public float dodge = 1f;
    public float block = 0f;
    public float novaRegen = 0f;
    public float healthRegen = 0f;
    public float hasteRating = 0f;
    public int attackPower = 0;
    public float attackSpeed = 0f;
    public int defense = 0;
    public float armorPenetration = 0f;
    public float threatMultiplier = 1f;
    public int expertise = 0;

    [Header("Stat Caps")]
    public int maxDefense = 10000;
    public float maxDodge = 25f;
    public float maxCritRate = 30f;
    public float maxAttackSpeed = 35f;
    public int maxNova = 20000;
    public float maxCritHeal = 35f;

    [Header("Level Up Rewards")]
    public int healthIncreasePerLevel = 50;
    public int manaIncreasePerLevel = 25;

    [Header("Optional Links")]
    public PlayerHealth playerHealthComponent;
    public PlayerMana playerManaComponent;

    public event Action OnStatsChanged;
    public event Action<int> OnLevelUp;

    void Start()
    {
        if (playerHealthComponent == null)
            playerHealthComponent = GetComponent<PlayerHealth>();

        if (playerManaComponent == null)
            playerManaComponent = GetComponent<PlayerMana>();

        ClampStats();
        NotifyChanged();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        if (currentLevel >= maxLevel)
            return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel && currentLevel < maxLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        NotifyChanged();
    }

    void LevelUp()
    {
        currentLevel++;

        GiveTalentPointsIfNeeded();

        if (playerHealthComponent != null)
        {
            playerHealthComponent.maxHealth += healthIncreasePerLevel;
            playerHealthComponent.currentHealth = playerHealthComponent.maxHealth;
        }

        if (playerManaComponent != null)
        {
            playerManaComponent.maxMana += manaIncreasePerLevel;
            playerManaComponent.currentMana = playerManaComponent.maxMana;
            playerManaComponent.RestoreFullMana();
        }

        gameObject.SendMessage("RestoreFullEnergy", SendMessageOptions.DontRequireReceiver);

        xpToNextLevel = Mathf.CeilToInt(xpToNextLevel * xpGrowthMultiplier);

        Debug.Log("LEVEL UP! New Level: " + currentLevel + " | HP/Nova/Energy restored.");

        OnLevelUp?.Invoke(currentLevel);
        NotifyChanged();
    }

    void GiveTalentPointsIfNeeded()
    {
        if (talentPointLevelInterval <= 0)
            return;

        if (currentLevel % talentPointLevelInterval != 0)
            return;

        availableTalentPoints += talentPointsPerInterval;
        totalTalentPointsEarned += talentPointsPerInterval;

        Debug.Log("Talent points gained: +" + talentPointsPerInterval +
                  " | Available Talent Points: " + availableTalentPoints);
    }

    public bool SpendTalentPoint(int amount)
    {
        if (amount <= 0)
            return false;

        if (availableTalentPoints < amount)
            return false;

        availableTalentPoints -= amount;
        NotifyChanged();
        return true;
    }

    public void RefundTalentPoints(int amount)
    {
        if (amount <= 0)
            return;

        availableTalentPoints += amount;
        NotifyChanged();
    }

    public int GetAvailableTalentPoints()
    {
        return availableTalentPoints;
    }

    public int GetTotalTalentPointsEarned()
    {
        return totalTalentPointsEarned;
    }

    public float GetXPPercent()
    {
        if (xpToNextLevel <= 0)
            return 0f;

        return (float)currentXP / xpToNextLevel;
    }

    public int GetCurrentXP()
    {
        return currentXP;
    }

    public int GetXPToNextLevel()
    {
        return xpToNextLevel;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetStrength()
    {
        return strength;
    }

    public int GetSpirit()
    {
        return spirit;
    }

    public float GetNovaRegen()
    {
        return Mathf.Max(0f, novaRegen + (spirit * novaRegenPerSpirit));
    }

    public float GetHealthRegen()
    {
        return Mathf.Max(0f, healthRegen + (spirit * healthRegenPerSpirit));
    }

    public float GetCritRate()
    {
        return Mathf.Clamp(critRate, 0f, maxCritRate);
    }

    public float GetCritDamage()
    {
        return Mathf.Max(0f, critDamage);
    }

    public float GetHitRate()
    {
        return Mathf.Max(0f, hitRate);
    }

    public float GetAttackSpeed()
    {
        return Mathf.Clamp(attackSpeed, 0f, maxAttackSpeed);
    }

    public float GetThreatMultiplier()
    {
        return Mathf.Max(0f, threatMultiplier);
    }

    public void ClampStats()
    {
        defense = Mathf.Clamp(defense, 0, maxDefense);
        dodge = Mathf.Clamp(dodge, 0f, maxDodge);
        critRate = Mathf.Clamp(critRate, 0f, maxCritRate);
        attackSpeed = Mathf.Clamp(attackSpeed, 0f, maxAttackSpeed);
        nova = Mathf.Clamp(nova, 0, maxNova);
        critHeal = Mathf.Clamp(critHeal, 0f, maxCritHeal);
        spirit = Mathf.Max(0, spirit);
        armor = Mathf.Max(0, armor);
    }

    void NotifyChanged()
    {
        ClampStats();
        OnStatsChanged?.Invoke();
    }
}