using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class NpcCustomer : MonoBehaviour
{
    public float speed = 2f;
    public float rotationSpeed = 5f;
    public float waypointReachDistance = 0.5f;

    public string[] possibleProducts;

    private int minProductsToVisit = 1;
    private int maxProductsToVisit = 3;
    private List<string> shoppingList = new List<string>();

    public Waypoint startWaypoint;
    
    private List<Waypoint> currentPath = new List<Waypoint>();
    private int currentWaypointIndex = 0;
    private Waypoint currentTargetWaypoint;
    
    private NPCState state = NPCState.Idle;
    private float stateTimer = 0f;
    
    private int productsVisited = 0;
    private int productsToVisit;
    private List<string> visitedProducts = new List<string>();

    private void Start()
    {
        productsToVisit = Random.Range(minProductsToVisit, maxProductsToVisit + 1);

        if (startWaypoint != null)
        {
            transform.position = startWaypoint.transform.position;
            StartShopping();
        }
        else
        {
            Debug.LogWarning($"Object {gameObject.name} has no start waypoint assigned");
        }
    }

    private void Update()
    {
        switch (state)
        {
            case NPCState.Idle:
                // Idle actions
                break;
            
            case NPCState.Moving:
                MoveToWaypoint();
                break;
            
            case NPCState.LookingAtProduct:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    ContinueToNextWaypoint();
                }

                break;
            
            case NPCState.AtRegister:
                // Register actions
                break;
            
            case NPCState.RequestingHelp:
                // Quests
                break;
        }
    }

    private void StartShopping()
    {
        if (shoppingList.Count == 0)
        {
            int itemCount = Random.Range(1, 4);
            for (int i = 0; i < itemCount; i++)
            {
                shoppingList.Add(possibleProducts[Random.Range(0, possibleProducts.Length)]);
            }
        }

        FindPathToNextGoal();
    }

    void FindPathToNextGoal()
    {
        Waypoint targetWaypoint;

        if (productsVisited < productsToVisit)
        {
            targetWaypoint = FindRandomProductWaypoint();
        }
        else
        {
            targetWaypoint = FindRegisterWaypoint();
        }

        if (targetWaypoint)
        {
            currentPath = FindPath(GetClosestWaypoint(), targetWaypoint);
            currentWaypointIndex = 0;

            if (currentPath.Count > 0)
            {
                currentTargetWaypoint = currentPath[0];
                state = NPCState.Moving;
            }
        }
    }

    Waypoint GetClosestWaypoint()
    {
        if (currentTargetWaypoint)
        {
            return currentTargetWaypoint;
        }

        Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        Waypoint closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Waypoint wp in allWaypoints)
        {
            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = wp;
            }
        }

        return closest;
    }

    List<Waypoint> FindPath(Waypoint start, Waypoint goal)
    {
        List<Waypoint> path = new List<Waypoint>();
        List<Waypoint> visited = new List<Waypoint>();

        if (FindPathRecursive(start, goal, path, visited))
        {
            return path;
        }
        
        Debug.LogWarning($"No path found from {start.name} to {goal.name}");
        return new List<Waypoint>();
    }

    bool FindPathRecursive(Waypoint current, Waypoint goal, List<Waypoint> path, List<Waypoint> visited, int depth = 0)
    {
        if (depth > 50)
        {
            return false;
        }
        visited.Add(current);

        if (current == goal)
        {
            path.Add(current);
            return true;
        }

        List<Waypoint> shuffled = new List<Waypoint>(current.connectedWaypoints);
        ShuffleList(shuffled);

        foreach (Waypoint next in shuffled)
        {
            if (next && !visited.Contains(next))
            {
                if (FindPathRecursive(next, goal, path, visited, depth + 1))
                {
                    path.Insert(0, current);
                    return true;
                }
            }
        }

        return false;
    }

    void MoveToWaypoint()
    {
        if (!currentTargetWaypoint)
        {
            return;
        }

        Vector3 targetPos = currentTargetWaypoint.transform.position;
        targetPos.y = transform.position.y;

        Vector3 direction = (targetPos - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) < waypointReachDistance)
        {
            OnWaypointReached(currentTargetWaypoint);
        }
    }

    void OnWaypointReached(Waypoint waypoint)
    {
        if (waypoint.waypointType == WaypointType.Product)
        {
            bool shouldStop = Random.value < waypoint.stopChance;
            bool wantThisProduct = shoppingList.Contains(waypoint.productName);
            bool notVisitedYet = !visitedProducts.Contains(waypoint.productName);

            if (shouldStop && wantThisProduct && notVisitedYet)
            {
                state = NPCState.LookingAtProduct;
                stateTimer = Random.Range(waypoint.lookDur.x, waypoint.lookDur.y);

                ++productsVisited;
                visitedProducts.Add(waypoint.productName);
                Debug.Log($"{gameObject.name} is looking at {waypoint.productName}");
                return;
            }
        }
        else if (waypoint.waypointType == WaypointType.Register)
        {
            state = NPCState.AtRegister;
            Debug.Log($"{gameObject.name} reached the register!");
            return;
        }

        ContinueToNextWaypoint();
    }

    void ContinueToNextWaypoint()
    {
        ++currentWaypointIndex;

        if (currentWaypointIndex >= currentPath.Count)
        {
            FindPathToNextGoal();
        }
        else
        {
            currentTargetWaypoint = currentPath[currentWaypointIndex];
            state = NPCState.Moving;
        }
    }

    Waypoint FindRandomProductWaypoint()
    {
        Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        List<Waypoint> productWaypoints = new List<Waypoint>();

        foreach (Waypoint wp in allWaypoints)
        {
            if (wp.waypointType == WaypointType.Product && shoppingList.Contains(wp.productName) &&
                !visitedProducts.Contains(wp.productName))
            {
                productWaypoints.Add(wp);
            }
        }

        if (productWaypoints.Count > 0)
        {
            return productWaypoints[Random.Range(0, productWaypoints.Count)];
        }

        return FindRegisterWaypoint();
    }

    Waypoint FindRegisterWaypoint()
    {
        Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        foreach (Waypoint wp in allWaypoints)
        {
            if (wp.waypointType == WaypointType.Register)
            {
                return wp;
            }
        }
        
        return null;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                if (currentPath[i] != null && currentPath[i + 1] != null)
                {
                    Gizmos.DrawLine(currentPath[i].transform.position, currentPath[i + 1].transform.position);
                }
            }
        }
        
        if (currentTargetWaypoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTargetWaypoint.transform.position);
        }
    }
}

public enum NPCState
{
    Idle,
    Moving,
    LookingAtProduct,
    AtRegister,
    RequestingHelp,
}
