// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Nanodogs.UniversalScripts
{
    /// <summary>
    /// Handles first-person physics-based player movement including walking, sprinting,
    /// jumping, slope adherence, ladder climbing, and surface-based footstep audio.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class FirstPersonPlayerMovement : NanoMovementBase
    {
        [Header("Input Actions")]
        [Tooltip("Input action for 2D movement (WASD / Left Stick).")]
        public InputActionReference moveAction;
        [Tooltip("Input action for jumping (Space / Button South).")]
        public InputActionReference jumpAction;
        [Tooltip("Optional input action for sprinting (Left Shift / Left Stick Press). Falls back to Left Shift if unassigned.")]
        public InputActionReference sprintAction;

        [Header("Sprint Settings")]
        [Tooltip("Allows the player to sprint at an increased speed.")]
        public bool enableSprint = true;
        [Tooltip("Movement speed while sprinting.")]
        public float sprintSpeed = 9f;

        [Header("Smoothing & Air Control")]
        [Tooltip("Lower = snappier movement, Higher = smoother movement.")]
        public float accelerationSmoothTime = 0.08f;
        [Tooltip("Responsiveness multiplier when maneuvering in mid-air.")]
        [Range(0.1f, 1f)]
        public float airControlMultiplier = 0.5f;

        [Header("Ladder Settings")]
        public bool onLadder = false;
        public float ladderClimbSpeed = 5f;
        public float ladderStickStrength = 10f;
        public float ladderPushOffForce = 6f;
        private Vector3 ladderForward;
        private Vector3 ladderCenter;

        [Header("Footstep Settings")]
        [Tooltip("Base time interval between footsteps when walking.")]
        public float footstepInterval = 0.45f;
        [Tooltip("Distance down to check for footstep ground surface.")]
        public float footstepRayDistance = 1.2f;
        public LayerMask footstepLayerMask = ~0;

        [Tooltip("Assign audio clips based on ground tags (e.g., Stone, Wood, Grass).")]
        public List<FootstepSurface> footstepSurfaces = new List<FootstepSurface>();

        [Header("Audio Polish")]
        [Tooltip("Random pitch variation applied to footsteps to prevent repetitive sounds.")]
        [Range(0f, 0.2f)]
        public float footstepPitchVariance = 0.06f;

        // Events
        public event System.Action OnJumped;

        // Public State Properties
        public bool IsSprinting { get; private set; }
        public Vector2 CurrentMoveInput { get; private set; }
        public float CurrentHorizontalSpeed => rb != null ? new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude : 0f;
        public bool IsMoving => CurrentMoveInput.magnitude > 0.1f && CurrentHorizontalSpeed > 0.15f;

        // Internal State
        protected Vector3 inputDir;
        private Vector3 smoothVelocity;
        private AudioSource audioSource;
        private float footstepTimer;
        private Transform cameraTransform;

        protected void OnEnable()
        {
            if (moveAction != null) moveAction.action.Enable();
            if (jumpAction != null) jumpAction.action.Enable();
            if (sprintAction != null) sprintAction.action.Enable();
        }

        protected void OnDisable()
        {
            if (moveAction != null) moveAction.action.Disable();
            if (jumpAction != null) jumpAction.action.Disable();
            if (sprintAction != null) sprintAction.action.Disable();
        }

        protected override void Start()
        {
            base.Start();

            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            if (rb != null)
            {
                rb.freezeRotation = true;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.spatialBlend = 1f; // 3D sound
            }

            // Cache main camera transform
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        protected void Update()
        {
            HandleInput();
            HandleLadder();
            HandleFootsteps();
        }

        protected void FixedUpdate()
        {
            if (onLadder || rb == null) return;

            float targetSpeed = (enableSprint && IsSprinting) ? sprintSpeed : speed;
            Vector3 targetVelocity = inputDir * targetSpeed;
            targetVelocity.y = rb.linearVelocity.y;

            float smoothTime = IsGrounded() ? accelerationSmoothTime : (accelerationSmoothTime / Mathf.Max(0.01f, airControlMultiplier));

            rb.linearVelocity = Vector3.SmoothDamp(
                rb.linearVelocity,
                targetVelocity,
                ref smoothVelocity,
                smoothTime
            );
        }

        protected void HandleInput()
        {
            if (moveAction == null) return;

            CurrentMoveInput = moveAction.action.ReadValue<Vector2>();

            // Determine sprint state
            bool sprintPressed = false;
            if (enableSprint)
            {
                if (sprintAction != null)
                {
                    sprintPressed = sprintAction.action.IsPressed();
                }
                else if (Keyboard.current != null)
                {
                    sprintPressed = Keyboard.current.leftShiftKey.isPressed;
                }
            }

            // Only sprint if moving forward-ish and grounded
            IsSprinting = sprintPressed && CurrentMoveInput.y > 0.1f && IsGrounded() && !onLadder;

            // Camera-relative movement direction
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDir = (camRight * CurrentMoveInput.x + camForward * CurrentMoveInput.y);
            if (targetDir.magnitude > 1f) targetDir.Normalize();

            // Smooth input direction response
            inputDir = Vector3.Lerp(inputDir, targetDir, Time.deltaTime * 14f);

            // Handle jump
            if (jumpAction != null && jumpAction.action.WasPressedThisFrame() && IsGrounded() && !onLadder)
            {
                PerformJump();
            }
        }

        protected void PerformJump()
        {
            if (rb == null) return;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            OnJumped?.Invoke();
        }

        protected void HandleLadder()
        {
            if (!onLadder || rb == null) return;

            float verticalInput = CurrentMoveInput.y;
            Vector3 climbVelocity = Vector3.up * verticalInput * ladderClimbSpeed;
            rb.linearVelocity = climbVelocity;

            Vector3 toCenter = ladderCenter - transform.position;
            toCenter.y = 0f;
            rb.AddForce(toCenter * ladderStickStrength, ForceMode.Acceleration);

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
            {
                Vector3 pushDir = -ladderForward.normalized;
                rb.useGravity = true;
                onLadder = false;
                rb.AddForce(pushDir * ladderPushOffForce + Vector3.up * 2f, ForceMode.VelocityChange);
            }
        }

        protected void HandleFootsteps()
        {
            if (!IsGrounded() || onLadder || audioSource == null)
            {
                footstepTimer = 0f;
                return;
            }

            if (!IsMoving)
            {
                footstepTimer = 0f;
                return;
            }

            // Dynamically scale footstep cadence with movement speed
            float speedRatio = Mathf.Clamp(CurrentHorizontalSpeed / Mathf.Max(speed, 0.1f), 0.6f, 2.0f);
            float effectiveInterval = footstepInterval / speedRatio;

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= effectiveInterval)
            {
                footstepTimer = 0f;
                PlayFootstepSound();
            }
        }

        protected void PlayFootstepSound()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, footstepRayDistance, footstepLayerMask))
            {
                string surfaceTag = hit.collider.tag;
                AudioClip clip = GetClipForSurface(surfaceTag);
                if (clip != null)
                {
                    // Apply subtle pitch variance to eliminate machine-gun repetition
                    float originalPitch = audioSource.pitch;
                    audioSource.pitch = originalPitch + Random.Range(-footstepPitchVariance, footstepPitchVariance);
                    audioSource.PlayOneShot(clip);
                    audioSource.pitch = originalPitch;
                }
            }
        }

        protected AudioClip GetClipForSurface(string tag)
        {
            if (footstepSurfaces == null) return null;

            foreach (var surface in footstepSurfaces)
            {
                if (surface.tag.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (surface.clips != null && surface.clips.Length > 0)
                    {
                        int index = Random.Range(0, surface.clips.Length);
                        return surface.clips[index];
                    }
                }
            }
            return null;
        }

        public void SetLadderData(Vector3 forward, Vector3 center)
        {
            ladderForward = forward;
            ladderCenter = center;
        }
    }

    [System.Serializable]
    public class FootstepSurface
    {
        public string tag;
        public AudioClip[] clips;
    }
}
