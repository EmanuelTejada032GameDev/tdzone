using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// UI component for a single skill tree node.
/// Displays as a small icon+border square. On hover, shows a tooltip with
/// name, description, level progress, and cost. Click to purchase.
/// Placed manually in the editor for visual tree layout.
/// </summary>
public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Skill Reference")]
    [SerializeField] private SkillNodeSO skillNode;

    [Header("Node Display")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Image tooltipIconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("State Colors")]
    [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color unavailableColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color maxedColor = new Color(1f, 0.84f, 0f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private SkillTreeManager skillTreeManager;

    public SkillNodeSO BoundSkill => skillNode;

    public event Action<SkillNodeUI> OnSlotClicked;

    /// <summary>
    /// Binds this node to the SkillTreeManager and subscribes to events.
    /// Uses the SkillNodeSO assigned in the inspector.
    /// </summary>
    public void Bind(SkillTreeManager manager)
    {
        Unbind();

        if (skillNode == null || manager == null) return;

        skillTreeManager = manager;

        skillTreeManager.OnSkillPurchased += Manager_OnSkillPurchased;

        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged += Progression_OnCurrencyChanged;
        }

        // Hide tooltip by default
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        RefreshState();
    }

    /// <summary>
    /// Unsubscribes from all events.
    /// </summary>
    public void Unbind()
    {
        if (skillTreeManager != null)
        {
            skillTreeManager.OnSkillPurchased -= Manager_OnSkillPurchased;
        }

        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged -= Progression_OnCurrencyChanged;
        }

        skillTreeManager = null;
    }

    private void OnDisable()
    {
        Unbind();
    }

    /// <summary>
    /// Refreshes icon, border color, and tooltip content.
    /// </summary>
    public void RefreshState()
    {
        if (skillNode == null || skillTreeManager == null) return;

        int purchaseCount = skillTreeManager.GetPurchaseCount(skillNode.skillId);
        int maxPurchases = skillNode.maxPurchases;
        bool isMaxed = purchaseCount >= maxPurchases;
        int nextCost = skillTreeManager.GetNextCost(skillNode);
        PurchaseFailReason failReason = skillTreeManager.GetPurchaseFailReason(skillNode);
        bool isLocked = failReason == PurchaseFailReason.PrerequisiteNotMet ||
                        failReason == PurchaseFailReason.TowerNotUnlocked;

        // Node icon
        if (iconImage != null && skillNode.icon != null)
        {
            iconImage.sprite = skillNode.icon;
            iconImage.enabled = true;
            iconImage.color = isLocked ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.white;
        }

        // Border color
        if (borderImage != null)
        {
            if (isMaxed)
                borderImage.color = maxedColor;
            else if (isLocked)
                borderImage.color = lockedColor;
            else if (skillTreeManager.CanPurchase(skillNode))
                borderImage.color = availableColor;
            else
                borderImage.color = unavailableColor;
        }

        // Tooltip icon
        if (tooltipIconImage != null && skillNode.icon != null)
        {
            tooltipIconImage.sprite = skillNode.icon;
        }

        // Tooltip texts
        if (nameText != null)
        {
            nameText.SetText(skillNode.skillName);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(skillNode.description);
        }

        if (progressText != null)
        {
            progressText.SetText($"Lv. {purchaseCount}/{maxPurchases}");
        }

        if (costText != null)
        {
            costText.SetText(isMaxed ? "MAX" : nextCost.ToString());
        }
    }

    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        // Bring this node to front so tooltip renders above sibling nodes
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this);
    }

    #endregion

    #region Event Handlers

    private void Manager_OnSkillPurchased(SkillNodeSO skill)
    {
        RefreshState();
    }

    private void Progression_OnCurrencyChanged(object sender, int newCurrency)
    {
        RefreshState();
    }

    #endregion
}
