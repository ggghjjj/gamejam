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
    }

    private void UpdateScale()
    {
        if (_hero == null) return;
        float heroScale = transform.parent != null ? transform.parent.localScale.x : 1f;
        float diameter = (_hero.range * 2f) / heroScale;
        transform.localScale = Vector3.one * diameter;
    }
}
