using UnityEngine;
using UnityEngine.InputSystem;


public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // assign the camera (will be rotated for yaw & pitch)


    [Header("Look")]
    public float lookSensitivity = 150f; // degrees per second
    public float maxLookX = 80f; // clamp pitch
    public float lookSmoothTime = 0.04f;


    public InputActionReference lookAction; // Vector2 mouse delta


    private Vector2 lookInput;


    private float targetYaw, currentYaw, yawVelocity;
    private float targetPitch, currentPitch, pitchVelocity;


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        if (lookAction != null) lookAction.action.Enable();


        if (cameraTransform != null)
        {
            // initialize from camera local rotation (convert 0..360 -> -180..180)
            targetYaw = currentYaw = Movement.NormalizeAngle(cameraTransform.localEulerAngles.y);
            targetPitch = currentPitch = Movement.NormalizeAngle(cameraTransform.localEulerAngles.x);
        }
    }


    void OnEnable() { if (lookAction != null) lookAction.action.Enable(); }
    void OnDisable() { if (lookAction != null) lookAction.action.Disable(); }


    void Update()
    {
        lookInput = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;


        float yawDelta = lookInput.x * lookSensitivity * Time.deltaTime;
        float pitchDelta = lookInput.y * lookSensitivity * Time.deltaTime;


        targetYaw += yawDelta;


        targetPitch -= pitchDelta; // invert Y
        targetPitch = Mathf.Clamp(targetPitch, -maxLookX, maxLookX);


        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, lookSmoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, lookSmoothTime);


        if (cameraTransform != null)
        {
            // apply both yaw and pitch to the camera local rotation (X = pitch, Y = yaw)
            cameraTransform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        }
    }
}