using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ItemHolder : MonoBehaviour
{
    public Transform holdPoint;
    private float pickUpRange = 2.0f;
    
    private LayerMask itemLayer = -1;
    
    private float holdStrength = 50f;
    private float holdDamp = 5f;
    
    private float forceThrowMultiplier = 3f;
    private float minThrowForce = 5f;
    private float maxThrowForce = 20f;

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
        
        if (holdPoint == null)
        {
            GameObject holdPointObj = new GameObject("HoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = new Vector3(0f, 1f, 0.5f); // Front of duck
            holdPoint = holdPointObj.transform;
            Debug.Log("HoldPoint created automatically at duck's beak");
        }
        
        velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    // Update is called once per frame
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
        Debug.Log(itemLayer);
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRange, itemLayer);
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
            heldItem = null;
            heldItemRb = null;
            return;
        }
        
        joint = heldItem.AddComponent<ConfigurableJoint>();
        joint.connectedBody = playerRb;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = holdPoint.localPosition;
        
        // Configure joint for soft holding
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.5f;
        limit.bounciness = 0;
        limit.contactDistance = 0.1f;
        joint.linearLimit = limit;
        
        // Add spring
        SoftJointLimitSpring spring = new SoftJointLimitSpring();
        spring.spring = holdStrength;
        spring.damper = holdDamp;
        joint.linearLimitSpring = spring;
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
        
        // Destroy joint if it exists
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

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
        
        heldItem = null;
        heldItemRb = null;
    }
}
