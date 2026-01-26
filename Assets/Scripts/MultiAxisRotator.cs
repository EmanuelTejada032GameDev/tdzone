using UnityEngine;

public class MultiAxisRotator : MonoBehaviour
{
    [Header("Axis Rotation")]
    [SerializeField] private RotationAxisConfig xAxis;
    [SerializeField] private RotationAxisConfig yAxis;
    [SerializeField] private RotationAxisConfig zAxis;

    public RotationAxisConfig XAxis => xAxis;
    public RotationAxisConfig YAxis => yAxis;
    public RotationAxisConfig ZAxis => zAxis;

    [Header("Rotation Mode")]
    [SerializeField] private RotationMode rotationMode = RotationMode.TargetTracking;

    private void Awake()
    {
        Vector3 euler = transform.localEulerAngles;
        xAxis.currentAngle = Normalize(euler.x);
        yAxis.currentAngle = Normalize(euler.y);
        zAxis.currentAngle = Normalize(euler.z);
    }

    private void Update()
    {
        // Only apply constant rotation if in ConstantRotation mode
        if (rotationMode == RotationMode.ConstantRotation)
        {
            ApplyConstantRotation();
        }
    }

    public void RotateTowards(Vector3 worldDirection)
    {
        // Only work in TargetTracking mode
        if (rotationMode != RotationMode.TargetTracking) return;

        // Convert world direction to local space
        Vector3 localDir = transform.parent
            ? transform.parent.InverseTransformDirection(worldDirection)
            : worldDirection;

        // Rotate Y axis (horizontal/yaw rotation)
        if (yAxis.enabled)
        {
            float targetYaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            RotateAxisToAngle(yAxis, Axis.Y, targetYaw);
        }

        // Rotate X axis (vertical/pitch rotation)
        if (xAxis.enabled)
        {
            // Get horizontal distance (ignoring Y)
            float horizontalDist = Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z);

            // Calculate pitch angle (elevation)
            float targetPitch = -Mathf.Atan2(localDir.y, horizontalDist) * Mathf.Rad2Deg;

            RotateAxisToAngle(xAxis, Axis.X, targetPitch);
        }
    }

    private void ApplyConstantRotation()
    {
        // Rotate each enabled axis at its speed
        if (xAxis.enabled)
        {
            xAxis.currentAngle += xAxis.speed * Time.deltaTime;

            // Apply limits if enabled
            if (xAxis.useLimits)
            {
                xAxis.currentAngle = Mathf.Clamp(xAxis.currentAngle, xAxis.minAngle, xAxis.maxAngle);
            }
            else
            {
                // Wrap angle to prevent overflow
                xAxis.currentAngle = Mathf.Repeat(xAxis.currentAngle + 180f, 360f) - 180f;
            }

            ApplyRotation(Axis.X, xAxis.currentAngle);
        }

        if (yAxis.enabled)
        {
            yAxis.currentAngle += yAxis.speed * Time.deltaTime;

            if (yAxis.useLimits)
            {
                yAxis.currentAngle = Mathf.Clamp(yAxis.currentAngle, yAxis.minAngle, yAxis.maxAngle);
            }
            else
            {
                yAxis.currentAngle = Mathf.Repeat(yAxis.currentAngle + 180f, 360f) - 180f;
            }

            ApplyRotation(Axis.Y, yAxis.currentAngle);
        }

        if (zAxis.enabled)
        {
            zAxis.currentAngle += zAxis.speed * Time.deltaTime;

            if (zAxis.useLimits)
            {
                zAxis.currentAngle = Mathf.Clamp(zAxis.currentAngle, zAxis.minAngle, zAxis.maxAngle);
            }
            else
            {
                zAxis.currentAngle = Mathf.Repeat(zAxis.currentAngle + 180f, 360f) - 180f;
            }

            ApplyRotation(Axis.Z, zAxis.currentAngle);
        }
    }

    private void RotateAxisToAngle(RotationAxisConfig axis, Axis axisType, float targetAngle)
    {
        // Clamp the target angle BEFORE moving towards it
        if (axis.useLimits)
        {
            targetAngle = Mathf.Clamp(targetAngle, axis.minAngle, axis.maxAngle);
        }

        float newAngle = Mathf.MoveTowardsAngle(
            axis.currentAngle,
            targetAngle,
            axis.speed * Time.deltaTime
        );

        // Clamp again after moving (in case we're already outside limits)
        if (axis.useLimits)
        {
            newAngle = Mathf.Clamp(newAngle, axis.minAngle, axis.maxAngle);
        }

        axis.currentAngle = newAngle;
        ApplyRotation(axisType, axis.currentAngle);
    }

    private void ApplyRotation(Axis axis, float angle)
    {
        Vector3 euler = transform.localEulerAngles;

        switch (axis)
        {
            case Axis.X: euler.x = angle; break;
            case Axis.Y: euler.y = angle; break;
            case Axis.Z: euler.z = angle; break;
        }

        transform.localEulerAngles = euler;
    }

    private float Normalize(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }


    private enum Axis { X, Y, Z }

    public enum RotationMode
    {
        TargetTracking,    // Rotates towards a target (turrets, cameras tracking)
        ConstantRotation   // Rotates continuously (propellers, windmills, radar)
    }
}