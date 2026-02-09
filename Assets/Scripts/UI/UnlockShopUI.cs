using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container panel for the tower unlock shop UI. Instantiates TowerUnlockSlotUI slots
/// from a prefab for each unlockable tower in the UnlockManager.
/// </summary>
public class UnlockShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnlockManager unlockManager;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private TowerUnlockSlotUI slotPrefab;
    [SerializeField] private GameObject panelRoot;

    private List<TowerUnlockSlotUI> activeSlots = new List<TowerUnlockSlotUI>();

    private void Start()
    {
        if (unlockManager == null)
        {
            unlockManager = UnlockManager.Instance;
            if (unlockManager == null)
            {
                Debug.LogError("[UnlockShopUI] No UnlockManager found!");
                return;
            }
        }

        PopulateSlots();
    }

    /// <summary>
    /// Creates tower unlock slots for all unlockable towers.
    /// </summary>
    public void PopulateSlots()
    {
        ClearSlots();

        if (unlockManager == null || slotPrefab == null || slotsContainer == null)
        {
            Debug.LogWarning("[UnlockShopUI] Missing references, cannot populate slots.");
            return;
        }

        var towers = unlockManager.GetAllUnlockableTowers();
        if (towers == null) return;

        foreach (var tower in towers)
        {
            if (tower == null) continue;

            var slotGO = Instantiate(slotPrefab.gameObject, slotsContainer);
            var slot = slotGO.GetComponent<TowerUnlockSlotUI>();

            if (slot != null)
            {
                slot.BindTower(tower, unlockManager);
                slot.OnSlotClicked += Slot_OnClicked;
                activeSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// Clears all tower unlock slots.
    /// </summary>
    public void ClearSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= Slot_OnClicked;
                slot.UnbindTower();
                Destroy(slot.gameObject);
            }
        }
        activeSlots.Clear();
    }

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }

    private void Slot_OnClicked(TowerUnlockSlotUI slot)
    {
        if (slot.BoundTower == null || unlockManager == null) return;

        string towerId = slot.BoundTower.towerId;

        if (!unlockManager.IsTowerUnlocked(towerId))
        {
            unlockManager.TryUnlockTower(towerId);
        }
        else if (unlockManager.CanUpgrade(towerId))
        {
            unlockManager.TryUpgradeTower(towerId);
        }
    }
}
