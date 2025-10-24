using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float speed = 0f;
    public float maxSpeed = 2f;
    public float accelerate = 0.5f;
    public float decelerate = 6f;
    
    private Vector2 moveInput;


    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        Vector3 move = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;

        if (move.magnitude > 0.1f)
        {
            speed = Mathf.MoveTowards(speed, maxSpeed, accelerate * Time.deltaTime);
        }
        else
        {
            speed = Mathf.MoveTowards(speed, 0.0f, decelerate * Time.deltaTime);
        }
        
        transform.Translate(speed * Time.deltaTime * move, Space.World);
    }
}