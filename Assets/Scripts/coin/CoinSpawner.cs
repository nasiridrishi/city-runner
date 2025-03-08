using System.Collections.Generic;
using Player;
using UnityEngine;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab; // Add reference to your coin prefab
        
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask turnPointLayerMask; // Set this in inspector to include only the layers you want to detect
        [SerializeField] private LayerMask ignoreLayerMask; // Optional: Set this to layers you explicitly want to ignore

        private Vector3 turnStartPos;
        private float laneWidth = 3f;
        private float spawnChance = 0.5f;
        private float maxRaycastDistance = 500f; // Maximum distance to check for turn points

        // List to track spawned coins for cleanup if needed
        private List<GameObject> spawnedCoins = new List<GameObject>();
        private bool listeningForTurns = false;

        private void Start()
        {
        }

        private void Update()
        {
            if (!listeningForTurns)
            {
                GetComponent<PlayerTurnHandler>().OnPlayerTurnComplete += onPlayerTurned;
                listeningForTurns = true;
            }
        }

        private void SpawnCoin()
        {
            // Initialize variables for raycast
            RaycastHit hit;
            var hitTurnPoint = false;
            var pathLength = 0f;

            // Use layerMask to filter what objects the raycast can hit
            // The ~ operator inverts the mask, so ~ignoreLayerMask means "hit everything EXCEPT these layers"
            if (Physics.Raycast(turnStartPos, transform.forward, out hit, maxRaycastDistance, turnPointLayerMask))
            {
                if (hit.collider.CompareTag("TurnPoint"))
                {
                    hitTurnPoint = true;
                    pathLength = hit.distance;
                    Debug.Log($"Hit TurnPoint at distance: {pathLength}");
                }
                else
                {
                    // If we hit something else in the allowed layers
                    pathLength = hit.distance;
                    Debug.Log($"Hit {hit.collider.gameObject.name} at distance: {pathLength}");
                }
            }
            else
            {
                // If no hit, use maximum distance
                pathLength = maxRaycastDistance;
                Debug.Log($"No hit, using max distance: {pathLength}");
            }

            // Ensure pathLength doesn't exceed maxRaycastDistance
            pathLength = Mathf.Min(pathLength, maxRaycastDistance);

            // Split the path into smaller segments with random lengths between 8-12
            var segments = new List<float>();
            var currentPosition = 0f;

            while (currentPosition < pathLength)
            {
                var segmentLength = Random.Range(8f, 12f);

                // Make sure we don't exceed the path length
                if (currentPosition + segmentLength > pathLength) segmentLength = pathLength - currentPosition;

                segments.Add(segmentLength);
                currentPosition += segmentLength;
            }

            // Spawn coins for each segment based on spawn chance
            currentPosition = 0f;

            foreach (var segmentLength in segments)
            {
                // Check if we should spawn a coin in this segment
                if (Random.value < spawnChance)
                {
                    // Choose a random lane (-1, 0, 1)
                    var lane = Random.Range(-1, 2); // Range is inclusive of min, exclusive of max

                    for (var i = 0; i < segmentLength; i++)
                    {
                        var coinPosition = turnStartPos + transform.forward * (currentPosition + i) +
                                           transform.right * (lane * laneWidth);
                        // Instantiate coin at calculated position
                        var coin = Instantiate(coinPrefab, coinPosition + Vector3.up, Quaternion.Euler(90, 0, 0));

                        // Add to tracking list
                        spawnedCoins.Add(coin);
                    }
                }

                // Move to next segment
                currentPosition += segmentLength;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //todo
        }

        private void onPlayerTurned()
        {
            turnStartPos = transform.position;
            SpawnCoin();
        }

        // Optional: Method to clear spawned coins (useful for testing)
        public void ClearCoins()
        {
            foreach (var coin in spawnedCoins)
                if (coin != null)
                    Destroy(coin);

            spawnedCoins.Clear();
        }
        
        // Debug method to visualize the raycast
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && turnStartPos != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(turnStartPos, transform.forward * maxRaycastDistance);
            }
        }
    }
}
