using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public WaypointPath path;

    [Header("Wave Settings")]
    public int enemiesPerWave = 5;
    public float spawnInterval = 1f;
    public float waveCooldown = 5f;
    public float hpScalePerWave = 1.2f;
    public float speedScalePerWave = 1.05f;

    [Header("Base Enemy Stats")]
    public float baseHP = 30f;
    public float baseSpeed = 2f;
    public int baseExp = 10;

    private int _currentWave = 0;
    private int _enemiesAlive = 0;
    private bool _spawning = false;

    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        _currentWave++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentWave = _currentWave;
        }
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        _spawning = true;
        int count = enemiesPerWave + (_currentWave - 1) * 2; // more enemies each wave
        float hpMult = Mathf.Pow(hpScalePerWave, _currentWave - 1);
        float speedMult = Mathf.Pow(speedScalePerWave, _currentWave - 1);

        for (int i = 0; i < count; i++)
        {
            if (GameManager.Instance != null && GameManager.Instance.state != GameManager.GameState.Playing)
            {
                yield return new WaitUntil(() =>
                    GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing);
            }

            SpawnEnemy(baseHP * hpMult, baseSpeed * speedMult, baseExp);
            yield return new WaitForSeconds(spawnInterval);
        }
        _spawning = false;

        // Wait for all enemies to die, then start next wave
        yield return new WaitUntil(() => _enemiesAlive <= 0);
        yield return new WaitForSeconds(waveCooldown);

        if (GameManager.Instance == null || GameManager.Instance.state == GameManager.GameState.Playing)
        {
            StartNextWave();
        }
    }

    private void SpawnEnemy(float hp, float speed, int exp)
    {
        GameObject enemyGO = new GameObject($"Enemy_W{_currentWave}");
        enemyGO.AddComponent<SpriteRenderer>();
        enemyGO.AddComponent<CircleCollider2D>();

        EnemyBase enemy = enemyGO.AddComponent<EnemyBase>();
        enemy.Init(path, hp, speed, exp, new Color(0.9f, 0.2f, 0.2f));

        _enemiesAlive++;
        // Track when enemy is destroyed
        var tracker = enemyGO.AddComponent<DestroyNotifier>();
        tracker.onDestroy += () => _enemiesAlive--;
    }
}

// Helper to detect enemy destruction
public class DestroyNotifier : MonoBehaviour
{
    public System.Action onDestroy;

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
