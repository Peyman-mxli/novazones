using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 6f;
    public float chaseRange = 20f;
    public float maxChaseDistanceFromSpawn = 15f;
    public float attackRange = 2.2f;
    public float stopDistance = 1.8f;
    public float returnStopDistance = 0.1f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.5f;

    [Header("Combat Rolls")]
    public float missChance = 5f;

    [Header("Combat Text")]
    public GameObject floatingCombatTextPrefab;
    public bool showPlayerMissText = true;
    public bool showPlayerDodgeText = false;
    public bool showPlayerBlockText = false;

    [Header("Status Text Colors")]
    public Color missTextColor = Color.gray;
    public Color dodgeTextColor = new Color(0.3f, 0.8f, 1f);
    public Color blockTextColor = new Color(0.55f, 0.15f, 0.8f);

    [Header("Status Text Size")]
    public float playerStatusTextSize = 2.2f;
    public float playerStatusStartScale = 1f;

    private float nextAttackTime = 0f;
    private EnemyHealth enemyHealth;
    private EnemyTargetable enemyTargetable;
    private PlayerHealth playerHealth;
    private PlayerStats playerStats;
    private Rigidbody enemyRigidbody;
    private Animator enemyAnimator;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool restoredAtSpawn = false;
    private bool targetClearedAtReset = false;
    private bool isResetting = false;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyTargetable = GetComponent<EnemyTargetable>();
        enemyRigidbody = GetComponent<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        SetupRigidbody();
    }

    void OnEnable()
    {
        ResetAIState();
    }

    void Start()
    {
        ResetAIState();
    }

    void SetupRigidbody()
    {
        if (enemyRigidbody == null)
            return;

        enemyRigidbody.isKinematic = true;
        enemyRigidbody.useGravity = false;
        enemyRigidbody.linearVelocity = Vector3.zero;
        enemyRigidbody.angularVelocity = Vector3.zero;
    }

    public void ResetAIState()
    {
        nextAttackTime = 0f;
        restoredAtSpawn = false;
        targetClearedAtReset = false;
        isResetting = false;

        SetupRigidbody();
        SetAnimationSpeed(0f);

        player = null;
        playerHealth = null;
        playerStats = null;

        FindPlayerReferences();
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead())
        {
            SetAnimationSpeed(0f);
            return;
        }

        if (player == null || playerHealth == null)
            FindPlayerReferences();

        if (player == null || playerHealth == null || playerHealth.IsDead())
        {
            BeginReset();
            ReturnToSpawn(true);
            return;
        }

        float distanceToPlayer = GetFlatDistance(transform.position, player.position);
        float distanceFromSpawn = GetFlatDistance(transform.position, spawnPosition);

        if (distanceFromSpawn >= maxChaseDistanceFromSpawn)
        {
            BeginReset();
            ReturnToSpawn(true);
            return;
        }

        if (distanceToPlayer <= chaseRange)
        {
            isResetting = false;
            restoredAtSpawn = false;
            targetClearedAtReset = false;

            SmoothFacePosition(player.position);

            if (distanceToPlayer > stopDistance)
                ChasePlayer();
            else
                StopEnemyMovement();

            if (distanceToPlayer <= attackRange)
                AttackPlayer();
        }
        else
        {
            BeginReset();
            ReturnToSpawn(true);
        }
    }

    void FindPlayerReferences()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer == null)
            return;

        player = foundPlayer.transform;
        playerHealth = foundPlayer.GetComponent<PlayerHealth>();
        playerStats = foundPlayer.GetComponent<PlayerStats>();
    }

    void ChasePlayer()
    {
        Vector3 targetPosition = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        SetAnimationSpeed(moveSpeed);
    }

    void AttackPlayer()
    {
        if (isResetting || playerHealth == null || playerHealth.IsDead())
            return;

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");

        if (RollMiss())
        {
            if (showPlayerMissText)
                ShowPlayerStatusText("MISS", missTextColor);

            return;
        }

        if (RollPlayerDodge())
        {
            if (showPlayerDodgeText)
                ShowPlayerStatusText("DODGE", dodgeTextColor);

            return;
        }

        if (RollPlayerBlock())
        {
            int blockedDamage = Mathf.RoundToInt(damage * 0.5f);

            playerHealth.TakeDamage(blockedDamage);

            if (showPlayerBlockText)
                ShowPlayerStatusText("BLOCK", blockTextColor);

            return;
        }

        playerHealth.TakeDamage(damage);
    }

    bool RollMiss()
    {
        return Random.Range(0f, 100f) <= missChance;
    }

    bool RollPlayerDodge()
    {
        if (playerStats == null)
            return false;

        return Random.Range(0f, 100f) <= playerStats.dodge;
    }

    bool RollPlayerBlock()
    {
        if (playerStats == null)
            return false;

        return Random.Range(0f, 100f) <= playerStats.block;
    }

    void ShowPlayerStatusText(string message, Color color)
    {
        if (floatingCombatTextPrefab == null || playerHealth == null)
            return;

        Vector3 spawnPosition;

        if (playerHealth.playerCombatTextPoint != null)
            spawnPosition = playerHealth.playerCombatTextPoint.position;
        else
            spawnPosition = player.transform.position + Vector3.up * 2f;

        GameObject obj = Instantiate(
            floatingCombatTextPrefab,
            spawnPosition,
            Quaternion.identity
        );

        FloatingCombatText combatText = obj.GetComponent<FloatingCombatText>();

        if (combatText != null)
            combatText.ShowText(message, color, playerStatusTextSize, playerStatusStartScale);
    }

    void BeginReset()
    {
        if (!isResetting)
        {
            isResetting = true;
            nextAttackTime = 0f;
            StopEnemyMovement();
        }

        if (!targetClearedAtReset)
        {
            ClearPlayerTargetFrame();
            targetClearedAtReset = true;
        }
    }

    void ReturnToSpawn(bool restoreWhenReachedSpawn)
    {
        float distanceToSpawn = GetFlatDistance(transform.position, spawnPosition);

        if (distanceToSpawn <= returnStopDistance)
        {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
            StopEnemyMovement();

            if (restoreWhenReachedSpawn && !restoredAtSpawn && enemyHealth != null)
            {
                enemyHealth.RestoreFullHealthAndMana();
                restoredAtSpawn = true;
            }

            return;
        }

        Vector3 targetPosition = new Vector3(
            spawnPosition.x,
            transform.position.y,
            spawnPosition.z
        );

        SmoothFacePosition(targetPosition);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        SetAnimationSpeed(moveSpeed);
    }

    void ClearPlayerTargetFrame()
    {
        if (enemyTargetable == null)
            return;

        TargetFrameUI targetFrameUI = FindFirstObjectByType<TargetFrameUI>();

        if (targetFrameUI != null)
            targetFrameUI.ClearTargetIfMatches(enemyTargetable);
    }

    void SmoothFacePosition(Vector3 lookAtPosition)
    {
        Vector3 lookDirection = lookAtPosition - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    void StopEnemyMovement()
    {
        SetupRigidbody();
        SetAnimationSpeed(0f);
    }

    void SetAnimationSpeed(float speedValue)
    {
        if (enemyAnimator != null)
            enemyAnimator.SetFloat("Speed", speedValue);
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}