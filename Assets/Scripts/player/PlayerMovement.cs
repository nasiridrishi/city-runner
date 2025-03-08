using System;
using System.Collections;
using obstacle.type;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerLaneHandler))]
    public class PlayerMovement : MonoBehaviour
    {
        public Animator animator;
        public CharacterController controller;
        public float movementSpeed = 5f; // Speed of the player
        public float jumpHeight = 2f; // Height of the jump
        public float gravity = 9.81f; // Gravity strength

        private Vector3 velocity; // Stores vertical movement

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
        }

        private void ApplyLateralMovement(Vector3 movement)
        {
            _lateralMovement = movement;
        }

        private void OnDestroy()
        {
            GetComponent<PlayerLaneHandler>().OnLaneMovement -= ApplyLateralMovement;
        }

        private void Update()
        {
            if (IsDead) return;
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

            // Trigger slide animation if available
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
            animator.SetFloat("MovementSpeed", movementSpeed);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {

            // Check if we hit a wall that hasn't been broken
            if (hit.gameObject.CompareTag("Wall"))
            {
                var wall = hit.gameObject.GetComponent<ObstacleWall>();

                // Only die if the wall is still intact (has active collider)
                if (wall != null && wall.IsIntact())
                {

                    // Set player to dead state
                    IsDead = true;

                    // Stop movement
                    movementSpeed = 0;
                    velocity = Vector3.zero;

                    // Trigger death animation
                    if (animator != null)
                        animator.SetTrigger("Death");
                }
            }
        }


        private bool isTurning()
        {
            return GetComponent<PlayerTurnHandler>().IsTurning;
        }

        // Helper method to check if an animator has a parameter
        private bool HasParameter(string paramName, Animator animator)
        {
            foreach (var param in animator.parameters)
                if (param.name == paramName)
                    return true;

            return false;
        }
    }
}