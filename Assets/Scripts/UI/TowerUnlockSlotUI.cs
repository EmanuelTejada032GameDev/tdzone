using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for a single tower unlock/upgrade slot.
/// Displays tower icon, name, level, stats, cost, and an action button.
/// Refreshes only on discrete events — no Update() loop.
/// </summary>
public class TowerUnlockSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button actionButton;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color maxLevelColor = new Color(1f, 0.84f, 0f, 1f);
    [SerializeField] private Color cantAffordColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private TowerDataSO boundTower;
    private UnlockManager unlockManager;

    public TowerDataSO BoundTower => boundTower;

    public event Action<TowerUnlockSlotUI> OnSlotClicked;

    /// <summary>
    /// Binds this slot to a tower and subscribes to relevant events.
    /// </summary>
    public void BindTower(TowerDataSO tower, UnlockManager manager)
    {
        UnbindTower();

        if (tower == null || manager == null) return;

        boundTower = tower;
        unlockManager = manager;

        // Subscribe to events
        unlockManager.OnTowerUnlockAttempt += Manager_OnTowerUnlockAttempt;
        unlockManager.OnTowerUpgradeAttempt += Manager_OnTowerUpgradeAttempt;

        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged += Progression_OnCurrencyChanged;
        }

        // Wire action button
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionClicked);
        }

        RefreshState();
    }

    /// <summary>
    /// Unbinds the current tower and clears all subscriptions.
    /// </summary>
    public void UnbindTower()
    {
        if (unlockManager != null)
        {
            unlockManager.OnTowerUnlockAttempt -= Manager_OnTowerUnlockAttempt;
            unlockManager.OnTowerUpgradeAttempt -= Manager_OnTowerUpgradeAttempt;
        }

        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged -= Progression_OnCurrencyChanged;
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnActionClicked);
        }

        boundTower = null;
        unlockManager = null;
    }

    private void OnDisable()
    {
        UnbindTower();
    }

    /// <summary>
    /// Refreshes all visual state based on unlock/upgrade status.
    /// </summary>
    public void RefreshState()
    {
        if (boundTower == null || unlockManager == null) return;

        string towerId = boundTower.towerId;
        bool isUnlocked = unlockManager.IsTowerUnlocked(towerId);
        int currentLevel = unlockManager.GetTowerLevel(towerId);
        bool canUpgrade = isUnlocked && unlockManager.CanUpgrade(towerId);
        bool isMaxLevel = isUnlocked && !canUpgrade;

        // Icon
        if (iconImage != null && boundTower.icon != null)
        {
            iconImage.sprite = boundTower.icon;
            iconImage.enabled = true;
        }

        // Name
        if (nameText != null)
        {
            nameText.SetText(boundTower.towerName);
        }

        // Description
        if (descriptionText != null)
        {
            descriptionText.SetText(boundTower.description);
        }

        // Level
        if (levelText != null)
        {
            if (isUnlocked)
            {
                int maxLevel = boundTower.GetMaxLevel();
                levelText.SetText($"Lv.{currentLevel}/{maxLevel}");
            }
            else
            {
                levelText.SetText("Locked");
            }
        }

        // Stats
        if (statsText != null)
        {
            TowerDataSO displayData = isUnlocked
                ? unlockManager.GetTowerDataAtCurrentLevel(towerId) ?? boundTower
                : boundTower;

            statsText.SetText($"DMG: {displayData.damage:F0}  SPD: {displayData.fireRate:F1}  RNG: {displayData.range:F0}");
        }

        // Cost and action button
        if (!isUnlocked)
        {
            // Locked state — show unlock cost
            int unlockCost = unlockManager.GetUnlockCost(towerId);
            bool canAfford = unlockManager.CanAffordUnlock(towerId);

            if (costText != null) costText.SetText(unlockCost.ToString());
            if (actionButtonText != null) actionButtonText.SetText("Unlock");
            if (actionButton != null) actionButton.interactable = canAfford;
            if (borderImage != null) borderImage.color = canAfford ? lockedColor : cantAffordColor;

            if (iconImage != null) iconImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
        else if (isMaxLevel)
        {
            // Max level
            if (costText != null) costText.SetText("--");
            if (actionButtonText != null) actionButtonText.SetText("Max Level");
            if (actionButton != null) actionButton.interactable = false;
            if (borderImage != null) borderImage.color = maxLevelColor;

            if (iconImage != null) iconImage.color = Color.white;
        }
        else
        {
            // Unlocked, can upgrade
            int upgradeCost = unlockManager.GetUpgradeCost(towerId);
            bool canAfford = unlockManager.CanAffordUpgrade(towerId);

            if (costText != null) costText.SetText(upgradeCost.ToString());
            if (actionButtonText != null) actionButtonText.SetText("Upgrade");
            if (actionButton != null) actionButton.interactable = canAfford;
            if (borderImage != null) borderImage.color = canAfford ? unlockedColor : cantAffordColor;

            if (iconImage != null) iconImage.color = Color.white;
        }
    }

    private void OnActionClicked()
    {
        OnSlotClicked?.Invoke(this);
    }

    #region Event Handlers

    private void Manager_OnTowerUnlockAttempt(object sender, TowerUnlockEventArgs e)
    {
        if (boundTower != null && e.TowerId == boundTower.towerId)
        {
            RefreshState();
        }
    }

    private void Manager_OnTowerUpgradeAttempt(object sender, TowerUpgradeEventArgs e)
    {
        if (boundTower != null && e.TowerId == boundTower.towerId)
        {
            RefreshState();
        }
    }

    private void Progression_OnCurrencyChanged(object sender, int newCurrency)
    {
        RefreshState();
    }

    #endregion
}
