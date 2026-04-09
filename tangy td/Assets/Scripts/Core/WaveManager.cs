using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public EnemySpawner[] spawners; // one per path

    [Header("Continuous Spawn Settings")]
    public float baseSpawnInterval = 0.35f;    // denser from start
    public float minSpawnInterval = 0.05f;     // very fast late
    public float intervalDecayPerMinute = 0.12f;
    public float hpScalePerMinute = 1.3f;      // enemy HP grows over time
    public float speedScalePerMinute = 1.1f;
    public int bossEveryNKills = 80;           // boss every N kills

    [Header("Runtime")]
    public int currentWave = 0;
    public float elapsedTime = 0f;
    public int totalSpawned = 0;
    public int totalEnemiesForLevel = 100; // set based on level
    public int enemiesKilledThisLevel = 0;

    private int _enemiesAlive = 0;
    private float _waveTimer = 0f;

    // Events
    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveComplete;
    public System.Action<float> OnCooldownTick;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Total enemies based on level difficulty
        int level = GameSetup.CurrentLevel;
        totalEnemiesForLevel = 80 + level * 30; // 80, 110, 140, ...
        StartCoroutine(ContinuousSpawnLoop());
    }

    private IEnumerator ContinuousSpawnLoop()
    {
        // Wait for game to start
        yield return new WaitUntil(() =>
            GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing);

        yield return new WaitForSeconds(1f);
        currentWave = 1;
        if (GameManager.Instance != null) GameManager.Instance.currentWave = currentWave;
        OnWaveStart?.Invoke(currentWave);

        while (true)
        {
            // Check game state
            if (GameManager.Instance != null &&
                (GameManager.Instance.state == GameManager.GameState.GameOver ||
                 GameManager.Instance.state == GameManager.GameState.Victory))
                yield break;

            // Wait while paused
            yield return new WaitUntil(() =>
                GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing);

            elapsedTime += Time.deltaTime;
            _waveTimer += Time.deltaTime;

            // Stop spawning when reached total
            if (totalSpawned >= totalEnemiesForLevel)
            {
                yield return null;
                continue;
            }

            // Cosmetic wave counter (every 30 seconds)
            if (_waveTimer >= 30f)
            {
                _waveTimer = 0f;
                currentWave++;
                if (GameManager.Instance != null) GameManager.Instance.currentWave = currentWave;
                OnWaveStart?.Invoke(currentWave);

                // Victory check
                if (GameManager.Instance != null && currentWave > GameManager.Instance.wavesPerLevel)
                {
                    // Wait for remaining enemies
                    yield return new WaitUntil(() => _enemiesAlive <= 0);
                    GameManager.Instance.SetVictory();
                    yield break;
                }
            }

            // Calculate difficulty based on elapsed time
            float minutes = elapsedTime / 60f;
            float hpMult = Mathf.Pow(hpScalePerMinute, minutes);
            float speedMult = Mathf.Pow(speedScalePerMinute, minutes);
            float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - minutes * intervalDecayPerMinute);

            // Spawn an enemy on a random path's spawner
            EnemyType type = PickEnemyType();
            EnemySpawner chosenSpawner = spawners[totalSpawned % spawners.Length];
            chosenSpawner.SpawnEnemy(type, currentWave, hpMult, speedMult);
            _enemiesAlive++;
            totalSpawned++;

            // Boss every N kills
            if (GameManager.Instance != null && GameManager.Instance.totalKills > 0 &&
                GameManager.Instance.totalKills % bossEveryNKills == 0)
            {
                yield return new WaitForSeconds(0.3f);
                EnemySpawner bossSpawner = spawners[0];
                for (int i = 0; i < 2; i++)
                {
                    bossSpawner.SpawnEnemy(EnemyType.Tank, currentWave, hpMult, speedMult);
                    _enemiesAlive++;
                    yield return new WaitForSeconds(0.2f);
                }
                bossSpawner.SpawnEnemy(EnemyType.Boss, currentWave, hpMult, speedMult);
                _enemiesAlive++;
                ScreenShake.Shake(0.25f, 0.4f);
                SFXManager.PlayBoss();
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private EnemyType PickEnemyType()
    {
        float minutes = elapsedTime / 60f;

        // Early: all normal
        if (minutes < 0.5f) return EnemyType.Normal;

        float roll = Random.value;
        // Gradually introduce variety
        if (minutes >= 2f && roll < 0.1f) return EnemyType.Tank;
        if (minutes >= 0.5f && roll < 0.35f) return EnemyType.Fast;
        return EnemyType.Normal;
    }

    public void OnEnemyDied()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        enemiesKilledThisLevel++;

        // Victory: all enemies spawned and killed
        if (totalSpawned >= totalEnemiesForLevel && _enemiesAlive <= 0)
        {
            GameManager.Instance?.SetVictory();
        }
    }
}

public enum EnemyType
{
    Normal,
    Fast,
    Tank,
    Boss
}
