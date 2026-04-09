using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private Vector3 _originalPos;
    private float _shakeTimer;
    private float _shakeIntensity;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _originalPos = transform.localPosition;
    }

    public static void Shake(float intensity = 0.15f, float duration = 0.2f)
    {
        if (Instance == null) return;
        Instance._shakeIntensity = intensity;
        Instance._shakeTimer = duration;
    }

    private void LateUpdate()
    {
        if (_shakeTimer > 0f)
        {
            _shakeTimer -= Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * _shakeIntensity;
            transform.localPosition = _originalPos + (Vector3)offset;

            if (_shakeTimer <= 0f)
            {
                transform.localPosition = _originalPos;
            }
        }
    }
}
