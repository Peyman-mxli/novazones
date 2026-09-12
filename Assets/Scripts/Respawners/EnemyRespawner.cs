using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Enemy Respawn Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 3f;

    [Header("Existing Enemy In Scene")]
    public GameObject existingEnemyInScene;

    [Header("Spawn Height Fix")]
    public float spawnHeightOffset = 0f;

    private GameObject currentEnemy;
    private float timer;
    private bool countingDown = false;

    private void Start()
    {
        if (existingEnemyInScene != null)
        {
            currentEnemy = existingEnemyInScene;
            currentEnemy.name = existingEnemyInScene.name;

            EnemyLoot enemyLoot = currentEnemy.GetComponent<EnemyLoot>();
            if (enemyLoot != null)
                enemyLoot.ResetLoot();

            return;
        }

        SpawnEnemy();
    }

    private void Update()
    {
        if (currentEnemy != null)
            return;

        if (!countingDown)
        {
            countingDown = true;
            timer = respawnDelay;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyRespawner: Enemy Prefab is missing.");
            return;
        }

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }

        spawnPosition.y += spawnHeightOffset;

        currentEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        currentEnemy.name = enemyPrefab.name;

        EnemyLoot enemyLoot = currentEnemy.GetComponent<EnemyLoot>();
        if (enemyLoot != null)
            enemyLoot.ResetLoot();

        countingDown = false;
        timer = 0f;
    }
}