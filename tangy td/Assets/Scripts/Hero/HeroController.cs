using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HeroController : MonoBehaviour
{
    [Header("Stats (Stat System)")]
    public Stat damage = new Stat(10f);
    public Stat attackSpeed = new Stat(3f);
    public Stat moveSpeedStat = new Stat(5f);
    public Stat attackRange = new Stat(1.5f);        // halved from 3
    public Stat maxHPStat = new Stat(150f);

    [Header("Runtime")]
    public float currentHP;

    // Convenience accessors for other scripts
    public float attackDamage => damage.Value;
    public float fireRate => attackSpeed.Value;
    public float moveSpeed => moveSpeedStat.Value;
    public float range => attackRange.Value;
    public float maxHP => maxHPStat.Value;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateSquare(new Color(0f, 0.9f, 0.9f));
        sr.sortingOrder = 10;

        currentHP = maxHP;
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Green HP bar
        CreateHPBar();
    }

    private SpriteRenderer _hpBarFill;

    private void CreateHPBar()
    {
        var barRoot = new GameObject("HPBar");
        barRoot.transform.SetParent(transform, false);
        barRoot.transform.localPosition = new Vector3(0f, 2f, 0f); // compensate for 0.3 scale
        barRoot.transform.localScale = new Vector3(3f, 0.4f, 1f);

        var bg = new GameObject("BG");
        bg.transform.SetParent(barRoot.transform, false);
        bg.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateSquare(new Color(0.1f, 0.1f, 0.1f, 0.7f), 16);
        bg.GetComponent<SpriteRenderer>().sortingOrder = 9;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(barRoot.transform, false);
        _hpBarFill = fill.AddComponent<SpriteRenderer>();
        _hpBarFill.sprite = SpriteFactory.CreateSquare(new Color(0.2f, 0.9f, 0.2f), 16);
        _hpBarFill.sortingOrder = 10;
    }

    private void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        float ratio = Mathf.Clamp01(currentHP / maxHP);
        _hpBarFill.transform.localScale = new Vector3(ratio, 1f, 1f);
        _hpBarFill.transform.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0f);
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            float bob = Mathf.Sin(Time.time * 8f) * 0.03f;
            transform.localScale = new Vector3(0.3f - bob * 0.2f, 0.3f + bob, 1f);
        }
        else
        {
            transform.localScale = Vector3.one * 0.3f;
        }
    }

    private void FixedUpdate()
    {
        Vector2 newPos = _rb.position + _moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 camPos = cam.transform.position;
            newPos.x = Mathf.Clamp(newPos.x, camPos.x - halfWidth + 0.5f, camPos.x + halfWidth - 0.5f);
            newPos.y = Mathf.Clamp(newPos.y, camPos.y - halfHeight + 0.5f, camPos.y + halfHeight - 0.5f);
        }

        _rb.MovePosition(newPos);
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        UpdateHPBar();
        if (currentHP <= 0f)
        {
            currentHP = 0f;
            GameManager.Instance?.OnHeroDied();
        }
    }
}
