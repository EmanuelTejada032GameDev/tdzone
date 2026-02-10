using System;

/// <summary>
/// Top-level save container. Wraps PlayerProgressionData so we can
/// extend with settings or other data later without breaking saves.
/// </summary>
[Serializable]
public class SaveData
{
    public PlayerProgressionData progression;

    public static SaveData CreateDefault()
    {
        return new SaveData
        {
            progression = PlayerProgressionData.CreateDefault()
        };
    }
}
