using UnityEngine;

/// <summary>
/// Attach this to an empty GameObject in the scene.
/// On Awake it builds the entire game scene: Hero, WaypointPath, EnemySpawner, GameManager.
/// This avoids manual scene setup - just create one GO with this script.
/// </summary>
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

        // 2. Waypoint Path - Z-shaped path across the screen
        GameObject pathGO = new GameObject("EnemyPath");
        WaypointPath path = pathGO.AddComponent<WaypointPath>();

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-7f, 4f, 0f),    // top-left
            new Vector3(7f, 4f, 0f),     // top-right
            new Vector3(7f, 1.5f, 0f),   // mid-right
            new Vector3(-7f, 1.5f, 0f),  // mid-left
            new Vector3(-7f, -1f, 0f),   // lower-left
            new Vector3(7f, -1f, 0f),    // lower-right
            new Vector3(7f, -3.5f, 0f),  // bottom-right
            new Vector3(-7f, -3.5f, 0f), // bottom-left (end)
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

        // 3. Hero - starts at center-bottom area
        GameObject heroGO = new GameObject("Hero");
        heroGO.AddComponent<SpriteRenderer>();
        heroGO.AddComponent<HeroController>();
        heroGO.AddComponent<HeroShooter>();

        // Add collider for hero (optional, for future enemy contact damage)
        var heroCol = heroGO.AddComponent<BoxCollider2D>();
        heroCol.size = new Vector2(0.8f, 0.8f);

        heroGO.transform.position = new Vector3(0f, -4f, 0f);

        // 4. Enemy Spawner
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.path = path;

        Debug.Log("=== Tangy TD Scene Setup Complete ===");
        Debug.Log("Hero: WASD to move. Auto-shoots nearest enemy in range.");
        Debug.Log("Enemies spawn and follow the Z-shaped path.");
    }
}
