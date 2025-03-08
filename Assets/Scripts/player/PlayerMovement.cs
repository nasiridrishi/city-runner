using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public Animator animator;
        public CharacterController controller;
        public float movementSpeed = 5f; // Speed of the player
        public float jumpHeight = 2f; // Height of the jump
        public float gravity = 9.81f; // Gravity strength

        private Vector3 targetPosition;

        private Vector3 velocity; // Stores vertical movement

        public bool IsJumping
        {
            get;
            private set;
        }

        public bool IsSliding {
            get;
            private set;
        }
        public float slideDuration = 1f; // How long the slide lasts
        public float slideSpeedMultiplier = 1.5f; // Slide speed boost

        // Reference vectors for movement
        private Vector3 forwardDirection;
        private Vector3 rightDirection;

        // Lane handler integration
        private PlayerLaneHandler laneHandler;
        private PlayerTurnHandler turnHandler;
        private Vector3 lateralMovement = Vector3.zero;

        private void Start()
        {
            // Set initial target position
            targetPosition = transform.position;

            // Get lane handler
            laneHandler = GetComponent<PlayerLaneHandler>();
            if (laneHandler == null)
            {
                laneHandler = gameObject.AddComponent<PlayerLaneHandler>();
            }

            // Get turn handler
            turnHandler = GetComponent<PlayerTurnHandler>();
            if (turnHandler == null)
            {
                turnHandler = gameObject.AddComponent<PlayerTurnHandler>();
            }

            // Subscribe to lane movement events
            laneHandler.OnLaneMovement += ApplyLateralMovement;

            // Initialize direction vectors
            forwardDirection = transform.forward;
            rightDirection = transform.right;
        }

        private void ApplyLateralMovement(Vector3 movement)
        {
            lateralMovement = movement;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (laneHandler != null)
                laneHandler.OnLaneMovement -= ApplyLateralMovement;
        }

        private void Update()
        {
            // Update direction vectors in case they've changed due to turns
            forwardDirection = transform.forward;
            rightDirection = transform.right;
            
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
                    {
                        animator.ResetTrigger("Jump"); // Reset jump trigger
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && !IsSliding) // Jump on space press
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity); // Jump force
                    IsJumping = true;

                    if (animator != null && HasParameter("Jump", animator))
                    {
                        animator.SetTrigger("Jump"); // Trigger jump animation
                    }
                }
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime; // Apply gravity
            }
        }

        private void HandleSlide()
        {
            if ((!IsSliding && Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isTurning())
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            IsSliding = true;

            // Increase movement speed temporarily
            float originalSpeed = movementSpeed;
            movementSpeed *= slideSpeedMultiplier;

            // Trigger slide animation if available
            if (animator != null && HasParameter("Slide", animator))
            {
                animator.SetTrigger("Slide");
            }

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
            Vector3 movement = Vector3.zero;

            // Determine primary movement axis based on player orientation
            if (Mathf.Abs(transform.forward.z) > Mathf.Abs(transform.forward.x))
            {
                // Primarily facing Z direction
                movement.z = Mathf.Sign(transform.forward.z) * movementSpeed * Time.deltaTime;

                // Apply lane movement from lane handler if present
                if (lateralMovement != Vector3.zero)
                {
                    movement.x = lateralMovement.x;
                }
            }
            else
            {
                // Primarily facing X direction
                movement.x = Mathf.Sign(transform.forward.x) * movementSpeed * Time.deltaTime;

                // Apply lane movement from lane handler if present
                if (lateralMovement != Vector3.zero)
                {
                    movement.z = lateralMovement.z;
                }
            }

            // Reset lateral movement after applied
            lateralMovement = Vector3.zero;

            // Apply vertical movement
            movement.y = velocity.y * Time.deltaTime;

            // Apply movement using the CharacterController
            controller.Move(movement);

            // Update animator speed
            if (animator != null)
            {
                animator.SetFloat("MovementSpeed", movementSpeed);
            }
        }

        private bool isTurning()
        {
            return GetComponent<PlayerTurnHandler>().IsTurning;
        }

        // Helper method to check if an animator has a parameter
        private bool HasParameter(string paramName, Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }

            return false;
        }
    }
}