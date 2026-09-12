using UnityEngine;

public class CorpseLootGlow : MonoBehaviour
{
    [Header("References")]
    public EnemyLoot enemyLoot;
    public EnemyHealth enemyHealth;

    [Header("Visual")]
    public GameObject glowObject;

    [Header("Manual Size Control")]
    public float glowSize = 1.5f;
    public float glowHeight = 0f;

    [Header("Fade Settings")]
    public bool fadeWhenLootEmpty = true;
    public float fadeDisableDelay = 1.5f;

    private ParticleSystem[] glowParticles;
    private float fadeTimer;
    private bool wasGlowOn;

    private void Awake()
    {
        if (enemyLoot == null)
            enemyLoot = GetComponent<EnemyLoot>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        CacheParticles();
        ApplyManualSettings();

        if (glowObject != null)
            glowObject.SetActive(false);
    }

    private void OnValidate()
    {
        // Do NOT clamp glowSize here.
        // This allows typing values like 0.5 without Unity forcing it to 0.01 while you type.
        ApplyManualSettings();
    }

    private void Update()
    {
        if (enemyLoot == null || enemyHealth == null || glowObject == null)
            return;

        ApplyManualSettings();

        bool enemyIsDead = enemyHealth.IsDead();
        bool stillHasLoot = enemyIsDead && enemyLoot.HasAnyLoot();

        if (stillHasLoot)
        {
            TurnGlowOn();
            return;
        }

        if (!enemyIsDead)
        {
            TurnGlowOffInstant();
            return;
        }

        if (fadeWhenLootEmpty)
            FadeGlowOff();
        else
            TurnGlowOffInstant();
    }

    private void CacheParticles()
    {
        if (glowObject == null)
        {
            glowParticles = null;
            return;
        }

        glowParticles = glowObject.GetComponentsInChildren<ParticleSystem>(true);
    }

    private void ApplyManualSettings()
    {
        if (glowObject == null)
            return;

        float safeGlowSize = glowSize;

        if (safeGlowSize < 0f)
            safeGlowSize = 0f;

        glowObject.transform.localScale = Vector3.one * safeGlowSize;

        Vector3 localPosition = glowObject.transform.localPosition;
        localPosition.y = glowHeight;
        glowObject.transform.localPosition = localPosition;
    }

    private void TurnGlowOn()
    {
        fadeTimer = 0f;
        ApplyManualSettings();

        if (!glowObject.activeSelf)
            glowObject.SetActive(true);

        if (!wasGlowOn)
        {
            CacheParticles();

            if (glowParticles != null)
            {
                for (int i = 0; i < glowParticles.Length; i++)
                {
                    if (glowParticles[i] != null)
                        glowParticles[i].Play(true);
                }
            }

            wasGlowOn = true;
        }
    }

    private void FadeGlowOff()
    {
        if (!glowObject.activeSelf)
            return;

        if (glowParticles != null)
        {
            for (int i = 0; i < glowParticles.Length; i++)
            {
                if (glowParticles[i] != null)
                    glowParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        fadeTimer += Time.deltaTime;

        if (fadeTimer >= fadeDisableDelay)
            TurnGlowOffInstant();
    }

    private void TurnGlowOffInstant()
    {
        fadeTimer = 0f;
        wasGlowOn = false;

        if (glowParticles != null)
        {
            for (int i = 0; i < glowParticles.Length; i++)
            {
                if (glowParticles[i] != null)
                    glowParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (glowObject != null)
            glowObject.SetActive(false);
    }
}