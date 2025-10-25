using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> connectedWaypoints = new List<Waypoint>();
    public WaypointType waypointType = WaypointType.Path;

    public string productName;
    public float stopChance = 0.5f;

    public Vector2 lookDur = new Vector2(1f, 3f);

    public Color gizmoColor = Color.yellow;


    public void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f);


        if (connectedWaypoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Waypoint wp in connectedWaypoints)
            {
                if (wp != null)
                {
                    Gizmos.DrawLine(transform.position, wp.transform.position);

                    Vector3 direction = (wp.transform.position - transform.position).normalized;
                    Vector3 midPoint = (transform.position + wp.transform.position) / 2f;
                    DrawArrow(midPoint, direction, 0.3f);
                }
            }
        }
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
            waypointType.ToString() + (waypointType == WaypointType.Product ? $"\n{productName}" : ""));
#endif
    }

    void DrawArrow(Vector3 pos, Vector3 dir, float size)
    {
        Vector3 right = Quaternion.Euler(0, 30, 0) * dir * size;
        Vector3 left = Quaternion.Euler(0, -30, 0) * dir * size;
        
        Gizmos.DrawLine(pos, pos - right);
        Gizmos.DrawLine(pos, pos - left);
    }
}



public enum WaypointType
{
    Path,  // Regular waypoints - aisles, corners etc.
    Product,  // Possible stop points - products
    Register, // End goal - register
    Entrance, // Starting place - entrance
}
