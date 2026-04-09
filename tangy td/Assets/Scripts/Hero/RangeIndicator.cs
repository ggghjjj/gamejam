using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private HeroController _hero;

    private void Start()
    {
        _hero = GetComponentInParent<HeroController>();
        UpdateScale();
    }

    private void Update()
    {
        UpdateScale();
        // Gentle pulse
        float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.03f;
        transform.localScale *= pulse;
    }

    private void UpdateScale()
    {
        if (_hero == null) return;
        // Hero is scaled 0.5, sprite is 1 unit = 1 world unit at scale 1
        // We need the circle to cover attackRange radius in world space
        float heroScale = transform.parent != null ? transform.parent.localScale.x : 1f;
        float diameter = (_hero.attackRange * 2f) / heroScale;
        transform.localScale = Vector3.one * diameter;
    }
}
