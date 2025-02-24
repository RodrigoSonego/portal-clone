using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float sensitivity = 2f;

    [SerializeField] float moveSpeed = 5.0f;

    Rigidbody rb;

    InputAction moveAction;
    InputAction lookAction;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        InputSystem.actions.FindAction("Pause").performed += ToggleMouseLock;

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }


    void FixedUpdate()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
       
        // X = camera axis Y
        // Y = camera axix X
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        print(lookValue);


        Move(moveValue);
        UpdateCamera(lookValue);
    }

    void Move(Vector2 moveValue)
    {
        Vector3 newPosition = rb.position + (new Vector3(moveValue.x, 0, moveValue.y) * moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    void UpdateCamera(Vector2 offset)
    {
        Vector3 camEuler = cam.transform.rotation.eulerAngles;
        float xRotation = camEuler.x - (offset.y * sensitivity);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        Vector3 bodyEuler = transform.rotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(0, bodyEuler.y + offset.x, 0);
    }

    private void ToggleMouseLock(InputAction.CallbackContext context)
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
