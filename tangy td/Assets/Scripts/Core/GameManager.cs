using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { WaitingToStart, Playing, Paused, GameOver, Victory }

    [Header("Game State")]
    public GameState state = GameState.WaitingToStart;
    public int currentWave = 0;
    public int wavesPerLevel = 10;

    [Header("Player Stats")]
    public int playerLives = 30;
    public int experience = 0;
    public int level = 1;
    public int expToNextLevel = 30;
    public float expScalePerLevel = 1.3f;
    public int totalKills = 0;
    public int gold = 0;

    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action<int, int> OnExpChanged;
    public System.Action<int> OnLevelUp;
    public System.Action OnGameOver;
    public System.Action OnGameStart;
    public System.Action OnVictory;
    public System.Action<int> OnGoldChanged;

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
            SFXManager.PlayLevelUp();
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

    public void StartGame()
    {
        if (state != GameState.WaitingToStart) return;
        state = GameState.Playing;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
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

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public void SetVictory()
    {
        if (state == GameState.Victory || state == GameState.GameOver) return;
        state = GameState.Victory;
        OnVictory?.Invoke();
    }
}
