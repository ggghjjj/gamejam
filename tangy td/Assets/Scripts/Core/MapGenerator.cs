using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a unique map layout based on a level seed.
/// Different levels get different paths, trees, and background colors.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    private List<WaypointPath> _paths = new List<WaypointPath>();
    private List<EnemySpawner> _spawners = new List<EnemySpawner>();

    public List<WaypointPath> Paths => _paths;
    public List<EnemySpawner> Spawners => _spawners;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void GenerateMap(int levelIndex)
    {
        // Seed based on level for deterministic but varied maps
        Random.InitState(levelIndex * 1337 + 42);

        Camera cam = Camera.main;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        float margin = 0.8f;
        float xL = -halfW + margin;
        float xR = halfW - margin;
        float yT = halfH - margin;
        float yB = -halfH + margin;

        // Background color varies per level
        if (cam != null)
        {
            Color[] bgColors = new Color[]
            {
                new Color(0.08f, 0.15f, 0.1f),  // dark forest
                new Color(0.12f, 0.1f, 0.08f),   // desert brown
                new Color(0.06f, 0.08f, 0.15f),   // night blue
                new Color(0.1f, 0.12f, 0.06f),    // swamp green
                new Color(0.12f, 0.06f, 0.1f),    // purple dusk
            };
            cam.backgroundColor = bgColors[levelIndex % bgColors.Length];
        }

        // Generate 1-2 paths depending on level
        int pathCount = levelIndex < 3 ? 1 : 2;

        for (int p = 0; p < pathCount; p++)
        {
            Vector3[] waypoints = GenerateRandomPath(xL, xR, yT, yB, p, pathCount);
            WaypointPath path = CreatePath($"Path_{p}", waypoints);
            _paths.Add(path);

            // Spawner per path
            GameObject spawnerGO = new GameObject($"Spawner_{p}");
            EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
            spawner.path = path;
            _spawners.Add(spawner);

            // Visualize path
            VisualizePath(path, GetPathColor(p));

            // Markers
            SpawnPathMarkers(path, p);
        }

        // Trees (avoid path areas)
        SpawnTrees(xL, xR, yT, yB, 10 + levelIndex * 2);

        // Reset random state
        Random.InitState(System.Environment.TickCount);
    }

    private Vector3[] GenerateRandomPath(float xL, float xR, float yT, float yB, int pathIndex, int totalPaths)
    {
        List<Vector3> points = new List<Vector3>();

        // Start from top area, end at bottom
        float startX, endX;
        if (totalPaths == 1)
        {
            startX = Random.Range(xL + 1f, xR - 1f);
            endX = Random.Range(xL + 1f, xR - 1f);
        }
        else
        {
            // Split screen for multiple paths
            float halfScreen = (xR - xL) / 2f;
            if (pathIndex == 0)
            {
                startX = Random.Range(xL, xL + halfScreen * 0.6f);
                endX = Random.Range(xL, xL + halfScreen * 0.8f);
            }
            else
            {
                startX = Random.Range(xR - halfScreen * 0.6f, xR);
                endX = Random.Range(xR - halfScreen * 0.8f, xR);
            }
        }

        points.Add(new Vector3(startX, yT, 0f));

        // Generate 3-5 intermediate zigzag points
        int segments = Random.Range(3, 6);
        float yStep = (yT - yB) / (segments + 1);

        for (int i = 0; i < segments; i++)
        {
            float y = yT - yStep * (i + 1);
            float x = Random.Range(xL + 0.5f, xR - 0.5f);

            // Constrain to path's side if multi-path
            if (totalPaths > 1)
            {
                float mid = (xL + xR) / 2f;
                if (pathIndex == 0) x = Mathf.Min(x, mid + 0.5f);
                else x = Mathf.Max(x, mid - 0.5f);
            }

            points.Add(new Vector3(x, y, 0f));
        }

        points.Add(new Vector3(endX, yB, 0f));
        return points.ToArray();
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

    private Color GetPathColor(int index)
    {
        Color[] colors = new Color[]
        {
            new Color(0.45f, 0.35f, 0.18f, 0.3f),
            new Color(0.35f, 0.3f, 0.2f, 0.25f),
        };
        return colors[index % colors.Length];
    }

    private void VisualizePath(WaypointPath path, Color color)
    {
        if (path == null || path.Length < 2) return;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 from = path.GetPosition(i);
            Vector3 to = path.GetPosition(i + 1);
            float dist = Vector3.Distance(from, to);
            int segments = Mathf.CeilToInt(dist / 0.25f);

            Vector3 dir = (to - from).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            for (int d = 0; d < segments; d++)
            {
                float t = (float)d / segments;
                Vector3 pos = Vector3.Lerp(from, to, t);

                // Wide road - 3 parallel strips
                for (int w = -1; w <= 1; w++)
                {
                    GameObject road = new GameObject("Road");
                    road.transform.position = pos + perp * w * 0.2f;
                    road.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

                    var sr = road.AddComponent<SpriteRenderer>();
                    Color c = color;
                    if (w == 0) c.a *= 1.2f; // center brighter
                    sr.sprite = SpriteFactory.CreateSquare(c, 8);
                    sr.sortingOrder = 0;
                }
            }
        }
    }

    private void SpawnPathMarkers(WaypointPath path, int pathIndex)
    {
        if (path == null || path.Length < 2) return;

        // Spawn nest (top)
        Vector3 spawnPos = path.GetPosition(0);
        GameObject nest = new GameObject($"Nest_{pathIndex}");
        nest.transform.position = spawnPos;

        // Outer ring
        var nestSR = nest.AddComponent<SpriteRenderer>();
        nest.transform.localScale = Vector3.one * 0.4f;
        nestSR.sprite = SpriteFactory.CreateCircle(new Color(0.7f, 0.1f, 0.1f, 0.8f), 32);
        nestSR.sortingOrder = 2;
        // Skull-like inner dots
        var inner = new GameObject("Core");
        inner.transform.SetParent(nest.transform, false);
        inner.transform.localScale = Vector3.one * 0.4f;
        var innerSR = inner.AddComponent<SpriteRenderer>();
        innerSR.sprite = SpriteFactory.CreateCircle(new Color(0.2f, 0f, 0f), 16);
        innerSR.sortingOrder = 3;

        // Base (bottom)
        Vector3 endPos = path.GetPosition(path.Length - 1);
        GameObject baseGO = new GameObject($"Base_{pathIndex}");
        baseGO.transform.position = endPos;
        baseGO.transform.localScale = Vector3.one * 0.35f;
        baseGO.transform.rotation = Quaternion.Euler(0, 0, 45);
        var baseSR = baseGO.AddComponent<SpriteRenderer>();
        baseSR.sprite = SpriteFactory.CreateSquare(new Color(0.2f, 0.5f, 1f, 0.8f), 16);
        baseSR.sortingOrder = 2;
        // Heart icon (inner circle)
        var heart = new GameObject("Heart");
        heart.transform.SetParent(baseGO.transform, false);
        heart.transform.localScale = Vector3.one * 0.5f;
        heart.transform.localRotation = Quaternion.Euler(0, 0, -45); // counter-rotate
        var heartSR = heart.AddComponent<SpriteRenderer>();
        heartSR.sprite = SpriteFactory.CreateCircle(new Color(1f, 0.3f, 0.3f), 16);
        heartSR.sortingOrder = 3;
    }

    private void SpawnTrees(float xL, float xR, float yT, float yB, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(xL + 0.3f, xR - 0.3f);
            float y = Random.Range(yB + 0.5f, yT - 0.3f);

            // Skip if too close to any path
            bool tooClose = false;
            foreach (var path in _paths)
            {
                for (int w = 0; w < path.Length; w++)
                {
                    if (Vector2.Distance(new Vector2(x, y), path.GetPosition(w)) < 0.8f)
                    { tooClose = true; break; }
                }
                if (tooClose) break;
            }
            if (tooClose) continue;

            GameObject tree = new GameObject($"Tree_{i}");
            tree.transform.position = new Vector3(x, y, 0f);

            // Trunk
            var trunk = new GameObject("Trunk");
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localScale = new Vector3(0.06f, 0.12f, 1f);
            trunk.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            var trunkSR = trunk.AddComponent<SpriteRenderer>();
            trunkSR.sprite = SpriteFactory.CreateSquare(new Color(0.35f, 0.2f, 0.1f), 8);
            trunkSR.sortingOrder = 3;

            // Crown layers
            float crownSize = Random.Range(0.15f, 0.25f);
            for (int c = 0; c < 3; c++)
            {
                var crown = new GameObject($"Crown_{c}");
                crown.transform.SetParent(tree.transform, false);
                crown.transform.localPosition = new Vector3(
                    Random.Range(-0.03f, 0.03f), 0.04f + c * 0.02f, 0f);
                crown.transform.localScale = Vector3.one * (crownSize - c * 0.02f);
                var crownSR = crown.AddComponent<SpriteRenderer>();
                crownSR.sprite = SpriteFactory.CreateCircle(new Color(
                    Random.Range(0.1f, 0.2f),
                    Random.Range(0.35f, 0.55f),
                    Random.Range(0.08f, 0.18f)), 16);
                crownSR.sortingOrder = 4 + c;
            }

            var col = tree.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
        }
    }
}
