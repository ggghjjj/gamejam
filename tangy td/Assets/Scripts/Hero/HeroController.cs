using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HeroController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP;
    public float attackDamage = 10f;
    public float attackSpeed = 1f;    // shots per second
    public float attackRange = 3f;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        // Generate blue square sprite
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateSquare(new Color(0.2f, 0.4f, 0.9f));
        sr.sortingOrder = 10;

        currentHP = maxHP;
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector2 newPos = _rb.position + _moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

        // Clamp to camera bounds
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

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            currentHP = 0f;
            GameManager.Instance?.OnHeroDied();
        }
    }
}
