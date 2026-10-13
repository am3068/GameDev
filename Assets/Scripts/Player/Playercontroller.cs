
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
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
    private float verticalVelocity;
    private float cameraPitch;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
                cameraTransform = childCamera.transform;
        }

        LockCursor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Cursor.lockState != CursorLockMode.Locked &&
            Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }

        if (Cursor.lockState == CursorLockMode.Locked)
            Look();

        Move();
    }

    private void Look()
    {
        if (cameraTransform == null ||
            Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch, -maxLookAngle, maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movement =
            transform.right * x + transform.forward * z;

        movement = Vector3.ClampMagnitude(movement, 1f);

        // Keep the character grounded.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Jump only when grounded.
        if (controller.isGrounded &&
            Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity.
        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        // Hold Left Shift to sprint.
        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : moveSpeed;

        controller.Move(
            movement * currentSpeed * Time.deltaTime
        );
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
