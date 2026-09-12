using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Nova Pahlavan";
    public Sprite playerPortrait;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Incoming Combat Text")]
    public bool showIncomingCombatText = true;
    public GameObject floatingCombatTextPrefab;
    public Transform playerCombatTextPoint;
    public Vector3 combatTextFallbackOffset = new Vector3(0f, 2.2f, 0f);
    public Color incomingDamageColor = new Color(1f, 0.2f, 0.2f);
    public float incomingDamageTextSize = 1.2f;
    public float incomingDamageStartScale = 1f;

    [Header("Healing Combat Text")]
    public bool showHealingCombatText = true;
    public Color healingTextColor = new Color(0.2f, 1f, 0.2f);
    public float healingTextSize = 1.4f;
    public float healingStartScale = 1f;

    [Header("Respawn Settings")]
    public float respawnCooldown = 10f;
    public float respawnHeightOffset = 1.2f;

    [Header("Ground Snap")]
    public LayerMask groundLayer;
    public float groundRaycastStartHeight = 25f;
    public float groundRaycastDistance = 100f;

    private bool isDead = false;
    private float respawnReadyTime = 0f;
    private Vector3 deathPosition;
    private Quaternion deathRotation;

    private Rigidbody rb;
    private PlayerMana playerMana;
    private PlayerCombat playerCombat;
    private PlayerTargeting playerTargeting;

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        playerMana = GetComponent<PlayerMana>();
        playerCombat = GetComponent<PlayerCombat>();
        playerTargeting = GetComponent<PlayerTargeting>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        ShowIncomingDamageText(damage);

        Debug.Log(playerName + " took " + damage + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void ShowIncomingDamageText(int damage)
    {
        if (!showIncomingCombatText)
            return;

        ShowHealthCombatText("-" + damage.ToString(), incomingDamageColor, incomingDamageTextSize, incomingDamageStartScale);
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;

        int healthBeforeHeal = currentHealth;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        int actualHealAmount = currentHealth - healthBeforeHeal;

        if (actualHealAmount > 0)
            ShowHealingText(actualHealAmount);
    }

    void ShowHealingText(int amount)
    {
        if (!showHealingCombatText)
            return;

        ShowHealthCombatText("+" + amount.ToString(), healingTextColor, healingTextSize, healingStartScale);
    }

    void ShowHealthCombatText(string message, Color color, float textSize, float startScale)
    {
        if (floatingCombatTextPrefab == null)
            return;

        Vector3 spawnPosition = GetPlayerCombatTextPosition();

        GameObject obj = Instantiate(
            floatingCombatTextPrefab,
            spawnPosition,
            Quaternion.identity
        );

        FloatingCombatText combatText = obj.GetComponent<FloatingCombatText>();

        if (combatText != null)
            combatText.ShowText(message, color, textSize, startScale);
    }

    Vector3 GetPlayerCombatTextPosition()
    {
        if (playerCombatTextPoint != null)
            return playerCombatTextPoint.position;

        return transform.position + combatTextFallbackOffset;
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0)
            return 0f;

        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool CanRespawn()
    {
        if (!isDead)
            return false;

        return Time.time >= respawnReadyTime;
    }

    public float GetRespawnCooldownRemaining()
    {
        if (!isDead)
            return 0f;

        float remaining = respawnReadyTime - Time.time;

        if (remaining < 0f)
            remaining = 0f;

        return remaining;
    }

    public Vector3 GetDeathPosition()
    {
        return deathPosition;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0;

        deathPosition = transform.position;
        deathRotation = transform.rotation;
        respawnReadyTime = Time.time + respawnCooldown;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerCombat != null)
            playerCombat.StopAutoAttackExternal();

        if (playerTargeting != null)
            playerTargeting.ClearTarget();

        Debug.Log(playerName + " died. Respawn available in " + respawnCooldown + " seconds.");
    }

    public void RespawnHere()
    {
        if (!CanRespawn())
        {
            Debug.Log("Respawn Here is still on cooldown.");
            return;
        }

        Vector3 soulCurrentPosition = transform.position;
        Quaternion soulCurrentRotation = transform.rotation;

        Vector3 respawnPosition = GetGroundedRespawnPosition(soulCurrentPosition);

        RespawnAtPosition(respawnPosition, soulCurrentRotation);
    }

    public void RespawnAtNearestAngel()
    {
        if (!CanRespawn())
        {
            Debug.Log("Respawn at Nearby NovaAngel is still on cooldown.");
            return;
        }

        NovaAngelRespawn nearestAngel = FindNearestAngel();

        if (nearestAngel == null)
        {
            Debug.LogWarning("No NovaAngelRespawn found in the scene. Respawning at current soul position instead.");

            Vector3 fallbackPosition = GetGroundedRespawnPosition(transform.position);
            RespawnAtPosition(fallbackPosition, transform.rotation);
            return;
        }

        Transform spawnPoint = nearestAngel.GetSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("Nearest NovaAngelRespawn has no spawn point assigned. Respawning at current soul position instead.");

            Vector3 fallbackPosition = GetGroundedRespawnPosition(transform.position);
            RespawnAtPosition(fallbackPosition, transform.rotation);
            return;
        }

        Vector3 angelPosition = GetGroundedRespawnPosition(spawnPoint.position);
        Quaternion angelRotation = spawnPoint.rotation;

        RespawnAtPosition(angelPosition, angelRotation);
    }

    Vector3 GetGroundedRespawnPosition(Vector3 targetPosition)
    {
        Vector3 rayStart = targetPosition + Vector3.up * groundRaycastStartHeight;
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundRaycastDistance, groundLayer))
            return hit.point + Vector3.up * respawnHeightOffset;

        return targetPosition + Vector3.up * respawnHeightOffset;
    }

    void RespawnAtPosition(Vector3 position, Quaternion rotation)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = position;
        transform.rotation = rotation;

        currentHealth = maxHealth;

        if (playerMana != null)
            playerMana.RestoreFullMana();

        isDead = false;

        Debug.Log(playerName + " respawned with full HP and full Mana. Current Health: " + currentHealth);
    }

    NovaAngelRespawn FindNearestAngel()
    {
        NovaAngelRespawn[] angels = FindObjectsByType<NovaAngelRespawn>(FindObjectsSortMode.None);

        if (angels == null || angels.Length == 0)
            return null;

        NovaAngelRespawn nearest = null;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < angels.Length; i++)
        {
            if (angels[i] == null)
                continue;

            Transform angelSpawn = angels[i].GetSpawnPoint();

            Vector3 angelPosition = angels[i].transform.position;

            if (angelSpawn != null)
                angelPosition = angelSpawn.position;

            float distance = Vector3.Distance(transform.position, angelPosition);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = angels[i];
            }
        }

        return nearest;
    }
}