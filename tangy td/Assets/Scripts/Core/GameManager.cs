using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver }

    [Header("Game State")]
    public GameState state = GameState.Playing;
    public int currentWave = 0;

    [Header("Player Stats")]
    public int playerLives = 20;
    public int experience = 0;
    public int level = 1;
    public int expToNextLevel = 50;
    public float expScalePerLevel = 1.3f;
    public int totalKills = 0;

    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action<int, int> OnExpChanged;       // current, required
    public System.Action<int> OnLevelUp;
    public System.Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddExperience(int amount)
    {
        if (state != GameState.Playing) return;

        totalKills++;

        // Apply exp bonus from upgrades
        float mult = UpgradeManager.Instance != null ? UpgradeManager.Instance.expBonusMult : 1f;
        experience += Mathf.RoundToInt(amount * mult);
        OnExpChanged?.Invoke(experience, expToNextLevel);

        while (experience >= expToNextLevel)
        {
            experience -= expToNextLevel;
            level++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * expScalePerLevel);
            OnLevelUp?.Invoke(level);
            Debug.Log($"Level Up! Now level {level}. Next level at {expToNextLevel} exp.");
        }
    }

    public void OnEnemyReachedEnd(int damage)
    {
        playerLives -= damage;
        OnLivesChanged?.Invoke(playerLives);
        Debug.Log($"Enemy reached end! Lives: {playerLives}");

        if (playerLives <= 0)
        {
            playerLives = 0;
            SetGameOver();
        }
    }

    public void OnHeroDied()
    {
        SetGameOver();
    }

    private void SetGameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        OnGameOver?.Invoke();
        Debug.Log("GAME OVER!");
    }

    public void PauseGame()
    {
        if (state != GameState.Playing) return;
        state = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (state != GameState.Paused) return;
        state = GameState.Playing;
        Time.timeScale = 1f;
    }
}
