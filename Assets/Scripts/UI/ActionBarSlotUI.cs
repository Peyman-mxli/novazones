using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActionBarSlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerClickHandler
{
    [Header("Slot UI")]
    public Image skillIconImage;

    [Header("Cooldown UI")]
    public Image cooldownOverlay;
    public TMP_Text cooldownText;

    [Header("Global Cooldown")]
    public float globalCooldownDuration = 1.5f;

    [Header("Allowed Drop Area")]
    public Transform allowedActionBarRoot;

    [Header("References")]
    public PlayerCombat playerCombat;

    [Header("Runtime")]
    public Sprite assignedSkillIcon;
    public string assignedSkillName;
    public float assignedSkillCooldown = 0f;
    public bool assignedSkillUsesGlobalCooldown = true;

    private Canvas parentCanvas;
    private GameObject dragIconObject;

    private float cooldownDuration = 0f;
    private float cooldownRemaining = 0f;
    private bool isCoolingDown = false;

    private static float globalCooldownEndTime = 0f;
    private static float globalCooldownStartTime = 0f;
    private static float globalCooldownLength = 0f;

    private Color normalIconColor = Color.white;
    private Color cooldownIconColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    private Color gcdIconColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();

        HideCooldown();
        RefreshSlot();
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateGlobalCooldownVisual();
    }

    public void SetSkill(Sprite icon, string skillName, float skillCooldown, bool usesGlobalCooldown = true)
    {
        assignedSkillIcon = icon;
        assignedSkillName = skillName;
        assignedSkillCooldown = skillCooldown;
        assignedSkillUsesGlobalCooldown = usesGlobalCooldown;

        RefreshSlot();
    }

    public void ClearSkill()
    {
        assignedSkillIcon = null;
        assignedSkillName = "";
        assignedSkillCooldown = 0f;
        assignedSkillUsesGlobalCooldown = true;

        HideCooldown();
        RefreshSlot();
    }

    public void RefreshSlot()
    {
        if (skillIconImage == null)
            return;

        skillIconImage.sprite = assignedSkillIcon;
        skillIconImage.enabled = assignedSkillIcon != null;

        if (assignedSkillIcon != null)
        {
            if (isCoolingDown)
                skillIconImage.color = cooldownIconColor;
            else if (ShouldShowGlobalCooldownVisual())
                skillIconImage.color = gcdIconColor;
            else
                skillIconImage.color = normalIconColor;
        }
        else
        {
            skillIconImage.color = new Color(1f, 1f, 1f, 0f);
        }

        skillIconImage.raycastTarget = false;
    }

    public void ExecuteAssignedSkill()
    {
        if (isCoolingDown)
            return;

        if (assignedSkillUsesGlobalCooldown && IsGlobalCooldownActive())
            return;

        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
            return;

        if (string.IsNullOrEmpty(assignedSkillName))
            return;

        if (assignedSkillName == "Auto Attack")
        {
            playerCombat.ToggleAutoAttackFromActionBar();
            return;
        }

        if (assignedSkillName == "Nova Strike")
        {
            bool castSuccess = playerCombat.CastNovaStrikeFromActionBar();

            if (castSuccess && assignedSkillUsesGlobalCooldown)
                StartGlobalCooldown();

            return;
        }
    }

    public void StartCooldown(float duration)
    {
        if (duration <= 0f)
            return;

        cooldownDuration = duration;
        cooldownRemaining = duration;
        isCoolingDown = true;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = 1f;
            cooldownOverlay.raycastTarget = false;
        }

        if (cooldownText != null)
        {
            cooldownText.enabled = true;
            cooldownText.raycastTarget = false;
        }

        if (skillIconImage != null)
            skillIconImage.color = cooldownIconColor;

        UpdateCooldownVisuals();
    }

    private void UpdateCooldown()
    {
        if (!isCoolingDown)
            return;

        cooldownRemaining -= Time.deltaTime;

        if (cooldownRemaining <= 0f)
        {
            HideCooldown();
            return;
        }

        UpdateCooldownVisuals();
    }

    private void UpdateCooldownVisuals()
    {
        float normalized = cooldownRemaining / cooldownDuration;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = Mathf.Clamp01(normalized);

        if (cooldownText != null)
        {
            if (cooldownRemaining >= 60f)
            {
                int minutes = Mathf.FloorToInt(cooldownRemaining / 60f);
                cooldownText.text = minutes + "m";
            }
            else if (cooldownRemaining >= 10f)
            {
                cooldownText.text = Mathf.CeilToInt(cooldownRemaining).ToString();
            }
            else
            {
                cooldownText.text = cooldownRemaining.ToString("0.0");
            }
        }
    }

    private void HideCooldown()
    {
        isCoolingDown = false;
        cooldownDuration = 0f;
        cooldownRemaining = 0f;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.enabled = false;
            cooldownOverlay.raycastTarget = false;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
            cooldownText.enabled = false;
            cooldownText.raycastTarget = false;
        }

        RefreshSlot();
    }

    private bool IsGlobalCooldownActive()
    {
        return Time.time < globalCooldownEndTime;
    }

    private void StartGlobalCooldown()
    {
        globalCooldownStartTime = Time.time;
        globalCooldownLength = globalCooldownDuration;
        globalCooldownEndTime = Time.time + globalCooldownDuration;
    }

    private bool ShouldShowGlobalCooldownVisual()
    {
        if (assignedSkillIcon == null)
            return false;

        if (!assignedSkillUsesGlobalCooldown)
            return false;

        if (isCoolingDown)
            return false;

        return IsGlobalCooldownActive();
    }

    private void UpdateGlobalCooldownVisual()
    {
        if (!ShouldShowGlobalCooldownVisual())
        {
            if (!isCoolingDown && skillIconImage != null && assignedSkillIcon != null)
                skillIconImage.color = normalIconColor;

            return;
        }

        float remaining = globalCooldownEndTime - Time.time;
        float normalized = remaining / globalCooldownLength;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.enabled = true;
            cooldownOverlay.fillAmount = Mathf.Clamp01(normalized);
            cooldownOverlay.raycastTarget = false;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
            cooldownText.enabled = false;
            cooldownText.raycastTarget = false;
        }

        if (skillIconImage != null)
            skillIconImage.color = gcdIconColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteAssignedSkill();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedSkillIcon == null)
            return;

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        dragIconObject = new GameObject("DraggedActionSkillIcon");
        dragIconObject.transform.SetParent(parentCanvas.transform, false);
        dragIconObject.transform.SetAsLastSibling();

        Image dragImage = dragIconObject.AddComponent<Image>();
        dragImage.sprite = assignedSkillIcon;
        dragImage.raycastTarget = false;

        RectTransform dragRect = dragIconObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = skillIconImage.rectTransform.sizeDelta;

        skillIconImage.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconObject != null)
            dragIconObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconObject != null)
            Destroy(dragIconObject);

        ActionBarSlotUI targetSlot = null;

        if (eventData.pointerEnter != null)
            targetSlot = eventData.pointerEnter.GetComponentInParent<ActionBarSlotUI>();

        if (targetSlot == null || !targetSlot.IsInsideAllowedActionBar())
        {
            ClearSkill();
            return;
        }

        if (targetSlot == this)
        {
            RefreshSlot();
            return;
        }

        Sprite oldIcon = targetSlot.assignedSkillIcon;
        string oldName = targetSlot.assignedSkillName;
        float oldCooldown = targetSlot.assignedSkillCooldown;
        bool oldUsesGlobalCooldown = targetSlot.assignedSkillUsesGlobalCooldown;

        targetSlot.SetSkill(
            assignedSkillIcon,
            assignedSkillName,
            assignedSkillCooldown,
            assignedSkillUsesGlobalCooldown
        );

        SetSkill(
            oldIcon,
            oldName,
            oldCooldown,
            oldUsesGlobalCooldown
        );
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!IsInsideAllowedActionBar())
            return;

        ActionBarSkillDragSource skillSource = null;

        if (eventData.pointerDrag != null)
        {
            skillSource = eventData.pointerDrag.GetComponent<ActionBarSkillDragSource>();

            if (skillSource == null)
                skillSource = eventData.pointerDrag.GetComponentInParent<ActionBarSkillDragSource>();

            if (skillSource == null)
                skillSource = eventData.pointerDrag.GetComponentInChildren<ActionBarSkillDragSource>();
        }

        if (skillSource == null || skillSource.skillIcon == null)
            return;

        SetSkill(
            skillSource.skillIcon,
            skillSource.skillName,
            skillSource.cooldownDuration,
            skillSource.usesGlobalCooldown
        );
    }

    private bool IsInsideAllowedActionBar()
    {
        if (allowedActionBarRoot == null)
            return false;

        return transform.IsChildOf(allowedActionBarRoot);
    }
}