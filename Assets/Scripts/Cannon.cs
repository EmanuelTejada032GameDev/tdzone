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
}