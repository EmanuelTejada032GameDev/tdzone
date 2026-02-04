using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles visual transformation of the tower when abilities are activated.
/// Manages instantiation and destruction of tower visual prefabs.
/// </summary>
public class TowerVisualSwapper : MonoBehaviour
{
    [Header("Visual Mount Points")]
    [Tooltip("Parent transform where tower visuals will be instantiated")]
    [SerializeField] private Transform visualMountPoint;

    [Header("Default Visual")]
    [Tooltip("The default tower visual (can be a child GameObject or prefab)")]
    [SerializeField] private GameObject defaultVisual;
    [Tooltip("If true, default visual is a scene object. If false, it's a prefab to instantiate.")]
    [SerializeField] private bool defaultIsSceneObject = true;

    [Header("Fire Point Settings")]
    [Tooltip("Tag used to find fire points in visual prefabs")]
    [SerializeField] private string firePointTag = "FirePoint";
    [Tooltip("Alternative: name pattern for fire point GameObjects")]
    [SerializeField] private string firePointNamePattern = "FirePoint";

    [Header("Current State")]
    [SerializeField] private GameObject currentVisual;
    [SerializeField] private TowerDataSO currentTowerData;
    [SerializeField] private List<Transform> currentFirePoints = new List<Transform>();

    [Header("Transition Settings")]
    [Tooltip("Time for visual transition effects (if any)")]
    [SerializeField] private float transitionTime = 0.2f;

    // Events
    public event Action<TowerDataSO> OnVisualChanged;
    public event Action OnVisualReverted;
    public event Action<List<Transform>> OnFirePointsChanged;

    private void Awake()
    {
        // If no mount point specified, use this transform
        if (visualMountPoint == null)
        {
            visualMountPoint = transform;
        }

        // Store reference to default visual if it's a scene object
        if (defaultIsSceneObject && defaultVisual != null)
        {
            currentVisual = defaultVisual;
        }
    }

    private void Start()
    {
        // If default visual is a prefab (not scene object), instantiate it on start
        if (!defaultIsSceneObject && defaultVisual != null && currentVisual == null)
        {
            currentVisual = Instantiate(
                defaultVisual,
                visualMountPoint.position,
                visualMountPoint.rotation,
                visualMountPoint
            );
        }

        // Find fire points in the initial visual
        if (currentVisual != null)
        {
            FindFirePoints();
            OnFirePointsChanged?.Invoke(currentFirePoints);
        }

        // Warn if no default visual is set
        if (defaultVisual == null)
        {
            Debug.LogWarning("TowerVisualSwapper: No default visual assigned!");
        }
    }

    /// <summary>
    /// Find all fire point transforms in the current visual
    /// </summary>
    private void FindFirePoints()
    {
        currentFirePoints.Clear();

        if (currentVisual == null)
            return;

        // Method 1: Find by tag
        if (!string.IsNullOrEmpty(firePointTag))
        {
            foreach (Transform child in currentVisual.GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag(firePointTag))
                {
                    currentFirePoints.Add(child);
                }
            }
        }

