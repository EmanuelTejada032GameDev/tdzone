using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages tower abilities - handles activation, cooldowns, and coordinates visual/stat changes.
/// Attach this to the main Tower GameObject.
/// </summary>
public class TowerAbilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerDatabaseSO towerDatabase;
    [SerializeField] private TowerVisualSwapper visualSwapper;
    [SerializeField] private Tower tower;
    [SerializeField] private Cannon[] cannons;

    [Header("Base Tower")]
    [SerializeField] private TowerDataSO baseTowerData;

    [Header("Abilities")]
    [SerializeField] private List<TowerAbility> abilities = new List<TowerAbility>();

    [Header("Runtime State (Debug View)")]
    [NonSerialized] private TowerAbility activeAbility;
    [SerializeField] private TowerDataSO currentTowerData; // Keep serialized for debug viewing

    // Original cannon configurations (to restore when ability ends)
    private CannonConfiguration[] originalCannonConfigs;

    // Continuous fire mode
    private ContinuousDamageZone activeDamageZone;

    // Events
    public event Action<TowerDataSO> OnTowerTypeChanged;
    public event Action<TowerAbility> OnAbilityActivated;
    public event Action<TowerAbility> OnAbilityDeactivated;
    public event Action<TowerAbility> OnAbilityCooldownComplete;
    public event Action<TowerAbility> OnAbilityUnlocked;

    // Properties
    public TowerAbility ActiveAbility => activeAbility;
    public TowerDataSO CurrentTowerData => currentTowerData;
    public IReadOnlyList<TowerAbility> Abilities => abilities;
    public bool HasActiveAbility => activeAbility != null;

    private void Awake()
    {
        // Reset runtime state
        activeAbility = null;

        // Get tower if not assigned
        if (tower == null)
        {
            tower = GetComponent<Tower>();
        }

        // Get cannons if not assigned
        if (cannons == null || cannons.Length == 0)
        {
            cannons = GetComponentsInChildren<Cannon>();
        }

        // Get visual swapper if not assigned
        if (visualSwapper == null)
        {
            visualSwapper = GetComponent<TowerVisualSwapper>();
        }

        // Set initial tower data
        if (baseTowerData == null && towerDatabase != null)
        {
            baseTowerData = towerDatabase.baseTower;
        }
        currentTowerData = baseTowerData;

        // Initialize abilities from database if not manually configured
        // Done in Awake so abilities are ready before UI Start() methods run
        if (abilities.Count == 0 && towerDatabase != null)
        {
            InitializeAbilitiesFromDatabase();
        }
    }

    private void Start()
    {
        // Subscribe to ability events
        foreach (var ability in abilities)
        {
            SubscribeToAbilityEvents(ability);
        }

        // Subscribe to visual swapper fire point changes
        if (visualSwapper != null)
        {
            visualSwapper.OnFirePointsChanged += HandleFirePointsChanged;
        }

        // Apply base tower configuration on start
        ApplyBaseTowerConfiguration();

        // Store original cannon configurations AFTER base config is applied
        // so the backup includes the correct projectileData
        StoreOriginalCannonConfigs();
    }

    /// <summary>
    /// Apply base tower configuration to cannons
    /// </summary>
    private void ApplyBaseTowerConfiguration()
    {
        if (baseTowerData == null)
        {
            Debug.LogWarning("TowerAbilityManager: No base tower data assigned!");
            return;
        }

        currentTowerData = baseTowerData;
        ApplyTowerConfiguration(baseTowerData);

    }

    /// <summary>
    /// Handle fire points changed from visual swapper
    /// </summary>
    private void HandleFirePointsChanged(System.Collections.Generic.List<Transform> firePoints)
    {
        if (firePoints == null || firePoints.Count == 0)
        {
            Debug.LogWarning("TowerAbilityManager: No fire points received from visual swapper");
            return;
        }

        // Assign fire points to cannons
        for (int i = 0; i < cannons.Length && i < firePoints.Count; i++)
        {
            if (cannons[i] != null)
            {
                cannons[i].SetFirePoint(firePoints[i]);
            }
        }

        // Warn if mismatch
        if (cannons.Length != firePoints.Count)
        {
            Debug.LogWarning($"TowerAbilityManager: Cannon count ({cannons.Length}) != fire point count ({firePoints.Count})");
        }
    }

    private void Update()
    {
        // Update all ability timers
        foreach (var ability in abilities)
        {
            ability.Tick(Time.deltaTime);
        }

        // Check for ability activation input
        HandleAbilityInput();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        foreach (var ability in abilities)
        {
            UnsubscribeFromAbilityEvents(ability);
        }

        // Unsubscribe from visual swapper
        if (visualSwapper != null)
        {
            visualSwapper.OnFirePointsChanged -= HandleFirePointsChanged;
        }

        // Unsubscribe from progression manager
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnTowerUnlocked -= HandleTowerUnlocked;
        }
    }

    /// <summary>
    /// Initialize abilities from the tower database
    /// </summary>
    private void InitializeAbilitiesFromDatabase()
    {
        if (towerDatabase == null || towerDatabase.unlockableTowers == null)
            return;

        KeyCode[] defaultKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

        for (int i = 0; i < towerDatabase.unlockableTowers.Length; i++)
        {
            var towerData = towerDatabase.unlockableTowers[i];
            if (towerData == null) continue;

            KeyCode key = i < defaultKeys.Length ? defaultKeys[i] : KeyCode.None;
            var ability = new TowerAbility(towerData, key);

            // Start locked - check PlayerProgressionManager for unlock status
            ability.Lock();

            // Check if already unlocked in progression data
            if (PlayerProgressionManager.Instance != null &&
                PlayerProgressionManager.Instance.IsTowerUnlocked(towerData.towerId))
            {
                ability.Unlock();
            }

            abilities.Add(ability);
        }

        // Subscribe to unlock events for runtime unlocks
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnTowerUnlocked += HandleTowerUnlocked;
        }
    }

    /// <summary>
    /// Handle tower unlock from progression system
    /// </summary>
    private void HandleTowerUnlocked(object sender, string towerId)
    {
        // Find the ability for this tower and unlock it
        foreach (var ability in abilities)
        {
            if (ability.TowerData != null && ability.TowerData.towerId == towerId)
            {
                ability.Unlock();
                OnAbilityUnlocked?.Invoke(ability);
                Debug.Log($"[TowerAbilityManager] Ability unlocked via progression: {ability.TowerData.towerName}");
                break;
            }
        }
    }

    /// <summary>
    /// Handle hotkey input for ability activation
    /// </summary>
    private void HandleAbilityInput()
    {
        foreach (var ability in abilities)
        {
            if (ability.ActivationKey == KeyCode.None)
                continue;

            if (Input.GetKeyDown(ability.ActivationKey))
            {
                TryActivateAbility(ability);
                break;
            }
        }
    }

    /// <summary>
    /// Try to activate an ability
    /// </summary>
    public bool TryActivateAbility(TowerAbility ability)
    {
        if (ability == null)
            return false;

        // Can't activate if another ability is active
        if (activeAbility != null)
        {
            //Debug.Log($"Cannot activate {ability.TowerData.towerName}: another ability is active");
            return false;
        }

        // Try to activate
        if (!ability.TryActivate())
        {
            //Debug.Log($"Cannot activate {ability.TowerData.towerName}: not ready (State: {ability.State})");
            return false;
        }

        activeAbility = ability;
        ApplyTowerType(ability.TowerData);

        return true;
    }

    /// <summary>
    /// Try to activate ability by index (0-based)
    /// </summary>
    public bool TryActivateAbility(int index)
    {
        if (index < 0 || index >= abilities.Count)
            return false;

        return TryActivateAbility(abilities[index]);
    }

    /// <summary>
    /// Try to activate ability by tower type
    /// </summary>
    public bool TryActivateAbility(TowerType type)
    {
        var ability = abilities.Find(a => a.TowerData != null && a.TowerData.towerType == type);
        return TryActivateAbility(ability);
    }

    /// <summary>
    /// Cancel the currently active ability
    /// </summary>
    public void CancelActiveAbility()
    {
        if (activeAbility == null)
            return;

        activeAbility.Deactivate();
    }

    /// <summary>
    /// Apply a tower type's configuration to the tower
    /// </summary>
    private void ApplyTowerType(TowerDataSO towerData)
    {
        if (towerData == null)
            return;

        currentTowerData = towerData;

        // Apply visual change
        if (visualSwapper != null)
        {
            visualSwapper.SwapVisual(towerData);
        }

        // Apply cannon configuration
        ApplyTowerConfiguration(towerData);

        // Handle continuous fire mode (e.g. Flamethrower)
        if (towerData.firingMode == FiringMode.Continuous)
        {
            ActivateContinuousFire(towerData);
        }

        OnTowerTypeChanged?.Invoke(towerData);

    }

    /// <summary>
    /// Revert to the base tower type
    /// </summary>
    private void RevertToBaseTower()
    {
        currentTowerData = baseTowerData;

        // Revert visual
        if (visualSwapper != null)
        {
            visualSwapper.RevertToDefault();
        }

        // Deactivate continuous fire if active
        DeactivateContinuousFire();

        // Restore original cannon configuration
        RestoreOriginalCannonConfigs();

        // Restore original Tower stats (fireCooldown, maxShootingRange)
        if (tower != null)
        {
            tower.RestoreOriginalStats();
        }

        OnTowerTypeChanged?.Invoke(baseTowerData);

    }

    /// <summary>
    /// Apply tower data configuration to all cannons, routing stats through TowerStatCalculator.
    /// </summary>
    private void ApplyTowerConfiguration(TowerDataSO towerData)
    {
        int modifiedDamage = (int)TowerStatCalculator.GetModifiedDamage(towerData.damage);

        float modifiedFireRate = TowerStatCalculator.GetModifiedFireRate(towerData.fireRate);
        float modifiedRange = TowerStatCalculator.GetModifiedRange(towerData.range);
        float cooldown = 1f / modifiedFireRate;

        // Apply projectile data and cooldown to cannons
        foreach (var cannon in cannons)
        {
            if (cannon == null) continue;

            cannon.SetConfiguration(towerData.projectileData, modifiedDamage);
            cannon.SetFiringMode(towerData.firingMode);
            cannon.cooldown = cooldown;
        }

        // Apply fireRate, range, and detection to Tower
        if (tower != null)
        {
            tower.SetFireCooldown(cooldown);
            tower.SetShootingRange(modifiedRange);
            tower.SetDetectionRadius(towerData.detectionRange);
        }
    }

    /// <summary>
    /// Activate continuous damage zone on the first cannon's fire point.
    /// </summary>
    private void ActivateContinuousFire(TowerDataSO towerData)
    {
        DeactivateContinuousFire();

        // Use the first cannon's fire point
        Transform firePoint = null;
        foreach (var cannon in cannons)
        {
            if (cannon != null && cannon.firePoint != null)
            {
                firePoint = cannon.firePoint;
                break;
            }
        }

        if (firePoint == null)
        {
            Debug.LogWarning("TowerAbilityManager: No fire point found for continuous fire!");
            return;
        }

        activeDamageZone = gameObject.AddComponent<ContinuousDamageZone>();
        activeDamageZone.Activate(towerData, firePoint, tower);
    }

    /// <summary>
    /// Deactivate and remove continuous damage zone.
    /// </summary>
    private void DeactivateContinuousFire()
    {
        if (activeDamageZone != null)
        {
            activeDamageZone.Deactivate();
            Destroy(activeDamageZone);
            activeDamageZone = null;
        }
    }

    /// <summary>
    /// Store original cannon configurations for restoration
    /// </summary>
    private void StoreOriginalCannonConfigs()
    {
        originalCannonConfigs = new CannonConfiguration[cannons.Length];

        for (int i = 0; i < cannons.Length; i++)
        {
            if (cannons[i] == null) continue;
            originalCannonConfigs[i] = cannons[i].GetConfiguration();
        }
    }

    /// <summary>
    /// Restore original cannon configurations
    /// </summary>
    private void RestoreOriginalCannonConfigs()
    {
        for (int i = 0; i < cannons.Length; i++)
        {
            if (cannons[i] == null || originalCannonConfigs[i] == null) continue;
            cannons[i].ApplyConfiguration(originalCannonConfigs[i]);
        }
    }

    private void SubscribeToAbilityEvents(TowerAbility ability)
    {
        ability.OnActivated += HandleAbilityActivated;
        ability.OnDeactivated += HandleAbilityDeactivated;
        ability.OnCooldownComplete += HandleAbilityCooldownComplete;
    }

    private void UnsubscribeFromAbilityEvents(TowerAbility ability)
    {
        ability.OnActivated -= HandleAbilityActivated;
        ability.OnDeactivated -= HandleAbilityDeactivated;
        ability.OnCooldownComplete -= HandleAbilityCooldownComplete;
    }

    private void HandleAbilityActivated(TowerAbility ability)
    {
        OnAbilityActivated?.Invoke(ability);
    }

    private void HandleAbilityDeactivated(TowerAbility ability)
    {
        if (activeAbility == ability)
        {
            activeAbility = null;
            RevertToBaseTower();
        }

        OnAbilityDeactivated?.Invoke(ability);
    }

    private void HandleAbilityCooldownComplete(TowerAbility ability)
    {
        OnAbilityCooldownComplete?.Invoke(ability);
    }

    /// <summary>
    /// Get ability by tower type
    /// </summary>
    public TowerAbility GetAbility(TowerType type)
    {
        return abilities.Find(a => a.TowerData != null && a.TowerData.towerType == type);
    }

    /// <summary>
    /// Get ability by index
    /// </summary>
    public TowerAbility GetAbility(int index)
    {
        if (index < 0 || index >= abilities.Count)
            return null;

        return abilities[index];
    }

    /// <summary>
    /// Add an ability manually
    /// </summary>
    public void AddAbility(TowerDataSO towerData, KeyCode activationKey, bool startUnlocked = false)
    {
        var ability = new TowerAbility(towerData, activationKey);

        if (startUnlocked)
            ability.Unlock();
        else
            ability.Lock();

        SubscribeToAbilityEvents(ability);
        abilities.Add(ability);
    }

    /// <summary>
    /// Unlock an ability by tower type
    /// </summary>
    public void UnlockAbility(TowerType type)
    {
        var ability = GetAbility(type);
        if (ability != null)
        {
            ability.Unlock();
            Debug.Log($"Unlocked ability: {ability.TowerData.towerName}");
        }
    }
}
