using UnityEngine;
using System.Collections;

/// <summary>
/// Handles continuous area damage for the flamethrower firing mode.
/// Created and managed by TowerAbilityManager when a Continuous tower type activates.
/// </summary>
public class ContinuousDamageZone : MonoBehaviour
{
    private float damagePerSecond;
    private float range;
    private float coneAngle;
    private StatusEffectType statusEffect;
    private float effectDuration;
    private float effectStrength;
    private float tickRate = 0.25f;

    private Transform firePoint;
    private GameObject activeEffect;
    private Coroutine damageCoroutine;
    private bool isActive;

    /// <summary>
    /// Activate the continuous damage zone with settings from TowerDataSO.
    /// </summary>
    public void Activate(TowerDataSO data, Transform firePoint)
    {
        if (isActive) return;

        this.firePoint = firePoint;
        damagePerSecond = data.continuousDamagePerSecond;
        range = data.continuousRange;
        coneAngle = data.continuousConeAngle;
        statusEffect = data.continuousStatusEffect;
        effectDuration = data.continuousEffectDuration;
        effectStrength = data.continuousEffectStrength;

        // Spawn the particle effect as child of fire point
        if (data.continuousEffectPrefab != null)
        {
            activeEffect = Instantiate(data.continuousEffectPrefab, firePoint.position, firePoint.rotation, firePoint);
            activeEffect.transform.localPosition = Vector3.zero;
            activeEffect.transform.localRotation = Quaternion.identity;
        }

        isActive = true;
        damageCoroutine = StartCoroutine(DamageTickRoutine());
    }

    /// <summary>
    /// Deactivate and clean up.
    /// </summary>
    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }
    }

    private IEnumerator DamageTickRoutine()
    {
        while (isActive)
        {
            DamageTick();
            yield return new WaitForSeconds(tickRate);
        }
    }

    private void DamageTick()
    {
        if (firePoint == null) return;

        int damagePerTick = Mathf.RoundToInt(damagePerSecond * tickRate);
        if (damagePerTick < 1) damagePerTick = 1;

        // Find all enemies in range
        Collider[] hits = Physics.OverlapSphere(firePoint.position, range, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            // Filter by cone angle
            Vector3 toEnemy = (hit.transform.position - firePoint.position).normalized;
            float angle = Vector3.Angle(firePoint.forward, toEnemy);

            if (angle > coneAngle) continue;

            // Apply damage
            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damagePerTick);
            }

            // Apply status effect
            if (statusEffect != StatusEffectType.None)
            {
                StatusEffectManager effectManager = hit.GetComponent<StatusEffectManager>();
                if (effectManager != null)
                {
                    effectManager.ApplyEffect(statusEffect, effectDuration, effectStrength);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Deactivate();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        // Draw range sphere
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(firePoint.position, range);

        // Draw cone lines
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.6f);
        Vector3 forward = firePoint.forward * range;

        Vector3 rightDir = Quaternion.AngleAxis(coneAngle, firePoint.up) * forward;
        Vector3 leftDir = Quaternion.AngleAxis(-coneAngle, firePoint.up) * forward;
        Vector3 upDir = Quaternion.AngleAxis(-coneAngle, firePoint.right) * forward;
        Vector3 downDir = Quaternion.AngleAxis(coneAngle, firePoint.right) * forward;

        Gizmos.DrawLine(firePoint.position, firePoint.position + rightDir);
        Gizmos.DrawLine(firePoint.position, firePoint.position + leftDir);
        Gizmos.DrawLine(firePoint.position, firePoint.position + upDir);
        Gizmos.DrawLine(firePoint.position, firePoint.position + downDir);
    }
#endif
}
