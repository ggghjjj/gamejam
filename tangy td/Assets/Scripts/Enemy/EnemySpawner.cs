using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public WaypointPath path;
    public WaypointPath[] paths; // multiple paths for variety

    [Header("Base Enemy Stats")]
    public float baseHP = 30f;
    public float baseSpeed = 2f;
    public int baseExp = 10;

    public void SpawnEnemy(EnemyType type, int wave, float hpMult, float speedMult)
    {
        float hp, speed, scale;
        int exp;
        Color color;

        switch (type)
        {
            case EnemyType.Fast:
                hp = baseHP * 0.5f * hpMult;
                speed = baseSpeed * 1.8f * speedMult;
                exp = baseExp + 5;
                color = new Color(0.2f, 0.85f, 0.3f); // green
                scale = 0.35f;
                break;
            case EnemyType.Tank:
                hp = baseHP * 3f * hpMult;
                speed = baseSpeed * 0.6f * speedMult;
                exp = baseExp + 15;
                color = new Color(0.6f, 0.2f, 0.8f); // purple
                scale = 0.7f;
                break;
            case EnemyType.Boss:
                hp = baseHP * 10f * hpMult;
                speed = baseSpeed * 0.5f * speedMult;
                exp = baseExp * 5;
                color = new Color(1f, 0.5f, 0.1f); // orange
                scale = 1f;
                break;
            default: // Normal
                hp = baseHP * hpMult;
                speed = baseSpeed * speedMult;
                exp = baseExp;
                color = new Color(0.9f, 0.2f, 0.2f); // red
                scale = 0.5f;
                break;
        }

        // Pick a random path if multiple available
        WaypointPath usedPath = path;
        if (paths != null && paths.Length > 0)
        {
            usedPath = paths[Random.Range(0, paths.Length)];
        }

        GameObject enemyGO = new GameObject($"Enemy_{type}_W{wave}");
        enemyGO.AddComponent<SpriteRenderer>();
        enemyGO.AddComponent<CircleCollider2D>();

        EnemyBase enemy = enemyGO.AddComponent<EnemyBase>();
        enemy.Init(usedPath, hp, speed, exp, color, scale);

        var tracker = enemyGO.AddComponent<DestroyNotifier>();
        tracker.onDestroy += () => WaveManager.Instance?.OnEnemyDied();
    }
}

public class DestroyNotifier : MonoBehaviour
{
    public System.Action onDestroy;
    private void OnDestroy() { onDestroy?.Invoke(); }
}
