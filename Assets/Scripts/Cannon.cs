using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
    [Header("Firing")]
    public int fireOrder;
    public float fireDelay = 0f;
    public float cooldown = 1f;

    [Header("Projectile")]
    [SerializeField] private ProjectileDataSO projectileData;
    [SerializeField] private int projectileDamage;
    [SerializeField] private FiringMode firingMode = FiringMode.Projectile;

    [Header("Aim")]
    public Transform firePoint;

    [Header("Effects")]
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
        if (firingMode == FiringMode.Continuous) return;
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
        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: No fire point assigned!");
            return;
        }

        if (projectileData == null || projectileData.projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: No ProjectileDataSO or projectile prefab assigned!");
            return;
        }

        // Spawn projectile
        GameObject projectileObj = Instantiate(projectileData.projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(projectileData, target, projectileDamage);
            projectile.OnProjectileHit += Projectile_OnProjectileHit;
        }

        // Muzzle flash from SO
        if (projectileData.muzzleFlashPrefab != null)
        {
            GameObject muzzle = Instantiate(projectileData.muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzle, 2f);
        }

        // Audio from SO
        if (audioSource != null && projectileData.fireSound != null)
        {
            audioSource.PlayOneShot(projectileData.fireSound);
        }
    }

    private void Projectile_OnProjectileHit(object sender, ProjectileHitEventArgs e)
    {
    }

    #region Configuration Methods

    /// <summary>
    /// Set cannon configuration using ProjectileDataSO
    /// </summary>
    public void SetConfiguration(ProjectileDataSO newProjectileData, int newDamage)
    {
        projectileData = newProjectileData;
        projectileDamage = newDamage;
    }

    /// <summary>
    /// Get current cannon configuration for storage/restoration
    /// </summary>
    public CannonConfiguration GetConfiguration()
    {
        return new CannonConfiguration
        {
            projectileData = projectileData,
            projectileDamage = projectileDamage,
            firingMode = firingMode,
            cooldown = cooldown
        };
    }

    /// <summary>
    /// Apply a stored configuration
    /// </summary>
    public void ApplyConfiguration(CannonConfiguration config)
    {
        if (config == null) return;

        projectileData = config.projectileData;
        projectileDamage = config.projectileDamage;
        firingMode = config.firingMode;
        cooldown = config.cooldown;
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
    /// Set the ProjectileDataSO
    /// </summary>
    public void SetProjectileData(ProjectileDataSO data)
    {
        projectileData = data;
    }

    /// <summary>
    /// Set the firing mode (Projectile or Continuous)
    /// </summary>
    public void SetFiringMode(FiringMode mode)
    {
        firingMode = mode;
    }

    /// <summary>
    /// Check if cannon is properly configured
    /// </summary>
    public bool IsConfigured()
    {
        return projectileData != null && firePoint != null;
    }

    #endregion
}

/// <summary>
/// Stores cannon configuration for backup/restore during ability activation
/// </summary>
[System.Serializable]
public class CannonConfiguration
{
    public ProjectileDataSO projectileData;
    public int projectileDamage;
    public FiringMode firingMode;
    public float cooldown;
}
