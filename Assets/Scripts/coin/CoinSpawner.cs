using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        public GameObject coinPrefab;
        public float spawnDistance = 50f;
        public float despawnDistance = 10f;
        public float laneWidth = 3f;

        public float coinSpawnInterval = 5f;
        public float minHorizontalSpacing = 3f;
        public int coinsPerPattern = 10;
        public float minDistanceToTriggerSpawn = 5f;

        private Vector3 lastSpawnPosition;
        private float lastSpawnZ = 0f;
        private readonly List<GameObject> spawnedCoins = new List<GameObject>();
        private Dictionary<int, float> lastLaneSpawnZ = new Dictionary<int, float>();
        private bool listenForTurns = false;

        private void Start()
        {
            lastSpawnPosition = transform.position;
            InitializeLaneTracking();
            ConnectToTurnHandler();
            SpawnCoinsAhead();
            Debug.Log("CoinSpawner initialized with player at " + transform.position);
        }

        private void ConnectToTurnHandler()
        {
            if (transform == null) return;

            var turnHandler = transform.GetComponent<PlayerTurnHandler>();
            if (turnHandler != null)
            {
                turnHandler.OnPlayerTurn += OnPlayerTurn;
                listenForTurns = true;
                Debug.Log("Connected to PlayerTurnHandler successfully");
            }
            else
            {
                Debug.LogWarning("PlayerTurnHandler not found on player");
            }
        }

        private void InitializeLaneTracking()
        {
            lastLaneSpawnZ[-1] = 0f;
            lastLaneSpawnZ[0] = 0f;
            lastLaneSpawnZ[1] = 0f;
        }

        private void Update()
        {
            if (transform == null) return;

            // Try to connect to turn handler if not already connected
            if (!listenForTurns)
            {
                ConnectToTurnHandler();
            }

            // Check if player has moved enough to spawn more coins
            var playerPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
            var lastSpawnPosXZ = new Vector3(lastSpawnPosition.x, 0, lastSpawnPosition.z);
            var distanceMoved = Vector3.Distance(playerPosXZ, lastSpawnPosXZ);
    
            // Draw debug line showing distance
            Debug.DrawLine(lastSpawnPosXZ + Vector3.up, playerPosXZ + Vector3.up, Color.yellow);
    
            // Add more logging to understand the values
            if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
            {
                Debug.Log($"Current distance: {distanceMoved:F2}, Threshold: {minDistanceToTriggerSpawn:F2}");
            }

            if (distanceMoved >= minDistanceToTriggerSpawn)
            {
                Debug.Log($"Player moved {distanceMoved:F2}m - spawning more coins");
                SpawnCoinsAhead();
                lastSpawnPosition = transform.position;
            }

            CleanupCoins();
        }

        private void OnDestroy()
        {
            if (transform != null)
            {
                var turnHandler = transform.GetComponent<PlayerTurnHandler>();
                if (turnHandler != null)
                    turnHandler.OnPlayerTurn -= OnPlayerTurn;
            }
        }

        private void SpawnCoinsAhead()
        {
            if (transform == null) return;

            var playerForward = transform.forward;
            var playerRight = transform.right;

            int pattern = Random.Range(0, 4);
            float spawnZ = 0f; // Start from near the player
            float spawnLength = spawnDistance;
            int coinsSpawned = 0;

            Debug.DrawRay(transform.position, playerForward * spawnDistance, Color.blue, 1f);

            while (spawnZ < spawnLength && coinsSpawned < coinsPerPattern)
            {
                List<int> potentialLanes = GetLanesForPattern(pattern, coinsSpawned);
                List<int> validLanes = new List<int>();

                foreach (int lane in potentialLanes)
                {
                    if (!lastLaneSpawnZ.ContainsKey(lane) ||
                        spawnZ >= lastLaneSpawnZ[lane] + minHorizontalSpacing)
                    {
                        validLanes.Add(lane);
                    }
                }

                if (validLanes.Count > 0)
                {
                    int selectedLane = validLanes[Random.Range(0, validLanes.Count)];
                    var laneOffset = playerRight * (laneWidth * selectedLane);

                    // Calculate spawn position relative to player's current orientation
                    var spawnPosition = transform.position +
                                        playerForward * (spawnZ + 10f) + // Start a bit further from player
                                        laneOffset;

                    spawnPosition.y += 1.0f;

                    var coin = Instantiate(coinPrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
                    spawnedCoins.Add(coin);
                    coinsSpawned++;

                    lastLaneSpawnZ[selectedLane] = spawnZ;
                }

                spawnZ += coinSpawnInterval;
            }

            lastSpawnZ = spawnZ;
            Debug.Log($"Spawned {coinsSpawned} coins ahead of player");
        }

        private List<int> GetLanesForPattern(int pattern, int position)
        {
            List<int> lanes = new List<int>();

            switch (pattern)
            {
                case 0: // Single lane pattern
                    lanes.Add(position % 3 - 1);
                    break;

                case 1: // Alternating pattern
                    lanes.Add((position % 2) * 2 - 1);
                    break;

                case 2: // Middle lane only
                    lanes.Add(0);
                    break;

                case 3: // Multiple lane options
                    lanes.Add(-1);
                    lanes.Add(0);
                    lanes.Add(1);
                    break;
            }

            return lanes;
        }

        private void CleanupCoins()
        {
            for (var i = spawnedCoins.Count - 1; i >= 0; i--)
            {
                if (spawnedCoins[i] == null)
                {
                    spawnedCoins.RemoveAt(i);
                    continue;
                }

                var coinPos = spawnedCoins[i].transform.position;
                var playerPos = transform.position;
                var playerToCoins = coinPos - playerPos;
                var dotProduct = Vector3.Dot(playerToCoins, transform.forward);

                if (dotProduct < -despawnDistance)
                {
                    Destroy(spawnedCoins[i]);
                    spawnedCoins.RemoveAt(i);
                }
            }
        }

        private void OnPlayerTurn(float turnAngle)
        {
            Debug.Log("Player turned, respawning coins");
            StartCoroutine(DelayedSpawnAfterTurn());
        }

        private IEnumerator DelayedSpawnAfterTurn()
        {
            // Clear existing coins
            foreach (var coin in spawnedCoins)
                if (coin != null)
                    Destroy(coin);

            spawnedCoins.Clear();

            // Wait for player's transform to be fully updated after turn
            yield return null;

            // Reset tracking variables
            lastSpawnZ = 0f;
            InitializeLaneTracking();

            // CRITICAL: Update lastSpawnPosition to current player position
            lastSpawnPosition = transform.position;

            // Spawn new coins in the player's new direction
            SpawnCoinsAhead();
        }
    }
}