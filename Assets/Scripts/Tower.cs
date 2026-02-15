using System;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private MultiAxisRotator baseRotator;
    [SerializeField] private MultiAxisRotator cannonRotator;

    [Header("Target")]
    [SerializeField] private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    [Header("Aiming")]
    [SerializeField] private float aimLockAngle = 5f;
    [SerializeField] private bool isTargetLocked;
    [SerializeField] private bool isTargetInShootingRange; 

    [Header("Ranges")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float minShootingRange = 2f;  
    [SerializeField] private float maxShootingRange = 15f; 

    [Header("Health")]
    [SerializeField] private HealthSystem HealthSystem;

    [Header("Cannons")]
    [SerializeField] private Cannon[] cannons;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private bool shootCannonsInOrder = false;

    [Header("Control Mode")]
    [SerializeField] private bool isManualMode = false;
    [SerializeField] private bool yAxisRotationOnly = true;

    [Header("Manual Mode Settings")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask groundLayer;
    private Camera mainCamera;

    [Header("Gizmo Visualization")]
    [SerializeField] private bool showDetectionRange = true;
    [SerializeField] private bool showShootingRange = true;
    [SerializeField] private bool showAngleLimits = true;
    [SerializeField] private bool showTargetInfo = true;
    [SerializeField] private int arcResolution = 30;

    private float fireTimer;

    // Original values for restoration after ability ends
    private float originalFireCooldown;
    private float originalMaxShootingRange;
    private float originalDetectionRadius;

    private void Awake()
    {
        if (HealthSystem is null)
        {
            HealthSystem = GetComponent<HealthSystem>();
        }

        cannons = GetComponentsInChildren<Cannon>();
        if (shootCannonsInOrder)
        {
            cannons = cannons.OrderBy(c => c.fireOrder).ToArray();
        }

        mainCamera = Camera.main;

        // Store original values for ability restore
        originalFireCooldown = fireCooldown;
        originalMaxShootingRange = maxShootingRange;
        originalDetectionRadius = detectionRadius;
    }

    private void OnEnable()
    {
        HealthSystem.OnDamaged += HealthSystem_OnDamaged;
        HealthSystem.OnDied += HealthSystem_OnDied;
    }

    private void OnDisable()
    {
        HealthSystem.OnDamaged -= HealthSystem_OnDamaged;
        HealthSystem.OnDied -= HealthSystem_OnDied;
    }

    private void HealthSystem_OnDied(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HealthSystem?.TakeDamage(2);
        }

        if (isManualMode)
        {
            HandleManualMode();
        }
        else
        {
            HandleAutonomousMode();
        }
    }

    private void HandleManualMode()
    {
        // Aim at mouse position in world
        HandleMouseAiming();

        // Fire on mouse click
        if (Input.GetKey(fireKey))
        {
            FireCannonsManual();
        }
    }

    private void HandleMouseAiming()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Try to hit ground layer first
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            AimAtPosition(hit.point);
        }
        else
        {
            // Fallback: create a plane at tower's Y level
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                AimAtPosition(worldPoint);
            }
        }
    }

    private void AimAtPosition(Vector3 worldPosition)
    {
        Vector3 direction = worldPosition - transform.position;

        // Base rotator - horizontal rotation
        baseRotator.RotateTowards(direction);

        // Cannon pitch only if NOT in Y-axis-only mode
        if (!yAxisRotationOnly)
        {
            Vector3 cannonDirection = worldPosition - cannonRotator.transform.position;
            cannonRotator.RotateTowards(cannonDirection);
        }
    }

    private void FireCannonsManual()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;

        foreach (var cannon in cannons)
        {
            cannon.TryFire(null);
        }

        fireTimer = fireCooldown;
    }

    private void HandleAutonomousMode()
    {
        if (currentTarget == null)
        {
            Collider[] enemiesInRadius = Physics.OverlapSphere(
                transform.position,
                detectionRadius,
                LayerMask.GetMask("Enemy")
            );

            if (enemiesInRadius.Length > 0)
            {
                Transform closestEnemy = null;
                float closestDistance = float.MaxValue;

                foreach (Collider enemy in enemiesInRadius)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy.transform;
                    }
                }

                SetCurrentTarget(closestEnemy);
            }
        }
        else
        {
            // Clear target if out of detection range
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > detectionRadius)
            {
                ClearCurrentTarget();
                return;
            }

            AimAtTarget();

            // Check both conditions
            isTargetInShootingRange = IsTargetInShootingRange();
            isTargetLocked = IsTargetLocked();

            // Only fire if BOTH conditions are met
            if (isTargetLocked && isTargetInShootingRange)
            {
                FireCannons();
            }
        }
    }

    private void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
    }

    private void ClearCurrentTarget()
    {
        currentTarget = null;
        isTargetLocked = false;
        isTargetInShootingRange = false;
    }

    private void AimAtTarget()
    {
        if (!currentTarget) return;

        Vector3 baseDirection = currentTarget.position - transform.position;
        baseRotator.RotateTowards(baseDirection);

        // Cannon pitch only if NOT in Y-axis-only mode
        if (!yAxisRotationOnly)
        {
            Vector3 cannonDirection = currentTarget.position - cannonRotator.transform.position;
            cannonRotator.RotateTowards(cannonDirection);
        }
    }

    private bool IsTargetLocked()
    {
        if (!currentTarget) return false;

        Vector3 toTarget = (currentTarget.position - cannonRotator.transform.position).normalized;

        if (yAxisRotationOnly)
        {
            // Only check horizontal angle when Y-axis only mode
            Vector3 flatForward = cannonRotator.transform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 flatToTarget = toTarget;
            flatToTarget.y = 0f;
            flatToTarget.Normalize();

            float angle = Vector3.Angle(flatForward, flatToTarget);
            return angle <= aimLockAngle;
        }
        else
        {
            float angle = Vector3.Angle(cannonRotator.transform.forward, toTarget);
            return angle <= aimLockAngle;
        }
    }

    //Complete shooting range check
    private bool IsTargetInShootingRange()
    {
        if (!currentTarget || !cannonRotator) return false;

        Vector3 cannonPos = cannonRotator.transform.position;
        Vector3 targetPos = currentTarget.position;
        float distance = Vector3.Distance(cannonPos, targetPos);

        // Check distance range
        if (distance < minShootingRange || distance > maxShootingRange)
        {
            return false;
        }

        // Check angle limits
        return IsTargetWithinAngleLimits(targetPos);
    }

    //Check if target is reachable within rotation constraints
    private bool IsTargetWithinAngleLimits(Vector3 targetWorldPos)
    {
        if (!cannonRotator) return false;

        // Convert target direction to local space of the cannon's parent
        Vector3 toTarget = targetWorldPos - cannonRotator.transform.position;
        Vector3 localDir = cannonRotator.transform.parent
            ? cannonRotator.transform.parent.InverseTransformDirection(toTarget)
            : toTarget;

        // Check Y-axis (horizontal/yaw) limits
        RotationAxisConfig yAxis = cannonRotator.YAxis;
        if (yAxis != null && yAxis.enabled && yAxis.useLimits)
        {
            float targetYaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            if (targetYaw < yAxis.minAngle || targetYaw > yAxis.maxAngle)
            {
                return false;
            }
        }

        // Check X-axis (vertical/pitch) limits
        RotationAxisConfig xAxis = cannonRotator.XAxis;
        if (xAxis != null && xAxis.enabled && xAxis.useLimits)
        {
            float horizontalDist = Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z);
            float targetPitch = -Mathf.Atan2(localDir.y, horizontalDist) * Mathf.Rad2Deg;

            if (targetPitch < xAxis.minAngle || targetPitch > xAxis.maxAngle)
            {
                return false;
            }
        }

        return true;
    }

    private void FireCannons()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;

        foreach (var cannon in cannons)
        {
            cannon.TryFire(currentTarget);
        }

        fireTimer = fireCooldown;
    }

    // ============= CONFIGURATION METHODS =============

    public void SetFireCooldown(float cooldown)
    {
        fireCooldown = cooldown;
    }

    public void SetShootingRange(float range)
    {
        maxShootingRange = range;
    }

    public void SetDetectionRadius(float radius)
    {
        detectionRadius = radius;
    }

    public void RestoreOriginalStats()
    {
        fireCooldown = originalFireCooldown;
        maxShootingRange = originalMaxShootingRange;
        detectionRadius = originalDetectionRadius;
    }

    // ============= GIZMO VISUALIZATION =============

    private void OnDrawGizmosSelected()
    {
        // 1. Detection Range (Yellow wire sphere)
        if (showDetectionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        if (!cannonRotator) return;

        Vector3 cannonPos = cannonRotator.transform.position;

        // 2. Shooting Range Visualization
        if (showShootingRange)
        {
            // Min range (red - dead zone)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(cannonPos, minShootingRange);

            // Max range (green - effective range)
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(cannonPos, maxShootingRange);
        }

        // 3. Angle Limits Visualization
        if (showAngleLimits)
        {
            DrawAngleLimitsVisualization();
        }

        // 4. Current Target Info
        if (showTargetInfo && currentTarget != null)
        {
            DrawTargetInfo();
        }
    }

    private void DrawAngleLimitsVisualization()
    {
        RotationAxisConfig yAxis = cannonRotator.YAxis;
        RotationAxisConfig xAxis = cannonRotator.XAxis;

        Vector3 cannonPos = cannonRotator.transform.position;
        Transform cannonParent = cannonRotator.transform.parent;

        // Draw horizontal (yaw) angle limits
        if (yAxis != null && yAxis.enabled && yAxis.useLimits)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Cyan
            DrawHorizontalArc(cannonPos, cannonParent, yAxis.minAngle, yAxis.maxAngle, maxShootingRange);
        }

        // Draw vertical (pitch) angle limits
        if (xAxis != null && xAxis.enabled && xAxis.useLimits)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.4f); // Magenta
            DrawVerticalArc(cannonPos, cannonParent, xAxis.minAngle, xAxis.maxAngle, maxShootingRange);
        }

        // Draw 3D coverage volume (combined limits)
        if (yAxis != null && yAxis.enabled && yAxis.useLimits &&
            xAxis != null && xAxis.enabled && xAxis.useLimits)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            DrawCoverageVolume(cannonPos, cannonParent, xAxis, yAxis, maxShootingRange);
        }
    }

    private void DrawHorizontalArc(Vector3 center, Transform parent, float minAngle, float maxAngle, float radius)
    {
        Vector3 forward = parent ? parent.forward : Vector3.forward;
        Vector3 right = parent ? parent.right : Vector3.right;

        Vector3 prevPoint = center + Quaternion.AngleAxis(minAngle, parent ? parent.up : Vector3.up) * forward * radius;

        for (int i = 1; i <= arcResolution; i++)
        {
            float t = (float)i / arcResolution;
            float angle = Mathf.Lerp(minAngle, maxAngle, t);

            Vector3 direction = Quaternion.AngleAxis(angle, parent ? parent.up : Vector3.up) * forward;
            Vector3 point = center + direction * radius;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Draw limit lines
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(minAngle, parent ? parent.up : Vector3.up) * forward * radius);
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(maxAngle, parent ? parent.up : Vector3.up) * forward * radius);
    }

    private void DrawVerticalArc(Vector3 center, Transform parent, float minAngle, float maxAngle, float radius)
    {
        Vector3 forward = parent ? parent.forward : Vector3.forward;
        Vector3 right = parent ? parent.right : Vector3.right;

        Vector3 prevPoint = center + Quaternion.AngleAxis(minAngle, right) * forward * radius;

        for (int i = 1; i <= arcResolution; i++)
        {
            float t = (float)i / arcResolution;
            float angle = Mathf.Lerp(minAngle, maxAngle, t);

            Vector3 direction = Quaternion.AngleAxis(angle, right) * forward;
            Vector3 point = center + direction * radius;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Draw limit lines
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(minAngle, right) * forward * radius);
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(maxAngle, right) * forward * radius);
    }

    private void DrawCoverageVolume(Vector3 center, Transform parent, RotationAxisConfig xAxis, RotationAxisConfig yAxis, float radius)
    {
        Vector3 forward = parent ? parent.forward : Vector3.forward;
        Vector3 right = parent ? parent.right : Vector3.right;
        Vector3 up = parent ? parent.up : Vector3.up;

        int steps = 8;

        // Draw grid showing coverage area
        for (int yStep = 0; yStep <= steps; yStep++)
        {
            float yawAngle = Mathf.Lerp(yAxis.minAngle, yAxis.maxAngle, (float)yStep / steps);

            Vector3 prevPoint = Vector3.zero;

            for (int xStep = 0; xStep <= steps; xStep++)
            {
                float pitchAngle = Mathf.Lerp(xAxis.minAngle, xAxis.maxAngle, (float)xStep / steps);

                Vector3 direction = Quaternion.AngleAxis(yawAngle, up) *
                                   Quaternion.AngleAxis(pitchAngle, right) * forward;
                Vector3 point = center + direction * radius;

                if (xStep > 0)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }
                prevPoint = point;
            }
        }
    }

    private void DrawTargetInfo()
    {
        Vector3 cannonPos = cannonRotator.transform.position;
        Vector3 targetPos = currentTarget.position;
        float distance = Vector3.Distance(cannonPos, targetPos);

        bool inDistanceRange = distance >= minShootingRange && distance <= maxShootingRange;
        bool inAngleLimits = IsTargetWithinAngleLimits(targetPos);
        bool canShoot = inDistanceRange && inAngleLimits;

        // Color-coded line to target
        if (canShoot)
        {
            Gizmos.color = Color.green; // Can shoot
        }
        else if (!inDistanceRange)
        {
            Gizmos.color = Color.red; // Out of range
        }
        else
        {
            Gizmos.color = Color.yellow; // In range but can't rotate to it
        }

        Gizmos.DrawLine(cannonPos, targetPos);

        // Draw sphere at target
        Gizmos.DrawWireSphere(targetPos, 0.5f);

        // Distance indicator at midpoint
        Vector3 midPoint = (cannonPos + targetPos) / 2f;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(midPoint, Vector3.one * 0.3f);

#if UNITY_EDITOR
        // Draw text label in Scene view
        UnityEditor.Handles.Label(midPoint + Vector3.up * 0.5f,
            $"Dist: {distance:F1}m\n" +
            $"In Range: {inDistanceRange}\n" +
            $"In Angles: {inAngleLimits}\n" +
            $"Can Shoot: {canShoot}");
#endif
    }
}