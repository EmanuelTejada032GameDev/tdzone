using UnityEngine;

[System.Serializable]
public class RotationAxisConfig
{
    public bool enabled = false;
    public float speed = 90f;

    public bool useLimits = false;
    public float minAngle = -60f;
    public float maxAngle = 60f;

    [HideInInspector]
    public float currentAngle;
}
