using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private HeroController _hero;
    private SpriteRenderer _sr;

    private void Start()
    {
        _hero = GetComponentInParent<HeroController>();
        _sr = GetComponent<SpriteRenderer>();
        UpdateScale();
        gameObject.SetActive(false); // hidden by default
    }

    private void Update()
    {
        UpdateScale();

        // Show only when mouse hovers over hero
        if (_hero != null)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float dist = Vector2.Distance(_hero.transform.position, mouse);
            gameObject.SetActive(dist < 0.5f);
        }
    }

    private void UpdateScale()
    {
        if (_hero == null) return;
        float heroScale = transform.parent != null ? transform.parent.localScale.x : 1f;
        float diameter = (_hero.range * 2f) / heroScale;
        transform.localScale = Vector3.one * diameter;
    }
}
