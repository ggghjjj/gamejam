using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Header("Waypoints - assign child transforms or drag scene objects")]
    public Transform[] waypoints;

    public int Length => waypoints != null ? waypoints.Length : 0;

    public Vector3 GetPosition(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length)
            return transform.position;
        return waypoints[index].position;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Draw waypoint sphere
            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            // Draw line to next
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Mark start and end
        if (waypoints[0] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(waypoints[0].position, 0.35f);
        }
        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.35f);
        }
    }
}
