using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public Transform cameraTransform; // used to determine movement direction (yaw+pitch on camera but we project to XZ)


    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float moveSmoothTime = 0.05f; // smoothing for horizontal linearVelocity (FixedUpdate)


    [Header("Input")]
    public InputActionReference moveAction; // Vector2
    public InputActionReference jumpAction; // Button

    private Rigidbody rb;


    // inputs captured in Update
    private Vector2 moveInput;


    // movement smoothing state (horizontal only)
    private Vector3 horizontallinearVelocityRef;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;


        if (moveAction != null) moveAction.action.Enable();
    }


    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        jumpAction.action.performed += Jump;
    }
    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1.25f))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    void Update()
    {
        moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    }


    void FixedUpdate()
    {
        // Project camera forward/right onto the XZ plane so pitch doesn't affect movement
        Vector3 forward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;


        Vector3 desiredHorizontal = (forward * moveInput.y + right * moveInput.x) * moveSpeed;


        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);


        // Smooth horizontal linearVelocity for nicer acceleration/deceleration
        Vector3 smoothed = Vector3.SmoothDamp(currentHorizontal, new Vector3(desiredHorizontal.x, 0f, desiredHorizontal.z), ref horizontallinearVelocityRef, moveSmoothTime);


        rb.linearVelocity = new Vector3(smoothed.x, rb.linearVelocity.y, smoothed.z);
    }


    // helper to convert 0..360 to -180..180
    public static float NormalizeAngle(float a)
    {
        if (a > 180f) a -= 360f;
        return a;
    }
}