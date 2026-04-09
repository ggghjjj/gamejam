using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Jelly-like hover animation: bounces size when pointer enters/exits
/// </summary>
public class JellyHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float baseSize = 70f;
    public float hoverScale = 1.25f;
    public float bounceSpeed = 8f;

    private RectTransform _rt;
    private float _targetSize;
    private float _currentSize;
    private float _velocity;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _targetSize = baseSize;
        _currentSize = baseSize;
    }

    private void Update()
    {
        // Spring-like interpolation for jelly feel
        float diff = _targetSize - _currentSize;
        _velocity += diff * bounceSpeed * Time.unscaledDeltaTime;
        _velocity *= 0.85f; // damping
        _currentSize += _velocity;

        if (_rt != null)
        {
            _rt.sizeDelta = new Vector2(_currentSize, _currentSize);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetSize = baseSize * hoverScale;
        _velocity = 15f; // initial bounce impulse
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetSize = baseSize;
        _velocity = -10f; // shrink bounce
    }
}
