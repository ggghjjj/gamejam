using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TowerBase : MonoBehaviour
{
    [Header("Data")]
    public TowerType type;
    public string towerName;
    public Stat damage;
    public Stat attackSpeed;
    public Stat range;
    public float totalDamageDealt = 0f;

    private float _shootTimer;
    private SpriteRenderer _sr;
    private GameObject _rangeCircle;

    public void Init(TowerData data)
    {
        type = data.type;
        towerName = data.towerName;
        damage = new Stat(data.baseDamage);
        attackSpeed = new Stat(data.baseAttackSpeed);
        range = new Stat(data.baseRange);

        _sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * data.spriteScale;

        // Visual by type
        switch (type)
        {
            case TowerType.Archer:
                _sr.sprite = SpriteFactory.CreateSquare(data.color);  // triangle-ish square
                break;
            case TowerType.Warrior:
                _sr.sprite = SpriteFactory.CreateSquare(data.color);
                break;
            case TowerType.Mage:
                _sr.sprite = SpriteFactory.CreateCircle(data.color);
                break;
        }
        _sr.sortingOrder = 4;

        // Range indicator
        _rangeCircle = new GameObject("TowerRange");
        _rangeCircle.transform.SetParent(transform, false);
        _rangeCircle.transform.localPosition = Vector3.zero;
        var rangeSR = _rangeCircle.AddComponent<SpriteRenderer>();
        Color rangeColor = data.color;
        rangeColor.a = 0.06f;
        rangeSR.sprite = SpriteFactory.CreateCircle(rangeColor, 64);
        rangeSR.sortingOrder = 1;
        UpdateRangeVisual();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.state != GameManager.GameState.Playing)
            return;

        _shootTimer -= Time.deltaTime;
        if (_shootTimer > 0f) return;

        _shootTimer = 1f / attackSpeed.Value;

        switch (type)
        {
            case TowerType.Archer:
                AttackNearest();
                break;
            case TowerType.Warrior:
                AttackMelee();
                break;
            case TowerType.Mage:
                AttackAOE();
                break;
        }
    }

    // ========== Archer: shoot nearest enemy ==========
    private void AttackNearest()
    {
        Transform target = FindNearest();
        if (target == null) return;

        GameObject bulletGO = new GameObject("TowerBullet");
        bulletGO.transform.position = transform.position;
        bulletGO.transform.localScale = Vector3.one * 0.12f;

        var sr = bulletGO.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(1f, 0.7f, 0.2f), 16);
        sr.sortingOrder = 15;

        var bullet = bulletGO.AddComponent<Bullet>();
        Vector2 dir = (target.position - transform.position).normalized;
        bullet.Init(dir, damage.Value);
        totalDamageDealt += damage.Value;
    }

    // ========== Warrior: melee AOE around self ==========
    private void AttackMelee()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range.Value * 0.6f);
        bool hitAny = false;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage.Value);
                totalDamageDealt += damage.Value;
                hitAny = true;
            }
        }

        if (hitAny)
        {
            // Slash visual: expanding ring
            SpawnSlashEffect();
        }
    }

    // ========== Mage: AOE at densest cluster ==========
    private void AttackAOE()
    {
        Vector3 targetPos = FindDensestPoint();
        if (targetPos == Vector3.zero) return;

        // Explosion at target
        SpawnExplosion(targetPos);

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 1.2f);
        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage.Value);
                totalDamageDealt += damage.Value;
                VFXFactory.SpawnDamagePopup(hit.transform.position, damage.Value);
            }
        }
    }

    // ========== Targeting helpers ==========
    private Transform FindNearest()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range.Value);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsDead) continue;
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist) { minDist = dist; nearest = hit.transform; }
        }
        return nearest;
    }

    private Vector3 FindDensestPoint()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range.Value);
        Vector3 bestPos = Vector3.zero;
        int bestCount = 0;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsDead) continue;

            // Count enemies near this one
            int count = 0;
            foreach (var other in hits)
            {
                if (other.GetComponent<EnemyBase>() != null && !other.GetComponent<EnemyBase>().IsDead)
                {
                    if (Vector2.Distance(hit.transform.position, other.transform.position) < 1.5f)
                        count++;
                }
            }
            if (count > bestCount)
            {
                bestCount = count;
                bestPos = hit.transform.position;
            }
        }
        return bestCount > 0 ? bestPos : Vector3.zero;
    }

    // ========== VFX ==========
    private void SpawnSlashEffect()
    {
        GameObject slash = new GameObject("Slash");
        slash.transform.position = transform.position;
        slash.transform.localScale = Vector3.one * 0.3f;

        var sr = slash.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(1f, 0.8f, 0.3f, 0.5f), 32);
        sr.sortingOrder = 12;

        var mover = slash.AddComponent<ParticleMove>();
        mover.velocity = Vector2.zero;
        mover.lifetime = 0.15f;
        mover.fadeOut = true;

        // Quick expand
        var expander = slash.AddComponent<ScaleExpand>();
        expander.targetScale = range.Value * 1.2f;
        expander.duration = 0.15f;
    }

    private void SpawnExplosion(Vector3 pos)
    {
        GameObject exp = new GameObject("Explosion");
        exp.transform.position = pos;
        exp.transform.localScale = Vector3.one * 0.2f;

        var sr = exp.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(0.5f, 0.2f, 1f, 0.6f), 32);
        sr.sortingOrder = 12;

        var mover = exp.AddComponent<ParticleMove>();
        mover.velocity = Vector2.zero;
        mover.lifetime = 0.25f;
        mover.fadeOut = true;

        var expander = exp.AddComponent<ScaleExpand>();
        expander.targetScale = 2.5f;
        expander.duration = 0.25f;
    }

    private void UpdateRangeVisual()
    {
        if (_rangeCircle == null) return;
        float scale = range.Value * 2f / transform.localScale.x;
        _rangeCircle.transform.localScale = Vector3.one * scale;
    }
}

/// <summary>
/// Simple component that expands localScale over time
/// </summary>
public class ScaleExpand : MonoBehaviour
{
    public float targetScale = 2f;
    public float duration = 0.2f;
    private float _timer;
    private Vector3 _startScale;

    private void Start() { _startScale = transform.localScale; }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / duration);
        transform.localScale = Vector3.Lerp(_startScale, Vector3.one * targetScale, t);
    }
}
