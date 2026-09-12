using UnityEngine;

public class PlayerLevelUpEffect : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Level Up Effect")]
    public GameObject levelUpEffectPrefab;
    public Transform effectSpawnPoint;
    public float effectLifetime = 3f;

    void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
            playerStats.OnLevelUp += PlayLevelUpEffect;
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnLevelUp -= PlayLevelUpEffect;
    }

    void PlayLevelUpEffect(int newLevel)
    {
        if (levelUpEffectPrefab == null)
            return;

        Vector3 spawnPosition = transform.position;

        if (effectSpawnPoint != null)
            spawnPosition = effectSpawnPoint.position;

        GameObject effect = Instantiate(
            levelUpEffectPrefab,
            spawnPosition,
            Quaternion.identity
        );

        effect.transform.SetParent(transform);

        Destroy(effect, effectLifetime);
    }
}