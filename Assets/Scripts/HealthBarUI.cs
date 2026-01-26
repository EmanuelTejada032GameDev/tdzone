using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 10f;

    private float targetFillAmount;

    private void Awake()
    {
        if (healthSystem == null)
            Debug.LogError("HealthSystem reference missing on HealthBarUI");

        if (fillImage == null)
            Debug.LogError("Fill Image reference missing on HealthBarUI");
    }

    private void OnEnable()
    {
        healthSystem.OnDamaged += OnHealthChanged;
        healthSystem.OnHealed += OnHealthChanged;
        healthSystem.OnDied += OnDied;
    }

    private void OnDisable()
    {
        healthSystem.OnDamaged -= OnHealthChanged;
        healthSystem.OnHealed -= OnHealthChanged;
        healthSystem.OnDied -= OnDied;
    }

    private void Start()
    {
        UpdateHealthImmediate();
    }

    private void Update()
    {
        if (!smoothTransition) return;

        // Use unscaledDeltaTime so UI still updates when game is paused
        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFillAmount,
            Time.unscaledDeltaTime * smoothSpeed
        );
    }

    private void OnHealthChanged(object sender, EventArgs e)
    {
        targetFillAmount = healthSystem.NormalizedHealthAmount;

        if (!smoothTransition)
            fillImage.fillAmount = targetFillAmount;
    }

    private void OnDied(object sender, EventArgs e)
    {
        targetFillAmount = 0f;

        if (!smoothTransition)
            fillImage.fillAmount = 0f;
    }

    private void UpdateHealthImmediate()
    {
        targetFillAmount = healthSystem.NormalizedHealthAmount;
        fillImage.fillAmount = targetFillAmount;
    }
}
