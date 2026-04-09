using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followStrength = 0.3f; // 0=不跟随, 1=完全跟随
    public float smoothSpeed = 5f;

    private Vector3 _basePosition;

    private void Start()
    {
        _basePosition = transform.position;

        if (target == null)
        {
            var hero = FindAnyObjectByType<HeroController>();
            if (hero != null) target = hero.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Blend between base position and hero position
        Vector3 desired = Vector3.Lerp(_basePosition, target.position, followStrength);
        desired.z = _basePosition.z; // keep camera Z

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
