using System.Reflection;
using UnityEngine;

public class EnemyXPReward : MonoBehaviour
{
    [Header("Base XP Reward")]
    public int xpReward = 25;

    [Header("Enemy Health Link")]
    public MonoBehaviour enemyHealthComponent;

    private bool xpAlreadyGiven = false;

    void Start()
    {
        if (enemyHealthComponent == null)
            enemyHealthComponent = GetComponent<EnemyHealth>();

        xpAlreadyGiven = false;
    }

    void Update()
    {
        if (enemyHealthComponent == null)
            return;

        if (!xpAlreadyGiven && IsEnemyDead())
        {
            GiveXPToPlayer();
            xpAlreadyGiven = true;
        }

        if (xpAlreadyGiven && !IsEnemyDead())
            xpAlreadyGiven = false;
    }

    bool IsEnemyDead()
    {
        MethodInfo method = enemyHealthComponent.GetType().GetMethod("IsDead");

        if (method != null)
            return (bool)method.Invoke(enemyHealthComponent, null);

        return false;
    }

    void GiveXPToPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        PlayerXP playerXP = playerObject.GetComponent<PlayerXP>();
        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        EnemyHealth enemy = enemyHealthComponent as EnemyHealth;

        if (playerXP == null || playerStats == null || enemy == null)
            return;

        int playerLevel = playerStats.GetCurrentLevel();
        int enemyLevel = enemy.enemyLevel;

        int levelDiff = enemyLevel - playerLevel;

        int finalXP = xpReward;

        if (levelDiff <= -5)
        {
            finalXP = 10; // gray weak enemy
        }
        else if (levelDiff <= 0)
        {
            finalXP = xpReward; // green normal
        }
        else if (levelDiff == 1) finalXP = Mathf.RoundToInt(xpReward * 1.2f);
        else if (levelDiff == 2) finalXP = Mathf.RoundToInt(xpReward * 1.4f);
        else if (levelDiff == 3) finalXP = Mathf.RoundToInt(xpReward * 1.6f);
        else if (levelDiff == 4) finalXP = Mathf.RoundToInt(xpReward * 1.8f);
        else if (levelDiff >= 5) finalXP = xpReward * 2;

        playerXP.GainXP(finalXP);

        Debug.Log(gameObject.name + " gave " + finalXP + " XP.");
    }
}