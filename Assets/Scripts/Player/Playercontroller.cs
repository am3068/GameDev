
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform survivorBook;

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
    private const float InteractionRange = 3f;
    private const float PortalInteractionRange = 2f;
    private const float BookMarkerRange = 6f;
    private const float PortalMarkerRange = 12f;
    private const float CrystalMarkerRange = 12f;
    private readonly RaycastHit[] interactionHits = new RaycastHit[32];
    private readonly InteractionDisplay interactionDisplay = new InteractionDisplay();
    private PortalFogVisibility[] portals;
    private Camera interactionCamera;
    private Renderer bookRenderer;
    private InventorySystem inventory;
    private Collider pickupTarget;
    private PortalFogVisibility portalTarget;
    private bool bookTarget;
    private string interactionPrompt;
    private int interactionFrame = -1;
    private float timeScaleBeforeReading;

    public bool HandledInteractionThisFrame => interactionFrame == Time.frameCount || IsReadingBook;
    public bool IsReadingBook { get; private set; }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inventory = GetComponent<InventorySystem>();
        portals = FindObjectsByType<PortalFogVisibility>(FindObjectsSortMode.None);
        if (survivorBook)
            bookRenderer = survivorBook.GetComponentInChildren<Renderer>();
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

        if (cameraTransform)
            interactionCamera = cameraTransform.GetComponentInChildren<Camera>();

        LockCursor();
    }

    private void Update()
    {
        if (IsReadingBook)
            return;

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
        UpdateInteractionTarget();
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
        if (value.isPressed && !IsReadingBook)
            jumpTimer = 0.15f;
    }

    public void OnUseItem(InputValue value)
    {
        if (!value.isPressed || !isActiveAndEnabled)
            return;

        if (IsReadingBook)
        {
            interactionFrame = Time.frameCount;
            CloseBook();
            return;
        }

        UpdateInteractionTarget();
        if (bookTarget)
        {
            interactionFrame = Time.frameCount;
            IsReadingBook = true;
            timeScaleBeforeReading = Time.timeScale;
            Time.timeScale = 0f;
            lookInput = Vector2.zero;
            jumpTimer = 0f;
        }
        else if (pickupTarget)
        {
            interactionFrame = Time.frameCount;
            inventory.Collect(pickupTarget);
            UpdateInteractionTarget();
        }
        else if (portalTarget)
        {
            interactionFrame = Time.frameCount;
            portalTarget.TryActivate(inventory);
            UpdateInteractionTarget();
        }
    }

    private void UpdateInteractionTarget()
    {
        interactionPrompt = null;
        pickupTarget = null;
        portalTarget = null;
        bookTarget = false;
        if (!cameraTransform || Time.timeScale == 0f || Cursor.lockState != CursorLockMode.Locked)
            return;

        var ray = new Ray(cameraTransform.position, cameraTransform.forward);
        float nearest = InteractionRange;
        int count = Physics.RaycastNonAlloc(ray, interactionHits, InteractionRange,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            var hit = interactionHits[i];
            if (!hit.collider || hit.transform.IsChildOf(transform) || hit.distance >= nearest)
                continue;

            bool isPickup = inventory && InventorySystem.GetPickupName(hit.collider) != null;
            if (hit.collider.isTrigger && !isPickup)
                continue;

            nearest = hit.distance;
            pickupTarget = isPickup ? hit.collider : null;
        }

        if (bookRenderer && bookRenderer.gameObject.activeInHierarchy)
        {
            var bounds = bookRenderer.bounds;
            bounds.Expand(.08f);
            if (bounds.IntersectRay(ray, out float distance) && distance <= nearest)
            {
                bookTarget = true;
                pickupTarget = null;
                interactionPrompt = "[E] Read survivor's book";
                return;
            }
        }

        if (pickupTarget)
        {
            string item = InventorySystem.GetPickupName(pickupTarget);
            interactionPrompt = inventory.CanCollect(item) ? "[E] Collect " + item.ToLowerInvariant() : "Inventory full";
            return;
        }

        if (portals == null)
            return;

        float bestDistance = PortalInteractionRange * PortalInteractionRange;
        foreach (var portal in portals)
        {
            if (!portal)
                continue;
            float distance = portal.GetApproachDistanceSquared(transform.position);
            if (distance >= bestDistance)
                continue;
            portalTarget = portal;
            bestDistance = distance;
        }

        if (portalTarget)
            interactionPrompt = portalTarget.GetInteractionPrompt(inventory);
    }

    private void OnGUI()
    {
        if (IsReadingBook)
        {
            interactionDisplay.Draw(true, null);
            return;
        }

        if (Time.timeScale <= 0f || Cursor.lockState != CursorLockMode.Locked)
            return;

        if (interactionCamera && Event.current.type == EventType.Repaint)
        {
            if (bookRenderer && bookRenderer.gameObject.activeInHierarchy)
            {
                Bounds bounds = bookRenderer.bounds;
                Vector3 position = new Vector3(bounds.center.x, bounds.max.y + .015f, bounds.center.z);
                if ((position - interactionCamera.transform.position).sqrMagnitude <= BookMarkerRange * BookMarkerRange)
                    DrawInteractionMarker(position, survivorBook);
            }

            if (portals != null)
            {
                foreach (var portal in portals)
                {
                    if (portal && !portal.IsActivated &&
                        portal.GetApproachDistanceSquared(transform.position) < PortalMarkerRange * PortalMarkerRange)
                        DrawInteractionMarker(portal.InteractionMarkerPosition, portal.transform);
                }
            }

            var crystalMarkers = CrystalPickupMarker.Active;
            for (int i = 0; i < crystalMarkers.Count; i++)
            {
                var marker = crystalMarkers[i];
                Vector3 position = marker.Position;
                if ((position - interactionCamera.transform.position).sqrMagnitude <= CrystalMarkerRange * CrystalMarkerRange)
                    DrawInteractionMarker(position, marker.transform);
            }
        }

        interactionDisplay.Draw(false, interactionPrompt);
    }

    private void DrawInteractionMarker(Vector3 position, Transform target)
    {
        Vector3 viewport = interactionCamera.WorldToViewportPoint(position);
        if (viewport.z < interactionCamera.nearClipPlane || viewport.x <= 0f || viewport.x >= 1f ||
            viewport.y <= 0f || viewport.y >= 1f)
            return;

        if (Physics.Linecast(interactionCamera.transform.position, position, out RaycastHit hit,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(target))
            return;

        Vector3 screen = interactionCamera.ViewportToScreenPoint(viewport);
        interactionDisplay.DrawMarker(new Vector2(screen.x, Screen.height - screen.y));
    }

    private void OnDestroy()
    {
        interactionDisplay.ReleaseMarker();
    }

    private void CloseBook()
    {
        if (!IsReadingBook)
            return;

        IsReadingBook = false;
        Time.timeScale = timeScaleBeforeReading;
    }

    private void OnDisable()
    {
        CloseBook();
        interactionPrompt = null;
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

        if (IsReadingBook)
        {
            CloseBook();
            return;
        }

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
