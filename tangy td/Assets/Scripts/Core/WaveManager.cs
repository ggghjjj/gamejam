using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public EnemySpawner spawner;

    [Header("Wave Settings")]
    public int baseEnemiesPerWave = 5;
    public int extraEnemiesPerWave = 2;
    public float spawnInterval = 0.8f;
    public float waveCooldown = 4f;
    public float hpScalePerWave = 1.2f;
    public float speedScalePerWave = 1.05f;
    public int bossEveryNWaves = 5;

    [Header("Runtime")]
    public int currentWave = 0;
    public bool waveInProgress = false;

    private int _enemiesAlive = 0;

    // Events
    public System.Action<int> OnWaveStart;          // wave number
    public System.Action<int> OnWaveComplete;       // wave number
    public System.Action<float> OnCooldownTick;     // seconds remaining

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        // Small delay before first wave
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.state == GameManager.GameState.GameOver)
                yield break;

            // Start new wave
            currentWave++;
            waveInProgress = true;
            if (GameManager.Instance != null) GameManager.Instance.currentWave = currentWave;
            OnWaveStart?.Invoke(currentWave);

            // Spawn enemies for this wave
            yield return StartCoroutine(SpawnWaveEnemies());

            // Wait for all enemies to die
            yield return new WaitUntil(() => _enemiesAlive <= 0);

            waveInProgress = false;
            OnWaveComplete?.Invoke(currentWave);

            // Cooldown between waves
            float cooldown = waveCooldown;
            while (cooldown > 0f)
            {
                // Pause-aware wait
                if (GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing)
                {
                    cooldown -= Time.deltaTime;
                    OnCooldownTick?.Invoke(cooldown);
                }
                yield return null;
            }
        }
    }

    private IEnumerator SpawnWaveEnemies()
    {
        int count = baseEnemiesPerWave + (currentWave - 1) * extraEnemiesPerWave;
        float hpMult = Mathf.Pow(hpScalePerWave, currentWave - 1);
        float speedMult = Mathf.Pow(speedScalePerWave, currentWave - 1);

        bool isBossWave = (currentWave % bossEveryNWaves == 0);

        for (int i = 0; i < count; i++)
        {
            // Wait while paused
            yield return new WaitUntil(() =>
                GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing);

            if (GameManager.Instance != null && GameManager.Instance.state == GameManager.GameState.GameOver)
                yield break;

            // Decide enemy type based on wave
            EnemyType type = PickEnemyType(i, count);
            spawner.SpawnEnemy(type, currentWave, hpMult, speedMult);
            _enemiesAlive++;

            yield return new WaitForSeconds(spawnInterval);
        }

        // Spawn boss at end of boss wave
        if (isBossWave)
        {
            yield return new WaitForSeconds(0.5f);
            spawner.SpawnEnemy(EnemyType.Boss, currentWave, hpMult, speedMult);
            _enemiesAlive++;
        }
    }

    private EnemyType PickEnemyType(int index, int total)
    {
        if (currentWave < 3) return EnemyType.Normal;

        // After wave 3, mix in fast enemies
        float roll = Random.value;
        if (currentWave >= 5 && roll < 0.15f) return EnemyType.Tank;
        if (roll < 0.4f) return EnemyType.Fast;
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
