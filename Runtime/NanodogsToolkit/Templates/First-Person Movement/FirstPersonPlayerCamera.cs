// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// Advanced first-person player camera with smooth look controls, dual-harmonic movement bobbing,
    /// natural idle breathing sway, strafe banking, landing/jump recoil, and dynamic FOV sprint kick.
    /// </summary>
    public class FirstPersonPlayerCamera : MonoBehaviour
    {
        [Header("Input References")]
        [Tooltip("Input action for look delta (Mouse Delta / Right Stick).")]
        public InputActionReference lookAction;
        [Tooltip("Input action for movement (WASD / Left Stick).")]
        public InputActionReference moveAction;

        [Header("Rotation Settings")]
        [Tooltip("Mouse sensitivity factor.")]
        public float mouseSensitivity = 100f;
        [Tooltip("Gamepad sensitivity factor.")]
        public float gamepadSensitivity = 200f;
        [Tooltip("Smoothing time for mouse/gamepad look movement.")]
        public float smoothTime = 0.05f;
        [Tooltip("Assign the Transform used for yaw (horizontal Y-axis rotation).")]
        public Transform yawPivot;

        [Header("Head Bob Settings")]
        [Tooltip("Enables head bobbing while moving.")]
        public bool enableHeadBob = true;
        [Tooltip("Frequency of footstep bob cycles.")]
        public float bobFrequency = 2.0f;
        [Tooltip("Overall displacement amplitude for head bobbing.")]
        public float bobAmplitude = 0.05f;
        [Tooltip("Speed at which head bobbing fades in and out.")]
        public float bobFadeSpeed = 8f;
        [Tooltip("Minimum movement input magnitude required to start bobbing.")]
        public float bobStartThreshold = 0.1f;
        [Tooltip("Optional custom curve for horizontal bob (evaluated if procedural harmonics disabled).")]
        public AnimationCurve bobCurveX;
        [Tooltip("Optional custom curve for vertical bob (evaluated if procedural harmonics disabled).")]
        public AnimationCurve bobCurveY;

        [Header("Head Bob Polish")]
        [Tooltip("If true, uses smooth procedural Lissajous harmonic curves with rotational tilting. If false, evaluates bobCurveX/Y.")]
        public bool useProceduralHarmonics = true;
        [Tooltip("Multiplier for horizontal lateral bobbing sway.")]
        [Range(0f, 2f)]
        public float bobHorizontalMultiplier = 0.6f;
        [Tooltip("Multiplier for vertical bobbing dip.")]
        [Range(0f, 2f)]
        public float bobVerticalMultiplier = 1.0f;
        [Tooltip("Subtle camera roll tilt while stepping (degrees).")]
        [Range(0f, 5f)]
        public float bobRollAmplitude = 1.2f;
        [Tooltip("Subtle camera pitch nod on footstep impact (degrees).")]
        [Range(0f, 5f)]
        public float bobPitchAmplitude = 0.6f;
        [Tooltip("Dynamically scales bob frequency with the player's movement speed.")]
        public bool scaleBobWithSpeed = true;

        [Header("Idle Camera Movement (Breathing Sway)")]
        [Tooltip("Enables subtle breathing and organic camera sway when standing still.")]
        public bool enableIdleMovement = true;
        [Tooltip("Frequency of the idle breathing cycle.")]
        public float idleBobFrequency = 1.2f;
        [Tooltip("Translational amplitude of idle breathing.")]
        public float idleBobPositionAmplitude = 0.012f;
        [Tooltip("Rotational amplitude of idle breathing in degrees (pitch and roll).")]
        public float idleBobRotationAmplitude = 0.35f;
        [Tooltip("Speed at which idle sway blends in when stopping and out when moving.")]
        public float idleFadeSpeed = 4f;

        [Header("Dynamic Strafe Banking & Look Sway")]
        [Tooltip("Tilts the camera roll slightly when strafing left or right.")]
        public bool enableStrafeTilt = true;
        [Tooltip("Maximum roll angle when strafing (degrees).")]
        [Range(0f, 5f)]
        public float strafeTiltAngle = 1.6f;
        [Tooltip("Speed of strafe tilt response.")]
        public float strafeTiltSpeed = 8f;

        [Tooltip("Slight camera roll banking when turning the mouse quickly.")]
        public bool enableLookBanking = true;
        [Tooltip("Amount of roll banking applied during mouse rotation.")]
        public float lookBankingStrength = 0.015f;
        [Tooltip("Maximum roll angle from rapid mouse turning.")]
        public float maxLookBankingAngle = 2.0f;

        [Header("Landing & Jump Recoil")]
        [Tooltip("Enables camera dip and pitch recoil when landing from a fall.")]
        public bool enableLandingBob = true;
        [Tooltip("Vertical downward displacement on landing.")]
        public float landingBobAmount = 0.08f;
        [Tooltip("Downward pitch nod angle on landing (degrees).")]
        public float landingPitchAmount = 2.0f;
        [Tooltip("Speed of landing compression.")]
        public float landingBobSpeed = 12f;
        [Tooltip("Speed of return after landing.")]
        public float landingBobReturnSpeed = 6f;
        [Tooltip("Minimum downward velocity required to trigger landing bob.")]
        public float landingVelocityThreshold = -3.5f;

        [Tooltip("Enables subtle camera kick when jumping.")]
        public bool enableJumpRecoil = true;
        [Tooltip("Positional recoil displacement when jumping.")]
        public float jumpRecoilPosition = 0.025f;
        [Tooltip("Pitch recoil nod when jumping (degrees).")]
        public float jumpRecoilPitch = 1.0f;

        [Header("Dynamic FOV Sprint Kick")]
        [Tooltip("Subtly expands the camera FOV while sprinting.")]
        public bool enableFovKick = true;
        [Tooltip("Extra degrees of FOV added while sprinting.")]
        public float sprintFovOffset = 6f;
        [Tooltip("Speed of FOV expansion and recovery.")]
        public float fovTransitionSpeed = 8f;

        [Header("References & Ground Check")]
        [Tooltip("Rigidbody of the player character.")]
        public Rigidbody playerRigidbody;
        [Tooltip("Movement controller component of the player.")]
        public FirstPersonPlayerMovement playerMovement;
        [Tooltip("Layer mask for ground detection.")]
        public LayerMask groundMask = ~0;
        [Tooltip("Raycast distance for ground detection.")]
        public float groundCheckDistance = 0.5f;
        [Tooltip("Origin transform for ground detection.")]
        public Transform groundCheckOrigin;

        // Internal State - Look
        private float xRotation = 0f;
        private Vector2 smoothedDelta;
        private Vector2 targetDelta;

        // Internal State - Bobbing
        protected Vector3 initialCameraLocalPos;
        protected float bobWeight;
        private float bobTimer;
        private Vector3 currentBobPosOffset;
        private Vector3 currentBobRotOffset;

        // Internal State - Idle Sway
        private float idleWeight;
        private float idleTimer;
        private Vector3 currentIdlePosOffset;
        private Vector3 currentIdleRotOffset;

        // Internal State - Tilt & Banking
        private float currentStrafeRoll;
        private float currentLookRoll;

        // Internal State - Landing & Jump
        private bool wasGroundedLastFrame = true;
        private float landingOffsetY;
        private float landingPitchOffset;
        private float landingPosLerpVelocity;
        private float landingPitchLerpVelocity;
        private float jumpRecoilOffset;
        private float jumpRecoilVelocity;

        // Internal State - FOV
        private Camera camComponent;
        private float baseFov = 60f;

        protected void OnEnable()
        {
            if (lookAction != null) lookAction.action.Enable();
            if (moveAction != null) moveAction.action.Enable();

            if (playerMovement != null)
            {
                playerMovement.OnJumped += HandleJumpRecoil;
            }
        }

        protected void OnDisable()
        {
            if (lookAction != null) lookAction.action.Disable();
            if (moveAction != null) moveAction.action.Disable();

            if (playerMovement != null)
            {
                playerMovement.OnJumped -= HandleJumpRecoil;
            }
        }

        protected void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            initialCameraLocalPos = transform.localPosition;

            camComponent = GetComponent<Camera>();
            if (camComponent != null)
            {
                baseFov = camComponent.fieldOfView;
            }

            // Auto-resolve references if unassigned
            if (playerMovement == null)
            {
                playerMovement = GetComponentInParent<FirstPersonPlayerMovement>();
            }

            if (playerMovement != null)
            {
                playerMovement.OnJumped += HandleJumpRecoil;

                if (playerRigidbody == null)
                {
                    playerRigidbody = playerMovement.rb != null ? playerMovement.rb : playerMovement.GetComponent<Rigidbody>();
                }
            }

            if (groundCheckOrigin == null)
            {
                groundCheckOrigin = playerRigidbody != null ? playerRigidbody.transform : transform.parent;
            }
        }

        protected void Update()
        {
            HandleLook();
            HandleHeadBob();
            HandleIdleMovement();
            HandleDynamicTilt();
            HandleLandingBob();
            HandleJumpSettling();
            HandleFovKick();
            ApplyCameraTransforms();
        }

        protected void HandleLook()
        {
            if (lookAction == null || yawPivot == null) return;

            // Determine sensitivity based on input device
            float sensitivity = mouseSensitivity;
            var lastControl = lookAction.action.activeControl;
            if (lastControl != null)
            {
                if (lastControl.device is Gamepad)
                    sensitivity = gamepadSensitivity;
                else if (lastControl.device is Mouse)
                    sensitivity = mouseSensitivity;
            }

            Vector2 rawDelta = lookAction.action.ReadValue<Vector2>() * sensitivity;
            float smoothFactor = 1f - Mathf.Exp(-smoothTime * 60f * Time.deltaTime);
            targetDelta = rawDelta;
            smoothedDelta = Vector2.Lerp(smoothedDelta, targetDelta, smoothFactor);

            Vector2 finalDelta = smoothedDelta * Time.deltaTime;

            // Look pitch (X axis)
            xRotation -= finalDelta.y;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Yaw (Y axis) applied to yaw pivot
            yawPivot.Rotate(Vector3.up * finalDelta.x);
        }

        protected void HandleHeadBob()
        {
            if (!enableHeadBob || moveAction == null)
            {
                bobWeight = Mathf.MoveTowards(bobWeight, 0f, bobFadeSpeed * Time.deltaTime);
                currentBobPosOffset = Vector3.zero;
                currentBobRotOffset = Vector3.zero;
                return;
            }

            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            bool hasInput = moveInput.magnitude > bobStartThreshold;
            bool isGrounded = IsGrounded();

            // Check actual movement velocity if rigidbody is present
            bool isActuallyMoving = true;
            float currentSpeed = 0f;
            if (playerRigidbody != null)
            {
                Vector3 horizontalVel = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
                currentSpeed = horizontalVel.magnitude;
                isActuallyMoving = currentSpeed > 0.15f;
            }

            bool shouldBob = isGrounded && hasInput && isActuallyMoving;
            float targetWeight = shouldBob ? 1f : 0f;
            bobWeight = Mathf.MoveTowards(bobWeight, targetWeight, bobFadeSpeed * Time.deltaTime);

            if (bobWeight > 0.0001f)
            {
                // Scale frequency dynamically with speed
                float baseSpeed = (playerMovement != null && playerMovement.speed > 0.1f) ? playerMovement.speed : 5f;
                float speedMultiplier = scaleBobWithSpeed ? Mathf.Clamp(currentSpeed / baseSpeed, 0.6f, 2.0f) : 1f;

                bobTimer += Time.deltaTime * bobFrequency * speedMultiplier * Mathf.PI * 2f;
                if (bobTimer > Mathf.PI * 200f) bobTimer -= Mathf.PI * 200f; // Prevent floating point overflow

                if (useProceduralHarmonics || bobCurveX == null || bobCurveY == null || bobCurveX.length == 0)
                {
                    // Lissajous dual harmonic:
                    // Horizontal sway (stride): sin(t)
                    // Vertical dip (steps): -cos(2t) * 0.5 (dips on each foot)
                    float sin1 = Mathf.Sin(bobTimer);
                    float cos2 = Mathf.Cos(bobTimer * 2f);

                    float xPos = sin1 * bobAmplitude * bobHorizontalMultiplier;
                    float yPos = (-cos2 * 0.5f) * bobAmplitude * bobVerticalMultiplier;

                    // Rotational roll & pitch
                    float zRoll = -sin1 * bobRollAmplitude;
                    float xPitch = cos2 * bobPitchAmplitude;

                    currentBobPosOffset = new Vector3(xPos, yPos, 0f) * bobWeight;
                    currentBobRotOffset = new Vector3(xPitch, 0f, zRoll) * bobWeight;
                }
                else
                {
                    // Normalized [0..1] curve evaluation
                    float normalizedTimer = (bobTimer / (Mathf.PI * 2f)) % 1f;
                    if (normalizedTimer < 0f) normalizedTimer += 1f;

                    float xOffset = bobCurveX.Evaluate(normalizedTimer) * bobAmplitude * bobHorizontalMultiplier;
                    float yOffset = bobCurveY.Evaluate(normalizedTimer) * bobAmplitude * bobVerticalMultiplier;

                    float sin1 = Mathf.Sin(bobTimer);
                    float cos2 = Mathf.Cos(bobTimer * 2f);

                    currentBobPosOffset = new Vector3(xOffset, yOffset, 0f) * bobWeight;
                    currentBobRotOffset = new Vector3(cos2 * bobPitchAmplitude, 0f, -sin1 * bobRollAmplitude) * bobWeight;
                }
            }
            else
            {
                currentBobPosOffset = Vector3.Lerp(currentBobPosOffset, Vector3.zero, Time.deltaTime * bobFadeSpeed);
                currentBobRotOffset = Vector3.Lerp(currentBobRotOffset, Vector3.zero, Time.deltaTime * bobFadeSpeed);
            }
        }

        protected void HandleIdleMovement()
        {
            if (!enableIdleMovement)
            {
                idleWeight = Mathf.MoveTowards(idleWeight, 0f, idleFadeSpeed * Time.deltaTime);
                currentIdlePosOffset = Vector3.zero;
                currentIdleRotOffset = Vector3.zero;
                return;
            }

            // Idle is active when not bobbing and grounded
            bool isGrounded = IsGrounded();
            bool isMoving = bobWeight > 0.05f;
            float targetIdleWeight = (!isMoving && isGrounded) ? 1f : 0f;
            idleWeight = Mathf.MoveTowards(idleWeight, targetIdleWeight, idleFadeSpeed * Time.deltaTime);

            if (idleWeight > 0.0001f)
            {
                idleTimer += Time.deltaTime * idleBobFrequency;
                if (idleTimer > 10000f) idleTimer -= 10000f;

                // Subtle breathing wave
                float breathCycle = Mathf.Sin(idleTimer);
                float breathSlow = Mathf.Cos(idleTimer * 0.5f);

                // Position offset (mostly vertical expansion and subtle lateral drift)
                float idlePosY = breathCycle * idleBobPositionAmplitude;
                float idlePosX = breathSlow * (idleBobPositionAmplitude * 0.4f);

                // Rotational offset (gentle pitch nod, roll drift, and organic yaw drift)
                float idlePitch = breathCycle * idleBobRotationAmplitude;
                float idleRoll = breathSlow * (idleBobRotationAmplitude * 0.35f);
                float idleYaw = (Mathf.PerlinNoise(idleTimer * 0.3f, 42.1f) - 0.5f) * (idleBobRotationAmplitude * 0.4f);

                currentIdlePosOffset = new Vector3(idlePosX, idlePosY, 0f) * idleWeight;
                currentIdleRotOffset = new Vector3(idlePitch, idleYaw, idleRoll) * idleWeight;
            }
            else
            {
                currentIdlePosOffset = Vector3.Lerp(currentIdlePosOffset, Vector3.zero, Time.deltaTime * idleFadeSpeed);
                currentIdleRotOffset = Vector3.Lerp(currentIdleRotOffset, Vector3.zero, Time.deltaTime * idleFadeSpeed);
            }
        }

        protected void HandleDynamicTilt()
        {
            Vector2 moveInput = (moveAction != null) ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            // Strafe banking (tilts camera into the strafe direction)
            float targetStrafeRoll = 0f;
            if (enableStrafeTilt)
            {
                targetStrafeRoll = -moveInput.x * strafeTiltAngle;
            }
            currentStrafeRoll = Mathf.Lerp(currentStrafeRoll, targetStrafeRoll, 1f - Mathf.Exp(-strafeTiltSpeed * Time.deltaTime));

            // Mouse look banking (subtle roll when turning mouse quickly)
            float targetLookRoll = 0f;
            if (enableLookBanking)
            {
                targetLookRoll = Mathf.Clamp(-smoothedDelta.x * lookBankingStrength, -maxLookBankingAngle, maxLookBankingAngle);
            }
            currentLookRoll = Mathf.Lerp(currentLookRoll, targetLookRoll, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        protected void HandleLandingBob()
        {
            if (!enableLandingBob || playerRigidbody == null) return;

            bool isGrounded = IsGrounded();

            if (!wasGroundedLastFrame && isGrounded)
            {
                float downVelocity = playerRigidbody.linearVelocity.y;
                if (downVelocity < landingVelocityThreshold)
                {
                    float excess = Mathf.Abs(downVelocity) - Mathf.Abs(landingVelocityThreshold);
                    float intensity = Mathf.Clamp01(excess / 8f);

                    landingOffsetY = -landingBobAmount * (1f + intensity);
                    landingPitchOffset = landingPitchAmount * (1f + intensity);
                }
            }

            // Smooth spring return to zero
            landingOffsetY = Mathf.SmoothDamp(landingOffsetY, 0f, ref landingPosLerpVelocity, 1f / landingBobReturnSpeed);
            landingPitchOffset = Mathf.SmoothDamp(landingPitchOffset, 0f, ref landingPitchLerpVelocity, 1f / landingBobReturnSpeed);

            wasGroundedLastFrame = isGrounded;
        }

        protected void HandleJumpRecoil()
        {
            if (!enableJumpRecoil) return;
            jumpRecoilOffset = jumpRecoilPosition;
        }

        protected void HandleJumpSettling()
        {
            if (jumpRecoilOffset > 0.0001f || jumpRecoilOffset < -0.0001f)
            {
                jumpRecoilOffset = Mathf.SmoothDamp(jumpRecoilOffset, 0f, ref jumpRecoilVelocity, 0.18f);
            }
        }

        protected void HandleFovKick()
        {
            if (!enableFovKick || camComponent == null) return;

            bool isSprinting = playerMovement != null && playerMovement.IsSprinting;
            float targetFov = baseFov + (isSprinting ? sprintFovOffset : 0f);

            camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFov, 1f - Mathf.Exp(-fovTransitionSpeed * Time.deltaTime));
        }

        protected void ApplyCameraTransforms()
        {
            // Calculate combined position
            Vector3 finalLocalPos = initialCameraLocalPos + currentBobPosOffset + currentIdlePosOffset;
            finalLocalPos.y += landingOffsetY + jumpRecoilOffset;

            // Apply position with frame-rate independent smoothing
            float posBlend = 1f - Mathf.Exp(-bobFadeSpeed * 2.5f * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, finalLocalPos, posBlend);

            // Calculate combined rotation:
            // Pitch: look pitch + bob pitch + idle pitch + landing pitch - jump pitch
            float jumpPitch = (jumpRecoilPosition > 0.001f) ? (jumpRecoilOffset / jumpRecoilPosition) * jumpRecoilPitch : 0f;
            float totalPitch = xRotation + currentBobRotOffset.x + currentIdleRotOffset.x + landingPitchOffset - jumpPitch;
            float totalYaw = currentIdleRotOffset.y;
            float totalRoll = currentBobRotOffset.z + currentIdleRotOffset.z + currentStrafeRoll + currentLookRoll;

            transform.localRotation = Quaternion.Euler(totalPitch, totalYaw, totalRoll);
        }

        public bool IsGrounded()
        {
            if (playerMovement != null)
            {
                return playerMovement.IsGrounded();
            }

            Vector3 origin = (groundCheckOrigin != null) ? groundCheckOrigin.position : transform.position;
            return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);
        }

        protected void OnDrawGizmosSelected()
        {
            Vector3 origin = (groundCheckOrigin != null) ? groundCheckOrigin.position : transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
            Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.08f);
        }
    }
}
