using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public WaypointPath path;
    public WaypointPath[] paths; // multiple paths for variety

    [Header("Base Enemy Stats")]
    public float baseHP = 15f;          // weaker early (was 30)
    public float baseSpeed = 1.5f;      // slower early (was 2)
    public int baseExp = 8;

    public void SpawnEnemy(EnemyType type, int wave, float hpMult, float speedMult)
    {
        float hp, speed, scale;
        int exp, gold;
        Color color;

        switch (type)
        {
            case EnemyType.Fast:
                hp = baseHP * 0.5f * hpMult;
                speed = baseSpeed * 1.8f * speedMult;
                exp = baseExp + 5;
                gold = 1;
                color = new Color(0.2f, 0.85f, 0.3f);
                scale = 0.18f;
                break;
            case EnemyType.Tank:
                hp = baseHP * 3f * hpMult;
                speed = baseSpeed * 0.6f * speedMult;
                exp = baseExp + 15;
                gold = 5;
                color = new Color(0.6f, 0.2f, 0.8f);
                scale = 0.4f;
                break;
            case EnemyType.Boss:
                hp = baseHP * 10f * hpMult;
                speed = baseSpeed * 0.5f * speedMult;
                exp = baseExp * 5;
                gold = 20;
                color = new Color(1f, 0.5f, 0.1f);
                scale = 0.6f;
                break;
            default: // Normal
                hp = baseHP * hpMult;
                speed = baseSpeed * speedMult;
                exp = baseExp;
                gold = 2;
                color = new Color(0.9f, 0.2f, 0.2f);
                scale = 0.25f;
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
        enemy.Init(usedPath, hp, speed, exp, color, scale, gold);

        var tracker = enemyGO.AddComponent<DestroyNotifier>();
        tracker.onDestroy += () => WaveManager.Instance?.OnEnemyDied();
    }
}

public class DestroyNotifier : MonoBehaviour
{
    public System.Action onDestroy;
    private void OnDestroy() { onDestroy?.Invoke(); }
}
