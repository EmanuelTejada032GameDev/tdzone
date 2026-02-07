using System;
using System.Collections.Generic;

/// <summary>
/// Serializable data container for persistent player progression.
/// Used for saving/loading and runtime tracking.
/// </summary>
[Serializable]
public class PlayerProgressionData
{
    /// <summary>
    /// Persistent currency used for unlocks and skill purchases
    /// </summary>
    public int currency;

    /// <summary>
    /// List of unlocked tower IDs (e.g., "flamethrower", "sniper")
    /// </summary>
    public List<string> unlockedTowerIds = new List<string>();

    /// <summary>
    /// Tower upgrade levels. Key: towerId, Value: level (1-3)
    /// </summary>
    public SerializableDictionary<string, int> towerLevels = new SerializableDictionary<string, int>();

    /// <summary>
    /// Skill purchase counts. Key: skillId, Value: times purchased
    /// </summary>
    public SerializableDictionary<string, int> skillPurchases = new SerializableDictionary<string, int>();

    /// <summary>
    /// Creates a fresh progression data with defaults
    /// </summary>
    public static PlayerProgressionData CreateDefault()
    {
        return new PlayerProgressionData
        {
            currency = 0,
            unlockedTowerIds = new List<string>(),
            towerLevels = new SerializableDictionary<string, int>(),
            skillPurchases = new SerializableDictionary<string, int>()
        };
    }

    /// <summary>
    /// Check if a tower is unlocked
    /// </summary>
    public bool IsTowerUnlocked(string towerId)
    {
        return unlockedTowerIds.Contains(towerId);
    }

    /// <summary>
    /// Get the level of a tower (returns 0 if not unlocked)
    /// </summary>
    public int GetTowerLevel(string towerId)
    {
        if (!IsTowerUnlocked(towerId)) return 0;
        return towerLevels.TryGetValue(towerId, out int level) ? level : 1;
    }

    /// <summary>
    /// Get how many times a skill has been purchased
    /// </summary>
    public int GetSkillPurchaseCount(string skillId)
    {
        return skillPurchases.TryGetValue(skillId, out int count) ? count : 0;
    }
}

/// <summary>
/// A serializable dictionary wrapper for Unity JSON serialization.
/// Unity's JsonUtility doesn't support Dictionary, so we use lists.
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<TKey> keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();

    public TValue this[TKey key]
    {
        get
        {
            int index = keys.IndexOf(key);
            if (index < 0) throw new KeyNotFoundException($"Key '{key}' not found");
            return values[index];
        }
        set
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                values[index] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            value = values[index];
            return true;
        }
        value = default;
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return keys.Contains(key);
    }

    public void Remove(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public int Count => keys.Count;
}
