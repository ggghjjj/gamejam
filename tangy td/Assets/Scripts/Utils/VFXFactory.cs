using UnityEngine;
using System.Collections;

public static class VFXFactory
{
    /// <summary>
    /// 敌人死亡时的粒子爆散效果
    /// </summary>
    public static void SpawnDeathParticles(Vector3 position, Color color, int count = 6)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("Particle");
            p.transform.position = position;
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateCircle(color, 8);
            sr.sortingOrder = 20;

            var mover = p.AddComponent<ParticleMove>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(3f, 6f);
            mover.lifetime = Random.Range(0.2f, 0.4f);
        }
    }

    /// <summary>
    /// 子弹命中时的小闪光
    /// </summary>
    public static void SpawnHitFlash(Vector3 position)
    {
        GameObject flash = new GameObject("HitFlash");
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.3f;

        var sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(Color.white, 8);
        sr.sortingOrder = 25;

        var mover = flash.AddComponent<ParticleMove>();
        mover.velocity = Vector2.zero;
        mover.lifetime = 0.08f;
        mover.fadeOut = true;
    }

    /// <summary>
    /// 经验球
    /// </summary>
    public static GameObject SpawnExpOrb(Vector3 position, int expValue)
    {
        GameObject orb = new GameObject("ExpOrb");
        orb.transform.position = position;
        orb.transform.localScale = Vector3.one * 0.15f;

        var sr = orb.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(new Color(0.3f, 1f, 0.5f), 8);
        sr.sortingOrder = 8;

        var pickup = orb.AddComponent<ExpOrb>();
        pickup.expValue = expValue;

        return orb;
    }

    /// <summary>
    /// 伤害跳字 — 命中时在敌人头上弹出数字
    /// </summary>
    public static void SpawnDamagePopup(Vector3 position, float damage, bool isCrit = false)
    {
        GameObject popup = new GameObject("DmgPopup");
        popup.transform.position = position + Vector3.up * 0.3f;

        // Use a Canvas in world space for text
        var canvas = popup.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 30;
        var rt = popup.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2f, 1f);
        rt.localScale = Vector3.one * 0.02f;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(popup.transform, false);
        var text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = Mathf.RoundToInt(damage).ToString();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = isCrit ? 48 : 32;
        text.color = isCrit ? new Color(1f, 0.2f, 0.1f) : Color.white;
        text.fontStyle = isCrit ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = TextAnchor.MiddleCenter;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(200, 100);

        var mover = popup.AddComponent<ParticleMove>();
        mover.velocity = new Vector2(Random.Range(-0.5f, 0.5f), 2f);
        mover.lifetime = 0.6f;
        mover.fadeOut = false;
    }
}

/// <summary>
/// 简易粒子运动：直线移动 + 自动销毁，可选淡出
/// </summary>
public class ParticleMove : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 0.3f;
    public bool fadeOut = false;

    private float _timer;
    private SpriteRenderer _sr;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity *= 0.92f; // drag

        _timer += Time.deltaTime;

        if (fadeOut && _sr != null)
        {
            Color c = _sr.color;
            c.a = 1f - (_timer / lifetime);
            _sr.color = c;
        }

        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// 经验球：自动飞向英雄并被拾取
/// </summary>
public class ExpOrb : MonoBehaviour
{
    public int expValue = 10;
    public float attractRange = 2f;
    public float flySpeed = 8f;

    private Transform _hero;
    private bool _attracted = false;
    private float _lifetime = 0f;

    private void Update()
    {
        _lifetime += Time.deltaTime;

        // Find hero
        if (_hero == null)
        {
            var hero = Object.FindAnyObjectByType<HeroController>();
            if (hero != null) _hero = hero.transform;
        }
        if (_hero == null) return;

        float dist = Vector2.Distance(transform.position, _hero.position);

        // Start attracting when close enough or after short delay
        if (dist < attractRange || _lifetime > 0.5f)
        {
            _attracted = true;
        }

        if (_attracted)
        {
            transform.position = Vector3.MoveTowards(transform.position, _hero.position, flySpeed * Time.deltaTime);
            flySpeed += 15f * Time.deltaTime; // accelerate

            if (dist < 0.3f)
            {
                GameManager.Instance?.AddExperience(expValue);
                Destroy(gameObject);
            }
        }

        // Safety timeout
        if (_lifetime > 10f)
        {
            Destroy(gameObject);
        }
    }
}
