using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "TD Zone/Projectile Data")]
public class ProjectileDataSO : ScriptableObject
{
    [Header("Identity")]
    public string projectileId;

    [Header("Prefab")]
    [Tooltip("The projectile prefab to instantiate (must have Projectile.cs)")]
    public GameObject projectilePrefab;

    [Header("Movement")]
    public Projectile.ProjectileType projectileType = Projectile.ProjectileType.Straight;
    public float speed = 20f;
    public float lifetime = 5f;

    [Header("Homing")]
    public float homingStrength = 5f;
    public float homingDelay = 0.1f;

    [Header("Ballistic")]
    public float arcHeight = 2f;

    [Header("VFX")]
    [Tooltip("Particle effect spawned at fire point on shoot")]
    public GameObject muzzleFlashPrefab;
    [Tooltip("Particle effect spawned on hit")]
    public GameObject impactEffectPrefab;
    [Tooltip("Trail prefab spawned as child of projectile")]
    public GameObject trailPrefab;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip impactSound;

    [Header("Status Effect")]
    public StatusEffectType statusEffect = StatusEffectType.None;
    public float effectDuration = 2f;
    public float effectStrength = 1f;
}
