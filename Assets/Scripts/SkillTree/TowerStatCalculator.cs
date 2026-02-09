using UnityEngine;

/// <summary>
/// Static utility that computes modified tower stats based on purchased skills.
/// All methods return base values unchanged when no skills are purchased.
/// </summary>
public static class TowerStatCalculator
{
    /// <summary>
    /// Get the total accumulated value for a stat modifier type across all purchased skills.
    /// </summary>
    private static float GetTotalValue(StatModifierType statType)
    {
        if (SkillTreeManager.Instance == null) return 0f;

        var allSkills = SkillTreeManager.Instance.AllSkills;
        if (allSkills == null) return 0f;

        float total = 0f;
        foreach (var skill in allSkills)
        {
            if (skill == null || skill.statType != statType) continue;

            int count = SkillTreeManager.Instance.GetPurchaseCount(skill.skillId);
            total += skill.valuePerPurchase * count;
        }
        return total;
    }

    // --- Percentage-based modifiers (base * (1 + total%/100)) ---

    public static float GetModifiedDamage(float baseDamage)
    {
        float bonus = GetTotalValue(StatModifierType.Damage);
        return baseDamage * (1f + bonus / 100f);
    }

    public static float GetModifiedFireRate(float baseRate)
    {
        float bonus = GetTotalValue(StatModifierType.FireRate);
        return baseRate * (1f + bonus / 100f);
    }

    public static float GetModifiedRange(float baseRange)
    {
        float bonus = GetTotalValue(StatModifierType.Range);
        return baseRange * (1f + bonus / 100f);
    }

    // --- Additive modifiers ---

    public static float GetModifiedAbilityDuration(float baseDuration)
    {
        float bonus = GetTotalValue(StatModifierType.AbilityDuration);
        return baseDuration + bonus;
    }

    // --- Reduction modifiers (base * (1 - total%/100)) ---

    public static float GetModifiedAbilityCooldown(float baseCooldown)
    {
        float reduction = GetTotalValue(StatModifierType.AbilityCooldown);
        return baseCooldown * (1f - Mathf.Clamp01(reduction / 100f));
    }

    // --- Tower-specific: Burn ---

    public static float GetModifiedBurnDuration(float baseDuration)
    {
        float bonus = GetTotalValue(StatModifierType.BurnDuration);
        return baseDuration + bonus;
    }

    public static float GetModifiedBurnDamage(float baseDamage)
    {
        float bonus = GetTotalValue(StatModifierType.BurnDamage);
        return baseDamage * (1f + bonus / 100f);
    }

    // --- Tower-specific: Electric ---

    public static bool GetChainLightningEnabled()
    {
        return GetTotalValue(StatModifierType.ChainLightningUnlock) > 0f;
    }

    public static int GetChainCount()
    {
        return Mathf.RoundToInt(GetTotalValue(StatModifierType.ChainCount));
    }

    public static float GetChainChance()
    {
        return GetTotalValue(StatModifierType.ChainChance);
    }

    // --- Tower-specific: Sniper ---

    public static int GetPierceCount()
    {
        return Mathf.RoundToInt(GetTotalValue(StatModifierType.Pierce));
    }

    public static float GetCriticalChance()
    {
        return GetTotalValue(StatModifierType.CriticalChance);
    }
}
