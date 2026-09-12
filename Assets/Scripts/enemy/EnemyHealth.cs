using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Info")]
    public string enemyName = "Nova Enemy";
    public int enemyLevel = 1;
    public Sprite enemyPortrait;

    [Header("Combat Text Settings")]
    public Transform combatTextPoint;
    public Vector3 combatTextFallbackOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Combat Stats")]
    public float dodgeChance = 3f;
    public float blockChance = 0f;
    public int defense = 0;
    public int armor = 0;

    [Header("Health Settings")]
    public int maxHealth = 50;
    public int currentHealth;

    [Header("Mana / Nova Settings")]
    public bool usesMana = true;
    public int maxMana = 50;
    public int currentMana = 50;

    [Header("Energy Settings")]
    public bool usesEnergy = false;
    public int maxEnergy = 100;
    public int currentEnergy = 100;

    [Header("Rage Settings")]
    public bool usesRage = false;
    public int maxRage = 100;
    public int currentRage = 0;

    [Header("Optional Death Effect")]
    public GameObject deathEffect;

    [Header("Death Settings")]
    public float destroyDelay = 20f;
    public bool keepColliderForLoot = true;

    private bool isDead = false;
    private Collider[] enemyColliders;
    private Rigidbody enemyRigidbody;
    private Animator enemyAnimator;

    void Awake()
    {
        enemyColliders = GetComponentsInChildren<Collider>(true);
        enemyRigidbody = GetComponent<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        ResetEnemyState();
    }

    void Start()
    {
        ResetEnemyState();
    }

    public Vector3 GetCombatTextPosition()
    {
        if (combatTextPoint != null)
            return combatTextPoint.position;

        return transform.position + combatTextFallbackOffset;
    }

    public Transform GetCombatTextPoint()
    {
        return combatTextPoint;
    }

    public void ResetEnemyState()
    {
        isDead = false;

        currentHealth = maxHealth;
        currentMana = maxMana;
        currentEnergy = maxEnergy;
        currentRage = 0;

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();

        if (enemyColliders == null || enemyColliders.Length == 0)
            enemyColliders = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < enemyColliders.Length; i++)
        {
            if (enemyColliders[i] != null)
                enemyColliders[i].enabled = true;
        }

        if (enemyRigidbody == null)
            enemyRigidbody = GetComponent<Rigidbody>();

        if (enemyRigidbody != null)
        {
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.useGravity = false;
            enemyRigidbody.linearVelocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public bool RollDodge(float attackerHitRate, int attackerExpertise)
    {
        float finalDodgeChance = dodgeChance;

        finalDodgeChance -= attackerHitRate;
        finalDodgeChance -= attackerExpertise * 0.1f;

        finalDodgeChance = Mathf.Max(0f, finalDodgeChance);

        return Random.Range(0f, 100f) <= finalDodgeChance;
    }

    public bool RollBlock()
    {
        return Random.Range(0f, 100f) <= blockChance;
    }

    public int ApplyArmorReduction(int damage)
    {
        float reduction = armor / (100f + armor);
        float reducedDamage = damage * (1f - reduction);

        return Mathf.Max(1, Mathf.RoundToInt(reducedDamage));
    }

    public void RestoreFullHealthAndMana()
    {
        if (isDead)
            return;

        currentHealth = maxHealth;
        currentMana = maxMana;
        currentEnergy = maxEnergy;
        currentRage = 0;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        if (usesRage)
            AddRage(10);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void UseMana(int amount)
    {
        if (!usesMana || isDead)
            return;

        currentMana = Mathf.Max(0, currentMana - amount);
    }

    public void RestoreMana(int amount)
    {
        if (!usesMana || isDead)
            return;

        currentMana = Mathf.Min(maxMana, currentMana + amount);
    }

    public void UseEnergy(int amount)
    {
        if (!usesEnergy || isDead)
            return;

        currentEnergy = Mathf.Max(0, currentEnergy - amount);
    }

    public void RestoreEnergy(int amount)
    {
        if (!usesEnergy || isDead)
            return;

        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
    }

    public void UseRage(int amount)
    {
        if (!usesRage || isDead)
            return;

        currentRage = Mathf.Max(0, currentRage - amount);
    }

    public void AddRage(int amount)
    {
        if (!usesRage || isDead)
            return;

        currentRage = Mathf.Min(maxRage, currentRage + amount);
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0)
            return 0f;

        return (float)currentHealth / maxHealth;
    }

    public float GetManaPercent()
    {
        if (maxMana <= 0)
            return 0f;

        return (float)currentMana / maxMana;
    }

    public float GetEnergyPercent()
    {
        if (maxEnergy <= 0)
            return 0f;

        return (float)currentEnergy / maxEnergy;
    }

    public float GetRagePercent()
    {
        if (maxRage <= 0)
            return 0f;

        return (float)currentRage / maxRage;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetFloat("Speed", 0f);
            enemyAnimator.SetTrigger("Dead");
        }

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        PrepareEnemyAfterDeath();
        Destroy(gameObject, destroyDelay);
    }

    void PrepareEnemyAfterDeath()
    {
        if (enemyColliders != null)
        {
            for (int i = 0; i < enemyColliders.Length; i++)
            {
                if (enemyColliders[i] != null)
                    enemyColliders[i].enabled = keepColliderForLoot;
            }
        }

        if (enemyRigidbody != null)
        {
            enemyRigidbody.linearVelocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.useGravity = false;
        }
    }
}