using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the ability bar UI, creating and updating ability slots
/// based on the TowerAbilityManager's available abilities.
/// </summary>
public class AbilityBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerAbilityManager abilityManager;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private AbilitySlotUI slotPrefab;

    [Header("Settings")]
    [SerializeField] private bool autoPopulateOnStart = true;
    [SerializeField] private bool showLockedAbilities = false;
    [SerializeField] private int maxVisibleSlots = 4;

    private List<AbilitySlotUI> activeSlots = new List<AbilitySlotUI>();

    private void Start()
    {
        if (abilityManager == null)
        {
            abilityManager = FindFirstObjectByType<TowerAbilityManager>();
            if (abilityManager == null)
            {
                Debug.LogError("[AbilityBarUI] No TowerAbilityManager found in scene!");
                return;
            }
        }

        // Subscribe to manager events
        abilityManager.OnAbilityActivated += Manager_OnAbilityActivated;
        abilityManager.OnAbilityDeactivated += Manager_OnAbilityDeactivated;
        abilityManager.OnAbilityUnlocked += Manager_OnAbilityUnlocked;

        if (autoPopulateOnStart)
        {
            PopulateSlots();
        }
    }

    private void OnDisable()
    {
        if (abilityManager != null)
        {
            abilityManager.OnAbilityActivated -= Manager_OnAbilityActivated;
            abilityManager.OnAbilityDeactivated -= Manager_OnAbilityDeactivated;
            abilityManager.OnAbilityUnlocked -= Manager_OnAbilityUnlocked;
        }
    }

    /// <summary>
    /// Creates ability slots based on available abilities from the manager.
    /// </summary>
    public void PopulateSlots()
    {
        ClearSlots();

        if (abilityManager == null || slotPrefab == null || slotsContainer == null)
        {
            Debug.LogWarning("[AbilityBarUI] Missing references, cannot populate slots.");
            return;
        }

        var abilities = abilityManager.Abilities;
        int slotsCreated = 0;

        foreach (var ability in abilities)
        {
            if (slotsCreated >= maxVisibleSlots) break;

            // Skip locked abilities if configured
            if (!showLockedAbilities && ability.IsLocked) continue;

            CreateSlot(ability);
            slotsCreated++;
        }
    }

    /// <summary>
    /// Refreshes all slots to reflect current ability states.
    /// Call this when abilities are unlocked/locked dynamically.
    /// </summary>
    public void RefreshSlots()
    {
        PopulateSlots();
    }

    /// <summary>
    /// Clears all ability slots from the UI.
    /// </summary>
    public void ClearSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                slot.UnbindAbility();
                slot.OnSlotClicked -= Slot_OnClicked;
                Destroy(slot.gameObject);
            }
        }
        activeSlots.Clear();
    }

    private AbilitySlotUI CreateSlot(TowerAbility ability)
    {
        var slotGO = Instantiate(slotPrefab.gameObject, slotsContainer);
        var slot = slotGO.GetComponent<AbilitySlotUI>();

        if (slot != null)
        {
            slot.BindAbility(ability);
            slot.OnSlotClicked += Slot_OnClicked;
            activeSlots.Add(slot);
        }

        return slot;
    }

    /// <summary>
    /// Gets the slot UI for a specific ability.
    /// </summary>
    public AbilitySlotUI GetSlot(TowerAbility ability)
    {
        return activeSlots.Find(s => s.BoundAbility == ability);
    }

    /// <summary>
    /// Gets the slot at the specified index.
    /// </summary>
    public AbilitySlotUI GetSlot(int index)
    {
        if (index < 0 || index >= activeSlots.Count) return null;
        return activeSlots[index];
    }

    #region Event Handlers

    private void Manager_OnAbilityActivated(TowerAbility ability)
    {
        // Visual feedback handled by individual AbilitySlotUI
        // This can be used for additional bar-level effects if needed
    }

    private void Manager_OnAbilityDeactivated(TowerAbility ability)
    {
        // Visual feedback handled by individual AbilitySlotUI
    }

    private void Manager_OnAbilityUnlocked(TowerAbility ability)
    {
        // Refresh slots to show newly unlocked ability
        RefreshSlots();
    }

    private void Slot_OnClicked(AbilitySlotUI slot)
    {
        if (slot.BoundAbility != null && abilityManager != null)
        {
            abilityManager.TryActivateAbility(slot.BoundAbility);
        }
    }

    #endregion
}
