// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Nanodogs.UniversalScripts
{
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class FirstPersonPlayerMovement : NanoMovementBase
    {
        [Header("Input Actions")]
        public InputActionReference moveAction;
        public InputActionReference jumpAction;

        private Vector3 inputDir;

        [Header("Smoothing")]
        [Tooltip("Lower = snappier movement, Higher = smoother movement.")]
        public float accelerationSmoothTime = 0.08f;
        private Vector3 currentVelocity;
        private Vector3 smoothVelocity;

        [Header("Ladder Settings")]
        public bool onLadder = false;
        public float ladderClimbSpeed = 5f;
        public float ladderStickStrength = 10f;
        public float ladderPushOffForce = 6f;
        private Vector3 ladderForward;
        private Vector3 ladderCenter;

        [Header("Footstep Settings")]
        public float footstepInterval = 0.4f;
        public float footstepRayDistance = 1.2f;
        public LayerMask footstepLayerMask;

        [Tooltip("Assign audio clips based on ground tags (e.g., Stone, Wood, Grass).")]
        public List<FootstepSurface> footstepSurfaces = new List<FootstepSurface>();

        private AudioSource audioSource;
        private float footstepTimer;

        private void OnEnable()
        {
            if (moveAction != null) moveAction.action.Enable();
            if (jumpAction != null) jumpAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.action.Disable();
            if (jumpAction != null) jumpAction.action.Disable();
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
        }

        private void Update()
        {
            if (moveAction == null || jumpAction == null) return;

            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

            // Camera-relative movement
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            // Remove any vertical tilt (important for head-bob, slopes, etc.)
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDir = (camRight * moveInput.x + camForward * moveInput.y);
            if (targetDir.magnitude > 1f) targetDir.Normalize();

            // Smooth input direction
            inputDir = Vector3.Lerp(inputDir, targetDir, Time.deltaTime * 10f);

            // Handle jump
            if (jumpAction.action.WasPressedThisFrame() && IsGrounded() && !onLadder)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }

            HandleLadder(moveInput);
            HandleFootsteps(moveInput);
        }


        private void FixedUpdate()
        {
            if (!onLadder)
            {
                Vector3 targetVelocity = inputDir * speed;
                targetVelocity.y = rb.linearVelocity.y;

                // Smooth movement to avoid stutter
                rb.linearVelocity = Vector3.SmoothDamp(
                    rb.linearVelocity,
                    targetVelocity,
                    ref smoothVelocity,
                    accelerationSmoothTime
                );
            }
        }

        private void HandleLadder(Vector2 moveInput)
        {
            if (!onLadder) return;

            float verticalInput = moveInput.y;
            Vector3 climbVelocity = Vector3.up * verticalInput * ladderClimbSpeed;
            rb.linearVelocity = climbVelocity;

            Vector3 toCenter = ladderCenter - transform.position;
            toCenter.y = 0f;
            rb.AddForce(toCenter * ladderStickStrength, ForceMode.Acceleration);

            if (jumpAction.action.WasPressedThisFrame())
            {
                Vector3 pushDir = -ladderForward.normalized;
                rb.useGravity = true;
                onLadder = false;
                rb.AddForce(pushDir * ladderPushOffForce + Vector3.up * 2f, ForceMode.VelocityChange);
            }
        }

        private void HandleFootsteps(Vector2 moveInput)
        {
            if (!IsGrounded() || onLadder) return;

            bool isMoving = moveInput.magnitude > 0.1f;
            if (!isMoving) { footstepTimer = 0f; return; }

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                PlayFootstepSound();
            }
        }

        private void PlayFootstepSound()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, footstepRayDistance, footstepLayerMask))
            {
                string surfaceTag = hit.collider.tag;
                AudioClip clip = GetClipForSurface(surfaceTag);
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        private AudioClip GetClipForSurface(string tag)
        {
            foreach (var surface in footstepSurfaces)
            {
                if (surface.tag.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (surface.clips.Length > 0)
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
