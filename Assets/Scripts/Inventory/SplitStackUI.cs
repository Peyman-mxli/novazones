using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplitStackUI : MonoBehaviour
{
    public static SplitStackUI Instance;

    [Header("Main")]
    public GameObject panel;

    [Header("UI")]
    public TMP_InputField amountInput;
    public Button plusButton;
    public Button minusButton;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Preview")]
    public Image itemPreviewIcon;

    private int maxAmount = 1;
    private int currentAmount = 1;

    private System.Action<int> onConfirm;

    private void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Open(int max, Sprite icon, System.Action<int> confirmCallback)
    {
        if (panel == null)
            return;

        maxAmount = Mathf.Max(2, max);
        currentAmount = Mathf.Clamp(maxAmount / 2, 1, maxAmount - 1);

        onConfirm = confirmCallback;

        if (itemPreviewIcon != null)
        {
            itemPreviewIcon.enabled = icon != null;
            itemPreviewIcon.sprite = icon;
        }

        UpdateUI();
        panel.SetActive(true);

        if (amountInput != null)
        {
            amountInput.Select();
            amountInput.ActivateInputField();
        }
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        onConfirm = null;
    }

    private void UpdateUI()
    {
        if (amountInput != null)
            amountInput.text = currentAmount.ToString();
    }

    private void ReadInputAmount()
    {
        if (amountInput == null)
            return;

        if (int.TryParse(amountInput.text, out int value))
            currentAmount = Mathf.Clamp(value, 1, maxAmount - 1);
        else
            currentAmount = 1;

        UpdateUI();
    }

    public void Increase()
    {
        ReadInputAmount();

        if (currentAmount < maxAmount - 1)
            currentAmount++;

        UpdateUI();
    }

    public void Decrease()
    {
        ReadInputAmount();

        if (currentAmount > 1)
            currentAmount--;

        UpdateUI();
    }

    public void Confirm()
    {
        ReadInputAmount();

        if (onConfirm != null)
            onConfirm.Invoke(currentAmount);

        Close();
    }

    public void Cancel()
    {
        Close();
    }
}