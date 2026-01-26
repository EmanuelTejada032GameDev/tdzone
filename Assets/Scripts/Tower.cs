using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private MultiAxisRotator baseRotator;
    [SerializeField] private MultiAxisRotator cannonRotator;

    [Header("Target")]
    [SerializeField] private Transform currentTarget;


    [SerializeField] private HealthSystem HealthSystem;

    private void Awake()
    {
        if (HealthSystem is null)
        {
            HealthSystem = GetComponent<HealthSystem>();
        }

        HealthSystem.OnDamaged += (sender, e) =>
        {
            Debug.Log("Tower took damage! Current Health: " + HealthSystem.HealthAmount);
        };

        HealthSystem.OnDied += (sender, e) =>
        {
            Debug.Log("Tower HealthSystem Died");
        };
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (HealthSystem is not null) HealthSystem.TakeDamage(2);
        }

        AimAtTarget();
    }

    private void AimAtTarget()
    {
        if (!currentTarget) return;

        // Base rotates towards target from tower base position
        Vector3 baseDirection = currentTarget.position - transform.position;
        baseRotator.RotateTowards(baseDirection);

        // Cannon rotates towards target from cannon position
        Vector3 cannonDirection = currentTarget.position - cannonRotator.transform.position;
        cannonRotator.RotateTowards(cannonDirection);
    }

}