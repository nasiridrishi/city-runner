using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerTurnHandler : MonoBehaviour
    {
        public float turnDetectionRadius = 1f;
        public float turnCooldown = 0.5f;
        
        // Event for notifying about turns
        public event Action<float> OnPlayerTurn;
        
        private float cooldownTimer = 0f;
        private PlayerLaneHandler laneHandler;
        
        private void Start()
        {
            laneHandler = GetComponent<PlayerLaneHandler>();
            if (laneHandler == null)
            {
                laneHandler = gameObject.AddComponent<PlayerLaneHandler>();
            }
        }
        
        private void Update()
        {
            // Update cooldown timer
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            
            CheckForTurnPoints();
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
                if (turnPoint != null && cooldownTimer <= 0)
                {
                    PerformTurn(turnPoint);
                    break;
                }
            }
        }
        
        private void PerformTurn(PlayerTurnPoint turnPoint)
        {
            float yRotation = turnPoint.direction == PlayerTurnPoint.TurnDirection.Left ?
                -turnPoint.turnAngle : turnPoint.turnAngle;
                
            StartCoroutine(SmoothTurn(yRotation));
            
            // Notify lane handler about turn
            laneHandler.ResetLaneOnTurn();
            
            // Set cooldown timer
            cooldownTimer = turnCooldown;
            
            // Notify listeners about the turn
            OnPlayerTurn?.Invoke(yRotation);
            
            // Trigger animation if needed
            TriggerTurnAnimation();
        }
        
        private IEnumerator SmoothTurn(float yRotation)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + yRotation, 0);
            float duration = 0.3f;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final rotation is exactly the target rotation
            transform.rotation = targetRotation;
        }
        
        private void TriggerTurnAnimation()
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null && HasParameter("Turn", animator))
            {
                animator.SetTrigger("Turn");
            }
        }
        
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