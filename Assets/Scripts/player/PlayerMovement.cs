using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;
    public float movementSpeed = 5f; // Speed of the player
    public float laneChangeSpeed = 3f; // Controls how quickly lane changes happen (lower = slower/more dramatic)
    public float leanAngle = 15f; // How much the character leans during lane change
    
    public float jumpHeight = 2f; // Height of the jump
    public float gravity = 9.81f; // Gravity strength

    [Range(-1, 1)]
    private int currentLane = 0; // -1 = Left, 0 = Middle, 1 = Right
    [Range(-1, 1)]
    private int lastLane = 0;
    Dictionary<int, int> lanes = new Dictionary<int, int>();
    private int leftLane = 289;
    private int middleLane = 291;
    private int rightLane = 293;
    
    private float turnSmoothVelocity;
    private Vector3 targetPosition;
    private float cooldownTimer = 0f; // Timer to track cooldown
    
    private bool isChangingLane = false;
    private float laneChangeProgress = 0f;
    private float startLaneX;
    private float targetLaneX;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    private Vector3 velocity; // Stores vertical movement
    private bool isJumping = false;

    private bool isSliding = false;
    public float slideDuration = 1f; // How long the slide lasts
    public float slideSpeedMultiplier = 1.5f; // Slide speed boost

    private void Start()
    {
        lanes.Add(-1, 289);
        lanes.Add(0, 291);
        lanes.Add(1, 293);
        // Set initial target position
        targetPosition = transform.position;
    }

    private void Update()
    {
        // Update cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        HandleLaneChange();
        HandleJump();
        HandleSlide();
        MovePlayer();
    }

    private void HandleLaneChange()
    {
        // Only allow new lane changes if not currently changing lanes
        if (!isChangingLane)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentLane < 1)
                {
                    lastLane = currentLane;
                    currentLane++;
                    StartLaneChange();
                }
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("Left key pressed. Current lane: " + currentLane);
                if (currentLane > -1) // Make sure we don't go beyond left lane
                {
                    lastLane = currentLane;
                    currentLane--;
                    StartLaneChange();
                }
            }
        }
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
                    animator.ResetTrigger("Jump");  // Reset jump trigger
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isSliding) // Jump on space press
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity); // Jump force
                isJumping = true;

                if (animator != null && HasParameter("Jump", animator))
                {
                    animator.SetTrigger("Jump");  // Trigger jump animation
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

    private void StartLaneChange()
    {
        isChangingLane = true;
        laneChangeProgress = 0f;
        startLaneX = transform.position.x;
        targetLaneX = lanes[currentLane];
        
        // Set up rotation for leaning effect
        startRotation = transform.rotation;
        
        // Calculate lean direction based on which way we're moving
        float leanZ = (currentLane > lastLane) ? -leanAngle : leanAngle;
        targetRotation = Quaternion.Euler(0, 0, leanZ);
        
        // Optionally trigger a lane change animation
        if (animator != null)
        {
            // Check if the animator has a LaneChange parameter before trying to set it
            if (HasParameter("LaneChange", animator))
            {
                animator.SetTrigger("LaneChange");
            }
        }
    }
    
    private void MovePlayer()
    {
        // Create movement vector
        Vector3 movement = Vector3.zero;
    
        // Add forward movement
        movement += transform.forward * movementSpeed * Time.deltaTime;

        // Handle lane change transition if in progress
        if (isChangingLane)
        {
            // Increase progress based on lane change speed
            laneChangeProgress += Time.deltaTime * laneChangeSpeed;
            
            if (laneChangeProgress >= 1.0f)
            {
                // Lane change complete
                isChangingLane = false;
                transform.rotation = startRotation; // Reset rotation
            }
            else
            {
                // Create a dramatic curve for lane changing (start slow, speed up, then slow down)
                float t = EaseInOutSine(laneChangeProgress);
                
                // Calculate the position for this frame
                float currentX = Mathf.Lerp(startLaneX, targetLaneX, t);
                
                // Calculate the horizontal movement needed
                float horizontalMovement = currentX - transform.position.x;
                movement.x = horizontalMovement;
                
                // Apply and then gradually remove lean effect
                float leanFactor = Mathf.Sin(t * Mathf.PI); // Peak in middle of transition
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, leanFactor);
            }
        }

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
