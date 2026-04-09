using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "TangyTD/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName = "New Upgrade";
    [TextArea] public string description = "";
    public Color cardColor = Color.white;
    public UpgradeEffect effect;
    public float value = 1f;

    // For synthesis: if player has these two upgrades, they combine into this one
    public UpgradeData synthesisIngredientA;
    public UpgradeData synthesisIngredientB;
    public bool isSynthesized = false;
}

public enum UpgradeEffect
{
    AttackDamage,       // +flat damage
    AttackSpeed,        // +% attack speed
    MoveSpeed,          // +% move speed
    AttackRange,        // +flat range
    MultiShot,          // +1 extra projectile
    PierceShot,         // bullets pierce through enemies
    MaxHP,              // +flat max HP
    HPRegen,            // +HP per second
    ExpBonus,           // +% exp gained
    CritChance,         // +% crit chance
}
