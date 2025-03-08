using UnityEngine;
using System;

namespace Player
{
    public class PlayerLaneHandler : MonoBehaviour
    {
        [Range(-1, 1)] public int currentLane = 0; // -1 = Left, 0 = Middle, 1 = Right
        private int lastLane = 0;
        public float laneWidth = 3f; // Set to exactly 3
        public float laneChangeSpeed = 3f;
        public float leanAngle = 15f;

        private bool isChangingLane;
        private float laneChangeProgress;
        private Vector3 targetLanePosition;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Quaternion targetRotation;

        // Event to notify PlayerMovement about movement adjustments
        public event Action<Vector3> OnLaneMovement;

        private void Update()
        {
            HandleLaneInput();

            if (isChangingLane) ProcessLaneChange();
        }

        private void HandleLaneInput()
        {
            if (!canChangeLane()) return;

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentLane > -1)
                {
                    lastLane = currentLane;
                    currentLane--;
                    StartLaneChange();
                }
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentLane < 1)
                {
                    lastLane = currentLane;
                    currentLane++;
                    StartLaneChange();
                }
            }
        }

        private void StartLaneChange()
        {
            isChangingLane = true;
            laneChangeProgress = 0f;
            startPosition = transform.position;

            // Store rotation for leaning effect
            startRotation = transform.rotation;

            var leanZ = currentLane > lastLane ? leanAngle : -leanAngle;
            targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, leanZ);

            // Calculate target position
            CalculateTargetLanePosition();

            // Trigger animation if available
            var animator = GetComponent<Animator>();
            if (animator != null && HasParameter("LaneChange", animator)) animator.SetTrigger("LaneChange");
        }

        private void CalculateTargetLanePosition()
        {
            // Get the right vector perpendicular to forward direction
            var rightVector = Vector3.Cross(transform.forward, Vector3.up).normalized;

            // Calculate base position (middle lane position)
            var basePosition = transform.position;

            // Remove any previous lane offset
            if (lastLane == -1)
                basePosition += rightVector * laneWidth;
            else if (lastLane == 1)
                basePosition -= rightVector * laneWidth;

            // Apply new lane offset
            if (currentLane == -1) // Left lane
                targetLanePosition = basePosition - rightVector * laneWidth;
            else if (currentLane == 1) // Right lane
                targetLanePosition = basePosition + rightVector * laneWidth;
            else // Middle lane
                targetLanePosition = basePosition;
        }

        private void ProcessLaneChange()
        {
            // Increase progress based on lane change speed
            laneChangeProgress += Time.deltaTime * laneChangeSpeed;

            if (laneChangeProgress >= 1.0f)
            {
                // Lane change complete
                isChangingLane = false;
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0); // Reset Z rotation

                // Ensure player is exactly at target position
                var finalMovement = targetLanePosition - transform.position;
                OnLaneMovement?.Invoke(finalMovement);
            }
            else
            {
                // Create a smooth curve for lane changing
                var t = EaseInOutSine(laneChangeProgress);

                // Calculate current position along the path
                var newPosition = Vector3.Lerp(startPosition, targetLanePosition, t);

                // Calculate movement needed this frame
                var movement = newPosition - transform.position;

                // Apply movement via event
                OnLaneMovement?.Invoke(movement);

                // Apply lean effect
                var leanFactor = Mathf.Sin(t * Mathf.PI);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, leanFactor);
            }
        }

        public void ResetLaneOnTurn()
        {
            // Reset to middle lane after a turn
            lastLane = currentLane;
            currentLane = 0;
            isChangingLane = false;

            // Recalculate target position with new orientation
            CalculateTargetLanePosition();
        }

        private bool canChangeLane()
        {
            var isSliding = GetComponent<PlayerMovement>().IsSliding;
            var isJumping = GetComponent<PlayerMovement>().IsJumping;
            return !isChangingLane && !isTurning() && !isSliding && !isJumping;
        }

        // Helper methods
        private bool HasParameter(string paramName, Animator animator)
        {
            foreach (var param in animator.parameters)
                if (param.name == paramName)
                    return true;
            return false;
        }

        private float EaseInOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
        }

        private bool isTurning()
        {
            return GetComponent<PlayerTurnHandler>().IsTurning;
        }
    }
}