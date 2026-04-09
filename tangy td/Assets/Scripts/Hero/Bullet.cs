using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float maxLifetime = 3f;

    private Vector2 _direction;
    private float _damage;
    private float _lifetime;
    private bool _pierce;

    public void Init(Vector2 direction, float damage, bool pierce = false)
    {
        _direction = direction;
        _damage = damage;
        _pierce = pierce;

        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;
        col.isTrigger = true;

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.velocity = _direction * speed;
    }

    private void Update()
    {
        _lifetime += Time.deltaTime;
        if (_lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(_damage);
            VFXFactory.SpawnHitFlash(transform.position);
            if (!_pierce)
            {
                Destroy(gameObject);
            }
        }
    }
}
