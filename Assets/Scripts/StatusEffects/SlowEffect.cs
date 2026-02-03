using UnityEngine;

/// <summary>
/// Slow effect - reduces enemy movement speed. REFRESH mode.
/// Takes the strongest slow, refreshes duration.
/// </summary>
public class SlowEffect : IStatusEffect
{
    public StatusEffectType Type => StatusEffectType.Slow;
    public EffectApplicationMode ApplicationMode => EffectApplicationMode.Refresh;

    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsExpired => RemainingTime <= 0f;

    // No stacking for slow
    public int CurrentStacks => 1;
    public int MaxStacks => 1;
    public bool CanAddStack => false;

    private float slowPercent;
    private StatusEffectManager target;

    /// <summary>
    /// Current slow percentage (for StatusEffectManager to read)
    /// </summary>
    public float SlowPercent => slowPercent;

    /// <summary>
    /// Create slow effect
    /// </summary>
    /// <param name="duration">How long the slow lasts</param>
    /// <param name="slowPercent">Slow amount (0.3 = 30% slow, enemy moves at 70% speed)</param>
    public SlowEffect(float duration, float slowPercent)
    {
        Duration = duration;
        RemainingTime = duration;
        this.slowPercent = Mathf.Clamp01(slowPercent);
    }

    public void Apply(StatusEffectManager target)
    {
        this.target = target;
        ApplySlowToEnemy();
    }

    public void Tick(float deltaTime)
    {
        RemainingTime -= deltaTime;
    }

    public void Remove()
    {
        // Restore original speed (unless shocked)
        if (target?.Enemy != null && !target.HasEffect(StatusEffectType.Shock))
        {
            target.Enemy.SetMoveSpeed(target.OriginalMoveSpeed);
        }
        target = null;
    }

    public void Reapply(float newDuration, float newStrength)
    {
        // Refresh mode: reset duration, take strongest slow
        if (newDuration > RemainingTime)
        {
            RemainingTime = newDuration;
            Duration = newDuration;
        }

        // Apply stronger slow if provided
        float newSlowPercent = Mathf.Clamp01(newStrength);
        if (newSlowPercent > slowPercent)
        {
            slowPercent = newSlowPercent;
            ApplySlowToEnemy();
        }
    }

    private void ApplySlowToEnemy()
    {
        if (target?.Enemy != null)
        {
            float newSpeed = target.OriginalMoveSpeed * (1f - slowPercent);
            target.Enemy.SetMoveSpeed(newSpeed);
        }
    }
}
