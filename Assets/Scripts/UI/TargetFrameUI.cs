using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetFrameUI : MonoBehaviour
{
    [Header("Current Target")]
    public EnemyTargetable currentTarget;

    [Header("Main UI")]
    public GameObject frameRoot;
    public TMP_Text targetNameText;
    public TMP_Text targetLevelText;
    public TMP_Text rankText;
    public Image targetPortraitImage;

    [Header("Rank Colors")]
    public Color enemyRankColor = new Color(0.65f, 0.2f, 1f);

    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthValueText;

    [Header("Mana / Nova Bar")]
    public Slider manaSlider;
    public TMP_Text manaValueText;

    [Header("Energy Bar")]
    public Slider energySlider;
    public TMP_Text energyValueText;

    [Header("Rage Bar")]
    public Slider rageSlider;
    public TMP_Text rageValueText;

    void Start()
    {
        ClearTarget();
    }

    void Update()
    {
        if (currentTarget == null)
        {
            HideFrame();
            return;
        }

        if (currentTarget.IsDead())
        {
            ClearTarget();
            return;
        }

        UpdateUI();
    }

    public void SetTarget(EnemyTargetable newTarget)
    {
        currentTarget = newTarget;

        if (currentTarget == null)
        {
            ClearTarget();
            return;
        }

        ShowFrame();
        UpdateUI();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        ResetUI();
        HideFrame();
    }

    public void ClearTargetIfMatches(EnemyTargetable targetToClear)
    {
        if (targetToClear == null)
            return;

        if (currentTarget == targetToClear)
            ClearTarget();
    }

    void UpdateUI()
    {
        if (currentTarget == null)
            return;

        UpdateLevelAndName();

        if (rankText != null)
        {
            rankText.text = "Enemy";
            rankText.color = enemyRankColor;
        }

        if (targetPortraitImage != null && currentTarget.GetTargetPortrait() != null)
            targetPortraitImage.sprite = currentTarget.GetTargetPortrait();

        UpdateHealthBar();
        UpdateManaBar();
        UpdateEnergyBar();
        UpdateRageBar();
    }

    void UpdateLevelAndName()
    {
        EnemyHealth enemyHealth = currentTarget.GetComponent<EnemyHealth>();

        Color difficultyColor = Color.white;

        if (enemyHealth != null)
        {
            PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();

            int playerLevel = 1;

            if (playerStats != null)
                playerLevel = playerStats.GetCurrentLevel();

            int enemyLevel = enemyHealth.enemyLevel;
            int diff = enemyLevel - playerLevel;

            if (diff >= 10)
            {
                difficultyColor = Color.red;

                if (targetLevelText != null)
                {
                    targetLevelText.text = "X";
                    targetLevelText.color = difficultyColor;
                }
            }
            else
            {
                if (diff <= -5)
                    difficultyColor = Color.gray;
                else if (diff <= 0)
                    difficultyColor = Color.green;
                else
                    difficultyColor = new Color(1f, 0.55f, 0f);

                if (targetLevelText != null)
                {
                    targetLevelText.text = enemyLevel.ToString();
                    targetLevelText.color = difficultyColor;
                }
            }
        }

        if (targetNameText != null)
        {
            targetNameText.text = currentTarget.GetTargetName();
            targetNameText.color = difficultyColor;
        }
    }

    void UpdateHealthBar()
    {
        int currentHp = currentTarget.GetCurrentHealth();
        int maxHp = currentTarget.GetMaxHealth();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHp;
            healthSlider.value = currentHp;
        }

        if (healthValueText != null)
        {
            if (currentHp <= 0 || currentTarget.IsDead())
            {
                healthValueText.text = "Dead";
                healthValueText.color = HexToColor("#FF3333");
            }
            else
            {
                int hpPercent = maxHp > 0
                    ? Mathf.RoundToInt((float)currentHp / maxHp * 100f)
                    : 0;

                healthValueText.text =
                    currentHp + "/" +
                    maxHp +
                    " (" + hpPercent + "%)";

                healthValueText.color = GetHealthColor(hpPercent);
            }
        }
    }

    Color GetHealthColor(int hpPercent)
    {
        if (hpPercent >= 70) return HexToColor("#33CC66");
        if (hpPercent >= 40) return HexToColor("#FFD700");
        if (hpPercent >= 1) return HexToColor("#FF3333");

        return HexToColor("#FF3333");
    }

    void UpdateManaBar()
    {
        if (currentTarget.UsesMana())
        {
            if (manaSlider != null)
            {
                manaSlider.gameObject.SetActive(true);
                manaSlider.maxValue = currentTarget.GetMaxMana();
                manaSlider.value = currentTarget.GetCurrentMana();
            }

            if (manaValueText != null)
            {
                manaValueText.gameObject.SetActive(true);

                int percent = currentTarget.GetMaxMana() > 0
                    ? Mathf.RoundToInt((float)currentTarget.GetCurrentMana() / currentTarget.GetMaxMana() * 100f)
                    : 0;

                manaValueText.text =
                    currentTarget.GetCurrentMana() + "/" +
                    currentTarget.GetMaxMana() +
                    " (" + percent + "%)";
            }
        }
        else
        {
            if (manaSlider != null) manaSlider.gameObject.SetActive(false);
            if (manaValueText != null) manaValueText.gameObject.SetActive(false);
        }
    }

    void UpdateEnergyBar()
    {
        if (currentTarget.UsesEnergy())
        {
            if (energySlider != null)
            {
                energySlider.gameObject.SetActive(true);
                energySlider.maxValue = currentTarget.GetMaxEnergy();
                energySlider.value = currentTarget.GetCurrentEnergy();
            }

            if (energyValueText != null)
            {
                energyValueText.gameObject.SetActive(true);

                int percent = currentTarget.GetMaxEnergy() > 0
                    ? Mathf.RoundToInt((float)currentTarget.GetCurrentEnergy() / currentTarget.GetMaxEnergy() * 100f)
                    : 0;

                energyValueText.text =
                    currentTarget.GetCurrentEnergy() + "/" +
                    currentTarget.GetMaxEnergy() +
                    " (" + percent + "%)";
            }
        }
        else
        {
            if (energySlider != null) energySlider.gameObject.SetActive(false);
            if (energyValueText != null) energyValueText.gameObject.SetActive(false);
        }
    }

    void UpdateRageBar()
    {
        if (currentTarget.UsesRage())
        {
            if (rageSlider != null)
            {
                rageSlider.gameObject.SetActive(true);
                rageSlider.maxValue = currentTarget.GetMaxRage();
                rageSlider.value = currentTarget.GetCurrentRage();
            }

            if (rageValueText != null)
            {
                rageValueText.gameObject.SetActive(true);

                int percent = currentTarget.GetMaxRage() > 0
                    ? Mathf.RoundToInt((float)currentTarget.GetCurrentRage() / currentTarget.GetMaxRage() * 100f)
                    : 0;

                rageValueText.text =
                    currentTarget.GetCurrentRage() + "/" +
                    currentTarget.GetMaxRage() +
                    " (" + percent + "%)";
            }
        }
        else
        {
            if (rageSlider != null) rageSlider.gameObject.SetActive(false);
            if (rageValueText != null) rageValueText.gameObject.SetActive(false);
        }
    }

    void ResetUI()
    {
        if (targetNameText != null) targetNameText.text = "";
        if (targetLevelText != null) targetLevelText.text = "";
        if (rankText != null) rankText.text = "";
        if (healthValueText != null) healthValueText.text = "";

        if (targetPortraitImage != null)
            targetPortraitImage.sprite = null;

        ResetResourceBar(manaSlider, manaValueText);
        ResetResourceBar(energySlider, energyValueText);
        ResetResourceBar(rageSlider, rageValueText);
    }

    void ResetResourceBar(Slider slider, TMP_Text valueText)
    {
        if (slider != null)
        {
            slider.value = 0;
            slider.gameObject.SetActive(false);
        }

        if (valueText != null)
        {
            valueText.text = "";
            valueText.gameObject.SetActive(false);
        }
    }

    void ShowFrame()
    {
        if (frameRoot != null)
            frameRoot.SetActive(true);
    }

    void HideFrame()
    {
        if (frameRoot != null)
            frameRoot.SetActive(false);
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}