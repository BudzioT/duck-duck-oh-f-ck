using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // Offset from character position
    
    [Header("Camera Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float smoothSpeed = 15f;
    
    [Header("Rotation Constraints")]
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 70f;
    
    [Header("Camera Collision")]
    [SerializeField] private bool enableCameraCollision = true;
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionMask;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned!");
            return;
        }
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentDistance = distance;
        
        // Initialize rotation to be behind the character
        currentX = target.eulerAngles.y;
        currentY = 20f;
    }
    
    void Update()
    {
        // Press Escape to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Click to lock cursor again
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Only rotate camera when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Clamp vertical rotation
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }
        
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        // Calculate target position for camera to look at
        Vector3 focusPosition = target.position + targetOffset;
        
        // Handle camera collision
        float finalDistance = distance;
        if (enableCameraCollision)
        {
            Vector3 direction = rotation * -Vector3.forward;
            RaycastHit hit;
            
            if (Physics.Raycast(focusPosition, direction, out hit, distance, collisionMask))
            {
                finalDistance = hit.distance - collisionOffset;
            }
        }
        
        // Calculate desired camera position
        Vector3 desiredPosition = focusPosition + rotation * new Vector3(0, 0, -finalDistance);
        
        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Make camera look at focus position
        transform.LookAt(focusPosition);
    }
}