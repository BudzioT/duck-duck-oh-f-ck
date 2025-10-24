using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform objToFollow;
    
    public Vector3 objOffset = new Vector3(0, 5, -10);
    public float speed = 5f;

    void LateUpdate()
    {
        if (!objToFollow)
            return;

        Vector3 rotatedOffset = objToFollow.rotation * objOffset;
        
        Vector3 targetPosition = objToFollow.position + objOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

        transform.LookAt(objToFollow);
    }
}