using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDeathUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    [Header("UI")]
    public GameObject deathPanel;
    public TMP_Text cooldownText;
    public Button respawnHereButton;
    public Button respawnAngelButton;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (playerHealth == null)
        {
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }

            return;
        }

        bool isDead = playerHealth.IsDead();

        if (deathPanel != null)
        {
            deathPanel.SetActive(isDead);
        }

        if (!isDead)
        {
            return;
        }

        float remaining = playerHealth.GetRespawnCooldownRemaining();
        bool canRespawn = playerHealth.CanRespawn();

        if (cooldownText != null)
        {
            if (canRespawn)
            {
                cooldownText.text = "Respawn Ready";
            }
            else
            {
                cooldownText.text = "Respawn in " + Mathf.CeilToInt(remaining) + " sec";
            }
        }

        if (respawnHereButton != null)
        {
            respawnHereButton.interactable = canRespawn;
        }

        if (respawnAngelButton != null)
        {
            respawnAngelButton.interactable = canRespawn;
        }
    }

    public void OnClickRespawnHere()
    {
        if (playerHealth == null)
            return;

        playerHealth.RespawnHere();
    }

    public void OnClickRespawnAtAngel()
    {
        if (playerHealth == null)
            return;

        playerHealth.RespawnAtNearestAngel();
    }
}