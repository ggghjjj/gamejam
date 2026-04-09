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

        // 2. Camera background - dark green
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.08f, 0.15f, 0.1f);
        }

        // 3. Waypoint Paths - auto-fit to camera bounds
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        float margin = 1f;
        float xL = -halfW + margin;
        float xR = halfW - margin;
        float yT = halfH - margin;
        float yB = -halfH + margin;

        // Path A: Z-shaped (left-to-right start)
        WaypointPath pathA = CreatePath("EnemyPath_A", new Vector3[]
        {
            new Vector3(xL, yT, 0f),
            new Vector3(xR, yT, 0f),
            new Vector3(xR, yT * 0.33f, 0f),
            new Vector3(xL, yT * 0.33f, 0f),
            new Vector3(xL, -yT * 0.33f, 0f),
            new Vector3(xR, -yT * 0.33f, 0f),
            new Vector3(xR, yB, 0f),
            new Vector3(xL, yB, 0f),
        });

        // Path B: S-shaped (right-to-left start)
        WaypointPath pathB = CreatePath("EnemyPath_B", new Vector3[]
        {
            new Vector3(xR, yT, 0f),
            new Vector3(xL, yT, 0f),
            new Vector3(xL, yT * 0.2f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(xR, -yT * 0.2f, 0f),
            new Vector3(xR, yB, 0f),
            new Vector3(xL, yB, 0f),
        });

        // 3. Hero
        GameObject heroGO = new GameObject("Hero");
        heroGO.AddComponent<SpriteRenderer>();
        heroGO.AddComponent<HeroController>();
        heroGO.AddComponent<HeroShooter>();
        var heroCol = heroGO.AddComponent<BoxCollider2D>();
        heroCol.size = new Vector2(0.8f, 0.8f);
        heroGO.transform.position = new Vector3(0f, yB + 0.5f, 0f);
        heroGO.transform.localScale = Vector3.one * 0.5f;

        // Range indicator circle (child of hero)
        GameObject rangeIndicator = new GameObject("RangeIndicator");
        rangeIndicator.transform.SetParent(heroGO.transform, false);
        rangeIndicator.transform.localPosition = Vector3.zero;
        var rangeSR = rangeIndicator.AddComponent<SpriteRenderer>();
        rangeSR.sprite = SpriteFactory.CreateCircle(new Color(0f, 0.9f, 0.9f, 0.08f), 64);
        rangeSR.sortingOrder = 1;
        // Scale to match attack range (hero scale is 0.5, so range circle needs to compensate)
        var rangeVis = rangeIndicator.AddComponent<RangeIndicator>();

        // 4. Enemy Spawner (supports multiple paths)
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.path = pathA; // default path
        spawner.paths = new WaypointPath[] { pathA, pathB };

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

        // 6.6 Placement System
        GameObject placementGO = new GameObject("PlacementSystem");
        PlacementSystem placement = placementGO.AddComponent<PlacementSystem>();

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

        // Game Flow UI (start screen + game over screen)
        GameObject flowUIGO = new GameObject("GameFlowUI");
        flowUIGO.transform.SetParent(canvasGO.transform, false);
        flowUIGO.AddComponent<GameFlowUI>();

        // Tower Placement UI
        placement.BuildPlacementUI(canvasGO.transform);

        // 8. Camera follow + screen shake
        if (cam != null)
        {
            var camFollow = cam.gameObject.AddComponent<CameraFollow>();
            camFollow.target = heroGO.transform;
            cam.gameObject.AddComponent<ScreenShake>();
        }

        // 9. Environment decorations
        SpawnEnvironment(xL, xR, yT, yB);

        // 10. Path visualization
        VisualizePath(pathA, new Color(0.6f, 0.45f, 0.2f, 0.15f));
        VisualizePath(pathB, new Color(0.5f, 0.4f, 0.25f, 0.12f));

        Debug.Log("=== Tangy TD \u573a\u666f\u642d\u5efa\u5b8c\u6210 ===");
        Debug.Log("WASD \u79fb\u52a8\u82f1\u96c4\uff0c\u81ea\u52a8\u5c04\u51fb\u8303\u56f4\u5185\u654c\u4eba");
        Debug.Log("\u5347\u7ea7\u540e\u53ef\u9009\u62e9\u5f3a\u5316!");
    }

    private void SpawnEnvironment(float xL, float xR, float yT, float yB)
    {
        // Scatter random dark-green "trees" as decoration
        int treeCount = 12;
        for (int i = 0; i < treeCount; i++)
        {
            float x = Random.Range(xL + 0.5f, xR - 0.5f);
            float y = Random.Range(yT * 0.5f, yB + 0.5f); // avoid top path area

            GameObject tree = new GameObject($"Tree_{i}");
            tree.transform.position = new Vector3(x, y, 0f);
            float size = Random.Range(0.2f, 0.4f);
            tree.transform.localScale = Vector3.one * size;

            var sr = tree.AddComponent<SpriteRenderer>();
            Color treeColor = new Color(
                Random.Range(0.05f, 0.15f),
                Random.Range(0.25f, 0.4f),
                Random.Range(0.05f, 0.15f),
                0.6f);
            sr.sprite = SpriteFactory.CreateCircle(treeColor, 16);
            sr.sortingOrder = 2;
        }
    }

    private void VisualizePath(WaypointPath path, Color color)
    {
        if (path == null || path.Length < 2) return;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 from = path.GetPosition(i);
            Vector3 to = path.GetPosition(i + 1);
            float dist = Vector3.Distance(from, to);
            int dots = Mathf.CeilToInt(dist / 0.5f);

            for (int d = 0; d < dots; d++)
            {
                float t = (float)d / dots;
                Vector3 pos = Vector3.Lerp(from, to, t);

                GameObject dot = new GameObject("PathDot");
                dot.transform.position = pos;
                dot.transform.localScale = Vector3.one * 0.15f;

                var sr = dot.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.CreateSquare(color, 8);
                sr.sortingOrder = 0;
            }
        }
    }
    }

    private WaypointPath CreatePath(string name, Vector3[] positions)
    {
        GameObject pathGO = new GameObject(name);
        WaypointPath path = pathGO.AddComponent<WaypointPath>();
        Transform[] waypoints = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wp = new GameObject($"WP_{i}");
            wp.transform.parent = pathGO.transform;
            wp.transform.position = positions[i];
            waypoints[i] = wp.transform;
        }
        path.waypoints = waypoints;
        return path;
    }
}
