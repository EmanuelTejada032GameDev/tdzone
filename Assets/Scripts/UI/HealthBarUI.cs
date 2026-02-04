using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject healthBarValueContainer;
    [SerializeField] private TextMeshProUGUI healthValueText;
    [SerializeField] private TextMeshProUGUI healthMaxValueText;


    [Header("Settings")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool showHealthBarValue;


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
        if (healthSystem == null) return;

        healthSystem.OnDamaged += OnHealthChanged;
        healthSystem.OnHealed += OnHealthChanged;
        healthSystem.OnDied += OnDied;
    }

    private void OnDisable()
    {
        if (healthSystem == null) return;
        healthSystem.OnDamaged -= OnHealthChanged;
        healthSystem.OnHealed -= OnHealthChanged;
        healthSystem.OnDied -= OnDied;
    }

    private void Start()
    {
        UpdateHealthImmediate();
        if (showHealthBarValue)
        {
            ShowHealthBarValue();
        }
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
        UpdateHealthBarValuesText();
        if (!smoothTransition)
            fillImage.fillAmount = targetFillAmount;
    }

    private void OnDied(object sender, EventArgs e)
    {
        targetFillAmount = 0f;
        UpdateHealthBarValuesText();
        if (!smoothTransition)
            fillImage.fillAmount = 0f;
    }

    private void UpdateHealthImmediate()
    {
        UpdateHealthBarValuesText();
        targetFillAmount = healthSystem.NormalizedHealthAmount;
        fillImage.fillAmount = targetFillAmount;
    }


    private void ShowHealthBarValue()
    {
        healthBarValueContainer.SetActive(true);
        UpdateHealthBarValuesText();
    }

    private void UpdateHealthBarValuesText()
    {
        healthValueText.SetText(healthSystem.HealthAmount.ToString());
        healthMaxValueText.SetText(healthSystem.MaxHealth.ToString());
    }

}
