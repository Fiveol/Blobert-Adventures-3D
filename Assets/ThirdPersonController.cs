using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private PlayerInputActions input;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private bool isSprinting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = new PlayerInputActions();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Sprint.performed += ctx => isSprinting = true;
        input.Player.Sprint.canceled += ctx => isSprinting = false;

        input.Player.Jump.performed += ctx => Jump();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Camera-relative movement
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * moveInput.y + right * moveInput.x;

        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (direction.magnitude >= 0.1f)
        {
            // Rotate toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Apply gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction.normalized * targetSpeed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (!controller.isGrounded) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}