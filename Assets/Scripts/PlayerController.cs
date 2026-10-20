using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 1.8f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 35f;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDampTime = 0.1f;

    // Input System
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    // Componentes
    private CharacterController controller;

    // Movimento
    private Vector3 velocity;
    private Vector3 moveVelocity;

    // Câmera
    private float pitch;

    // Controle geral
    private bool movementEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction =
            InputSystem.actions.FindAction("Move");

        lookAction =
            InputSystem.actions.FindAction("Look");

        jumpAction =
            InputSystem.actions.FindAction("Jump");

        sprintAction =
            InputSystem.actions.FindAction("Sprint");

        pitch = cameraTarget.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        InputSystem.actions
            .FindActionMap("Player")
            .Enable();
    }

    private void OnDisable()
    {
        InputSystem.actions
            .FindActionMap("Player")
            .Disable();
    }

    private void Update()
    {
        if (movementEnabled)
        {
            Move();
        }
        else
        {
            velocity = Vector3.zero;
            moveVelocity = Vector3.zero;
        }

        Animacoes();
    }

    private void LateUpdate()
    {
        Look();
    }

    // ==================================================
    // CÂMERA
    // ==================================================

    private void Look()
    {
        Vector2 look =
            lookAction.ReadValue<Vector2>() *
            lookSensitivity;

        // Rotação horizontal do jogador.
        transform.Rotate(
            Vector3.up * look.x
        );

        // Rotação vertical da câmera.
        pitch -= look.y;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        cameraTarget.localRotation =
            Quaternion.Euler(
                pitch,
                0f,
                0f
            );
    }

    // ==================================================
    // MOVIMENTO
    // ==================================================

    private void Move()
    {
        bool grounded =
            controller.isGrounded;

        // Mantém o jogador encostado no chão.
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Pulo.
        if (jumpAction.WasPressedThisFrame() && grounded)
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            animator.SetTrigger("Jump");
        }

        // Gravidade.
        velocity.y +=
            gravity * Time.deltaTime;

        Vector2 input =
            moveAction.ReadValue<Vector2>();

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        // shift corre
        float speed = sprintAction.IsPressed() ? runSpeed : walkSpeed;

        // vai acelerando aos poucos pro blend tree nao trocar seco
        moveVelocity = Vector3.MoveTowards(
            moveVelocity,
            move * speed,
            acceleration * Time.deltaTime
        );

        controller.Move(
            (moveVelocity + velocity) *
            Time.deltaTime
        );
    }

    // ==================================================
    // ANIMAÇÕES
    // ==================================================

    private void Animacoes()
    {
        // velocidade em relaçao ao player (x = lados, z = frente)
        Vector3 localVelocity =
            transform.InverseTransformDirection(moveVelocity);

        animator.SetFloat("VelocityX", localVelocity.x, animationDampTime, Time.deltaTime);
        animator.SetFloat("VelocityZ", localVelocity.z, animationDampTime, Time.deltaTime);
        animator.SetBool("IsGrounded", controller.isGrounded);
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        movementEnabled = isEnabled;

        if (!isEnabled)
        {
            velocity = Vector3.zero;
            moveVelocity = Vector3.zero;
        }
    }
}
