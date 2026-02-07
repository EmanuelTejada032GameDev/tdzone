using UnityEngine;
using System;

/// <summary>
/// Manages persistent player progression across game sessions.
/// Tracks currency, unlocks, and skill purchases.
/// </summary>
public class PlayerProgressionManager : MonoBehaviour
{
    public static PlayerProgressionManager Instance { get; private set; }

    [Header("Run End Bonus")]
    [SerializeField] private float victoryBonusMultiplier = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private PlayerProgressionData progressionData;

    // Events
    public event EventHandler<int> OnCurrencyChanged;
    public event EventHandler<string> OnTowerUnlocked;
    public event EventHandler<TowerLevelUpEventArgs> OnTowerLeveledUp;
    public event EventHandler<SkillPurchasedEventArgs> OnSkillPurchased;
    public event EventHandler<RunEndEventArgs> OnRunEnd;

    // Properties
    public int Currency => progressionData?.currency ?? 0;
    public PlayerProgressionData Data => progressionData;

    private void Awake()
    {
        // Singleton with persistence
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize data
        LoadOrCreateData();
    }

    private void Start()
    {
        SubscribeToGameEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }

    private void LoadOrCreateData()
    {
        // TODO: Load from SaveManager when implemented
        // For now, create fresh data or use existing (for testing in inspector)
        if (progressionData == null)
        {
            progressionData = PlayerProgressionData.CreateDefault();
        }

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] Initialized with {progressionData.currency} currency");
        }
    }

    private void SubscribeToGameEvents()
    {
        // Subscribe to run end events only
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += OnGameOver;
        }
        WaveSpawner.OnAllWavesCompleted += OnRunVictory;
    }

    private void UnsubscribeFromGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameOver;
        }
        WaveSpawner.OnAllWavesCompleted -= OnRunVictory;
    }

    #region Currency

    /// <summary>
    /// Add currency to player's persistent balance
    /// </summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;

        progressionData.currency += amount;
        OnCurrencyChanged?.Invoke(this, progressionData.currency);

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] +{amount} currency. Total: {progressionData.currency}");
        }
    }

    /// <summary>
    /// Attempt to spend currency. Returns true if successful.
    /// </summary>
    public bool SpendCurrency(int amount)
    {
        if (amount <= 0) return false;
        if (progressionData.currency < amount) return false;

        progressionData.currency -= amount;
        OnCurrencyChanged?.Invoke(this, progressionData.currency);

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] -{amount} currency. Remaining: {progressionData.currency}");
        }

        return true;
    }

    /// <summary>
    /// Check if player can afford an amount
    /// </summary>
    public bool CanAfford(int amount)
    {
        return progressionData.currency >= amount;
    }

    #endregion

    #region Tower Unlocks

    /// <summary>
    /// Check if a tower is unlocked
    /// </summary>
    public bool IsTowerUnlocked(string towerId)
    {
        return progressionData.IsTowerUnlocked(towerId);
    }

    /// <summary>
    /// Unlock a tower (does not handle currency - use UnlockManager for that)
    /// </summary>
    public void UnlockTower(string towerId)
    {
        if (progressionData.IsTowerUnlocked(towerId)) return;

        progressionData.unlockedTowerIds.Add(towerId);
        progressionData.towerLevels[towerId] = 1;

        OnTowerUnlocked?.Invoke(this, towerId);

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] Tower unlocked: {towerId}");
        }
    }

    /// <summary>
    /// Get the current level of a tower
    /// </summary>
    public int GetTowerLevel(string towerId)
    {
        return progressionData.GetTowerLevel(towerId);
    }

    /// <summary>
    /// Upgrade a tower's level (does not handle currency - use UnlockManager for that)
    /// </summary>
    public void UpgradeTowerLevel(string towerId)
    {
        if (!IsTowerUnlocked(towerId)) return;

        int currentLevel = GetTowerLevel(towerId);
        int newLevel = currentLevel + 1;
        progressionData.towerLevels[towerId] = newLevel;

        OnTowerLeveledUp?.Invoke(this, new TowerLevelUpEventArgs
        {
            TowerId = towerId,
            OldLevel = currentLevel,
            NewLevel = newLevel
        });

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] Tower {towerId} upgraded to level {newLevel}");
        }
    }

    #endregion

    #region Skills

    /// <summary>
    /// Get how many times a skill has been purchased
    /// </summary>
    public int GetSkillPurchaseCount(string skillId)
    {
        return progressionData.GetSkillPurchaseCount(skillId);
    }

    /// <summary>
    /// Purchase a skill (increments purchase count)
    /// Does not handle currency - use SkillTreeManager for that
    /// </summary>
    public void PurchaseSkill(string skillId)
    {
        int currentCount = GetSkillPurchaseCount(skillId);
        int newCount = currentCount + 1;
        progressionData.skillPurchases[skillId] = newCount;

        OnSkillPurchased?.Invoke(this, new SkillPurchasedEventArgs
        {
            SkillId = skillId,
            PurchaseCount = newCount
        });

        if (debugMode)
        {
            Debug.Log($"[PlayerProgression] Skill {skillId} purchased ({newCount} times total)");
        }
    }

    #endregion

    #region Run End Handlers

    /// <summary>
    /// Called when player dies (tower destroyed) - collect earned gold
    /// </summary>
    private void OnGameOver(object sender, EventArgs e)
    {
        CollectRunGold(isVictory: false);
    }

    /// <summary>
    /// Called when all waves completed - collect earned gold with bonus
    /// </summary>
    private void OnRunVictory(object sender, EventArgs e)
    {
        CollectRunGold(isVictory: true);
    }

    /// <summary>
    /// Collect gold earned during the run and add to persistent currency
    /// </summary>
    private void CollectRunGold(bool isVictory)
    {
        if (GameManager.Instance == null) return;

        int earnedGold = GameManager.Instance.PlayerGold;

        if (isVictory)
        {
            // Apply victory bonus
            earnedGold = Mathf.RoundToInt(earnedGold * victoryBonusMultiplier);
        }

        if (earnedGold > 0)
        {
            AddCurrency(earnedGold);
        }

        // Fire run end event for UI
        OnRunEnd?.Invoke(this, new RunEndEventArgs
        {
            IsVictory = isVictory,
            GoldEarned = earnedGold,
            TotalCurrency = progressionData.currency
        });

        if (debugMode)
        {
            string resultType = isVictory ? "VICTORY" : "DEFEAT";
            Debug.Log($"[PlayerProgression] Run ended ({resultType})! Collected {earnedGold} currency. Total: {progressionData.currency}");
        }
    }

    #endregion

    #region Save/Load (placeholder for SaveManager)

    /// <summary>
    /// Set progression data (used by SaveManager)
    /// </summary>
    public void SetData(PlayerProgressionData data)
    {
        progressionData = data ?? PlayerProgressionData.CreateDefault();
        OnCurrencyChanged?.Invoke(this, progressionData.currency);
    }

    /// <summary>
    /// Get a copy of the current progression data (used by SaveManager)
    /// </summary>
    public PlayerProgressionData GetDataCopy()
    {
        // Return the data reference for now
        // TODO: Return a deep copy when save system is implemented
        return progressionData;
    }

    /// <summary>
    /// Reset all progression (for testing or new game)
    /// </summary>
    public void ResetProgression()
    {
        progressionData = PlayerProgressionData.CreateDefault();
        OnCurrencyChanged?.Invoke(this, 0);

        if (debugMode)
        {
            Debug.Log("[PlayerProgression] Progression reset!");
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Debug: Add 100 Currency")]
    private void DebugAdd100Currency()
    {
        AddCurrency(100);
    }

    [ContextMenu("Debug: Add 1000 Currency")]
    private void DebugAdd1000Currency()
    {
        AddCurrency(1000);
    }

    [ContextMenu("Debug: Log Status")]
    private void DebugLogStatus()
    {
        Debug.Log($"[PlayerProgression] Currency: {progressionData.currency}");
        Debug.Log($"[PlayerProgression] Unlocked Towers: {string.Join(", ", progressionData.unlockedTowerIds)}");
        Debug.Log($"[PlayerProgression] Tower Levels: {progressionData.towerLevels.Count}");
        Debug.Log($"[PlayerProgression] Skills Purchased: {progressionData.skillPurchases.Count}");
    }

    #endregion
}

public class TowerLevelUpEventArgs : EventArgs
{
    public string TowerId;
    public int OldLevel;
    public int NewLevel;
}

public class SkillPurchasedEventArgs : EventArgs
{
    public string SkillId;
    public int PurchaseCount;
}

public class RunEndEventArgs : EventArgs
{
    public bool IsVictory;
    public int GoldEarned;
    public int TotalCurrency;
}
