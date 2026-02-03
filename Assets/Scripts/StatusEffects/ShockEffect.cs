using UnityEngine;

/// <summary>
/// Shock effect - briefly stuns enemy. REFRESH mode.
/// Re-applying refreshes the stun.
/// </summary>
public class ShockEffect : IStatusEffect
{
    public StatusEffectType Type => StatusEffectType.Shock;
    public EffectApplicationMode ApplicationMode => EffectApplicationMode.Refresh;

    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsExpired => RemainingTime <= 0f;

    // No stacking for shock
    public int CurrentStacks => 1;
    public int MaxStacks => 1;
    public bool CanAddStack => false;

    private float stunDuration;
    private float stunTimer;
    private bool isStunned;
    private StatusEffectManager target;

    /// <summary>
    /// Stun duration value (for StatusEffectManager to read)
    /// </summary>
    public float StunDuration => stunDuration;

    /// <summary>
    /// Create shock effect
    /// </summary>
    /// <param name="duration">Total effect duration (for tracking/visuals)</param>
    /// <param name="stunDuration">Actual stun time</param>
    public ShockEffect(float duration, float stunDuration)
    {
        Duration = duration;
        RemainingTime = duration;
        this.stunDuration = stunDuration;
        this.stunTimer = stunDuration;
        this.isStunned = true;
    }

    public void Apply(StatusEffectManager target)
    {
        this.target = target;
        ApplyStun();
    }

    public void Tick(float deltaTime)
    {
        RemainingTime -= deltaTime;

        if (isStunned)
        {
            stunTimer -= deltaTime;

            if (stunTimer <= 0f)
            {
                // Stun ended, restore movement
                isStunned = false;
                RestoreMovement();
            }
        }
    }

    public void Remove()
    {
        if (isStunned)
        {
            RestoreMovement();
        }
        target = null;
    }

    public void Reapply(float newDuration, float newStrength)
    {
        // Refresh mode: reset stun timer
        if (newDuration > RemainingTime)
        {
            RemainingTime = newDuration;
            Duration = newDuration;
        }

        // Re-apply stun
        if (newStrength > 0)
        {
            stunDuration = newStrength;
        }
        stunTimer = stunDuration;
        isStunned = true;
        ApplyStun();
    }

    private void ApplyStun()
    {
        if (target?.Enemy != null)
        {
            target.Enemy.SetMoveSpeed(0f);
        }
    }

    private void RestoreMovement()
    {
        if (target?.Enemy != null)
        {
            // Check if slow effect is active - if so, apply slow speed instead of full speed
            if (target.HasEffect(StatusEffectType.Slow))
            {
                // Let slow effect handle the speed
                // We just don't restore to full speed
            }
            else
            {
                target.Enemy.SetMoveSpeed(target.OriginalMoveSpeed);
            }
        }
    }
}
