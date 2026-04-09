using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    public static int CurrentLevel = 0; // set by level select before scene load

    private void Awake()
    {
        // 1. GameManager
        if (GameManager.Instance == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        // 2. Map Generator - creates paths, trees, markers based on level
        GameObject mapGenGO = new GameObject("MapGenerator");
        MapGenerator mapGen = mapGenGO.AddComponent<MapGenerator>();
        mapGen.GenerateMap(CurrentLevel);

        Camera cam = Camera.main;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float yB = -halfH + 1f;

        // 3. Hero
        GameObject heroGO = new GameObject("Hero");
        heroGO.AddComponent<SpriteRenderer>();
        heroGO.AddComponent<HeroController>();
        heroGO.AddComponent<HeroShooter>();
        var heroCol = heroGO.AddComponent<BoxCollider2D>();
        heroCol.size = new Vector2(0.8f, 0.8f);
        heroGO.transform.position = new Vector3(0f, yB + 0.5f, 0f);
        heroGO.transform.localScale = Vector3.one * 0.3f;

        // Range indicator
        GameObject rangeIndicator = new GameObject("RangeIndicator");
        rangeIndicator.transform.SetParent(heroGO.transform, false);
        var rangeSR = rangeIndicator.AddComponent<SpriteRenderer>();
        rangeSR.sprite = SpriteFactory.CreateCircle(new Color(0f, 0.9f, 0.9f, 0.08f), 64);
        rangeSR.sortingOrder = 1;
        rangeIndicator.AddComponent<RangeIndicator>();

        // 4. Wave Manager - uses spawners from MapGenerator
        GameObject waveGO = new GameObject("WaveManager");
        WaveManager waveManager = waveGO.AddComponent<WaveManager>();
        waveManager.spawners = mapGen.Spawners.ToArray();

        // 5. Upgrade Manager
        GameObject upgradeGO = new GameObject("UpgradeManager");
        upgradeGO.AddComponent<UpgradeManager>();

        // 6. Object Pool + Placement + SFX
        new GameObject("ObjectPool").AddComponent<ObjectPool>();
        GameObject placementGO = new GameObject("PlacementSystem");
        PlacementSystem placement = placementGO.AddComponent<PlacementSystem>();
        new GameObject("SFXManager").AddComponent<SFXManager>();

        // 7. UI Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // UI components
        var hudGO = new GameObject("HUD"); hudGO.transform.SetParent(canvasGO.transform, false); hudGO.AddComponent<HUDManager>();
        var upgradeUIGO = new GameObject("UpgradeUI"); upgradeUIGO.transform.SetParent(canvasGO.transform, false); upgradeUIGO.AddComponent<UpgradeUI>();
        var flowUIGO = new GameObject("GameFlowUI"); flowUIGO.transform.SetParent(canvasGO.transform, false); flowUIGO.AddComponent<GameFlowUI>();
        placement.BuildPlacementUI(canvasGO.transform);
        var victoryUIGO = new GameObject("VictoryUI"); victoryUIGO.transform.SetParent(canvasGO.transform, false); victoryUIGO.AddComponent<VictoryUI>();

        // 8. Camera follow + shake
        if (cam != null)
        {
            cam.gameObject.AddComponent<CameraFollow>().target = heroGO.transform;
            cam.gameObject.AddComponent<ScreenShake>();
        }
    }
}
