using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    private void Awake()
    {
        // 1. GameManager
        if (GameManager.Instance == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        // 2. Waypoint Path - Z-shaped path, auto-fit to camera bounds
        GameObject pathGO = new GameObject("EnemyPath");
        WaypointPath path = pathGO.AddComponent<WaypointPath>();

        Camera cam = Camera.main;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        float margin = 1f; // keep path inside screen edge
        float xL = -halfW + margin;
        float xR = halfW - margin;
        float yT = halfH - margin;
        float yB = -halfH + margin;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(xL, yT, 0f),
            new Vector3(xR, yT, 0f),
            new Vector3(xR, yT * 0.33f, 0f),
            new Vector3(xL, yT * 0.33f, 0f),
            new Vector3(xL, -yT * 0.33f, 0f),
            new Vector3(xR, -yT * 0.33f, 0f),
            new Vector3(xR, yB, 0f),
            new Vector3(xL, yB, 0f),
        };

        Transform[] waypoints = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wp = new GameObject($"WP_{i}");
            wp.transform.parent = pathGO.transform;
            wp.transform.position = positions[i];
            waypoints[i] = wp.transform;
        }
        path.waypoints = waypoints;

        // 3. Hero
        GameObject heroGO = new GameObject("Hero");
        heroGO.AddComponent<SpriteRenderer>();
        heroGO.AddComponent<HeroController>();
        heroGO.AddComponent<HeroShooter>();
        var heroCol = heroGO.AddComponent<BoxCollider2D>();
        heroCol.size = new Vector2(0.8f, 0.8f);
        heroGO.transform.position = new Vector3(0f, yB + 0.5f, 0f);
        heroGO.transform.localScale = Vector3.one * 0.5f;

        // 4. Enemy Spawner
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.path = path;

        // 5. Wave Manager
        GameObject waveGO = new GameObject("WaveManager");
        WaveManager waveManager = waveGO.AddComponent<WaveManager>();
        waveManager.spawner = spawner;

        // 6. Upgrade Manager
        GameObject upgradeGO = new GameObject("UpgradeManager");
        upgradeGO.AddComponent<UpgradeManager>();

        // 6.5 Object Pool
        GameObject poolGO = new GameObject("ObjectPool");
        poolGO.AddComponent<ObjectPool>();

        // 7. UI Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // blend between width and height matching
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem (required for UI clicks)
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // HUD
        GameObject hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        hudGO.AddComponent<HUDManager>();

        // Upgrade UI
        GameObject upgradeUIGO = new GameObject("UpgradeUI");
        upgradeUIGO.transform.SetParent(canvasGO.transform, false);
        upgradeUIGO.AddComponent<UpgradeUI>();

        // 8. Camera follow
        if (cam != null)
        {
            var camFollow = cam.gameObject.AddComponent<CameraFollow>();
            camFollow.target = heroGO.transform;
        }

        Debug.Log("=== Tangy TD \u573a\u666f\u642d\u5efa\u5b8c\u6210 ===");
        Debug.Log("WASD \u79fb\u52a8\u82f1\u96c4\uff0c\u81ea\u52a8\u5c04\u51fb\u8303\u56f4\u5185\u654c\u4eba");
        Debug.Log("\u5347\u7ea7\u540e\u53ef\u9009\u62e9\u5f3a\u5316!");
    }
}
