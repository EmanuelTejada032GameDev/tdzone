using System;
using UnityEngine;

/// <summary>
/// Represents a single tower ability's runtime state.
/// Each ability corresponds to a tower type that can be temporarily activated.
/// </summary>
[Serializable]
public class TowerAbility
{
    [Header("Configuration")]
    [SerializeField] private TowerDataSO towerData;
    [SerializeField] private KeyCode activationKey = KeyCode.Alpha1;

    [Header("Runtime State (Read Only)")]
    [SerializeField] private AbilityState state = AbilityState.Ready;
    [SerializeField] private float cooldownRemaining;
    [SerializeField] private float durationRemaining;

    // Properties
    public TowerDataSO TowerData => towerData;
    public KeyCode ActivationKey => activationKey;
    public AbilityState State => state;
    public float CooldownRemaining => cooldownRemaining;
    public float DurationRemaining => durationRemaining;

    // Calculated properties
    public bool IsReady => state == AbilityState.Ready;
    public bool IsActive => state == AbilityState.Active;
    public bool IsOnCooldown => state == AbilityState.Cooldown;
    public bool IsLocked => state == AbilityState.Locked;

    public float CooldownProgress => towerData != null && towerData.abilityCooldown > 0
        ? 1f - (cooldownRemaining / towerData.abilityCooldown)
        : 1f;

    public float DurationProgress => towerData != null && towerData.abilityDuration > 0
        ? 1f - (durationRemaining / towerData.abilityDuration)
        : 1f;

    // Events
    public event Action<TowerAbility> OnActivated;
    public event Action<TowerAbility> OnDeactivated;
    public event Action<TowerAbility> OnCooldownComplete;

    public TowerAbility() { }

    public TowerAbility(TowerDataSO data, KeyCode key)
    {
        towerData = data;
        activationKey = key;
        state = AbilityState.Ready;
    }

    /// <summary>
    /// Try to activate this ability. Returns true if successful.
    /// </summary>
    public bool TryActivate()
    {
        if (state != AbilityState.Ready)
            return false;

        if (towerData == null)
            return false;

        state = AbilityState.Active;
        durationRemaining = towerData.abilityDuration;

        OnActivated?.Invoke(this);
        return true;
    }

    /// <summary>
    /// Force deactivate the ability and start cooldown.
    /// </summary>
    public void Deactivate()
    {
        if (state != AbilityState.Active)
            return;

        state = AbilityState.Cooldown;
        cooldownRemaining = towerData.abilityCooldown;
        durationRemaining = 0f;

        OnDeactivated?.Invoke(this);
    }

    /// <summary>
    /// Update the ability timers. Call this every frame.
    /// </summary>
    public void Tick(float deltaTime)
    {
        switch (state)
        {
            case AbilityState.Active:
                durationRemaining -= deltaTime;
                if (durationRemaining <= 0f)
                {
                    Deactivate();
                }
                break;

            case AbilityState.Cooldown:
                cooldownRemaining -= deltaTime;
                if (cooldownRemaining <= 0f)
                {
                    cooldownRemaining = 0f;
                    state = AbilityState.Ready;
                    OnCooldownComplete?.Invoke(this);
                }
                break;
        }
    }

    /// <summary>
    /// Lock the ability (not yet unlocked by player)
    /// </summary>
    public void Lock()
    {
        if (state == AbilityState.Active)
            Deactivate();

        state = AbilityState.Locked;
        cooldownRemaining = 0f;
        durationRemaining = 0f;
    }

    /// <summary>
    /// Unlock the ability (player purchased it)
    /// </summary>
    public void Unlock()
    {
        if (state == AbilityState.Locked)
        {
            state = AbilityState.Ready;
        }
    }

    /// <summary>
    /// Reset ability to ready state (for testing/debug)
    /// </summary>
    public void Reset()
    {
        state = AbilityState.Ready;
        cooldownRemaining = 0f;
        durationRemaining = 0f;
    }
}

public enum AbilityState
{
    Locked,     // Not yet unlocked
    Ready,      // Available to use
    Active,     // Currently active
    Cooldown    // On cooldown after use
}
