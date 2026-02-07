using UnityEngine;
using System;

/// <summary>
/// Manages tower unlocks and upgrades.
/// Handles cost validation and currency spending via PlayerProgressionManager.
/// </summary>
public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TowerDatabaseSO towerDatabase;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Events
    public event EventHandler<TowerUnlockEventArgs> OnTowerUnlockAttempt;
    public event EventHandler<TowerUpgradeEventArgs> OnTowerUpgradeAttempt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize tower database
        if (towerDatabase != null)
        {
            towerDatabase.Initialize();
        }
        else
        {
            Debug.LogError("[UnlockManager] TowerDatabase not assigned!");
        }
    }

    #region Tower Unlock

    /// <summary>
    /// Get the unlock cost for a tower
    /// </summary>
    public int GetUnlockCost(string towerId)
    {
        TowerDataSO tower = towerDatabase?.GetTowerById(towerId);
        return tower?.unlockCost ?? 0;
    }

    /// <summary>
    /// Check if player can afford to unlock a tower
    /// </summary>
    public bool CanAffordUnlock(string towerId)
    {
        int cost = GetUnlockCost(towerId);
        return PlayerProgressionManager.Instance?.CanAfford(cost) ?? false;
    }

    /// <summary>
    /// Check if a tower is already unlocked
    /// </summary>
    public bool IsTowerUnlocked(string towerId)
    {
        return PlayerProgressionManager.Instance?.IsTowerUnlocked(towerId) ?? false;
    }

    /// <summary>
    /// Attempt to unlock a tower. Returns true if successful.
    /// </summary>
    public bool TryUnlockTower(string towerId)
    {
        if (PlayerProgressionManager.Instance == null)
        {
            Debug.LogError("[UnlockManager] PlayerProgressionManager not found!");
            return false;
        }

        TowerDataSO tower = towerDatabase?.GetTowerById(towerId);
        if (tower == null)
        {
            Debug.LogWarning($"[UnlockManager] Tower not found: {towerId}");
            FireUnlockAttempt(towerId, false, UnlockFailReason.TowerNotFound);
            return false;
        }

        // Check if already unlocked
        if (IsTowerUnlocked(towerId))
        {
            if (debugMode) Debug.Log($"[UnlockManager] Tower already unlocked: {towerId}");
            FireUnlockAttempt(towerId, false, UnlockFailReason.AlreadyUnlocked);
            return false;
        }

        // Check if can afford
        int cost = tower.unlockCost;
        if (!PlayerProgressionManager.Instance.CanAfford(cost))
        {
            if (debugMode) Debug.Log($"[UnlockManager] Cannot afford tower {towerId}. Cost: {cost}, Have: {PlayerProgressionManager.Instance.Currency}");
            FireUnlockAttempt(towerId, false, UnlockFailReason.InsufficientCurrency);
            return false;
        }

        // Spend currency and unlock
        if (PlayerProgressionManager.Instance.SpendCurrency(cost))
        {
            PlayerProgressionManager.Instance.UnlockTower(towerId);

            if (debugMode) Debug.Log($"[UnlockManager] Tower unlocked: {towerId} for {cost} currency");
            FireUnlockAttempt(towerId, true, UnlockFailReason.None);
            return true;
        }

        return false;
    }

    private void FireUnlockAttempt(string towerId, bool success, UnlockFailReason reason)
    {
        OnTowerUnlockAttempt?.Invoke(this, new TowerUnlockEventArgs
        {
            TowerId = towerId,
            Success = success,
            FailReason = reason
        });
    }

    #endregion

    #region Tower Upgrade

    /// <summary>
    /// Get the current level of an unlocked tower
    /// </summary>
    public int GetTowerLevel(string towerId)
    {
        return PlayerProgressionManager.Instance?.GetTowerLevel(towerId) ?? 0;
    }

    /// <summary>
    /// Get the TowerDataSO for a specific tower at its current unlocked level
    /// </summary>
    public TowerDataSO GetTowerDataAtCurrentLevel(string towerId)
    {
        if (!IsTowerUnlocked(towerId)) return null;

        TowerDataSO baseTower = towerDatabase?.GetTowerById(towerId);
        if (baseTower == null) return null;

        int currentLevel = GetTowerLevel(towerId);
        return towerDatabase.GetTowerAtLevel(baseTower.towerType, currentLevel);
    }

    /// <summary>
    /// Get the next level TowerDataSO for an unlocked tower
    /// </summary>
    public TowerDataSO GetNextLevelData(string towerId)
    {
        TowerDataSO current = GetTowerDataAtCurrentLevel(towerId);
        return current?.nextLevel;
    }

    /// <summary>
    /// Get the cost to upgrade a tower to its next level
    /// </summary>
    public int GetUpgradeCost(string towerId)
    {
        TowerDataSO nextLevel = GetNextLevelData(towerId);
        return nextLevel?.upgradeCost ?? 0;
    }

    /// <summary>
    /// Check if a tower can be upgraded (has next level)
    /// </summary>
    public bool CanUpgrade(string towerId)
    {
        return GetNextLevelData(towerId) != null;
    }

    /// <summary>
    /// Check if player can afford the next upgrade for a tower
    /// </summary>
    public bool CanAffordUpgrade(string towerId)
    {
        int cost = GetUpgradeCost(towerId);
        if (cost <= 0) return false;
        return PlayerProgressionManager.Instance?.CanAfford(cost) ?? false;
    }

    /// <summary>
    /// Attempt to upgrade a tower to its next level. Returns true if successful.
    /// </summary>
    public bool TryUpgradeTower(string towerId)
    {
        if (PlayerProgressionManager.Instance == null)
        {
            Debug.LogError("[UnlockManager] PlayerProgressionManager not found!");
            return false;
        }

        // Check if unlocked
        if (!IsTowerUnlocked(towerId))
        {
            if (debugMode) Debug.Log($"[UnlockManager] Cannot upgrade - tower not unlocked: {towerId}");
            FireUpgradeAttempt(towerId, false, UpgradeFailReason.NotUnlocked);
            return false;
        }

        // Check if can upgrade
        TowerDataSO nextLevel = GetNextLevelData(towerId);
        if (nextLevel == null)
        {
            if (debugMode) Debug.Log($"[UnlockManager] Cannot upgrade - already max level: {towerId}");
            FireUpgradeAttempt(towerId, false, UpgradeFailReason.MaxLevel);
            return false;
        }

        // Check if can afford
        int cost = nextLevel.upgradeCost;
        if (!PlayerProgressionManager.Instance.CanAfford(cost))
        {
            if (debugMode) Debug.Log($"[UnlockManager] Cannot afford upgrade for {towerId}. Cost: {cost}, Have: {PlayerProgressionManager.Instance.Currency}");
            FireUpgradeAttempt(towerId, false, UpgradeFailReason.InsufficientCurrency);
            return false;
        }

        // Spend currency and upgrade
        if (PlayerProgressionManager.Instance.SpendCurrency(cost))
        {
            PlayerProgressionManager.Instance.UpgradeTowerLevel(towerId);

            if (debugMode) Debug.Log($"[UnlockManager] Tower upgraded: {towerId} to level {GetTowerLevel(towerId)} for {cost} currency");
            FireUpgradeAttempt(towerId, true, UpgradeFailReason.None);
            return true;
        }

        return false;
    }

    private void FireUpgradeAttempt(string towerId, bool success, UpgradeFailReason reason)
    {
        OnTowerUpgradeAttempt?.Invoke(this, new TowerUpgradeEventArgs
        {
            TowerId = towerId,
            Success = success,
            FailReason = reason,
            NewLevel = GetTowerLevel(towerId)
        });
    }

    #endregion

    #region Queries

    /// <summary>
    /// Get all unlockable towers from the database
    /// </summary>
    public TowerDataSO[] GetAllUnlockableTowers()
    {
        return towerDatabase?.unlockableTowers ?? new TowerDataSO[0];
    }

    /// <summary>
    /// Get tower data by ID
    /// </summary>
    public TowerDataSO GetTowerData(string towerId)
    {
        return towerDatabase?.GetTowerById(towerId);
    }

    #endregion

    #region Debug

    [ContextMenu("Debug: List All Towers")]
    private void DebugListAllTowers()
    {
        if (towerDatabase == null)
        {
            Debug.Log("[UnlockManager] No tower database assigned");
            return;
        }

        Debug.Log($"[UnlockManager] Base Tower: {towerDatabase.baseTower?.towerId}");

        foreach (var tower in towerDatabase.unlockableTowers)
        {
            bool unlocked = IsTowerUnlocked(tower.towerId);
            int level = GetTowerLevel(tower.towerId);
            Debug.Log($"[UnlockManager] {tower.towerId}: Unlocked={unlocked}, Level={level}, UnlockCost={tower.unlockCost}");
        }
    }

    [ContextMenu("Debug: Unlock First Tower (Free)")]
    private void DebugUnlockFirstTowerFree()
    {
        if (towerDatabase == null || towerDatabase.unlockableTowers.Length == 0)
        {
            Debug.Log("[UnlockManager] No towers to unlock");
            return;
        }

        var tower = towerDatabase.unlockableTowers[0];
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.UnlockTower(tower.towerId);
            Debug.Log($"[UnlockManager] DEBUG: Force unlocked {tower.towerId} (free)");
        }
    }

    [ContextMenu("Debug: Unlock All Towers (Free)")]
    private void DebugUnlockAllTowersFree()
    {
        if (towerDatabase == null) return;

        foreach (var tower in towerDatabase.unlockableTowers)
        {
            if (PlayerProgressionManager.Instance != null && !IsTowerUnlocked(tower.towerId))
            {
                PlayerProgressionManager.Instance.UnlockTower(tower.towerId);
                Debug.Log($"[UnlockManager] DEBUG: Force unlocked {tower.towerId} (free)");
            }
        }
    }

    [ContextMenu("Debug: Try Unlock First Tower (With Cost)")]
    private void DebugTryUnlockFirstTower()
    {
        if (towerDatabase == null || towerDatabase.unlockableTowers.Length == 0)
        {
            Debug.Log("[UnlockManager] No towers to unlock");
            return;
        }

        var tower = towerDatabase.unlockableTowers[0];
        bool success = TryUnlockTower(tower.towerId);
        Debug.Log($"[UnlockManager] DEBUG: TryUnlockTower({tower.towerId}) = {success}");
    }

    #endregion
}

#region Event Args

public class TowerUnlockEventArgs : EventArgs
{
    public string TowerId;
    public bool Success;
    public UnlockFailReason FailReason;
}

public class TowerUpgradeEventArgs : EventArgs
{
    public string TowerId;
    public bool Success;
    public UpgradeFailReason FailReason;
    public int NewLevel;
}

public enum UnlockFailReason
{
    None,
    TowerNotFound,
    AlreadyUnlocked,
    InsufficientCurrency
}

public enum UpgradeFailReason
{
    None,
    NotUnlocked,
    MaxLevel,
    InsufficientCurrency
}

#endregion
