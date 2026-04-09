using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 30f;
    public float moveSpeed = 2f;
    public int expValue = 10;
    public int goldValue = 2;
    public float contactDamage = 5f;
    private float _contactTimer;

    [Header("Runtime")]
    public float currentHP;
    public bool IsDead { get; private set; }

    private WaypointPath _path;
    private int _currentWaypointIndex;
    private SpriteRenderer _sr;
    private Color _color;
    private float _baseScale;
    private float _hitScaleTimer;
    private float _bobOffset;
    private SpriteRenderer _hpBarFill;
    private GameObject _hpBarRoot;

    public void Init(WaypointPath path, float hp, float speed, int exp, Color color, float scale = 1f, int gold = 2)
    {
        _path = path;
        maxHP = hp;
        currentHP = hp;
        moveSpeed = speed;
        expValue = exp;
        goldValue = gold;
        _currentWaypointIndex = 0;
        _baseScale = scale;
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);

        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = SpriteFactory.CreateCircle(color);
        _sr.sortingOrder = 5;
        _color = color;

        transform.localScale = Vector3.one * scale;
        // Spawn at path start with slight random offset to avoid stacking
        Vector2 offset = Random.insideUnitCircle * 0.15f;
        transform.position = _path.GetPosition(0) + (Vector3)offset;

        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        // Rigidbody for trigger detection
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        gameObject.tag = "Enemy";

        // HP bar only for big enemies (Tank/Boss) - Boss gets red bar
        if (scale >= 0.4f)
        {
            bool isBoss = scale >= 0.55f;
            CreateHPBar(scale, isBoss);
        }
    }

    private void CreateHPBar(float scale, bool isBoss = false)
    {
        _hpBarRoot = new GameObject("HPBar");
        _hpBarRoot.transform.SetParent(transform, false);
        _hpBarRoot.transform.localPosition = new Vector3(0f, 0.7f / scale, 0f);
        _hpBarRoot.transform.localScale = new Vector3(1f / scale, 0.15f / scale, 1f);

        // Background (dark)
        var bg = new GameObject("BG");
        bg.transform.SetParent(_hpBarRoot.transform, false);
        var bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.sprite = SpriteFactory.CreateSquare(new Color(0.15f, 0.15f, 0.15f, 0.8f), 16);
        bgSR.sortingOrder = 9;

        // Fill (green → red)
        var fill = new GameObject("Fill");
        fill.transform.SetParent(_hpBarRoot.transform, false);
        _hpBarFill = fill.AddComponent<SpriteRenderer>();
        _hpBarFill.sprite = SpriteFactory.CreateSquare(isBoss ? new Color(0.9f, 0.15f, 0.15f) : Color.green, 16);
        _hpBarFill.sortingOrder = 10;
    }

    private void Update()
    {
        if (IsDead || _path == null) return;
        if (GameManager.Instance != null && GameManager.Instance.state != GameManager.GameState.Playing)
            return;

        MoveAlongPath();
        ClampToScreen();
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

    private void ClampToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, -w, w);
        p.y = Mathf.Clamp(p.y, -h, h);
        transform.position = p;
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

        UpdateHPBar();

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die(grantExp: true);
        }
    }

    private void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        float ratio = Mathf.Clamp01(currentHP / maxHP);
        _hpBarFill.transform.localScale = new Vector3(ratio, 1f, 1f);
        _hpBarFill.transform.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0f);
        // Color: green → yellow → red
        _hpBarFill.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        Color original = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (_sr != null) _sr.color = original;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsDead) return;
        _contactTimer -= Time.deltaTime;
        if (_contactTimer > 0f) return;
        _contactTimer = 0.5f; // damage every 0.5s

        var hero = other.GetComponent<HeroController>();
        if (hero != null)
        {
            hero.TakeDamage(contactDamage);
            return;
        }

        var tower = other.GetComponent<TowerBase>();
        if (tower != null)
        {
            tower.currentHP -= contactDamage;
            if (tower.currentHP <= 0f) Destroy(tower.gameObject);
        }
    }

    private void Die(bool grantExp)
    {
        if (IsDead) return;
        IsDead = true;

        VFXFactory.SpawnDeathParticles(transform.position, _color);
        SFXManager.PlayDeath();
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.totalKills++;
                GameManager.Instance.AddGold(goldValue);
            }
            VFXFactory.SpawnExpOrb(transform.position, expValue);
        }

        Destroy(gameObject, 0.02f);
    }
}
