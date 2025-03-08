using UnityEngine;

namespace Player
{
    public class CameraPosition : MonoBehaviour
    {
        public GameObject player;
        public float smoothSpeed = 5f;
        public float heightOffset = 4f; // Height above player
        public float distanceOffset = 5f; // Distance behind player

        private void LateUpdate()
        {
            // Calculate position based on player's forward direction for proper "behind" positioning
            var behindPosition = -player.transform.forward * distanceOffset;
            var upPosition = Vector3.up * heightOffset;

            // Combine positions to get the desired camera position
            var desiredPosition = player.transform.position + behindPosition + upPosition;

            // Smoothly move camera to desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Make camera look at player's head
            var lookTarget = player.transform.position + new Vector3(0, 2.0f, 0);
            transform.LookAt(lookTarget);
        }
    }
}