        // Method 2: If no tagged objects found, try finding by name pattern
        if (currentFirePoints.Count == 0 && !string.IsNullOrEmpty(firePointNamePattern))
        {
            foreach (Transform child in currentVisual.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains(firePointNamePattern))
                {
                    currentFirePoints.Add(child);
                }
            }
        }

        // Method 3: Look for "SpawnLoc" (common in asset packs)
        if (currentFirePoints.Count == 0)
        {
            foreach (Transform child in currentVisual.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("SpawnLoc") || child.name.Contains("Muzzle"))
                {
                    currentFirePoints.Add(child);
                }
            }
        }

        if (currentFirePoints.Count == 0)
        {
            Debug.LogWarning($"TowerVisualSwapper: No fire points found in visual '{currentVisual.name}'");
        }
        else
        {
            Debug.Log($"TowerVisualSwapper: Found {currentFirePoints.Count} fire point(s) in '{currentVisual.name}'");
        }
    }

    /// <summary>
    /// Get the current fire points
    /// </summary>
    public List<Transform> GetFirePoints()
    {
        return currentFirePoints;
    }

    /// <summary>
    /// Swap to a new tower visual based on tower data
    /// </summary>
    public void SwapVisual(TowerDataSO towerData)
    {
        if (towerData == null)
            return;

        // If no visual prefab specified, just track the data change
        if (towerData.towerVisualPrefab == null)
        {
            currentTowerData = towerData;
            OnVisualChanged?.Invoke(towerData);
            return;
        }

        // Hide/destroy current visual
        HideCurrentVisual();

        // Instantiate new visual
        currentVisual = Instantiate(
            towerData.towerVisualPrefab,
            visualMountPoint.position,
            visualMountPoint.rotation,
            visualMountPoint
        );

        currentTowerData = towerData;

        // Find fire points in the new visual
        FindFirePoints();

        // Apply any visual effects (color tint, particles, etc.)
        ApplyVisualEffects(towerData);

        OnVisualChanged?.Invoke(towerData);
        OnFirePointsChanged?.Invoke(currentFirePoints);
    }

    /// <summary>
    /// Revert to the default tower visual
    /// </summary>
    public void RevertToDefault()
    {
        // Hide/destroy current ability visual
        if (currentVisual != null && currentVisual != defaultVisual)
        {
            Destroy(currentVisual);
        }

        // Show default visual
        if (defaultIsSceneObject && defaultVisual != null)
        {
            defaultVisual.SetActive(true);
            currentVisual = defaultVisual;
        }
        else if (!defaultIsSceneObject && defaultVisual != null)
        {
            // Instantiate default prefab
            currentVisual = Instantiate(
                defaultVisual,
                visualMountPoint.position,
                visualMountPoint.rotation,
                visualMountPoint
            );
        }

        // Find fire points in the default visual
        FindFirePoints();

        currentTowerData = null;
        OnVisualReverted?.Invoke();
        OnFirePointsChanged?.Invoke(currentFirePoints);
    }

    /// <summary>
    /// Hide or destroy the current visual
    /// </summary>
    private void HideCurrentVisual()
    {
        if (currentVisual == null)
            return;

        if (defaultIsSceneObject && currentVisual == defaultVisual)
        {
            // Just hide the default scene object
            defaultVisual.SetActive(false);
        }
        else
        {
            // Destroy instantiated prefab
            Destroy(currentVisual);
        }

        currentVisual = null;
    }

    /// <summary>
    /// Apply visual effects based on tower data (color, particles, etc.)
    /// </summary>
    private void ApplyVisualEffects(TowerDataSO towerData)
    {
        if (currentVisual == null || towerData == null)
            return;

        // Apply projectile color to any renderers if desired
        // This could be expanded to change material colors, enable particles, etc.

        // Example: Enable any particle systems on the new visual
        var particles = currentVisual.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particles)
        {
            ps.Play();
        }

        // Example: Apply color tint to renderers
        ApplyColorTint(towerData.projectileColor);
    }

    /// <summary>
    /// Apply a color tint to the current visual's renderers
    /// </summary>
    private void ApplyColorTint(Color color)
    {
        if (currentVisual == null)
            return;

        // Get all renderers
        var renderers = currentVisual.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            // Create material instance to avoid modifying shared materials
            var materials = renderer.materials;
            foreach (var mat in materials)
            {
                // Check if material has emission property
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", color * 0.5f);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    /// <summary>
    /// Get the current visual GameObject
    /// </summary>
    public GameObject GetCurrentVisual()
    {
        return currentVisual;
    }

    /// <summary>
    /// Get the current tower data (null if using default)
    /// </summary>
    public TowerDataSO GetCurrentTowerData()
    {
        return currentTowerData;
    }

    /// <summary>
    /// Check if currently showing an ability visual
    /// </summary>
    public bool IsShowingAbilityVisual()
    {
        return currentTowerData != null && currentVisual != defaultVisual;
    }

    /// <summary>
    /// Set the default visual reference (useful for runtime setup)
    /// </summary>
    public void SetDefaultVisual(GameObject visual, bool isSceneObject)
    {
        defaultVisual = visual;
        defaultIsSceneObject = isSceneObject;

        if (isSceneObject)
        {
            currentVisual = visual;
        }
    }
}
