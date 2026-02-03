using UnityEngine;

/// <summary>
/// Burn effect - deals damage over time. STACKABLE.
/// Each stack adds more damage per tick.
/// </summary>
public class BurnEffect : IStatusEffect
{
    public StatusEffectType Type => StatusEffectType.Burn;
    public EffectApplicationMode ApplicationMode => EffectApplicationMode.Stack;

    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsExpired => RemainingTime <= 0f;

    // Stack properties
    public int CurrentStacks { get; private set; }
    public int MaxStacks { get; private set; }
    public bool CanAddStack => CurrentStacks < MaxStacks;

    // Configurable
    private const float TICK_INTERVAL = 0.5f;
    private const int DEFAULT_MAX_STACKS = 5;

    private float damagePerSecondPerStack;
    private float tickTimer;
    private StatusEffectManager target;

    /// <summary>
    /// Damage per second value (for StatusEffectManager to read)
    /// </summary>
    public float DamagePerSecond => damagePerSecondPerStack;

    public BurnEffect(float duration, float damagePerSecond, int maxStacks = DEFAULT_MAX_STACKS)
    {
        Duration = duration;
        RemainingTime = duration;
        damagePerSecondPerStack = damagePerSecond;
        MaxStacks = maxStacks;
        CurrentStacks = 1;
    }

    public void Apply(StatusEffectManager target)
    {
        this.target = target;
        tickTimer = TICK_INTERVAL;
    }

    public void Tick(float deltaTime)
    {
        RemainingTime -= deltaTime;
        tickTimer -= deltaTime;

        if (tickTimer <= 0f && target?.HealthSystem != null)
        {
            // Calculate total damage based on stacks
            float totalDamagePerTick = damagePerSecondPerStack * TICK_INTERVAL * CurrentStacks;
            int damage = Mathf.CeilToInt(totalDamagePerTick);
            target.HealthSystem.TakeDamage(damage);
            tickTimer = TICK_INTERVAL;
        }
    }

    public void Remove()
    {
        target = null;
    }

    public void Reapply(float newDuration, float newStrength)
    {
        // Stack mode: add stack and refresh duration
        if (CanAddStack)
        {
            CurrentStacks++;
        }

        // Always refresh duration to the longer one
        if (newDuration > RemainingTime)
        {
            RemainingTime = newDuration;
            Duration = newDuration;
        }

        // Take stronger damage if provided
        if (newStrength > damagePerSecondPerStack)
        {
            damagePerSecondPerStack = newStrength;
        }
    }
}
