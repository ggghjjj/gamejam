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
    private float _baseScale;
    private float _hitScaleTimer;
    private float _bobOffset; // random offset so enemies don't bob in sync

    public void Init(WaypointPath path, float hp, float speed, int exp, Color color, float scale = 1f)
    {
        _path = path;
        maxHP = hp;
        currentHP = hp;
        moveSpeed = speed;
        expValue = exp;
        _currentWaypointIndex = 0;
        _baseScale = scale;
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);

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
        ApplyBob();
        ApplyHitScale();
    }

    private void MoveAlongPath()
    {
        if (_currentWaypointIndex >= _path.Length)
        {
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

    private void ApplyBob()
    {
        // Gentle vertical bobbing while moving
        float bob = Mathf.Sin((Time.time + _bobOffset) * 5f) * 0.04f;
        float scaleY = _baseScale + bob;
        float scaleX = _baseScale - bob * 0.5f; // slight squash-stretch
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private void ApplyHitScale()
    {
        if (_hitScaleTimer > 0f)
        {
            _hitScaleTimer -= Time.deltaTime;
            float t = _hitScaleTimer / 0.1f; // 0.1s duration
            float bump = 1f + t * 0.3f; // scale up to 1.3x then back
            transform.localScale *= bump;
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHP -= damage;
        _hitScaleTimer = 0.1f;

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

        VFXFactory.SpawnDeathParticles(transform.position, _color);

        if (grantExp)
        {
            if (GameManager.Instance != null) GameManager.Instance.totalKills++;
            VFXFactory.SpawnExpOrb(transform.position, expValue);
        }

        Destroy(gameObject, 0.02f);
    }
}
