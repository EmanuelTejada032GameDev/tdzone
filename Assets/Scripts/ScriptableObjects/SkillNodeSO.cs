using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Node", menuName = "TD Zone/Skill Node")]
public class SkillNodeSO : ScriptableObject
{
    [Header("Identity")]
    public string skillId;
    public string skillName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Purchase Settings")]
    [Tooltip("How many times this skill can be purchased")]
    public int maxPurchases = 4;
    [Tooltip("Cost of the first purchase")]
    public int baseCost = 100;
    [Tooltip("Cost multiplier per purchase (cost = baseCost * costMultiplier^currentCount)")]
    public float costMultiplier = 1.5f;

    [Header("Stat Modification")]
    public StatModifierType statType;
    [Tooltip("Value added per purchase (e.g., 5 for +5% damage per purchase)")]
    public float valuePerPurchase = 5f;

    [Header("Requirements")]
    [Tooltip("Skills that must be maxed out before this one can be purchased")]
    public SkillNodeSO[] prerequisites;
    [Tooltip("Tower that must be unlocked before this skill is available (optional)")]
    public TowerDataSO requiredTowerUnlock;

    /// <summary>
    /// Get the cost for the next purchase given how many times already purchased.
    /// </summary>
    public int GetCostForNextPurchase(int currentCount)
    {
        if (currentCount >= maxPurchases)
            return -1;

        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentCount));
    }
}
