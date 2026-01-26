using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Visualization")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoSize = 0.5f;

    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    private void OnDrawGizmos()
    {
        // Draw spawn point in scene view
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // Draw direction arrow
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw larger when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.5f);
    }
}