using System;
using UnityEngine;

public enum PurchaseFailReason
{
    None,
    MaxedOut,
    PrerequisiteNotMet,
    TowerNotUnlocked,
    InsufficientCurrency
}

/// <summary>
/// Manages skill purchases - validates requirements and coordinates with PlayerProgressionManager.
/// </summary>
public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    [Header("Skill Definitions")]
    [SerializeField] private SkillNodeSO[] allSkills;

    // Events
    public event Action<SkillNodeSO> OnSkillPurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Get all skill definitions.
    /// </summary>
    public SkillNodeSO[] AllSkills => allSkills;

    /// <summary>
    /// Get how many times a skill has been purchased.
    /// </summary>
    public int GetPurchaseCount(string skillId)
    {
        if (PlayerProgressionManager.Instance == null) return 0;
        return PlayerProgressionManager.Instance.GetSkillPurchaseCount(skillId);
    }

    /// <summary>
    /// Get the SkillNodeSO by skillId.
    /// </summary>
    public SkillNodeSO GetSkillNode(string skillId)
    {
        if (allSkills == null) return null;

        foreach (var skill in allSkills)
        {
            if (skill != null && skill.skillId == skillId)
                return skill;
        }
        return null;
    }

    /// <summary>
    /// Get the cost of the next purchase for a skill.
    /// Returns -1 if maxed out.
    /// </summary>
    public int GetNextCost(SkillNodeSO skillNode)
    {
        if (skillNode == null) return -1;

        int currentCount = GetPurchaseCount(skillNode.skillId);
        return skillNode.GetCostForNextPurchase(currentCount);
    }

    /// <summary>
    /// Check if a skill can be purchased right now.
    /// </summary>
    public bool CanPurchase(SkillNodeSO skillNode)
    {
        return GetPurchaseFailReason(skillNode) == PurchaseFailReason.None;
    }

    /// <summary>
    /// Get the reason why a skill cannot be purchased.
    /// Returns PurchaseFailReason.None if purchase is valid.
    /// </summary>
    public PurchaseFailReason GetPurchaseFailReason(SkillNodeSO skillNode)
    {
        if (skillNode == null) return PurchaseFailReason.MaxedOut;
        if (PlayerProgressionManager.Instance == null) return PurchaseFailReason.InsufficientCurrency;

        int currentCount = GetPurchaseCount(skillNode.skillId);

        // Check max purchases
        if (currentCount >= skillNode.maxPurchases)
            return PurchaseFailReason.MaxedOut;

        // Check prerequisites
        if (skillNode.prerequisites != null)
        {
            foreach (var prereq in skillNode.prerequisites)
            {
                if (prereq == null) continue;

                int prereqCount = GetPurchaseCount(prereq.skillId);
                if (prereqCount < 1)
                    return PurchaseFailReason.PrerequisiteNotMet;
            }
        }

        // Check tower unlock requirement
        if (skillNode.requiredTowerUnlock != null)
        {
            if (!PlayerProgressionManager.Instance.IsTowerUnlocked(skillNode.requiredTowerUnlock.towerId))
                return PurchaseFailReason.TowerNotUnlocked;
        }

        // Check currency
        int cost = skillNode.GetCostForNextPurchase(currentCount);
        if (!PlayerProgressionManager.Instance.CanAfford(cost))
            return PurchaseFailReason.InsufficientCurrency;

        return PurchaseFailReason.None;
    }

    /// <summary>
    /// Attempt to purchase a skill. Returns true if successful.
    /// </summary>
    public bool TryPurchaseSkill(SkillNodeSO skillNode)
    {
        PurchaseFailReason reason = GetPurchaseFailReason(skillNode);
        if (reason != PurchaseFailReason.None)
        {
            Debug.Log($"[SkillTree] Cannot purchase {skillNode.skillName}: {reason}");
            return false;
        }

        int cost = GetNextCost(skillNode);

        // Spend currency
        if (!PlayerProgressionManager.Instance.SpendCurrency(cost))
            return false;

        // Record purchase
        PlayerProgressionManager.Instance.PurchaseSkill(skillNode.skillId);

        int newCount = GetPurchaseCount(skillNode.skillId);
        Debug.Log($"[SkillTree] Purchased {skillNode.skillName} ({newCount}/{skillNode.maxPurchases}) for {cost} currency");

        OnSkillPurchased?.Invoke(skillNode);

        return true;
    }

    #region Debug

    [ContextMenu("Debug: Log All Skills")]
    private void DebugLogAllSkills()
    {
        if (allSkills == null)
        {
            Debug.Log("[SkillTree] No skills configured");
            return;
        }

        foreach (var skill in allSkills)
        {
            if (skill == null) continue;

            int count = GetPurchaseCount(skill.skillId);
            int nextCost = GetNextCost(skill);
            bool canBuy = CanPurchase(skill);
            Debug.Log($"[SkillTree] {skill.skillName} ({count}/{skill.maxPurchases}) | Next cost: {nextCost} | Can buy: {canBuy} | Type: {skill.statType} +{skill.valuePerPurchase}/purchase");
        }
    }

    [ContextMenu("Debug: Purchase First Available Skill")]
    private void DebugPurchaseFirstAvailable()
    {
        if (allSkills == null) return;

        foreach (var skill in allSkills)
        {
            if (skill != null && CanPurchase(skill))
            {
                TryPurchaseSkill(skill);
                return;
            }
        }

        Debug.Log("[SkillTree] No skills available for purchase");
    }

    #endregion
}
