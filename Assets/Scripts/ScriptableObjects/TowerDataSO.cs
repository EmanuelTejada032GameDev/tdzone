using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "TD Zone/Tower Data")]
public class TowerDataSO : ScriptableObject
{
    [Header("Identity")]
    public string towerId;
    public string towerName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public TowerType towerType;

    [Header("Base Stats")]
    public float damage = 10f;
    public float fireRate = 1f;
    public float range = 15f;
    public float detectionRange = 20f;
    public int cannonCount = 1;

    [Header("Visuals")]
    [Tooltip("Prefab to swap when this tower is active. If null, uses default tower visual.")]
    public GameObject towerVisualPrefab;

    [Header("Firing")]
    public FiringMode firingMode = FiringMode.Projectile;

    [Header("Projectile Mode")]
    [Tooltip("Used when firingMode = Projectile")]
    public ProjectileDataSO projectileData;

    [Header("Continuous Mode")]
    [Tooltip("Used when firingMode = Continuous (e.g. FlameThrower-Red.prefab)")]
    public GameObject continuousEffectPrefab;
    public float continuousDamagePerSecond = 5f;
    public float continuousRange = 8f;
    public float continuousConeAngle = 30f;
    public StatusEffectType continuousStatusEffect = StatusEffectType.None;
    public float continuousEffectDuration = 2f;
    public float continuousEffectStrength = 1f;

    [Header("Ability Settings (for unlockable towers)")]
    [Tooltip("How long the ability lasts when activated")]
    public float abilityDuration = 10f;
    [Tooltip("Cooldown before ability can be used again")]
    public float abilityCooldown = 30f;

    [Header("Upgrade Path")]
    [Tooltip("Next level of this tower. Null if max level.")]
    public TowerDataSO nextLevel;
    public int upgradeLevel = 1;
    [Tooltip("Currency cost to upgrade to this level")]
    public int upgradeCost = 0;

    [Header("Unlock Requirements")]
    public int unlockCost = 0;

    /// <summary>
    /// Returns true if this tower is the base (Normal) tower
    /// </summary>
    public bool IsBaseTower => towerType == TowerType.Normal;

    /// <summary>
    /// Returns true if this tower can be upgraded further
    /// </summary>
    public bool CanUpgrade => nextLevel != null;

    /// <summary>
    /// Gets the max level in this tower's upgrade chain
    /// </summary>
    public int GetMaxLevel()
    {
        int maxLevel = upgradeLevel;
        TowerDataSO current = nextLevel;
        while (current != null)
        {
            maxLevel = current.upgradeLevel;
            current = current.nextLevel;
        }
        return maxLevel;
    }
}
