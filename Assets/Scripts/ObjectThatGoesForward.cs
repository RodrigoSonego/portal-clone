using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectThatGoesForward : MonoBehaviour
{
    [SerializeField] private float speed = 2;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * speed;
    }
}
