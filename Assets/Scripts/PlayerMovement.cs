using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;
    public float movementSpeed = 5f; // Speed of the player
    public float laneChangeSpeed = 3f; // Controls how quickly lane changes happen (lower = slower/more dramatic)
    
    [Range(-1, 1)]
    private int currentLane = 0; // -1 = Left, 0 = Middle, 1 = Right
    [Range(-1, 1)]
    private int lastLane = 0;
    Dictionary<int, int> lanes = new Dictionary<int, int>();
    private int leftLane = 289;
    private int middleLane = 291;
    private int rightLane = 293;
    
    private Vector3 targetPosition;
    private float cooldownTimer = 0f; // Timer to track cooldown
    
    private bool isChangingLane = false;
    private float laneChangeProgress = 0f;
    private float startLaneX;
    private float targetLaneX;

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
    
    private void StartLaneChange()
    {
        isChangingLane = true;
        laneChangeProgress = 0f;
        startLaneX = transform.position.x;
        targetLaneX = lanes[currentLane];
        
        // Clear any existing animation states
        ResetRollAnimations();
        
        // Start the new appropriate animation
        if (animator != null)
        {
            if (currentLane > lastLane) // Moving right
            {
                if (HasParameter("RollRight", animator))
                    animator.SetBool("RollRight", true);
            }
            else // Moving left
            {
                if (HasParameter("RollLeft", animator))
                    animator.SetBool("RollLeft", true);
            }
            
            // Also trigger the general lane change animation if available
            if (HasParameter("LaneChange", animator))
            {
                animator.SetTrigger("LaneChange");
            }
        }
    }
    
    private void ResetRollAnimations()
    {
        if (animator != null)
        {
            if (HasParameter("RollLeft", animator))
                animator.SetBool("RollLeft", false);
                
            if (HasParameter("RollRight", animator))
                animator.SetBool("RollRight", false);
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
                
                // Reset the roll animation booleans
                ResetRollAnimations();
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
            }
        }

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
