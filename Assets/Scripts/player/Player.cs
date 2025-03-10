using System;
using System.Collections;
using obstacle.type;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerLaneHandler))]
    [RequireComponent(typeof(PlayerLifeManager))]
    public class Player : MonoBehaviour
    {
        public Animator animator;
        public CharacterController controller;
        public float movementSpeed = 5f; // Speed of the player
        public float jumpHeight = 2f; // Height of the jump
        public float gravity = 9.81f; // Gravity strength

        [Header("Default Settings")]
        [SerializeField] private float defaultMovementSpeed = 5f; // Store default speed for reset after death

        private Vector3 velocity; // Stores vertical movement
        private PlayerLifeManager lifeManager; // Reference to the life manager

        public bool IsJumping { get; private set; }

        public bool IsSliding { get; private set; }
        public float slideDuration = 1f; // How long the slide lasts
        public float slideSpeedMultiplier = 1.5f; // Slide speed boost
        private Vector3 _lateralMovement = Vector3.zero;

        public bool IsDead { get; set; }

        private void Start()
        {
            // Subscribe to lane movement events
            GetComponent<PlayerLaneHandler>().OnLaneMovement += ApplyLateralMovement;
            
            // Get reference to the life manager
            lifeManager = GetComponent<PlayerLifeManager>();
            
            // Store default speed
            defaultMovementSpeed = movementSpeed;
            
            // Set initial state
            IsDead = false;
        }

        private void ApplyLateralMovement(Vector3 movement)
        {
            _lateralMovement = movement;
        }

        private void OnDestroy()
        {
            // Make sure to unsubscribe to prevent memory leaks
            var laneHandler = GetComponent<PlayerLaneHandler>();
            if (laneHandler != null)
                laneHandler.OnLaneMovement -= ApplyLateralMovement;
        }

        private void Update()
        {
            if (IsDead)
            {
                // Check if we just died this frame - only call HandlePlayerDeath once
                if (lifeManager != null && velocity.y > -0.1f) // Small threshold to avoid calling multiple times
                {
                    lifeManager.HandlePlayerDeath();
                    velocity.y = -0.1f; // Mark as processed
                }
                return;
            }
            
            HandleJump();
            HandleSlide();
            MovePlayer();
        }

        private void HandleJump()
        {
            if (controller.isGrounded && !isTurning())
            {
                if (IsJumping)
                {
                    // Player just landed, reset jump animation
                    IsJumping = false;

                    if (animator != null && HasParameter("Jump", animator))
                        animator.ResetTrigger("Jump"); // Reset jump trigger
                }

                if (Input.GetKeyDown(KeyCode.Space) && !IsSliding) // Jump on space press
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity); // Jump to force
                    IsJumping = true;

                    if (animator != null && HasParameter("Jump", animator))
                        animator.SetTrigger("Jump"); // Trigger jump animation
                }
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime; // Apply gravity
            }
        }

        private void HandleSlide()
        {
            if (((!IsSliding && Input.GetKeyDown(KeyCode.S)) || Input.GetKeyDown(KeyCode.DownArrow)) && !isTurning())
                StartCoroutine(Slide());
        }

        private IEnumerator Slide()
        {
            IsSliding = true;

            // Increase movement speed temporarily
            var originalSpeed = movementSpeed;
            movementSpeed *= slideSpeedMultiplier;

            // Sound play Workaround, for some reason slide event is not triggered,
            //todo investigate why
            SoundManager.instance.soundRollForward();
            animator.SetTrigger("Slide");

            // Wait for slide duration
            yield return new WaitForSeconds(slideDuration);

            IsSliding = false;
            animator.ResetTrigger("Slide");
            // Restore original movement speed
            movementSpeed = originalSpeed;
            animator.Play("Idle");
        }

        private void MovePlayer()
        {
            // Create movement vector
            var movement = Vector3.zero;

            // Determine primary movement axis based on player orientation
            if (Mathf.Abs(transform.forward.z) > Mathf.Abs(transform.forward.x))
            {
                // Primarily facing Z direction
                movement.z = Mathf.Sign(transform.forward.z) * movementSpeed * Time.deltaTime;

                // Apply lane movement from lane handler if present
                if (_lateralMovement != Vector3.zero) movement.x = _lateralMovement.x;
            }
            else
            {
                // Primarily facing X direction
                movement.x = Mathf.Sign(transform.forward.x) * movementSpeed * Time.deltaTime;

                // Apply lane movement from lane handler if present
                if (_lateralMovement != Vector3.zero) movement.z = _lateralMovement.z;
            }

            // Reset lateral movement after applied
            _lateralMovement = Vector3.zero;

            // Apply vertical movement
            movement.y = velocity.y * Time.deltaTime;

            // Apply movement using the CharacterController
            controller.Move(movement);

            // Update animator speed
            if (animator != null)
                animator.SetFloat("MovementSpeed", movementSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Fire") && !IsDead)
            {
                Die();
            }
            if (other.tag.Equals("coin"))
            {
                ScoreManager.instance.AddScore(1);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Check if we hit a wall that hasn't been broken
            if (hit.gameObject.CompareTag("Wall") && !IsDead)
            {
                var wall = hit.gameObject.GetComponent<ObstacleWall>();

                // Only die if the wall is still intact (has active collider)
                if (wall != null && wall.IsIntact())
                {
                    Die();
                }
            }
        }
        
        // New method to centralize death logic
        private void Die()
        {
            // Set player to dead state
            IsDead = true;

            // Stop movement
            movementSpeed = 0;
            velocity = Vector3.zero;

            // Trigger death animation
            if (animator != null && HasParameter("Death", animator))
                animator.SetTrigger("Death");
                
            // Play death sound
            soundDeath();
        }

        // Original method for backward compatibility
        public void ResetPlayerState()
        {
            IsDead = false;
            IsJumping = false;
            IsSliding = false;
            movementSpeed = defaultMovementSpeed;
            velocity = Vector3.zero;
            
            // Reset animations
            if (animator != null)
            {
                animator.ResetTrigger("Death");
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("Slide");
                animator.Play("Idle");
            }
        }

        // New method with position and rotation parameters
        public void ResetPlayerState(Vector3 respawnPos, Quaternion respawnRot)
        {
            IsDead = false;
            IsJumping = false;
            IsSliding = false;
            movementSpeed = defaultMovementSpeed;
            velocity = Vector3.zero;
            
            // Reset animations
            if (animator != null)
            {
                animator.ResetTrigger("Death");
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("Slide");
                animator.Play("Idle");
            }
            
            // Reset position and rotation
            // Important: We need to temporarily disable the controller to teleport
            controller.enabled = false;
            transform.position = respawnPos;
            transform.rotation = respawnRot;
            controller.enabled = true;
        }

        private bool isTurning()
        {
            return GetComponent<PlayerTurnHandler>().IsTurning;
        }

        // Helper method to check if an animator has a parameter
        private bool HasParameter(string paramName, Animator animator)
        {
            if (animator == null) return false;
            
            foreach (var param in animator.parameters)
                if (param.name == paramName)
                    return true;

            return false;
        }
        
        //for the shake of animation events since they can't access
        //the sound manager directly
        public void soundCoinCollect()
        {
            SoundManager.instance.soundCoinCollect();
        }
    
        public void soundJump()
        {
            SoundManager.instance.soundJump();
        }
    
        public void soundLand()
        {
            SoundManager.instance.soundLand();
        }
    
        public void soundRollForward()
        {
            Debug.Log("Rolling forward sound");
            SoundManager.instance.soundRollForward();
        }
    
        public void soundLaneChange()
        {
            SoundManager.instance.soundLaneChange();
        }
    
        public void soundFootstep1()
        {
            SoundManager.instance.soundFootstep1();
        }
    
        public void soundFootstep2()
        {
            SoundManager.instance.soundFootstep2();
        }
    
        public void soundCastMagic()
        {
            SoundManager.instance.soundCastMagic();
        }
    
        public void soundBlast()
        {
            // TODO: Implement blast sound
        }

        public void soundDeath()
        {
           SoundManager.instance.soundDeath();
        }
    }
}
