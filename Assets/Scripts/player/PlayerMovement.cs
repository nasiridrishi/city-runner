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

        private float turnSmoothVelocity;
        private Vector3 targetPosition;
        private float cooldownTimer = 0f; // Timer to track cooldown

        private Vector3 velocity; // Stores vertical movement
        private bool isJumping = false;

        private bool isSliding = false;
        public float slideDuration = 1f; // How long the slide lasts
        public float slideSpeedMultiplier = 1.5f; // Slide speed boost
        
        private bool isTurning = false;
        public float turnDetectionRadius = 1f; // How close to trigger a turn point
        private Vector3 forwardDirection; // Current direction of player movement
        private Vector3 rightDirection; // Right vector perpendicular to movement
        private float turnProgress = 0f;
        private Vector3 turnCenter;
        private float turnStartAngle;
        private float turnEndAngle;
        private float currentTurnRadius;
        private float turnSpeedMultiplier = 1f;
        
        // Lane handler integration
        private PlayerLaneHandler laneHandler;
        private Vector3 lateralMovement = Vector3.zero;
        
        public float turnCooldown = 0.5f; // Cooldown time in seconds after each turn

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
            // Update cooldown timer
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }

            CheckForTurnPoints();
            HandleJump();
            HandleSlide();
            MovePlayer();
        }
        
        private void CheckForTurnPoints()
        {
            // Cast a small sphere forward to detect turn points
            RaycastHit[] hits = Physics.SphereCastAll(
                transform.position,
                turnDetectionRadius,
                transform.forward,
                turnDetectionRadius * 2
            );

            foreach (RaycastHit hit in hits)
            {
                PlayerTurnPoint turnPoint = hit.collider.GetComponent<PlayerTurnPoint>();
                if (turnPoint != null && !isTurning)
                {
                    // Execute instant turn
                    PerformInstantTurn(turnPoint);
                    break;
                }
            }
        }

        private void PerformInstantTurn(PlayerTurnPoint turnPoint)
        {
            // Only proceed if not on cooldown
            if (cooldownTimer <= 0)
            {
                // Return to the original logic
                float yRotation = turnPoint.direction == PlayerTurnPoint.TurnDirection.Left ?
                   -turnPoint.turnAngle : turnPoint.turnAngle;

                // Apply instant rotation around Y axis
                transform.Rotate(0, yRotation, 0);

                // Update direction vectors
                forwardDirection = transform.forward;
                rightDirection = transform.right;

                // Notify lane handler about turn
                laneHandler.ResetLaneOnTurn();

                // Set cooldown timer
                cooldownTimer = turnCooldown;

                // Optional: Trigger turn animation if available
                if (animator != null && HasParameter("Turn", animator))
                {
                    animator.SetTrigger("Turn");
                }
            }
        }
        
        private void StartTurn(PlayerTurnPoint turnPoint)
        {
            isTurning = true;
            turnProgress = 0f;
            turnSpeedMultiplier = turnPoint.turnSpeed;
        
            // Calculate turn direction based on turnPoint.direction
            float angle = turnPoint.turnAngle;
            currentTurnRadius = turnPoint.turnRadius;
        
            // Calculate turn center point (center of the arc)
            Vector3 turnDir = turnPoint.direction == PlayerTurnPoint.TurnDirection.Left ? 
                -rightDirection : rightDirection;
            turnCenter = transform.position + turnDir * currentTurnRadius;
        
            // Calculate start and end angles for the arc
            Vector3 startVector = transform.position - turnCenter;
            turnStartAngle = Mathf.Atan2(startVector.z, startVector.x) * Mathf.Rad2Deg;
        
            // End angle depends on turn direction
            if (turnPoint.direction == PlayerTurnPoint.TurnDirection.Left)
                turnEndAngle = turnStartAngle + angle;
            else
                turnEndAngle = turnStartAngle - angle;
        }

        private void HandleJump()
        {
            if (controller.isGrounded)
            {
                if (isJumping)
                {
                    // Player just landed, reset jump animation
                    isJumping = false;

                    if (animator != null && HasParameter("Jump", animator))
                    {
                        animator.ResetTrigger("Jump"); // Reset jump trigger
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && !isSliding) // Jump on space press
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity); // Jump force
                    isJumping = true;

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
            if (!isSliding && Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            isSliding = true;

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

            isSliding = false;
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

        // Easing function to make lane changes more dramatic
        private float EaseInOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
        }
    }
}