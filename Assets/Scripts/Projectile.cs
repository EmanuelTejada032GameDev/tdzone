using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private ProjectileType projectileType = ProjectileType.Straight;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private int damage;

    [Header("Homing Settings")]
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float homingDelay = 0.1f;

    [Header("Ballistic Settings")]
    [SerializeField] private float arcHeight = 2f;

    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private TrailRenderer trail;

    [Header("Status Effect")]
    [SerializeField] private StatusEffectType statusEffectType = StatusEffectType.None;
    [SerializeField] private float statusEffectDuration = 2f;
    [SerializeField] private float statusEffectStrength = 1f;

    private Transform target;
    private Vector3 targetPosition;
    private float homingTimer;
    private float lifeTimer;

    // For ballistic arc
    private Vector3 startPosition;
    private float arcProgress;
    private float arcDuration;

    public event EventHandler<ProjectileHitEventArgs> OnProjectileHit;

    private void Start()
    {
        lifeTimer = lifetime;

        if (projectileType == ProjectileType.Ballistic && target != null)
        {
            startPosition = transform.position;
            float distance = Vector3.Distance(startPosition, target.position);
            arcDuration = distance / speed;
        }
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DestroyProjectile();
            return;
        }

        switch (projectileType)
        {
            case ProjectileType.Straight:
                MoveStraight();
                break;
            case ProjectileType.Homing:
                MoveHoming();
                break;
            case ProjectileType.Ballistic:
                MoveBallistic();
                break;
        }
    }

    public void Initialize(Transform target, int damage)
    {
        this.target = target;
        this.targetPosition = target ? target.position : transform.position + transform.forward * 100f;
        this.damage = damage;

        // Point towards target
        if (target)
        {
            transform.LookAt(target.position);
        }
    }

    public void Initialize(Vector3 targetPosition, int damage)
    {
        this.target = null;
        this.targetPosition = targetPosition;
        this.damage = damage;

        transform.LookAt(targetPosition);
    }

    /// <summary>
    /// Initialize with status effect parameters
    /// </summary>
    public void Initialize(Transform target, int damage, StatusEffectType effectType, float effectDuration, float effectStrength)
    {
        Initialize(target, damage);
        this.statusEffectType = effectType;
        this.statusEffectDuration = effectDuration;
        this.statusEffectStrength = effectStrength;
    }

    /// <summary>
    /// Set status effect parameters (can be called after Initialize)
    /// </summary>
    public void SetStatusEffect(StatusEffectType effectType, float duration, float strength)
    {
        this.statusEffectType = effectType;
        this.statusEffectDuration = duration;
        this.statusEffectStrength = strength;
    }

    private void MoveStraight()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void MoveHoming()
    {
        homingTimer += Time.deltaTime;

        // Update target position if target still exists
        if (target != null)
        {
            targetPosition = target.position;
        }

        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Apply homing after delay
        if (homingTimer >= homingDelay && target != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 newDirection = Vector3.Lerp(transform.forward, direction, homingStrength * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private void MoveBallistic()
    {
        if (target != null)
        {
            targetPosition = target.position;
        }

        arcProgress += Time.deltaTime / arcDuration;

        if (arcProgress >= 1f)
        {
            // Reached target
            transform.position = targetPosition;
            OnHit(null);
            return;
        }

        // Calculate position on arc
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, arcProgress);

        // Add arc height (parabola)
        float heightOffset = arcHeight * Mathf.Sin(arcProgress * Mathf.PI);
        currentPos.y += heightOffset;

        transform.position = currentPos;

        // Rotate to face movement direction
        if (arcProgress < 0.99f)
        {
            Vector3 nextPos = Vector3.Lerp(startPosition, targetPosition, arcProgress + 0.01f);
            nextPos.y += arcHeight * Mathf.Sin((arcProgress + 0.01f) * Mathf.PI);
            transform.LookAt(nextPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't hit the shooter
        if (other.CompareTag("Tower") || other.CompareTag("Cannon"))
            return;

        OnHit(other);
    }

    private void OnHit(Collider hitCollider)
    {
        // Try to damage the hit object
        if (hitCollider != null)
        {
            HealthSystem health = hitCollider.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // Apply status effect if any
            if (statusEffectType != StatusEffectType.None)
            {
                StatusEffectManager effectManager = hitCollider.GetComponent<StatusEffectManager>();
                if (effectManager != null)
                {
                    effectManager.ApplyEffect(statusEffectType, statusEffectDuration, statusEffectStrength);
                }
            }

            // Invoke hit event
            OnProjectileHit?.Invoke(this, new ProjectileHitEventArgs
            {
                HitObject = hitCollider.gameObject,
                HitPosition = transform.position,
                Damage = damage
            });
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);

        // Detach trail so it can fade out
        //if (trail != null)
        //{
        //    trail.transform.SetParent(null);
        //    Destroy(trail.gameObject, trail.time);
        //}

        //Destroy(gameObject);
    }

    public enum ProjectileType
    {
        Straight,   // Flies straight forward
        Homing,     // Tracks target
        Ballistic   // Follows arc trajectory
    }
}

public class ProjectileHitEventArgs : EventArgs
{
    public GameObject HitObject;
    public Vector3 HitPosition;
    public float Damage;
}