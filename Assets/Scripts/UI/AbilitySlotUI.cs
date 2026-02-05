using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for a single ability slot.
/// Displays ability state, cooldown progress, and duration progress.
/// </summary>
public class AbilitySlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image durationBar;
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("State Colors")]
    [SerializeField] private Color readyColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color activeColor = new Color(0.2f, 1f, 0.2f, 1f);
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("Settings")]
    [SerializeField] private bool showCooldownText = true;
    [SerializeField] private bool showDurationBar = true;

    private TowerAbility boundAbility;
    private bool isInitialized;

    public TowerAbility BoundAbility => boundAbility;

    public event Action<AbilitySlotUI> OnSlotClicked;

    private void OnDisable()
    {
        UnbindAbility();
    }

    private void Update()
    {
        if (!isInitialized || boundAbility == null) return;

        UpdateCooldownVisuals();
        UpdateDurationVisuals();
    }

    /// <summary>
    /// Binds this slot to a specific ability.
    /// </summary>
    public void BindAbility(TowerAbility ability)
    {
        UnbindAbility();

        if (ability == null)
        {
            SetEmpty();
            return;
        }

        boundAbility = ability;

        // Subscribe to ability events
        boundAbility.OnActivated += Ability_OnActivated;
        boundAbility.OnDeactivated += Ability_OnDeactivated;
        boundAbility.OnCooldownComplete += Ability_OnCooldownComplete;

        // Set up initial visuals
        SetupVisuals();
        isInitialized = true;
    }

    /// <summary>
    /// Unbinds the current ability and clears subscriptions.
    /// </summary>
    public void UnbindAbility()
    {
        if (boundAbility != null)
        {
            boundAbility.OnActivated -= Ability_OnActivated;
            boundAbility.OnDeactivated -= Ability_OnDeactivated;
            boundAbility.OnCooldownComplete -= Ability_OnCooldownComplete;
            boundAbility = null;
        }

        isInitialized = false;
    }

    /// <summary>
    /// Called when this slot is clicked (for mouse/touch input).
    /// </summary>
    public void OnClick()
    {
        OnSlotClicked?.Invoke(this);
    }

    private void SetupVisuals()
    {
        if (boundAbility == null) return;

        // Set icon from tower data
        if (iconImage != null && boundAbility.TowerData != null)
        {
            if (boundAbility.TowerData.icon != null)
            {
                iconImage.sprite = boundAbility.TowerData.icon;
                iconImage.enabled = true;
            }
            else
            {
                // No icon, show placeholder or keep existing
                iconImage.enabled = true;
            }
        }

        // Set hotkey text
        if (hotkeyText != null)
        {
            hotkeyText.SetText("["+GetHotkeyDisplayText(boundAbility.ActivationKey)+"]");
        }

        // Initialize state visuals
        UpdateStateVisuals();
    }

    private void SetEmpty()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (durationBar != null) durationBar.fillAmount = 0f;
        if (hotkeyText != null) hotkeyText.SetText("");
        if (cooldownText != null) cooldownText.SetText("");
        if (borderImage != null) borderImage.color = lockedColor;
    }

    private void UpdateStateVisuals()
    {
        if (boundAbility == null) return;

        Color stateColor = boundAbility.State switch
        {
            AbilityState.Ready => readyColor,
            AbilityState.Active => activeColor,
            AbilityState.Cooldown => cooldownColor,
            AbilityState.Locked => lockedColor,
            _ => readyColor
        };

        // Apply state color to border
        if (borderImage != null)
        {
            borderImage.color = stateColor;
        }

        // Apply dimming to icon when locked or on cooldown
        if (iconImage != null)
        {
            iconImage.color = boundAbility.IsLocked
                ? new Color(0.3f, 0.3f, 0.3f, 1f)
                : Color.white;
        }
    }

    private void UpdateCooldownVisuals()
    {
        if (boundAbility == null) return;

        bool isOnCooldown = boundAbility.IsOnCooldown;

        // Update cooldown overlay fill
        if (cooldownOverlay != null)
        {
            if (isOnCooldown)
            {
                // Fill amount decreases as cooldown progresses (1 = full cooldown, 0 = ready)
                cooldownOverlay.fillAmount = 1f - boundAbility.CooldownProgress;
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
            }
        }

        // Update cooldown text
        if (cooldownText != null && showCooldownText)
        {
            if (isOnCooldown && boundAbility.CooldownRemaining > 0f)
            {
                cooldownText.SetText(Mathf.CeilToInt(boundAbility.CooldownRemaining).ToString());
                cooldownText.enabled = true;
            }
            else
            {
                cooldownText.SetText("");
                cooldownText.enabled = false;
            }
        }
    }

    private void UpdateDurationVisuals()
    {
        if (boundAbility == null || durationBar == null || !showDurationBar) return;

        if (boundAbility.IsActive)
        {
            // Fill amount decreases as duration runs out
            durationBar.fillAmount = boundAbility.DurationProgress;
            durationBar.enabled = true;
        }
        else
        {
            durationBar.fillAmount = 0f;
            durationBar.enabled = false;
        }
    }

    private string GetHotkeyDisplayText(KeyCode key)
    {
        return key switch
        {
            KeyCode.Alpha1 => "1",
            KeyCode.Alpha2 => "2",
            KeyCode.Alpha3 => "3",
            KeyCode.Alpha4 => "4",
            KeyCode.Alpha5 => "5",
            KeyCode.Alpha6 => "6",
            KeyCode.Alpha7 => "7",
            KeyCode.Alpha8 => "8",
            KeyCode.Alpha9 => "9",
            KeyCode.Alpha0 => "0",
            _ => key.ToString()
        };
    }

    #region Event Handlers

    private void Ability_OnActivated(TowerAbility ability)
    {
        UpdateStateVisuals();
    }

    private void Ability_OnDeactivated(TowerAbility ability)
    {
        UpdateStateVisuals();
    }

    private void Ability_OnCooldownComplete(TowerAbility ability)
    {
        UpdateStateVisuals();
    }

    #endregion
}
