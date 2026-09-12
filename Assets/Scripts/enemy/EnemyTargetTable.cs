using UnityEngine;

public class EnemyTargetable : MonoBehaviour
{
    [Header("Target Info")]
    public string targetName = "Nova Wolfi";
    public Sprite targetPortrait;

    [Header("References")]
    public EnemyHealth enemyHealth;

    [Header("Click Target Fix")]
    public bool createBigClickBox = true;
    public Vector3 clickBoxCenter = new Vector3(0f, 1f, 0f);
    public Vector3 clickBoxSize = new Vector3(2.2f, 2f, 2.2f);

    private BoxCollider clickCollider;

    void Awake()
    {
        FindEnemyHealth();
        SetupClickCollider();
    }

    void Reset()
    {
        FindEnemyHealth();
        SetupClickCollider();
    }

    void FindEnemyHealth()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInChildren<EnemyHealth>();
    }

    void SetupClickCollider()
    {
        if (!createBigClickBox)
            return;

        clickCollider = GetComponent<BoxCollider>();

        if (clickCollider == null)
            clickCollider = gameObject.AddComponent<BoxCollider>();

        clickCollider.isTrigger = true;
        clickCollider.center = clickBoxCenter;
        clickCollider.size = clickBoxSize;
    }

    public string GetTargetName()
    {
        if (enemyHealth != null && !string.IsNullOrEmpty(enemyHealth.enemyName))
            return enemyHealth.enemyName;

        if (!string.IsNullOrEmpty(targetName))
            return targetName;

        return gameObject.name;
    }

    public Sprite GetTargetPortrait()
    {
        if (targetPortrait != null)
            return targetPortrait;

        if (enemyHealth != null && enemyHealth.enemyPortrait != null)
            return enemyHealth.enemyPortrait;

        return null;
    }

    public bool IsDead()
    {
        if (enemyHealth == null)
            return true;

        return enemyHealth.IsDead();
    }

    public int GetCurrentHealth()
    {
        if (enemyHealth == null)
            return 0;

        return enemyHealth.currentHealth;
    }

    public int GetMaxHealth()
    {
        if (enemyHealth == null)
            return 0;

        return enemyHealth.maxHealth;
    }

    public float GetHealthPercent()
    {
        if (enemyHealth == null)
            return 0f;

        return enemyHealth.GetHealthPercent();
    }

    public bool UsesMana()
    {
        if (enemyHealth == null)
            return false;

        return enemyHealth.usesMana;
    }

    public int GetCurrentMana()
    {
        if (enemyHealth == null || !enemyHealth.usesMana)
            return 0;

        return enemyHealth.currentMana;
    }

    public int GetMaxMana()
    {
        if (enemyHealth == null || !enemyHealth.usesMana)
            return 0;

        return enemyHealth.maxMana;
    }

    public float GetManaPercent()
    {
        if (enemyHealth == null || !enemyHealth.usesMana)
            return 0f;

        return enemyHealth.GetManaPercent();
    }

    public bool UsesEnergy()
    {
        if (enemyHealth == null)
            return false;

        return enemyHealth.usesEnergy;
    }

    public int GetCurrentEnergy()
    {
        if (enemyHealth == null || !enemyHealth.usesEnergy)
            return 0;

        return enemyHealth.currentEnergy;
    }

    public int GetMaxEnergy()
    {
        if (enemyHealth == null || !enemyHealth.usesEnergy)
            return 0;

        return enemyHealth.maxEnergy;
    }

    public float GetEnergyPercent()
    {
        if (enemyHealth == null || !enemyHealth.usesEnergy)
            return 0f;

        return enemyHealth.GetEnergyPercent();
    }

    public bool UsesRage()
    {
        if (enemyHealth == null)
            return false;

        return enemyHealth.usesRage;
    }

    public int GetCurrentRage()
    {
        if (enemyHealth == null || !enemyHealth.usesRage)
            return 0;

        return enemyHealth.currentRage;
    }

    public int GetMaxRage()
    {
        if (enemyHealth == null || !enemyHealth.usesRage)
            return 0;

        return enemyHealth.maxRage;
    }

    public float GetRagePercent()
    {
        if (enemyHealth == null || !enemyHealth.usesRage)
            return 0f;

        return enemyHealth.GetRagePercent();
    }
}