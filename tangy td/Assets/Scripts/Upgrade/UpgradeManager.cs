using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Pool")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();
    public List<UpgradeData> acquiredUpgrades = new List<UpgradeData>();

    // Hero stat modifiers (applied on top of base stats)
    public float bonusDamage = 0f;
    public float attackSpeedMult = 1f;
    public float moveSpeedMult = 1f;
    public float bonusRange = 0f;
    public int extraProjectiles = 0;
    public bool pierceShot = false;
    public float bonusMaxHP = 0f;
    public float hpRegen = 0f;
    public float expBonusMult = 1f;
    public float critChance = 0f;

    // Events
    public System.Action<List<UpgradeData>> OnUpgradeChoicesReady; // 3 choices
    public System.Action<UpgradeData> OnUpgradeApplied;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildDefaultUpgradePool();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUp += OnLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUp -= OnLevelUp;
        }
    }

    private void Update()
    {
        // HP regen
        if (hpRegen > 0f && GameManager.Instance != null && GameManager.Instance.state == GameManager.GameState.Playing)
        {
            HeroController hero = FindAnyObjectByType<HeroController>();
            if (hero != null && hero.currentHP < hero.maxHP)
            {
                hero.currentHP = Mathf.Min(hero.maxHP, hero.currentHP + hpRegen * Time.deltaTime);
            }
        }
    }

    private void OnLevelUp(int newLevel)
    {
        OfferUpgradeChoices();
    }

    public void OfferUpgradeChoices()
    {
        List<UpgradeData> choices = GetRandomChoices(3);
        if (choices.Count > 0)
        {
            GameManager.Instance?.PauseGame();
            OnUpgradeChoicesReady?.Invoke(choices);
        }
    }

    public List<UpgradeData> GetRandomChoices(int count)
    {
        List<UpgradeData> pool = new List<UpgradeData>(allUpgrades);
        List<UpgradeData> choices = new List<UpgradeData>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return choices;
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        acquiredUpgrades.Add(upgrade);
        ApplyEffect(upgrade);
        OnUpgradeApplied?.Invoke(upgrade);

        // Check for synthesis
        CheckSynthesis();

        GameManager.Instance?.ResumeGame();
    }

    private void ApplyEffect(UpgradeData upgrade)
    {
        HeroController hero = FindAnyObjectByType<HeroController>();

        switch (upgrade.effect)
        {
            case UpgradeEffect.AttackDamage:
                bonusDamage += upgrade.value;
                if (hero != null) hero.attackDamage += upgrade.value;
                break;
            case UpgradeEffect.AttackSpeed:
                attackSpeedMult += upgrade.value / 100f;
                if (hero != null) hero.attackSpeed *= (1f + upgrade.value / 100f);
                break;
            case UpgradeEffect.MoveSpeed:
                moveSpeedMult += upgrade.value / 100f;
                if (hero != null) hero.moveSpeed *= (1f + upgrade.value / 100f);
                break;
            case UpgradeEffect.AttackRange:
                bonusRange += upgrade.value;
                if (hero != null) hero.attackRange += upgrade.value;
                break;
            case UpgradeEffect.MultiShot:
                extraProjectiles += (int)upgrade.value;
                break;
            case UpgradeEffect.PierceShot:
                pierceShot = true;
                break;
            case UpgradeEffect.MaxHP:
                bonusMaxHP += upgrade.value;
                if (hero != null) { hero.maxHP += upgrade.value; hero.currentHP += upgrade.value; }
                break;
            case UpgradeEffect.HPRegen:
                hpRegen += upgrade.value;
                break;
            case UpgradeEffect.ExpBonus:
                expBonusMult += upgrade.value / 100f;
                break;
            case UpgradeEffect.CritChance:
                critChance += upgrade.value / 100f;
                break;
        }
    }

    private void CheckSynthesis()
    {
        // Check all upgrades for synthesis combos
        foreach (var upgrade in allUpgrades)
        {
            if (!upgrade.isSynthesized) continue;
            if (upgrade.synthesisIngredientA == null || upgrade.synthesisIngredientB == null) continue;
            if (acquiredUpgrades.Contains(upgrade)) continue;

            bool hasA = acquiredUpgrades.Contains(upgrade.synthesisIngredientA);
            bool hasB = acquiredUpgrades.Contains(upgrade.synthesisIngredientB);

            if (hasA && hasB)
            {
                Debug.Log($"Synthesis! {upgrade.synthesisIngredientA.upgradeName} + {upgrade.synthesisIngredientB.upgradeName} = {upgrade.upgradeName}");
                acquiredUpgrades.Add(upgrade);
                ApplyEffect(upgrade);
            }
        }
    }

    private void BuildDefaultUpgradePool()
    {
        allUpgrades.Clear();
        allUpgrades.Add(CreateUpgrade("\u5229\u5203", "\u653b\u51fb\u529b +5", UpgradeEffect.AttackDamage, 5f, new Color(1f, 0.3f, 0.3f)));
        allUpgrades.Add(CreateUpgrade("\u8fc5\u6377\u4e4b\u624b", "\u653b\u901f +20%", UpgradeEffect.AttackSpeed, 20f, new Color(1f, 0.8f, 0.2f)));
        allUpgrades.Add(CreateUpgrade("\u98ce\u4e4b\u9774", "\u79fb\u901f +15%", UpgradeEffect.MoveSpeed, 15f, new Color(0.3f, 0.9f, 1f)));
        allUpgrades.Add(CreateUpgrade("\u9e70\u773c", "\u653b\u51fb\u8303\u56f4 +1", UpgradeEffect.AttackRange, 1f, new Color(0.5f, 1f, 0.5f)));
        allUpgrades.Add(CreateUpgrade("\u591a\u91cd\u5c04\u51fb", "\u989d\u5916\u5f39\u5c04\u7269 +1", UpgradeEffect.MultiShot, 1f, new Color(0.9f, 0.5f, 0.1f)));
        allUpgrades.Add(CreateUpgrade("\u7a7f\u900f\u4e4b\u7bad", "\u5b50\u5f39\u53ef\u7a7f\u900f\u654c\u4eba", UpgradeEffect.PierceShot, 1f, new Color(0.7f, 0.3f, 1f)));
        allUpgrades.Add(CreateUpgrade("\u751f\u547d\u529b", "\u6700\u5927\u751f\u547d +25", UpgradeEffect.MaxHP, 25f, new Color(0.2f, 0.9f, 0.2f)));
        allUpgrades.Add(CreateUpgrade("\u518d\u751f", "\u6bcf\u79d2\u56de\u590d 2 \u751f\u547d", UpgradeEffect.HPRegen, 2f, new Color(0.4f, 1f, 0.6f)));
        allUpgrades.Add(CreateUpgrade("\u667a\u6167", "\u7ecf\u9a8c\u83b7\u53d6 +25%", UpgradeEffect.ExpBonus, 25f, new Color(0.6f, 0.6f, 1f)));
        allUpgrades.Add(CreateUpgrade("\u5e78\u8fd0\u4e00\u51fb", "\u66b4\u51fb\u7387 +10%", UpgradeEffect.CritChance, 10f, new Color(1f, 1f, 0.3f)));
    }

    private UpgradeData CreateUpgrade(string name, string desc, UpgradeEffect effect, float value, Color color)
    {
        UpgradeData data = ScriptableObject.CreateInstance<UpgradeData>();
        data.upgradeName = name;
        data.description = desc;
        data.effect = effect;
        data.value = value;
        data.cardColor = color;
        return data;
    }
}
