using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        public GameObject coinPrefab;
        public Transform playerTransform;
        public float spawnDistance = 50f; // How far ahead to spawn coins
        public float despawnDistance = 10f; // How far behind before deleting coins
        public float laneWidth = 3f; // Should match PlayerLaneHandler.laneWidth

        public List<LaneConfig> laneConfigurations = new List<LaneConfig>();
        private Vector3 lastSpawnPosition;

        private readonly Dictionary<int, float> nextSpawnPositions = new Dictionary<int, float>();
        private readonly List<GameObject> spawnedCoins = new List<GameObject>();

        private void Start()
        {
            Debug.Log("Coin Spawner started");
            // Initialize spawn positions for each lane
            foreach (var config in laneConfigurations) nextSpawnPositions[config.laneIndex] = 0f;

            lastSpawnPosition = playerTransform.position;

            // Subscribe to player turn events
            var turnHandler = playerTransform.GetComponent<PlayerTurnHandler>();
            if (turnHandler == null)
            {
                Debug.Log("Turn handler is null");
            }
            if (turnHandler != null) turnHandler.OnPlayerTurn += OnPlayerTurn;
        }

        private void Update()
        {
            // Check if player has moved enough to spawn new coins
            var distanceMoved = Vector3.Distance(
                new Vector3(playerTransform.position.x, 0, playerTransform.position.z),
                new Vector3(lastSpawnPosition.x, 0, lastSpawnPosition.z)
            );

            if (distanceMoved >= 5f) // Every 5 units, update spawn position
            {
                SpawnCoinsAhead();
                lastSpawnPosition = playerTransform.position;
            }

            // Clean up coins that are behind the player
            CleanupCoins();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerTransform != null)
            {
                var turnHandler = playerTransform.GetComponent<PlayerTurnHandler>();
                if (turnHandler != null) turnHandler.OnPlayerTurn -= OnPlayerTurn;
            }
        }

        private void SpawnCoinsAhead()
        {
            Debug.Log("Spawn Coins Ahead");
            if (laneConfigurations.Count == 0)
            {
                Debug.LogWarning("No lane configurations found");
            }
            var playerForward = playerTransform.forward;
            var playerRight = playerTransform.right;

            foreach (var config in laneConfigurations)
            {
                // Determine lane offset direction
                var laneOffset = playerRight * laneWidth * -config.laneIndex;

                // Calculate spawn position for this lane
                var spawnZ = nextSpawnPositions[config.laneIndex];

                // How much distance to cover with coins
                var spawnLength = spawnDistance;

                while (spawnZ < spawnLength)
                {
                    // Apply random probability for this coin
                    if (Random.value <= config.spawnProbability)
                    {
                        var spawnPosition = playerTransform.position +
                                            playerForward * (spawnDistance + spawnZ) +
                                            laneOffset;

                        // Add slight height to place coin above ground
                        spawnPosition.y += 1.0f;

                        var coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
                        spawnedCoins.Add(coin);
                    }

                    // Move to next spawn position in this lane
                    spawnZ += config.coinSpawnInterval;
                }

                // Update next spawn position
                nextSpawnPositions[config.laneIndex] = spawnZ - spawnLength;
            }
        }

        private void CleanupCoins()
        {
            for (var i = spawnedCoins.Count - 1; i >= 0; i--)
            {
                if (spawnedCoins[i] == null) // Already destroyed
                {
                    spawnedCoins.RemoveAt(i);
                    continue;
                }

                var coinPos = spawnedCoins[i].transform.position;
                var playerPos = playerTransform.position;

                // Check if coin is behind player by more than despawn distance
                var playerToCoins = coinPos - playerPos;
                var dotProduct = Vector3.Dot(playerToCoins, playerTransform.forward);

                if (dotProduct < -despawnDistance)
                {
                    Destroy(spawnedCoins[i]);
                    spawnedCoins.RemoveAt(i);
                }
            }
        }

        private void OnPlayerTurn(float turnAngle)
        {
            
            Debug.Log("onPlayer Turn Listener");
            // Clear and reset spawning when player turns
            foreach (var coin in spawnedCoins)
                if (coin != null)
                    Destroy(coin);

            spawnedCoins.Clear();

            // Reset spawn positions
            foreach (var config in laneConfigurations) nextSpawnPositions[config.laneIndex] = 0f;

            // Immediately spawn new coins in the new direction
            SpawnCoinsAhead();
        }

        [Serializable]
        public class LaneConfig
        {
            public int laneIndex; // -1 = Left, 0 = Middle, 1 = Right 
            public float coinSpawnInterval = 10f; // Distance between coins in this lane
            public float spawnProbability = 0.5f; // Chance of spawning a coin at each interval
        }
    }
}