using UnityEngine;

/// <summary>
/// Persistent player data that survives scene reloads.
/// Uses PlayerPrefs for simplicity in Game Jam.
/// </summary>
public static class PlayerSave
{
    public static int Diamonds
    {
        get => PlayerPrefs.GetInt("diamonds", 0);
        set { PlayerPrefs.SetInt("diamonds", value); PlayerPrefs.Save(); }
    }

    public static int GetLevelStars(int level) => PlayerPrefs.GetInt($"level_{level}_stars", 0);
    public static void SetLevelStars(int level, int stars)
    {
        if (stars > GetLevelStars(level))
        {
            PlayerPrefs.SetInt($"level_{level}_stars", stars);
            PlayerPrefs.Save();
        }
    }

    public static bool IsLevelCleared(int level) => GetLevelStars(level) > 0;

    public static int MaxUnlockedLevel
    {
        get => PlayerPrefs.GetInt("max_level", 0);
        set { PlayerPrefs.SetInt("max_level", value); PlayerPrefs.Save(); }
    }

    // Talent points spent
    public static int GetTalent(string key) => PlayerPrefs.GetInt($"talent_{key}", 0);
    public static void SetTalent(string key, int value)
    {
        PlayerPrefs.SetInt($"talent_{key}", value);
        PlayerPrefs.Save();
    }

    public static int TotalTalentSpent
    {
        get => PlayerPrefs.GetInt("talent_total_spent", 0);
        set { PlayerPrefs.SetInt("talent_total_spent", value); PlayerPrefs.Save(); }
    }

    public static void ResetTalents()
    {
        string[] keys = { "atk", "atkspd", "hp", "range", "movspd", "crit", "gold", "exp" };
        int refund = 0;
        foreach (var k in keys)
        {
            refund += GetTalent(k);
            SetTalent(k, 0);
        }
        Diamonds += refund;
        TotalTalentSpent = 0;
    }

    // Debug
    public static void AddTestDiamonds(int amount)
    {
        Diamonds += amount;
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
