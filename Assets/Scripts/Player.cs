using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 15f;
    public float acceleration = 10f;
    public float deceleration = 20f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    public bool invertY = false;
    public bool lockCursor = true;

    [Header("References")]
    public Camera playerCamera;
    public Transform cameraContainer;

    // Private variables
    private CharacterController characterController;
    private Vector3 currentVelocity;
    private float currentSpeed;
    private float verticalRotation = 0f;
    private bool isRunning = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // If no camera assigned, use main camera
        if (playerCamera == null)
            playerCamera = Camera.main;

        // If no camera container, create one
        if (cameraContainer == null)
        {
            GameObject camContainer = new GameObject("CameraContainer");
            camContainer.transform.parent = transform;
            camContainer.transform.localPosition = new Vector3(0, 1.8f, 0);
            cameraContainer = camContainer.transform;

            if (playerCamera != null)
            {
                playerCamera.transform.parent = cameraContainer;
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        // Lock cursor to center of screen
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleRunning();

        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (invertY) mouseY = -mouseY;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (cameraContainer != null)
        {
            cameraContainer.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        // Get raw movement input so partial presses keep strength information
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 moveInput = new Vector2(horizontal, vertical);
        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);

        // Resolve the desired move direction relative to the active camera.
        // The camera forward/right are projected on the XZ plane so looking up/down不会影响移动方向。
        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (playerCamera != null)
        {
            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
            {
                camForward = forward.normalized;
            }

            Vector3 right = playerCamera.transform.right;
            right.y = 0f;
            if (right.sqrMagnitude > 0.001f)
            {
                camRight = right.normalized;
            }
        }

        Vector3 desiredDirection = (camRight * moveInput.x + camForward * moveInput.y);
        if (desiredDirection.sqrMagnitude > 0.001f)
        {
            desiredDirection.Normalize();
        }

        float targetSpeed = (isRunning ? runSpeed : walkSpeed) * inputMagnitude;
        Vector3 desiredVelocity = desiredDirection * targetSpeed;

        // Smooth acceleration / deceleration on the horizontal plane
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        float accelerationRate = desiredVelocity.magnitude > currentHorizontal.magnitude ? acceleration : deceleration;
        currentHorizontal = Vector3.MoveTowards(currentHorizontal, desiredVelocity, accelerationRate * Time.deltaTime);
        currentSpeed = currentHorizontal.magnitude;

        // Apply gravity separately on Y axis
        float verticalVelocity = currentVelocity.y;
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.1f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        currentVelocity = currentHorizontal + Vector3.up * verticalVelocity;

        // Smoothly rotate player towards movement direction when there is input
        if (desiredDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        characterController.Move(currentVelocity * Time.deltaTime);
    }

    void HandleRunning()
    {
        // Hold Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    // Public methods for external control
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    public void SetMovementSpeed(float walk, float run)
    {
        walkSpeed = walk;
        runSpeed = run;
    }

    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}