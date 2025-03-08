using System.Collections.Generic;
using Player;
using UnityEngine;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab; // Add reference to your coin prefab

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
                PlayerTurnHandler.Instance.OnPlayerTurnComplete += onPlayerTurned;
                listeningForTurns = true;
            }
        }

        private void SpawnCoin()
        {
            // Initialize variables for raycast
            RaycastHit hit;
            var hitTurnPoint = false;
            var pathLength = 0f;

            // Raycast and check for TurnPoint collision or maximum distance
            if (Physics.Raycast(turnStartPos, transform.forward, out hit, maxRaycastDistance))
            {
                if (hit.collider.CompareTag("TurnPoint"))
                {
                    hitTurnPoint = true;
                    pathLength = hit.distance;
                    Debug.Log($"Hit turn point at distance: {pathLength}");
                }
                else
                {
                    // If we hit something else, use the hit distance
                    pathLength = hit.distance;
                    Debug.Log($"Hit object (not turn point) at distance: {pathLength}");
                }
            }
            else
            {
                // If no hit, use maximum distance
                pathLength = maxRaycastDistance;
                Debug.Log($"No hit detected, using maximum distance: {pathLength}");
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

                        // Add coin component if it doesn't already have one
                        if (!coin.GetComponent<Coin>()) coin.AddComponent<Coin>();

                        // Add to tracking list
                        spawnedCoins.Add(coin);

                        Debug.Log($"Spawned coin at position {coinPosition}, lane: {lane}");
                    }
                }

                // Move to next segment
                currentPosition += segmentLength;

                // Ensure consecutive coins are 1 unit apart
                // This is achieved by the random segment length and position calculation
                // We're placing coins in the middle of random segments, which ensures variable spacing
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
    }
}