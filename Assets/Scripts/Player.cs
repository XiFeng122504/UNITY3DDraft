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
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to player's facing direction
        Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

        // Determine target speed
        float targetSpeed = 0f;
        if (direction.magnitude > 0.1f)
        {
            targetSpeed = isRunning ? runSpeed : walkSpeed;
        }

        // Smoothly change current speed
        if (targetSpeed > currentSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
        }

        // Apply movement
        currentVelocity = direction * currentSpeed;

        // Add gravity
        if (!characterController.isGrounded)
        {
            currentVelocity.y -= 9.81f * Time.deltaTime;
        }
        else
        {
            currentVelocity.y = -0.1f; // Small downward force to keep grounded
        }

        // Move the character
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