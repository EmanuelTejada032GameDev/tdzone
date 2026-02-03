using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TowerDatabase", menuName = "TD Zone/Tower Database")]
public class TowerDatabaseSO : ScriptableObject
{
    [Header("Base Tower (Always Active)")]
    [Tooltip("The normal tower that is always available")]
    public TowerDataSO baseTower;

    [Header("Unlockable Towers")]
    [Tooltip("All towers that can be unlocked as abilities")]
    public TowerDataSO[] unlockableTowers;

    private Dictionary<string, TowerDataSO> _towerLookup;
    private Dictionary<TowerType, TowerDataSO> _typeLookup;

    /// <summary>
    /// Initialize lookup dictionaries. Call this on game start.
    /// </summary>
    public void Initialize()
    {
        _towerLookup = new Dictionary<string, TowerDataSO>();
        _typeLookup = new Dictionary<TowerType, TowerDataSO>();

        // Add base tower
        if (baseTower != null)
        {
            RegisterTower(baseTower);
        }

        // Add unlockable towers
        if (unlockableTowers != null)
        {
            foreach (var tower in unlockableTowers)
            {
                if (tower != null)
                {
                    RegisterTower(tower);
                }
            }
        }
    }

    private void RegisterTower(TowerDataSO tower)
    {
        // Register by ID
        if (!string.IsNullOrEmpty(tower.towerId))
        {
            _towerLookup[tower.towerId] = tower;
        }

        // Register base level by type (only level 1)
        if (tower.upgradeLevel == 1 && !_typeLookup.ContainsKey(tower.towerType))
        {
            _typeLookup[tower.towerType] = tower;
        }

        // Register all upgrade levels by ID
        TowerDataSO upgrade = tower.nextLevel;
        while (upgrade != null)
        {
            if (!string.IsNullOrEmpty(upgrade.towerId))
            {
                _towerLookup[upgrade.towerId] = upgrade;
            }
            upgrade = upgrade.nextLevel;
        }
    }

    /// <summary>
    /// Get tower by its unique ID
    /// </summary>
    public TowerDataSO GetTowerById(string towerId)
    {
        if (_towerLookup == null)
        {
            Initialize();
        }

        _towerLookup.TryGetValue(towerId, out TowerDataSO tower);
        return tower;
    }

    /// <summary>
    /// Get base level tower by type
    /// </summary>
    public TowerDataSO GetTowerByType(TowerType type)
    {
        if (_typeLookup == null)
        {
            Initialize();
        }

        _typeLookup.TryGetValue(type, out TowerDataSO tower);
        return tower;
    }

    /// <summary>
    /// Get specific level of a tower type
    /// </summary>
    public TowerDataSO GetTowerAtLevel(TowerType type, int level)
    {
        TowerDataSO tower = GetTowerByType(type);
        if (tower == null) return null;

        while (tower != null && tower.upgradeLevel < level)
        {
            tower = tower.nextLevel;
        }

        return tower?.upgradeLevel == level ? tower : null;
    }

    /// <summary>
    /// Get all towers of a specific type (all levels)
    /// </summary>
    public List<TowerDataSO> GetAllLevelsOfType(TowerType type)
    {
        List<TowerDataSO> levels = new List<TowerDataSO>();
        TowerDataSO tower = GetTowerByType(type);

        while (tower != null)
        {
            levels.Add(tower);
            tower = tower.nextLevel;
        }

        return levels;
    }
}
