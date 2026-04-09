using UnityEngine;

public class HeroShooter : MonoBehaviour
{
    private HeroController _hero;
    private float _shootTimer;

    private void Awake()
    {
        _hero = GetComponent<HeroController>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.state != GameManager.GameState.Playing)
            return;

        _shootTimer -= Time.deltaTime;
        if (_shootTimer > 0f) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        _shootTimer = 1f / _hero.fireRate;

        Vector2 baseDir = (target.position - transform.position).normalized;
        SpawnBullet(baseDir);

        // MultiShot: spawn extra projectiles with slight spread
        int extras = UpgradeManager.Instance != null ? UpgradeManager.Instance.extraProjectiles : 0;
        for (int i = 0; i < extras; i++)
        {
            float angle = (i + 1) * 15f * (i % 2 == 0 ? 1f : -1f);
            Vector2 dir = Rotate(baseDir, angle);
            SpawnBullet(dir);
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _hero.range);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsDead) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    private void SpawnBullet(Vector2 direction)
    {
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.transform.position = transform.position;
        bulletGO.transform.localScale = Vector3.one * 0.25f;

        var sr = bulletGO.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(1f, 0.9f, 0.2f), 16);
        sr.sortingOrder = 15;

        bool pierce = UpgradeManager.Instance != null && UpgradeManager.Instance.pierceShot;
        float crit = UpgradeManager.Instance != null ? UpgradeManager.Instance.critChance : 0f;
        float damage = _hero.attackDamage;

        // Crit check
        bool isCrit = false;
        if (crit > 0f && Random.value < crit)
        {
            damage *= 2f;
            isCrit = true;
            sr.sprite = SpriteFactory.CreateCircle(Color.red, 16); // red = crit
        }

        var bullet = bulletGO.AddComponent<Bullet>();
        bullet.Init(direction, damage, pierce, isCrit);
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    private void OnDrawGizmosSelected()
    {
        if (_hero == null) _hero = GetComponent<HeroController>();
        if (_hero == null) return;
        Gizmos.color = new Color(0.2f, 0.4f, 0.9f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _hero.range);
    }
}
