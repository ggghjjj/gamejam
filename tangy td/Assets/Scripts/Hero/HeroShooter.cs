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

        // Find nearest enemy in range
        Transform target = FindNearestEnemy();
        if (target == null) return;

        // Shoot
        _shootTimer = 1f / _hero.attackSpeed;
        SpawnBullet(target);
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _hero.attackRange);
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

    private void SpawnBullet(Transform target)
    {
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.transform.position = transform.position;

        var sr = bulletGO.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(1f, 0.9f, 0.2f), 16);
        sr.sortingOrder = 15;

        var bullet = bulletGO.AddComponent<Bullet>();
        Vector2 dir = (target.position - transform.position).normalized;
        bullet.Init(dir, _hero.attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (_hero == null) _hero = GetComponent<HeroController>();
        if (_hero == null) return;
        Gizmos.color = new Color(0.2f, 0.4f, 0.9f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _hero.attackRange);
    }
}
