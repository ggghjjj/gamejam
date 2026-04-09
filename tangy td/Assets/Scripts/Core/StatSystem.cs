using UnityEngine;

/// <summary>
/// 通用数值属性：FinalValue = (BaseValue + FlatBonus) * Multiplier
/// 用于英雄和防御塔的所有可升级属性
/// </summary>
[System.Serializable]
public class Stat
{
    public float BaseValue;
    public float FlatBonus;
    public float Multiplier = 1f;

    public float Value => (BaseValue + FlatBonus) * Multiplier;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
        FlatBonus = 0f;
        Multiplier = 1f;
    }

    public void AddFlat(float amount) => FlatBonus += amount;
    public void AddMultiplier(float percent) => Multiplier += percent / 100f;

    public override string ToString() => $"{Value:F1} (base:{BaseValue} +{FlatBonus} x{Multiplier:F2})";
}
