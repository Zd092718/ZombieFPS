using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Audio
    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    #endregion
    #region Control
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
    #endregion
    #region Properties
    [SerializeField] private Animator anim;
    private PlayerInput playerInput;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    #endregion

    [Header("Inventory")]
    [SerializeField] private int ammo = 0;
    [SerializeField] private int health = 80;
    private int maxHealth = 100;
    private int maxAmmo = 50;
    [SerializeField] private int ammoMagazine = 0;
    private int ammoMagazineMax = 15;

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
    private void PlayFootstepAudio()
    {

        AudioClip footsteps = audioClips[Random.Range(0, 3)];
        audioSource.PlayOneShot(footsteps);
    }
    private void GetAmmo()
    {
        ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
    }

    private void ExpendAmmo()
    {
        ammoMagazine = Mathf.Clamp(ammoMagazine - 1, 0, ammoMagazineMax);
    }

    private void Reload()
    {
        int ammoNeeded = ammoMagazineMax - ammoMagazine;
        int ammoAvailable = ammoNeeded < ammo ? ammoNeeded : ammo;
        ammo -= ammoAvailable;
        ammoMagazine = ammoAvailable;
    }

    private void GetHealth()
    {
        health = Mathf.Clamp(health + 40, 0, maxHealth);
    }

    private void DepleteHealth(int damage)
    {
        health = Mathf.Clamp(health - damage, 0, maxHealth);
        if (health <= 0)
        {
            audioSource.PlayOneShot(audioClips[5]);
        }
    }

    #region Control Input Functions
    public void OnLook(InputAction.CallbackContext context)
    {

        mouseDelta = context.ReadValue<Vector2>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetBool("isWalking", true);
            moveComposite = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            anim.SetBool("isWalking", false);
            moveComposite = Vector2.zero;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {
                audioSource.PlayOneShot(audioClips[0]);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && !anim.GetBool("fire"))
        {
            if (ammoMagazine > 0)
            {
                anim.SetTrigger("fire");
                ExpendAmmo();
            }
            else
            {
                audioSource.PlayOneShot(audioClips[4]);
            }
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetTrigger("reload");
            Reload();
        }
    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            anim.SetTrigger("melee");
        }
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGrounded()) audioSource.PlayOneShot(audioClips[1]);


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ammo"))
        {
            Destroy(other.gameObject);
            audioSource.PlayOneShot(audioClips[3]);
            GetAmmo();
            Debug.Log("Picked up ammo");
        }
        if (other.CompareTag("Medkit"))
        {
            Destroy(other.gameObject);
            audioSource.PlayOneShot(audioClips[2]);
            GetHealth();
            Debug.Log("Picked up medkit");
        }
        if (other.CompareTag("Hazard"))
        {
            DepleteHealth(10);
        }
    }
}
