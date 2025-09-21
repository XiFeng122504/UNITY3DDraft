using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 15f;
    public float acceleration = 10f;
    public float deceleration = 20f;

    [Header("References")]
    public Camera playerCamera;

    // Private variables
    private CharacterController characterController;
    private Vector3 currentVelocity;
    private float currentSpeed;
    private bool isRunning = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // If no camera assigned, use main camera
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Lock cursor for game mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleRunning();

        // Press Escape to toggle cursor
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

        // Rotate character to face movement direction (Assassin's Creed style)
        if (desiredDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
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
    public void SetMovementSpeed(float walk, float run)
    {
        walkSpeed = walk;
        runSpeed = run;
    }
}