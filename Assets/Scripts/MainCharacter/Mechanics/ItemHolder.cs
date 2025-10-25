using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ItemHolder : MonoBehaviour
{
    [Header("Hold Settings")]
    public Transform holdPoint;
    [SerializeField] private float pickUpRange = 2.0f;
    [SerializeField] private LayerMask itemLayer = -1; // -1 means "Everything" by default
    
    [Header("Physics Settings")]
    [SerializeField] private float holdStrength = 50f;
    [SerializeField] private float holdDamp = 5f;
    [SerializeField] private float maxDistance = 0.3f; // Maximum distance from hold point
    
    [Header("Throw Settings")]
    [SerializeField] private float forceThrowMultiplier = 3f;
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private GameObject heldItem;
    private Rigidbody heldItemRb;
    
    private ConfigurableJoint joint;
    
    private Rigidbody playerRb;

    private Vector3 lastPos;
    private Vector3 velocity;
    
    void Start()
    {
        lastPos = transform.position;
        playerRb = GetComponent<Rigidbody>();
        
        // Create hold point if not assigned
        if (holdPoint == null)
        {
            GameObject holdPointObj = new GameObject("HoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = new Vector3(0f, 1f, 0.5f); // Front of duck
            holdPoint = holdPointObj.transform;
            Debug.Log("HoldPoint created automatically at duck's beak");
        }
        
        // If itemLayer is still 0 (Nothing), set it to Everything
        if (itemLayer.value == 0)
        {
            itemLayer = -1;
            Debug.LogWarning("ItemLayer was not set! Defaulting to 'Everything'. Please set it in Inspector.");
        }
    }

    void Update()
    {
        velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
        
        if ((Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame) && !heldItem)
        {
            TryPickupItem();
        }
        else if ((Mouse.current.leftButton.wasReleasedThisFrame || Keyboard.current.eKey.wasPressedThisFrame) && heldItem) 
        {
            Debug.Log("Trying to throw away an item");
            ThrowItem();
        }
    }

    private void FixedUpdate()
    {
        if (heldItem && heldItemRb && !joint)
        {
            ApplyHoldForce();
        }
    }

    void TryPickupItem()
    {
        Debug.Log($"Checking for items within {pickUpRange} units. LayerMask: {itemLayer.value}");
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRange, itemLayer);
        
        Debug.Log($"Found {colliders.Length} colliders");
        
        if (colliders.Length > 0)
        {
            // Get closest item
            GameObject closestItem = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Collider col in colliders)
            {
                // Skip the player itself and any object that has the player's rigidbody
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform))
                    continue;
                
                // Skip if this object's rigidbody is the player's rigidbody
                Rigidbody checkRb = col.GetComponent<Rigidbody>();
                if (checkRb == playerRb)
                    continue;
                    
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Debug.Log($"Found object: {col.gameObject.name} at distance {distance}");
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = col.gameObject;
                }
            }
            
            if (closestItem != null)
            {
                Debug.Log($"Picking up closest item: {closestItem.name}");
                PickupItem(closestItem);
            }
            else
            {
                Debug.Log("No valid items found (all were player or children)");
            }
        }
    }

    void PickupItem(GameObject item)
    {
        heldItem = item;
        heldItemRb = heldItem.GetComponent<Rigidbody>();

        if (!heldItemRb)
        {
            Debug.LogWarning($"Item {item.name} doesn't have a Rigidbody! Cannot pick up.");
            heldItem = null;
            heldItemRb = null;
            return;
        }
        
        Debug.Log($"Successfully picked up: {item.name}");
        
        // Don't ignore collision with player - we want physics interaction
        // But reduce mass to prevent pushing player around
        heldItemRb.mass = Mathf.Min(heldItemRb.mass, 1f);
        
        joint = heldItem.AddComponent<ConfigurableJoint>();
        joint.connectedBody = playerRb;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = holdPoint.localPosition;
        
        // Make item stick closer - use locked motion with tight limits
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        
        // Very tight limit to keep item close
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = maxDistance; // Much tighter - only 0.3 units away max
        limit.bounciness = 0f;
        limit.contactDistance = 0.01f;
        joint.linearLimit = limit;
        
        // Strong spring to pull item back quickly
        SoftJointLimitSpring spring = new SoftJointLimitSpring();
        spring.spring = holdStrength * 10f; // Much stronger spring
        spring.damper = holdDamp * 5f; // More damping to reduce oscillation
        joint.linearLimitSpring = spring;
        
        // Allow free rotation
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        
        // Reduce drag while held for better physics feel
        heldItemRb.linearDamping = 0.5f;
        heldItemRb.angularDamping = 0.5f;
    }

    void ApplyHoldForce()
    {
        Vector3 toHoldPoint = holdPoint.position - heldItem.transform.position;
        float distance = toHoldPoint.magnitude;

        Vector3 springForce = toHoldPoint * holdStrength;
        Vector3 dampingForce = -heldItemRb.linearVelocity * holdDamp;

        Vector3 totalForce = springForce + dampingForce;
        heldItemRb.AddForce(totalForce, ForceMode.Force);

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);
        heldItemRb.MoveRotation(Quaternion.Slerp(heldItemRb.rotation, targetRotation, Time.fixedDeltaTime * 2f));
    }

    void ThrowItem()
    {
        if (heldItem == null) return;
        
        Debug.Log($"Throwing {heldItem.name} with velocity magnitude: {velocity.magnitude}");
        
        // Destroy joint if it exists
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
        
        // Reset damping to original values
        heldItemRb.linearDamping = 0.05f;
        heldItemRb.angularDamping = 0.05f;

        Vector3 throwDirection = transform.forward;
        float velocityMagnitude = new Vector3(velocity.x, 0, velocity.z).magnitude;

        Vector3 throwForce = throwDirection * minThrowForce;
        throwForce += velocity * forceThrowMultiplier;
        
        float forceMagnitude = Mathf.Clamp(throwForce.magnitude, minThrowForce, maxThrowForce);
        throwForce = throwForce.normalized * forceMagnitude;
        
        throwForce.y += 2f;
        
        heldItemRb.linearVelocity = Vector3.zero;
        heldItemRb.AddForce(throwForce, ForceMode.Impulse);
        heldItemRb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        
        Debug.Log($"Threw with force: {forceMagnitude}");
        
        heldItem = null;
        heldItemRb = null;
    }
    
    // Visualize pickup range in editor
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw pickup range
        Gizmos.color = heldItem != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
        
        // Draw hold point
        if (holdPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(holdPoint.position, 0.2f);
            
            // Draw line to held item
            if (heldItem != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(holdPoint.position, heldItem.transform.position);
            }
        }
    }
}