using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayerMask;
    private Vector2 moveComposite;


    [SerializeField] private Vector2 lookComposite;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }



    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 dir = transform.forward * moveComposite.y + transform.right * moveComposite.x;

        dir *= moveSpeed;

        dir.y = rb.velocity.y;

        rb.velocity = dir;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            moveComposite = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            moveComposite = Vector2.zero;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

        }
    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hit,
            (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }

        return false;
    }
}
