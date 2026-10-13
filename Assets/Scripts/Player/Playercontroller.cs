
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Transform playerCamera;
    private float cameraPitch;
    private float verticalVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            playerCamera = cam.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Release mouse cursor with Escape key.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Click inside Game view to capture the mouse if exited.
        if (Cursor.lockState != CursorLockMode.Locked &&
            Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        LookAround();
        MovePlayer();
    }

    private void LookAround()
    {
        if (playerCamera == null ||
            Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void MovePlayer()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movement =
            transform.right * x + transform.forward * z;

        movement = Vector3.ClampMagnitude(movement, 1f);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += -9.81f * Time.deltaTime;

        movement.y = verticalVelocity;

        controller.Move(movement * moveSpeed * Time.deltaTime);
    }
}
