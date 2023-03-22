using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayerMask;
    private Vector2 moveComposite;

    [Header("Look")]
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private float minXLook;
    [SerializeField] private float maxXLook;
    [SerializeField] private bool invertCamera;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float gamepadSensitivity;
    private float lookSensitivity;
    private float camCurXRot;
    private Vector2 mouseDelta;

    [SerializeField] private Animator anim;
    private PlayerInput playerInput;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    private void Move()
    {
        Vector3 dir = transform.forward * moveComposite.y + transform.right * moveComposite.x;

        dir *= moveSpeed;

        dir.y = rb.velocity.y;

        rb.velocity = dir;
    }



    public bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hit,
            (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }

        return false;
    }
    private void CameraLook()
    {
        if (playerInput.currentControlScheme == "Gamepad")
        {
            lookSensitivity = gamepadSensitivity;
        }
        else
        {
            lookSensitivity = mouseSensitivity;
        }

        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);

        //Invert camera if option is selected
        if (invertCamera)
        {
            cameraContainer.localEulerAngles = new Vector3(camCurXRot, 0, 0);

        }
        else
        {
            cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);
        }
        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);

        Debug.Log(playerInput.currentControlScheme);
    }

    public void OnLook(InputAction.CallbackContext context)
    {

        mouseDelta = context.ReadValue<Vector2>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetBool("isWalking", true);
            InvokeRepeating("PlayFootstepAudio", 0, 0.4f);
            moveComposite = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            anim.SetBool("isWalking", false);
            CancelInvoke("PlayFootstepAudio");
            moveComposite = Vector2.zero;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {
                audioSource.PlayOneShot(audioClips[4]);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                if(anim.GetBool("isWalking")) CancelInvoke("PlayFootstepAudio");
            }

        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            anim.SetBool("isFiring", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            anim.SetBool("isFiring", false);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetTrigger("reload");
        }
    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetTrigger("melee");
        }
    }

    private void PlayFootstepAudio()
    {

        AudioClip footsteps = audioClips[Random.Range(0, 3)];
        audioSource.PlayOneShot(footsteps);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(IsGrounded()) audioSource.PlayOneShot(audioClips[5]);
        if(anim.GetBool("isWalking")) InvokeRepeating("PlayFootstepAudio", 0, 0.4f);
    }
}
