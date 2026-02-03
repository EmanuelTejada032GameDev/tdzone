/// <summary>
/// Interface for all status effects that can be applied to enemies
/// </summary>
public interface IStatusEffect
{
    StatusEffectType Type { get; }
    EffectApplicationMode ApplicationMode { get; }

    float Duration { get; }
    float RemainingTime { get; }
    bool IsExpired { get; }

    // Stack support
    int CurrentStacks { get; }
    int MaxStacks { get; }
    bool CanAddStack { get; }

    /// <summary>
    /// Called when effect is first applied
    /// </summary>
    void Apply(StatusEffectManager target);

    /// <summary>
    /// Called every frame while effect is active
    /// </summary>
    void Tick(float deltaTime);

    /// <summary>
    /// Called when effect expires or is removed
    /// </summary>
    void Remove();

    /// <summary>
    /// Called when the same effect type is applied again.
    /// Handles refresh/stack/extend based on ApplicationMode.
    /// </summary>
    void Reapply(float newDuration, float newStrength);
}
