using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
    [Header("Firing")]
    public int fireOrder;
    public float fireDelay = 0f;
    public float cooldown = 1f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileDamage;
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Status Effect")]
    [SerializeField] private StatusEffectType statusEffectType = StatusEffectType.None;
    [SerializeField] private float statusEffectDuration = 2f;
    [SerializeField] private float statusEffectStrength = 1f;

    [Header("Aim")]
    public Transform firePoint;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioSource audioSource;

    private float cooldownTimer;
    private bool isFiring;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public void TryFire(Transform target)
    {
        if (cooldownTimer > 0f) return;
        if (isFiring) return;

        StartCoroutine(FireRoutine(target));
    }

    private IEnumerator FireRoutine(Transform target)
    {
        isFiring = true;

        if (fireDelay > 0f)
            yield return new WaitForSeconds(fireDelay);

        Shoot(target);

        cooldownTimer = cooldown;
        isFiring = false;
    }

    private void Shoot(Transform target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: No projectile prefab assigned!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: No fire point assigned!");
            return;
        }

        // Spawn projectile
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            // Initialize with target
            if (target != null)
            {
                projectile.Initialize(target, projectileDamage);
            }
            else
            {
                projectile.Initialize(firePoint.position + firePoint.forward * 100f, projectileDamage);
            }

            // Apply status effect if configured
            if (statusEffectType != StatusEffectType.None)
            {
                projectile.SetStatusEffect(statusEffectType, statusEffectDuration, statusEffectStrength);
            }

            // Subscribe to hit event (optional)
            projectile.OnProjectileHit += Projectile_OnProjectileHit;
        }

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Play fire sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    private void Projectile_OnProjectileHit(object sender, ProjectileHitEventArgs e)
    {
    }

    #region Configuration Methods

    /// <summary>
    /// Set cannon configuration for ability activation
    /// </summary>
    public void SetConfiguration(GameObject newProjectilePrefab, int newDamage,
        StatusEffectType effectType, float effectDuration, float effectStrength)
    {
        if (newProjectilePrefab != null)
        {
            projectilePrefab = newProjectilePrefab;
        }

        projectileDamage = newDamage;
        statusEffectType = effectType;
        statusEffectDuration = effectDuration;
        statusEffectStrength = effectStrength;
    }

    /// <summary>
    /// Get current cannon configuration for storage/restoration
    /// </summary>
    public CannonConfiguration GetConfiguration()
    {
        return new CannonConfiguration
        {
            projectilePrefab = projectilePrefab,
            projectileDamage = projectileDamage,
            projectileSpeed = projectileSpeed,
            statusEffectType = statusEffectType,
            statusEffectDuration = statusEffectDuration,
            statusEffectStrength = statusEffectStrength,
            muzzleFlash = muzzleFlash
        };
    }

    /// <summary>
    /// Apply a stored configuration
    /// </summary>
    public void ApplyConfiguration(CannonConfiguration config)
    {
        if (config == null) return;

        projectilePrefab = config.projectilePrefab;
        projectileDamage = config.projectileDamage;
        projectileSpeed = config.projectileSpeed;
        statusEffectType = config.statusEffectType;
        statusEffectDuration = config.statusEffectDuration;
        statusEffectStrength = config.statusEffectStrength;

        if (config.muzzleFlash != null)
        {
            muzzleFlash = config.muzzleFlash;
        }
    }

    /// <summary>
    /// Set just the status effect parameters
    /// </summary>
    public void SetStatusEffect(StatusEffectType effectType, float duration, float strength)
    {
        statusEffectType = effectType;
        statusEffectDuration = duration;
        statusEffectStrength = strength;
    }

    /// <summary>
    /// Set just the projectile prefab
    /// </summary>
    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
    }

    /// <summary>
    /// Set just the damage
    /// </summary>
    public void SetDamage(int damage)
    {
        projectileDamage = damage;
    }

    /// <summary>
    /// Set the fire point transform (called when visual swaps)
    /// </summary>
    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }

    /// <summary>
    /// Check if cannon is properly configured
    /// </summary>
    public bool IsConfigured()
    {
        return projectilePrefab != null && firePoint != null;
    }

    #endregion
}

/// <summary>
/// Stores cannon configuration for backup/restore during ability activation
/// </summary>
[System.Serializable]
public class CannonConfiguration
{
    public GameObject projectilePrefab;
    public int projectileDamage;
    public float projectileSpeed;
    public StatusEffectType statusEffectType;
    public float statusEffectDuration;
    public float statusEffectStrength;
    public ParticleSystem muzzleFlash;
}