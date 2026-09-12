using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpButtonUI : MonoBehaviour
{
    [Header("Main Help Button")]
    public Button helpButton;

    [Header("Menu Panel")]
    public GameObject helpMenuPanel;
    public Button autoStuckButton;
    public Button ticketButton;
    public Button closeButton;

    [Header("Ticket Panel")]
    public GameObject ticketPanel;
    public TMP_InputField ticketInputField;
    public Button sendTicketButton;
    public Button closeTicketButton;
    public TMP_Text ticketStatusText;

    [Header("Auto Stuck Settings")]
    public Transform player;
    public string angelTag = "Angel";
    public float respawnHeightOffset = 1.5f;

    private void Start()
    {
        if (helpButton != null)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(ToggleHelpMenu);
        }

        if (autoStuckButton != null)
        {
            autoStuckButton.onClick.RemoveAllListeners();
            autoStuckButton.onClick.AddListener(AutoStuckToClosestAngel);
        }

        if (ticketButton != null)
        {
            ticketButton.onClick.RemoveAllListeners();
            ticketButton.onClick.AddListener(OpenTicketPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAll);
        }

        if (sendTicketButton != null)
        {
            sendTicketButton.onClick.RemoveAllListeners();
            sendTicketButton.onClick.AddListener(SendTicket);
        }

        if (closeTicketButton != null)
        {
            closeTicketButton.onClick.RemoveAllListeners();
            closeTicketButton.onClick.AddListener(CloseTicketPanel);
        }

        CloseAll();
    }

    private void ToggleHelpMenu()
    {
        if (helpMenuPanel == null)
            return;

        bool newState = !helpMenuPanel.activeSelf;
        helpMenuPanel.SetActive(newState);

        if (ticketPanel != null)
            ticketPanel.SetActive(false);
    }

    private void OpenTicketPanel()
    {
        if (helpMenuPanel != null)
            helpMenuPanel.SetActive(false);

        if (ticketPanel != null)
            ticketPanel.SetActive(true);

        if (ticketStatusText != null)
            ticketStatusText.text = "";

        if (ticketInputField != null)
            ticketInputField.text = "";

        Debug.Log("Nova Support Ticket Panel opened.");
    }

    private void SendTicket()
    {
        if (ticketInputField == null)
            return;

        string message = ticketInputField.text.Trim();

        if (message.Length < 3)
        {
            if (ticketStatusText != null)
                ticketStatusText.text = "Write your problem first.";

            return;
        }

        Debug.Log("Nova Support Ticket Sent: " + message);

        ticketInputField.text = "";

        CloseTicketPanel();
    }

    private void CloseTicketPanel()
    {
        if (ticketPanel != null)
            ticketPanel.SetActive(false);

        if (ticketInputField != null)
            ticketInputField.text = "";

        if (ticketStatusText != null)
            ticketStatusText.text = "";
    }

    private void CloseAll()
    {
        if (helpMenuPanel != null)
            helpMenuPanel.SetActive(false);

        if (ticketPanel != null)
            ticketPanel.SetActive(false);
    }

    private void AutoStuckToClosestAngel()
    {
        if (player == null)
        {
            Debug.LogWarning("HelpButtonUI: Player is not assigned.");
            return;
        }

        GameObject[] angels = GameObject.FindGameObjectsWithTag(angelTag);

        if (angels.Length == 0)
        {
            Debug.LogWarning("HelpButtonUI: No Angel objects found. Make sure your Angel has tag: Angel");
            return;
        }

        GameObject closestAngel = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject angel in angels)
        {
            float distance = Vector3.Distance(player.position, angel.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAngel = angel;
            }
        }

        if (closestAngel == null)
            return;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = closestAngel.transform.position + Vector3.up * respawnHeightOffset;

        CloseAll();

        Debug.Log("Player AutoStuck moved to closest Angel: " + closestAngel.name);
    }
}