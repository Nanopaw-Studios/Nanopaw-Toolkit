// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// Advanced first-person player camera with smooth look controls, realistic grounded movement bobbing,
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
        [Tooltip("Base footstep cadence frequency (approx 1.8 - 2.0 steps per second for natural walking).")]
        public float bobFrequency = 1.9f;
        [Tooltip("Base displacement amplitude parameter.")]
        public float bobAmplitude = 0.18f;
        [Tooltip("Speed at which head bobbing fades in and out when starting and stopping.")]
        public float bobFadeSpeed = 8f;
        [Tooltip("Minimum movement input magnitude required to start bobbing.")]
        public float bobStartThreshold = 0.1f;
        [Tooltip("Optional custom curve for horizontal bob (evaluated if procedural harmonics disabled).")]
        public AnimationCurve bobCurveX;
        [Tooltip("Optional custom curve for vertical bob (evaluated if procedural harmonics disabled).")]
        public AnimationCurve bobCurveY;

        [Header("Head Bob Polish & Tuning")]
        [Tooltip("If true, uses smooth procedural Lissajous harmonic curves with rotational tilting. If false, evaluates bobCurveX/Y.")]
        public bool useProceduralHarmonics = true;
        [Tooltip("Master position scaling factor to convert bob amplitude into realistic displacement in meters.")]
        [Range(0.01f, 0.4f)]
        public float bobPositionScale = 0.10f;
        [Tooltip("Multiplier for horizontal lateral sway (keeps sway narrow and athletic).")]
        [Range(0f, 1f)]
        public float bobHorizontalMultiplier = 0.35f;
        [Tooltip("Multiplier for vertical bobbing dip.")]
        [Range(0f, 1f)]
        public float bobVerticalMultiplier = 0.65f;
        [Tooltip("Subtle camera roll tilt while stepping (degrees).")]
        [Range(0f, 2f)]
        public float bobRollAmplitude = 0.45f;
        [Tooltip("Subtle camera pitch nod on footstep impact (degrees).")]
        [Range(0f, 2f)]
        public float bobPitchAmplitude = 0.22f;
        [Tooltip("Dynamically scales bob cadence with the player's actual movement speed.")]
        public bool scaleBobWithSpeed = true;

        [Header("Idle Camera Movement (Breathing Sway)")]
        [Tooltip("Enables subtle breathing and organic camera sway when standing still.")]
        public bool enableIdleMovement = true;
        [Tooltip("Frequency of the idle breathing cycle (approx 1.0 - 1.2 for natural human resting respiration).")]
        public float idleBobFrequency = 1.1f;
        [Tooltip("Translational amplitude of idle breathing in meters.")]
        public float idleBobPositionAmplitude = 0.005f;
        [Tooltip("Rotational amplitude of idle breathing in degrees (pitch and roll).")]
        public float idleBobRotationAmplitude = 0.20f;
        [Tooltip("Speed at which idle sway blends in when stopping and out when moving.")]
        public float idleFadeSpeed = 3.5f;

        [Header("Dynamic Strafe Banking & Look Sway")]
        [Tooltip("Tilts the camera roll slightly when strafing left or right.")]
        public bool enableStrafeTilt = true;
        [Tooltip("Maximum roll angle when strafing (degrees).")]
        [Range(0f, 3f)]
        public float strafeTiltAngle = 1.0f;
        [Tooltip("Speed of strafe tilt response.")]
        public float strafeTiltSpeed = 8f;

        [Tooltip("Slight camera roll banking when turning the mouse quickly.")]
        public bool enableLookBanking = true;
        [Tooltip("Amount of roll banking applied during mouse rotation.")]
        public float lookBankingStrength = 0.008f;
        [Tooltip("Maximum roll angle from rapid mouse turning.")]
        public float maxLookBankingAngle = 1.2f;

        [Header("Landing & Jump Recoil")]
        [Tooltip("Enables camera dip and pitch recoil when landing from a fall.")]
        public bool enableLandingBob = true;
        [Tooltip("Vertical downward displacement on landing.")]
        public float landingBobAmount = 0.06f;
        [Tooltip("Downward pitch nod angle on landing (degrees).")]
        public float landingPitchAmount = 1.5f;
        [Tooltip("Speed of landing compression.")]
        public float landingBobSpeed = 12f;
        [Tooltip("Speed of return after landing.")]
        public float landingBobReturnSpeed = 6f;
        [Tooltip("Minimum downward velocity required to trigger landing bob.")]
        public float landingVelocityThreshold = -3.5f;

        [Tooltip("Enables subtle camera kick when jumping.")]
        public bool enableJumpRecoil = true;
        [Tooltip("Positional recoil displacement when jumping.")]
        public float jumpRecoilPosition = 0.02f;
        [Tooltip("Pitch recoil nod when jumping (degrees).")]
        public float jumpRecoilPitch = 0.8f;

        [Header("Dynamic FOV Sprint Kick")]
        [Tooltip("Subtly expands the camera FOV while sprinting.")]
        public bool enableFovKick = true;
        [Tooltip("Extra degrees of FOV added while sprinting.")]
        public float sprintFovOffset = 5f;
        [Tooltip("Speed of FOV expansion and recovery.")]
        public float fovTransitionSpeed = 7f;

        [Header("Panini Projection")]
        [Tooltip("Enables Panini projection on the camera to reduce wide FOV fisheye distortion and preserve straight vertical lines.")]
        public bool enablePaniniProjection = false;
        [Tooltip("Panini projection distance / strength factor (0 = standard perspective, 1 = full cylindrical panini projection).")]
        [Range(0f, 1f)]
        public float paniniDistance = 0.3f;
        [Tooltip("Panini projection crop-to-fit factor (1 = crops to screen edges without black borders).")]
        [Range(0f, 1f)]
        public float paniniCropToFit = 1f;
        [Tooltip("Subtly scales Panini projection strength when sprinting alongside FOV kick.")]
        public bool scalePaniniWithSprint = true;
        [Tooltip("Additional Panini distance offset applied while sprinting.")]
        [Range(0f, 0.5f)]
        public float sprintPaniniDistanceOffset = 0.1f;
        [Tooltip("Speed at which Panini projection transitions smoothly in and out.")]
        public float paniniTransitionSpeed = 8f;
        [Tooltip("Optional Volume component to control. If unassigned, automatically detects or creates a dedicated local Volume on this camera.")]
        public Volume postProcessVolume;

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

        // Internal State - Panini Projection
        private PaniniProjection paniniComponent;
        private float currentPaniniDistance;
        private Volume runtimeCreatedVolume;
        private VolumeProfile runtimeCreatedProfile;

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

            if (paniniComponent != null)
            {
                paniniComponent.active = false;
                paniniComponent.distance.overrideState = false;
            }
            currentPaniniDistance = 0f;
        }

        protected void OnDestroy()
        {
            if (runtimeCreatedProfile != null)
            {
                Destroy(runtimeCreatedProfile);
                runtimeCreatedProfile = null;
            }

            if (runtimeCreatedVolume != null)
            {
                Destroy(runtimeCreatedVolume);
                runtimeCreatedVolume = null;
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

            if (enablePaniniProjection || postProcessVolume != null)
            {
                InitializePanini();
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
            HandlePaniniProjection();
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
                currentBobPosOffset = Vector3.Lerp(currentBobPosOffset, Vector3.zero, Time.deltaTime * bobFadeSpeed);
                currentBobRotOffset = Vector3.Lerp(currentBobRotOffset, Vector3.zero, Time.deltaTime * bobFadeSpeed);
                return;
            }

            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            bool hasInput = moveInput.magnitude > bobStartThreshold;
            bool isGrounded = IsGrounded();

            // Check actual horizontal velocity
            bool isActuallyMoving = true;
            float currentSpeed = 0f;
            if (playerRigidbody != null)
            {
                Vector3 horizontalVel = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
                currentSpeed = horizontalVel.magnitude;
                isActuallyMoving = currentSpeed > 0.12f;
            }

            bool shouldBob = isGrounded && hasInput && isActuallyMoving;
            float targetWeight = shouldBob ? 1f : 0f;
            bobWeight = Mathf.MoveTowards(bobWeight, targetWeight, bobFadeSpeed * Time.deltaTime);

            if (bobWeight > 0.0001f)
            {
                // Dynamic cadence: 1 full stride = 2 footsteps
                float baseSpeed = (playerMovement != null && playerMovement.speed > 0.1f) ? playerMovement.speed : 5f;
                float speedRatio = (playerRigidbody != null && baseSpeed > 0.1f) ? (currentSpeed / baseSpeed) : 1f;
                float speedMultiplier = scaleBobWithSpeed ? Mathf.Clamp(speedRatio, 0.7f, 1.6f) : 1f;

                // Advance bobTimer so that bobFrequency directly controls steps per second:
                // bobFrequency * PI rad/s advances 2*PI (one full stride = 2 steps) in (2 / bobFrequency) seconds.
                bobTimer += Time.deltaTime * bobFrequency * speedMultiplier * Mathf.PI;
                if (bobTimer > Mathf.PI * 200f) bobTimer -= Mathf.PI * 200f;

                float effectiveAmp = bobAmplitude * bobPositionScale;
                bool isSprinting = playerMovement != null && playerMovement.IsSprinting;
                float sprintSwayDamp = isSprinting ? 0.65f : 1f;  // Narrower lateral stance when sprinting
                float sprintDipBoost = isSprinting ? 1.2f : 1f;   // Deeper vertical step drive when sprinting

                float stridePhase = bobTimer;
                float stepPhase = bobTimer * 2f;

                Vector3 targetBobPos;
                Vector3 targetBobRot;

                if (useProceduralHarmonics || bobCurveX == null || bobCurveY == null || bobCurveX.length == 0)
                {
                    // Stride sway (left to right, 1 cycle per 2 steps)
                    float sinStride = Mathf.Sin(stridePhase);

                    // Step vertical dip (parabolic compression downward from resting eye level, 2 dips per stride)
                    // cos(0) = 1 -> dip = 0 (eye level), cos(pi) = -1 -> dip = -1 (foot plant)
                    float cosStep = Mathf.Cos(stepPhase);
                    float verticalDip = -(1f - cosStep) * 0.5f;

                    float xPos = sinStride * effectiveAmp * bobHorizontalMultiplier * sprintSwayDamp;
                    float yPos = verticalDip * effectiveAmp * bobVerticalMultiplier * sprintDipBoost;

                    // Subtle, grounded rotational tilt (banking with stride + slight nod on heel strike)
                    float zRoll = -sinStride * bobRollAmplitude * sprintSwayDamp;
                    float xPitch = verticalDip * bobPitchAmplitude;

                    targetBobPos = new Vector3(xPos, yPos, 0f) * bobWeight;
                    targetBobRot = new Vector3(xPitch, 0f, zRoll) * bobWeight;
                }
                else
                {
                    // Normalized [0..1] curve evaluation
                    float normalizedTimer = (bobTimer / (Mathf.PI * 2f)) % 1f;
                    if (normalizedTimer < 0f) normalizedTimer += 1f;

                    float xOffset = (bobCurveX.Evaluate(normalizedTimer) - 0.5f) * effectiveAmp * bobHorizontalMultiplier * sprintSwayDamp;
                    float yOffset = -bobCurveY.Evaluate(normalizedTimer) * effectiveAmp * bobVerticalMultiplier * sprintDipBoost;

                    float sinStride = Mathf.Sin(stridePhase);
                    float cosStep = Mathf.Cos(stepPhase);
                    float verticalDip = -(1f - cosStep) * 0.5f;

                    targetBobPos = new Vector3(xOffset, yOffset, 0f) * bobWeight;
                    targetBobRot = new Vector3(verticalDip * bobPitchAmplitude, 0f, -sinStride * bobRollAmplitude * sprintSwayDamp) * bobWeight;
                }

                // Smooth responsive tracking without secondary spring lag
                currentBobPosOffset = Vector3.Lerp(currentBobPosOffset, targetBobPos, 1f - Mathf.Exp(-22f * Time.deltaTime));
                currentBobRotOffset = Vector3.Lerp(currentBobRotOffset, targetBobRot, 1f - Mathf.Exp(-22f * Time.deltaTime));
            }
            else
            {
                currentBobPosOffset = Vector3.Lerp(currentBobPosOffset, Vector3.zero, 1f - Mathf.Exp(-bobFadeSpeed * 2f * Time.deltaTime));
                currentBobRotOffset = Vector3.Lerp(currentBobRotOffset, Vector3.zero, 1f - Mathf.Exp(-bobFadeSpeed * 2f * Time.deltaTime));
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

            // Idle is active when grounded and not actively bobbing
            bool isGrounded = IsGrounded();
            bool isMoving = bobWeight > 0.05f;
            float targetIdleWeight = (!isMoving && isGrounded) ? 1f : 0f;
            idleWeight = Mathf.MoveTowards(idleWeight, targetIdleWeight, idleFadeSpeed * Time.deltaTime);

            if (idleWeight > 0.0001f)
            {
                idleTimer += Time.deltaTime * idleBobFrequency;
                if (idleTimer > 10000f) idleTimer -= 10000f;

                // Calm, natural breathing cycle
                float breathCycle = Mathf.Sin(idleTimer);
                float breathSlow = Mathf.Cos(idleTimer * 0.5f);

                // Very gentle vertical position breathing + subtle lateral shift
                float idlePosY = breathCycle * idleBobPositionAmplitude;
                float idlePosX = breathSlow * (idleBobPositionAmplitude * 0.3f);

                // Subtle pitch breathing nod and low-frequency noise drift
                float idlePitch = breathCycle * idleBobRotationAmplitude;
                float idleRoll = breathSlow * (idleBobRotationAmplitude * 0.3f);
                float idleYaw = (Mathf.PerlinNoise(idleTimer * 0.25f, 42.1f) - 0.5f) * (idleBobRotationAmplitude * 0.35f);

                Vector3 targetIdlePos = new Vector3(idlePosX, idlePosY, 0f) * idleWeight;
                Vector3 targetIdleRot = new Vector3(idlePitch, idleYaw, idleRoll) * idleWeight;

                currentIdlePosOffset = Vector3.Lerp(currentIdlePosOffset, targetIdlePos, 1f - Mathf.Exp(-14f * Time.deltaTime));
                currentIdleRotOffset = Vector3.Lerp(currentIdleRotOffset, targetIdleRot, 1f - Mathf.Exp(-14f * Time.deltaTime));
            }
            else
            {
                currentIdlePosOffset = Vector3.Lerp(currentIdlePosOffset, Vector3.zero, 1f - Mathf.Exp(-idleFadeSpeed * 2f * Time.deltaTime));
                currentIdleRotOffset = Vector3.Lerp(currentIdleRotOffset, Vector3.zero, 1f - Mathf.Exp(-idleFadeSpeed * 2f * Time.deltaTime));
            }
        }

        protected void HandleDynamicTilt()
        {
            Vector2 moveInput = (moveAction != null) ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            // Strafe banking (tilts camera subtly into strafe direction)
            float targetStrafeRoll = 0f;
            if (enableStrafeTilt)
            {
                targetStrafeRoll = -moveInput.x * strafeTiltAngle;
            }
            currentStrafeRoll = Mathf.Lerp(currentStrafeRoll, targetStrafeRoll, 1f - Mathf.Exp(-strafeTiltSpeed * Time.deltaTime));

            // Mouse look banking (subtle inertia when panning camera rapidly)
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

                    landingOffsetY = -landingBobAmount * (1f + intensity * 0.5f);
                    landingPitchOffset = landingPitchAmount * (1f + intensity * 0.5f);
                }
            }

            // Smooth spring return to resting position
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
                jumpRecoilOffset = Mathf.SmoothDamp(jumpRecoilOffset, 0f, ref jumpRecoilVelocity, 0.16f);
            }
        }

        protected void HandleFovKick()
        {
            if (!enableFovKick || camComponent == null) return;

            bool isSprinting = playerMovement != null && playerMovement.IsSprinting;
            float targetFov = baseFov + (isSprinting ? sprintFovOffset : 0f);

            camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFov, 1f - Mathf.Exp(-fovTransitionSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Initializes the post-processing Volume and gets or adds the PaniniProjection component override.
        /// </summary>
        public void InitializePanini()
        {
            if (postProcessVolume == null)
            {
                postProcessVolume = GetComponent<Volume>();
            }

            if (postProcessVolume == null)
            {
                runtimeCreatedVolume = gameObject.AddComponent<Volume>();
                runtimeCreatedVolume.isGlobal = true;
                runtimeCreatedVolume.priority = 100f;
                postProcessVolume = runtimeCreatedVolume;
            }

            if (postProcessVolume != null)
            {
                if (postProcessVolume.profile == null)
                {
                    runtimeCreatedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                    runtimeCreatedProfile.name = $"{gameObject.name}_PaniniVolumeProfile";
                    postProcessVolume.profile = runtimeCreatedProfile;
                }

                if (!postProcessVolume.profile.TryGet(out paniniComponent))
                {
                    paniniComponent = postProcessVolume.profile.Add<PaniniProjection>(overrides: true);
                }
            }
        }

        protected void HandlePaniniProjection()
        {
            if (!enablePaniniProjection && currentPaniniDistance <= 0.0001f)
            {
                if (paniniComponent != null && paniniComponent.active)
                {
                    paniniComponent.active = false;
                    paniniComponent.distance.overrideState = false;
                }
                return;
            }

            if (paniniComponent == null)
            {
                InitializePanini();
                if (paniniComponent == null) return;
            }

            bool isSprinting = scalePaniniWithSprint && playerMovement != null && playerMovement.IsSprinting;
            float targetDistance = enablePaniniProjection ? Mathf.Clamp01(paniniDistance + (isSprinting ? sprintPaniniDistanceOffset : 0f)) : 0f;

            currentPaniniDistance = Mathf.Lerp(currentPaniniDistance, targetDistance, 1f - Mathf.Exp(-paniniTransitionSpeed * Time.deltaTime));

            bool shouldBeActive = currentPaniniDistance > 0.0005f;
            paniniComponent.active = shouldBeActive;
            paniniComponent.distance.overrideState = shouldBeActive;
            paniniComponent.distance.value = currentPaniniDistance;
            paniniComponent.cropToFit.overrideState = shouldBeActive;
            paniniComponent.cropToFit.value = paniniCropToFit;
        }

        /// <summary>
        /// Enables or disables Panini projection and optionally updates distance and crop settings.
        /// </summary>
        public void SetPaniniProjection(bool enable, float distance = -1f, float cropToFit = -1f)
        {
            enablePaniniProjection = enable;
            if (distance >= 0f) paniniDistance = Mathf.Clamp01(distance);
            if (cropToFit >= 0f) paniniCropToFit = Mathf.Clamp01(cropToFit);
        }

        /// <summary>
        /// Sets the Panini projection distance / strength.
        /// </summary>
        public void SetPaniniDistance(float distance)
        {
            paniniDistance = Mathf.Clamp01(distance);
        }

        protected void OnValidate()
        {
            paniniDistance = Mathf.Clamp01(paniniDistance);
            paniniCropToFit = Mathf.Clamp01(paniniCropToFit);
            sprintPaniniDistanceOffset = Mathf.Clamp01(sprintPaniniDistanceOffset);

            if (Application.isPlaying && paniniComponent != null)
            {
                float targetDist = enablePaniniProjection ? paniniDistance : 0f;
                currentPaniniDistance = targetDist;
                bool shouldBeActive = targetDist > 0.0005f;
                paniniComponent.active = shouldBeActive;
                paniniComponent.distance.overrideState = shouldBeActive;
                paniniComponent.distance.value = targetDist;
                paniniComponent.cropToFit.overrideState = shouldBeActive;
                paniniComponent.cropToFit.value = paniniCropToFit;
            }
        }

        protected void ApplyCameraTransforms()
        {
            // Position: Combine resting position with cleanly smoothed offsets
            Vector3 finalLocalPos = initialCameraLocalPos + currentBobPosOffset + currentIdlePosOffset;
            finalLocalPos.y += landingOffsetY + jumpRecoilOffset;
            transform.localPosition = finalLocalPos;

            // Rotation: Look pitch + bob nods + idle breathing + landing impact - jump recoil
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
