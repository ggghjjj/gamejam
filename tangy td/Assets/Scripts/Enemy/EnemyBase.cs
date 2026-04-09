using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 30f;
    public float moveSpeed = 2f;
    public int expValue = 10;

    [Header("Runtime")]
    public float currentHP;
    public bool IsDead { get; private set; }

    private WaypointPath _path;
    private int _currentWaypointIndex;
    private SpriteRenderer _sr;
    private Color _color;

    public void Init(WaypointPath path, float hp, float speed, int exp, Color color, float scale = 1f)
    {
        _path = path;
        maxHP = hp;
        currentHP = hp;
        moveSpeed = speed;
        expValue = exp;
        _currentWaypointIndex = 0;

        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = SpriteFactory.CreateCircle(color);
        _sr.sortingOrder = 5;
        _color = color;

        transform.localScale = Vector3.one * scale;
        transform.position = _path.GetPosition(0);

        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.4f;

        gameObject.tag = "Enemy";
    }

    private void Update()
    {
        if (IsDead || _path == null) return;
        if (GameManager.Instance != null && GameManager.Instance.state != GameManager.GameState.Playing)
            return;

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (_currentWaypointIndex >= _path.Length)
        {
            // Reached end - damage player and destroy
            GameManager.Instance?.OnEnemyReachedEnd(1);
            Die(grantExp: false);
            return;
        }

        Vector3 target = _path.GetPosition(_currentWaypointIndex);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            _currentWaypointIndex++;
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHP -= damage;

        // Flash effect
        if (_sr != null)
        {
            StartCoroutine(FlashWhite());
        }

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die(grantExp: true);
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        Color original = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (_sr != null) _sr.color = original;
    }

    private void Die(bool grantExp)
    {
        if (IsDead) return;
        IsDead = true;

        // Death particles
        VFXFactory.SpawnDeathParticles(transform.position, _color);

        if (grantExp)
        {
            if (GameManager.Instance != null) GameManager.Instance.totalKills++;
            // Drop exp orb instead of directly adding exp
            VFXFactory.SpawnExpOrb(transform.position, expValue);
        }

        Destroy(gameObject, 0.02f);
    }
}
