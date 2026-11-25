
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 0.2f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction sprintAction;

    private float verticalVelocity;
    private float cameraPitch;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float jumpTimer;
    private bool sprintHeld;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (spawnPoint != null)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPoint.position, Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f));
            controller.enabled = true;
            cameraPitch = Mathf.Clamp(Mathf.DeltaAngle(0f, spawnPoint.eulerAngles.x), -maxLookAngle, maxLookAngle);
        }

        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "Player Input component is missing from the Player!"
            );
        }
        else if (playerInput.actions != null)
        {
            sprintAction = playerInput.actions.FindAction(
                "Player/Sprint",
                false
            );

            if (sprintAction == null)
            {
                Debug.LogError(
                    "Sprint action not found. Check PlayerInputActions."
                );
            }
        }

        if (cameraTransform == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
                cameraTransform = childCamera.transform;
        }

        if (spawnPoint != null && cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        LockCursor();
    }

    private void Update()
    {
        // Read Sprint action state every frame.
        // Releasing Shift will stop sprinting.
        sprintHeld = sprintAction != null &&
                     sprintAction.IsPressed();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
        }
        else
        {
            lookInput = Vector2.zero;
        }

        Move();
    }

    // Input Action: Move
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input Action: Look
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Input Action: Jump
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpTimer = 0.15f;
    }

    // Input Action: Sprint
    public void OnSprint(InputValue value)
    {
        // Update() checks the actual action state as well.
        sprintHeld = value.isPressed;
    }

    // Input Action: UnlockCursor
    public void OnUnlockCursor(InputValue value)
    {
        if (!value.isPressed)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Input Action: LockCursor
    public void OnLockCursor(InputValue value)
    {
        if (value.isPressed &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    private void Look()
    {
        if (cameraTransform == null)
            return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);

        
        lookInput = Vector2.zero;
    }

    private void Move()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (grounded && jumpTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpTimer = 0f;
        }

        jumpTimer -= Time.deltaTime;

        // Apply gravity vertical only
        verticalVelocity += gravity * Time.deltaTime;

        // Calculate horizontal movement.
        Vector3 horizontalMovement =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        horizontalMovement =
            Vector3.ClampMagnitude(horizontalMovement, 1f);

        // Choose the current horizontal speed.
        float currentSpeed = sprintHeld
            ? sprintSpeed
            : moveSpeed;

        Vector3 velocity = horizontalMovement * currentSpeed;

        
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
