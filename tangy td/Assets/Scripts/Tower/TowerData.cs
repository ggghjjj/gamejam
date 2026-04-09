using UnityEngine;

public enum TowerType
{
    Archer,  // 射手：射最近敌人
    Warrior, // 战士：范围挥砍
    Mage     // 法师：AOE 轰炸
}

[CreateAssetMenu(fileName = "NewTower", menuName = "TangyTD/TowerData")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public TowerType type;
    public Color color;
    public float baseDamage;
    public float baseAttackSpeed; // attacks per second
    public float baseRange;
    public float spriteScale = 0.6f;
}
