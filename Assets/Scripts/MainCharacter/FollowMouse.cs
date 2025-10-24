using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    public GameObject target;

    public float rotationSpeed = 10.0f;
    public float verticalTiltFactor = 0.2f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (!mainCamera)
        {
            Debug.Log("No camera you jerk");
        }
    }

    void Update()
    {
        if (!mainCamera) return;

        RotateToMouse();
    }

    void RotateToMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = hitPoint - transform.position;
            direction = -direction;
            
            // Optional slight vertical tilt
            direction.y *= verticalTiltFactor;
            direction.y = Mathf.Clamp(direction.y, -0.5f, 0.5f);

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

}
