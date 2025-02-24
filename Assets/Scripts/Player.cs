using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float moveSpeed = 5.0f;

    Rigidbody rb;

    InputAction moveAction;
    InputAction lookAction;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
       
        // X = camera axis Y
        // Y = camera axix X
        Vector2 lookValue = lookAction.ReadValue<Vector2>();

        Move(moveValue);
    }

    void Move(Vector2 moveValue)
    {
        Vector3 newPosition = rb.position + (new Vector3(moveValue.x, 0, moveValue.y) * moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }
}
