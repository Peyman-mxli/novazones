using UnityEngine;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon Damage")]
    public int weaponMinDamage = 10;
    public int weaponMaxDamage = 20;

    [Header("Damage Scaling")]
    public float strengthMultiplier = 1f;

    [Header("Auto Attack")]
    public float autoAttackCooldown = 2.1f;
    public float minimumAutoAttackCooldown = 1.1f;
    public float autoAttackRange = 3.5f;
    public string autoAttackTriggerName = "NormalAttack";

    [Header("Nova Strike")]
    public float novaStrikeRange = 3.5f;
    public float novaStrikeCooldown = 6f;
    public float novaStrikeDamageMultiplier = 1.75f;
    public string novaStrikeTriggerName = "NormalAttack";

    [Header("Animator Stop")]
    public string upperIdleStateName = "UpperIdle";
    public int upperBodyLayerIndex = 1;

    [Header("Basic Attack Talent")]
    public TalentData basicAttackMasteryTalent;
    public float hitRatePerRank = 0.3f;
    public float maxTalentHitRateBonus = 1.5f;

    [Header("Combat Rolls")]
    public float baseMissChance = 5f;
    public float blockDamageReduction = 0.5f;

    [Header("Floating Combat Text")]
    public GameObject floatingCombatTextPrefab;
    public Vector3 combatTextOffset = new Vector3(0f, 2f, 0f);

    [Header("References")]
    public PlayerTargeting playerTargeting;
    public Animator animator;
    public PlayerStats playerStats;

    private float lastAutoAttackTime = -999f;
    private float lastNovaStrikeTime = -999f;
    private bool autoAttacking = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        if (playerTargeting == null)
            playerTargeting = GetComponent<PlayerTargeting>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.IsDead())
        {
            StopAutoAttack();
            return;
        }

        HandleInput();
        HandleAutoAttackLoop();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            StopAutoAttack();
    }

    public void ToggleAutoAttack()
    {
        if (playerHealth != null && playerHealth.IsDead())
            return;

        EnemyTargetable target = GetCurrentTarget();

        if (target == null)
        {
            Debug.Log("No target selected.");
            return;
        }

        if (target.IsDead())
        {
            Debug.Log("Target is dead.");
            StopAutoAttack();
            return;
        }

        autoAttacking = !autoAttacking;

        if (autoAttacking)
            StartAutoAttackCooldownOnActionBar();
        else
            StopAttackAnimation();

        Debug.Log(autoAttacking ? "Auto Attack started." : "Auto Attack stopped.");
    }

    public void ToggleAutoAttackFromActionBar()
    {
        if (playerHealth != null && playerHealth.IsDead())
            return;

        EnemyTargetable target = GetCurrentTarget();

        if (target == null)
        {
            Debug.Log("No target selected.");
            return;
        }

        if (target.IsDead())
        {
            Debug.Log("Target is dead.");
            StopAutoAttack();
            return;
        }

        float distance = GetFlatDistance(transform.position, target.transform.position);

        if (distance > autoAttackRange)
        {
            Debug.Log("Target out of range.");
            return;
        }

        ToggleAutoAttack();
    }

    public bool CastNovaStrikeFromActionBar()
    {
        if (playerHealth != null && playerHealth.IsDead())
            return false;

        if (Time.time < lastNovaStrikeTime + novaStrikeCooldown)
        {
            Debug.Log("Nova Strike is not ready.");
            return false;
        }

        EnemyTargetable target = GetCurrentTarget();

        if (target == null)
        {
            Debug.Log("No target selected.");
            return false;
        }

        if (target.IsDead())
        {
            Debug.Log("Target is dead.");
            StopAutoAttack();
            return false;
        }

        float distance = GetFlatDistance(transform.position, target.transform.position);

        if (distance > novaStrikeRange)
        {
            Debug.Log("Target out of range.");
            return false;
        }

        FaceTarget(target);

        EnemyHealth enemyHealth = GetEnemyHealth(target);

        if (enemyHealth == null)
            return false;

        lastNovaStrikeTime = Time.time;

        if (animator != null && !string.IsNullOrEmpty(novaStrikeTriggerName))
            animator.SetTrigger(novaStrikeTriggerName);

        int finalDamage = Mathf.RoundToInt(CalculateFinalDamage() * novaStrikeDamageMultiplier);
        bool crit = RollCrit();
        bool blocked = enemyHealth.RollBlock();

        if (crit)
            finalDamage = Mathf.RoundToInt(finalDamage * (1f + (playerStats.GetCritDamage() / 100f)));

        finalDamage = enemyHealth.ApplyArmorReduction(finalDamage);

        if (blocked)
            finalDamage = Mathf.RoundToInt(finalDamage * (1f - blockDamageReduction));

        enemyHealth.TakeDamage(finalDamage);

        if (target.IsDead())
            StopAutoAttack();

        if (crit)
            ShowCritCombatText(enemyHealth, finalDamage.ToString());
        else if (blocked)
            ShowStatusCombatText(enemyHealth, "BLOCK " + finalDamage, HexToColor("#8E24AA"));
        else
            ShowNormalCombatText(enemyHealth, finalDamage.ToString());

        StartNamedSkillCooldownOnActionBar("Nova Strike", novaStrikeCooldown);

        Debug.Log("Nova Strike hit for " + finalDamage);

        return true;
    }

    void StopAutoAttack()
    {
        autoAttacking = false;
        StopAttackAnimation();
    }

    public void StopAutoAttackExternal()
    {
        StopAutoAttack();
    }

    void StopAttackAnimation()
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(autoAttackTriggerName))
            animator.ResetTrigger(autoAttackTriggerName);

        if (upperBodyLayerIndex >= 0 && upperBodyLayerIndex < animator.layerCount)
            animator.Play(upperIdleStateName, upperBodyLayerIndex, 0f);
    }

    void HandleAutoAttackLoop()
    {
        if (!autoAttacking)
            return;

        EnemyTargetable target = GetCurrentTarget();

        if (target == null || target.IsDead())
        {
            StopAutoAttack();
            return;
        }

        float distance = GetFlatDistance(transform.position, target.transform.position);

        if (distance > autoAttackRange)
            return;

        FaceTarget(target);

        if (Time.time >= lastAutoAttackTime + GetCurrentAutoAttackCooldown())
        {
            lastAutoAttackTime = Time.time;
            DoAutoAttack(target);
            StartAutoAttackCooldownOnActionBar();
        }
    }

    void StartAutoAttackCooldownOnActionBar()
    {
        StartNamedSkillCooldownOnActionBar("Auto Attack", GetCurrentAutoAttackCooldown());
    }

    void StartNamedSkillCooldownOnActionBar(string skillName, float cooldown)
    {
        ActionBarSlotUI[] slots = FindObjectsByType<ActionBarSlotUI>(FindObjectsSortMode.None);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            if (slots[i].assignedSkillName == skillName)
                slots[i].StartCooldown(cooldown);
        }
    }

    EnemyTargetable GetCurrentTarget()
    {
        if (playerTargeting == null)
            return null;

        if (playerTargeting.targetFrameUI == null)
            return null;

        return playerTargeting.targetFrameUI.currentTarget;
    }

    EnemyHealth GetEnemyHealth(EnemyTargetable target)
    {
        if (target == null)
            return null;

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
            enemyHealth = target.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            enemyHealth = target.GetComponentInChildren<EnemyHealth>();

        return enemyHealth;
    }

    void FaceTarget(EnemyTargetable target)
    {
        Vector3 lookDirection = target.transform.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    void DoAutoAttack(EnemyTargetable target)
    {
        EnemyHealth enemyHealth = GetEnemyHealth(target);

        if (enemyHealth == null || target.IsDead())
        {
            StopAutoAttack();
            return;
        }

        if (animator != null && !string.IsNullOrEmpty(autoAttackTriggerName))
            animator.SetTrigger(autoAttackTriggerName);

        if (RollMiss())
        {
            ShowStatusCombatText(enemyHealth, "MISS", Color.gray);
            Debug.Log("MISS! Basic Attack missed.");
            return;
        }

        if (enemyHealth.RollDodge(playerStats.GetHitRate() + GetBasicAttackHitRateBonus(), playerStats.expertise))
        {
            ShowStatusCombatText(enemyHealth, "DODGE", HexToColor("#00C853"));
            Debug.Log("DODGE! Enemy dodged the attack.");
            return;
        }

        int finalDamage = CalculateFinalDamage();
        bool crit = RollCrit();
        bool blocked = enemyHealth.RollBlock();

        if (crit)
            finalDamage = Mathf.RoundToInt(finalDamage * (1f + (playerStats.GetCritDamage() / 100f)));

        finalDamage = enemyHealth.ApplyArmorReduction(finalDamage);

        if (blocked)
            finalDamage = Mathf.RoundToInt(finalDamage * (1f - blockDamageReduction));

        enemyHealth.TakeDamage(finalDamage);

        if (target.IsDead())
            StopAutoAttack();

        Debug.Log("Basic Attack hit for " + finalDamage);

        if (crit)
            ShowCritCombatText(enemyHealth, finalDamage.ToString());
        else if (blocked)
            ShowStatusCombatText(enemyHealth, "BLOCK " + finalDamage, HexToColor("#8E24AA"));
        else
            ShowNormalCombatText(enemyHealth, finalDamage.ToString());
    }

    void ShowNormalCombatText(EnemyHealth enemyHealth, string message)
    {
        FloatingCombatText combatText = CreateCombatText(enemyHealth);

        if (combatText != null)
            combatText.ShowNormalHit(message);
    }

    void ShowCritCombatText(EnemyHealth enemyHealth, string message)
    {
        FloatingCombatText combatText = CreateCombatText(enemyHealth);

        if (combatText != null)
            combatText.ShowCritHit(message);
    }

    void ShowStatusCombatText(EnemyHealth enemyHealth, string message, Color color)
    {
        FloatingCombatText combatText = CreateCombatText(enemyHealth);

        if (combatText != null)
            combatText.ShowStatusText(message, color);
    }

    FloatingCombatText CreateCombatText(EnemyHealth enemyHealth)
    {
        if (floatingCombatTextPrefab == null)
            return null;

        Vector3 spawnPosition =
            enemyHealth != null
            ? enemyHealth.GetCombatTextPosition()
            : transform.position + combatTextOffset;

        GameObject obj = Instantiate(floatingCombatTextPrefab, spawnPosition, Quaternion.identity);

        return obj.GetComponent<FloatingCombatText>();
    }

    bool RollMiss()
    {
        float finalMissChance = baseMissChance;

        if (playerStats != null)
            finalMissChance -= playerStats.GetHitRate();

        finalMissChance -= GetBasicAttackHitRateBonus();
        finalMissChance = Mathf.Max(0f, finalMissChance);

        return Random.Range(0f, 100f) <= finalMissChance;
    }

    int CalculateFinalDamage()
    {
        int weaponDamage = Random.Range(weaponMinDamage, weaponMaxDamage + 1);
        int strengthDamage = 0;

        if (playerStats != null)
            strengthDamage = Mathf.RoundToInt(playerStats.GetStrength() * strengthMultiplier);

        return Mathf.Max(1, weaponDamage + strengthDamage);
    }

    bool RollCrit()
    {
        if (playerStats == null)
            return false;

        return Random.Range(0f, 100f) <= playerStats.GetCritRate();
    }

    public float GetCurrentAutoAttackCooldown()
    {
        int rank = GetBasicAttackMasteryRank();
        float cooldown = autoAttackCooldown;

        if (rank > 0)
        {
            float cooldownReductionPerRank = (autoAttackCooldown - minimumAutoAttackCooldown) / 5f;
            cooldown -= cooldownReductionPerRank * rank;
        }

        if (playerStats != null)
            cooldown -= playerStats.GetAttackSpeed() * 0.01f;

        return Mathf.Max(minimumAutoAttackCooldown, cooldown);
    }

    public float GetBasicAttackHitRateBonus()
    {
        int rank = GetBasicAttackMasteryRank();
        float bonus = rank * hitRatePerRank;

        return Mathf.Min(bonus, maxTalentHitRateBonus);
    }

    private int GetBasicAttackMasteryRank()
    {
        if (basicAttackMasteryTalent == null)
            return 0;

        return basicAttackMasteryTalent.currentRank;
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}