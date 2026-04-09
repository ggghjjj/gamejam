using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public EnemySpawner spawner;

    [Header("Continuous Spawn Settings")]
    public float baseSpawnInterval = 0.5f;     // seconds between spawns
    public float minSpawnInterval = 0.08f;     // fastest spawn rate
    public float intervalDecayPerMinute = 0.05f; // spawn gets faster over time
    public float hpScalePerMinute = 1.3f;      // enemy HP grows over time
    public float speedScalePerMinute = 1.1f;
    public int bossEveryNKills = 80;           // boss every N kills

    [Header("Runtime")]
    public int currentWave = 0; // purely cosmetic, increments every ~30s
    public float elapsedTime = 0f;
    public int totalSpawned = 0;

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

            // Spawn an enemy
            EnemyType type = PickEnemyType();
            spawner.SpawnEnemy(type, currentWave, hpMult, speedMult);
            _enemiesAlive++;
            totalSpawned++;

            // Boss every N kills
            if (GameManager.Instance != null && GameManager.Instance.totalKills > 0 &&
                GameManager.Instance.totalKills % bossEveryNKills == 0)
            {
                yield return new WaitForSeconds(0.3f);
                for (int i = 0; i < 2; i++)
                {
                    spawner.SpawnEnemy(EnemyType.Tank, currentWave, hpMult, speedMult);
                    _enemiesAlive++;
                    yield return new WaitForSeconds(0.2f);
                }
                spawner.SpawnEnemy(EnemyType.Boss, currentWave, hpMult, speedMult);
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
    }
}

public enum EnemyType
{
    Normal,
    Fast,
    Tank,
    Boss
}
