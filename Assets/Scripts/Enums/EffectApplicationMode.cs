/// <summary>
/// How a status effect behaves when reapplied
/// </summary>
public enum EffectApplicationMode
{
    /// <summary>
    /// Reset duration, take strongest values
    /// </summary>
    Refresh,

    /// <summary>
    /// Add stacks up to max, each stack adds damage/strength
    /// </summary>
    Stack,

    /// <summary>
    /// Add duration to remaining time
    /// </summary>
    Extend
}
