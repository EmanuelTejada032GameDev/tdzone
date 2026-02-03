using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages status effects on an enemy. Attach to enemy prefabs.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Debug")]
    [SerializeField] private List<string> activeEffectNames = new List<string>();

    private Dictionary<StatusEffectType, IStatusEffect> activeEffects = new Dictionary<StatusEffectType, IStatusEffect>();
    private List<StatusEffectType> effectsToRemove = new List<StatusEffectType>();

    // Cached original values for restoration
    private float originalMoveSpeed;
    private bool hasCachedOriginals = false;

    // Public accessors for effects to use
    public Enemy Enemy => enemy;
    public HealthSystem HealthSystem => healthSystem;
    public float OriginalMoveSpeed => originalMoveSpeed;

    public event Action<StatusEffectType> OnEffectApplied;
    public event Action<StatusEffectType> OnEffectRemoved;
    public event Action<StatusEffectType, int> OnEffectStackChanged;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (healthSystem == null)
            healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        CacheOriginalValues();
    }

    private void CacheOriginalValues()
    {
        if (hasCachedOriginals) return;

        if (enemy != null)
        {
            originalMoveSpeed = enemy.BaseMoveSpeed;
        }

        hasCachedOriginals = true;
    }

    private void Update()
    {
        if (activeEffects.Count == 0) return;

        // Tick all active effects
        foreach (var kvp in activeEffects)
        {
            kvp.Value.Tick(Time.deltaTime);

            if (kvp.Value.IsExpired)
            {
                effectsToRemove.Add(kvp.Key);
            }
        }

        // Remove expired effects
        foreach (var effectType in effectsToRemove)
        {
            RemoveEffect(effectType);
        }
        effectsToRemove.Clear();

        // Update debug list
        UpdateDebugList();
    }

    /// <summary>
    /// Apply a status effect. Handles refresh/stack/extend based on effect's ApplicationMode.
    /// </summary>
    public void ApplyEffect(IStatusEffect effect)
    {
        if (effect == null) return;

        CacheOriginalValues();

        if (activeEffects.TryGetValue(effect.Type, out IStatusEffect existingEffect))
        {
            // Effect already active - reapply (handles refresh/stack/extend internally)
            existingEffect.Reapply(effect.Duration, GetEffectStrength(effect));
            OnEffectStackChanged?.Invoke(effect.Type, existingEffect.CurrentStacks);
        }
        else
        {
            // New effect
            activeEffects[effect.Type] = effect;
            effect.Apply(this);
            OnEffectApplied?.Invoke(effect.Type);
        }

        UpdateDebugList();
    }

    /// <summary>
    /// Apply effect using parameters (convenience method)
    /// </summary>
    public void ApplyEffect(StatusEffectType type, float duration, float strength)
    {
        IStatusEffect effect = CreateEffect(type, duration, strength);
        if (effect != null)
        {
            ApplyEffect(effect);
        }
    }

    /// <summary>
    /// Remove a specific effect type
    /// </summary>
    public void RemoveEffect(StatusEffectType type)
    {
        if (activeEffects.TryGetValue(type, out IStatusEffect effect))
        {
            effect.Remove();
            activeEffects.Remove(type);
            OnEffectRemoved?.Invoke(type);
        }

        UpdateDebugList();
    }

    /// <summary>
    /// Remove all active effects
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var kvp in activeEffects)
        {
            kvp.Value.Remove();
        }
        activeEffects.Clear();
        UpdateDebugList();
    }

    /// <summary>
    /// Check if a specific effect is active
    /// </summary>
    public bool HasEffect(StatusEffectType type)
    {
        return activeEffects.ContainsKey(type);
    }

    /// <summary>
    /// Get remaining time of an effect
    /// </summary>
    public float GetEffectRemainingTime(StatusEffectType type)
    {
        if (activeEffects.TryGetValue(type, out IStatusEffect effect))
        {
            return effect.RemainingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Get current stacks of an effect
    /// </summary>
    public int GetEffectStacks(StatusEffectType type)
    {
        if (activeEffects.TryGetValue(type, out IStatusEffect effect))
        {
            return effect.CurrentStacks;
        }
        return 0;
    }

    /// <summary>
    /// Factory method to create effects
    /// </summary>
    private IStatusEffect CreateEffect(StatusEffectType type, float duration, float strength)
    {
        switch (type)
        {
            case StatusEffectType.Burn:
                return new BurnEffect(duration, strength);
            case StatusEffectType.Slow:
                return new SlowEffect(duration, strength);
            case StatusEffectType.Shock:
                return new ShockEffect(duration, strength);
            default:
                return null;
        }
    }

    /// <summary>
    /// Extract strength value from effect (effect-type specific)
    /// </summary>
    private float GetEffectStrength(IStatusEffect effect)
    {
        // This is a bit hacky but necessary since strength meaning varies per effect
        switch (effect)
        {
            case BurnEffect burn:
                return burn.DamagePerSecond;
            case SlowEffect slow:
                return slow.SlowPercent;
            case ShockEffect shock:
                return shock.StunDuration;
            default:
                return 0f;
        }
    }

    private void UpdateDebugList()
    {
        activeEffectNames.Clear();
        foreach (var kvp in activeEffects)
        {
            string stackInfo = kvp.Value.MaxStacks > 1 ? $" [{kvp.Value.CurrentStacks}/{kvp.Value.MaxStacks}]" : "";
            activeEffectNames.Add($"{kvp.Key}: {kvp.Value.RemainingTime:F1}s{stackInfo}");
        }
    }

    private void OnDestroy()
    {
        ClearAllEffects();
    }
}
