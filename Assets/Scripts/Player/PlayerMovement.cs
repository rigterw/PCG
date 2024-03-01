using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    private Vector2 moveDir;
    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(movementSpeed * (Vector3)moveDir);
    }

    public void UpdateMovementDir(InputAction.CallbackContext input){
       moveDir = input.ReadValue<Vector2>();
    }
}
