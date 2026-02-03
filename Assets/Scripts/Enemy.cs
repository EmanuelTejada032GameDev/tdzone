using UnityEngine;
using System;

[RequireComponent(typeof(HealthSystem))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;

    // Runtime speed (can be modified by effects)
    private float currentMoveSpeed;

    /// <summary>
    /// Current movement speed (may be modified by status effects)
    /// </summary>
    public float MoveSpeed => currentMoveSpeed;

    /// <summary>
    /// Base movement speed (unmodified)
    /// </summary>
    public float BaseMoveSpeed => moveSpeed;

    /// <summary>
    /// Set movement speed (used by status effects)
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        currentMoveSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// Reset speed to base value
    /// </summary>
    public void ResetMoveSpeed()
    {
        currentMoveSpeed = moveSpeed;
    }

    [Header("Combat")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Rewards")]
    [SerializeField] private int goldReward = 10;
    [SerializeField] private int scoreReward = 100;

    private Transform towerTransform;
    private Tower tower;
    private HealthSystem healthSystem;
    private float attackTimer;
    private bool hasReachedTower;

    public static event EventHandler<EnemyDestroyedEventArgs> OnEnemyDestroyed;
    public static event EventHandler OnEnemyReachedTower;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        currentMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        // Find the tower in the scene (since there's only one)
        tower = FindFirstObjectByType<Tower>();

        if (tower != null)
        {
            towerTransform = tower.transform;
        }
        else
        {
            Debug.LogError("Enemy: No tower found in scene!");
            Destroy(gameObject);
            return;
        }

        // Subscribe to health events
        healthSystem.OnDied += HealthSystem_OnDied;
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
    }

    private void Update()
    {
        if (towerTransform == null || hasReachedTower) return;

        // Use horizontal distance only (ignore Y)
        Vector3 horizontalPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 horizontalTowerPos = new Vector3(towerTransform.position.x, 0f, towerTransform.position.z);
        float distanceToTower = Vector3.Distance(horizontalPos, horizontalTowerPos);

        if (distanceToTower > stoppingDistance)
        {
            // Move towards tower
            MoveTowardsTower();
        }
        else
        {
            // Reached tower - attack it
            if (!hasReachedTower)
            {
                hasReachedTower = true;
                OnEnemyReachedTower?.Invoke(this, EventArgs.Empty);
            }

            AttackTower();
        }
    }

    private void MoveTowardsTower()
    {
        // Calculate direction to tower (ignore Y to keep enemy on ground)
        Vector3 targetPosition = new Vector3(towerTransform.position.x, transform.position.y, towerTransform.position.z);
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move towards tower (horizontal only)
        transform.position += direction * currentMoveSpeed * Time.deltaTime;

        // Rotate to face tower
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void AttackTower()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            // Deal damage to tower
            HealthSystem towerHealth = tower.GetComponent<HealthSystem>();
            if (towerHealth != null)
            {
                towerHealth.TakeDamage(damage);
                healthSystem.TakeDamage(healthSystem.HealthAmount);
            }

            attackTimer = attackCooldown;
        }
    }

    private void HealthSystem_OnDied(object sender, EventArgs e)
    {
        // Invoke destroyed event with rewards
        OnEnemyDestroyed?.Invoke(this, new EnemyDestroyedEventArgs
        {
            Position = transform.position,
            GoldReward = goldReward,
            ScoreReward = scoreReward
        });

        // Destroy this enemy
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (healthSystem != null)
        {
            healthSystem.OnDied -= HealthSystem_OnDied;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (towerTransform != null)
        {
            // Draw line to tower
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, towerTransform.position);

            // Draw stopping distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        }
    }
}

public class EnemyDestroyedEventArgs : EventArgs
{
    public Vector3 Position;
    public int GoldReward;
    public int ScoreReward;
}