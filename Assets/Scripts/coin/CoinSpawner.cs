using System.Collections.Generic;
using Player;
using UnityEngine;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        public GameObject coinPrefab;
        public Transform playerTransform;
        public float spawnDistance = 50f;
        public float despawnDistance = 10f;
        public float laneWidth = 2f;

        public float coinSpawnInterval = 5f;
        public float minHorizontalSpacing = 3f;
        public int coinsPerPattern = 10;

        private Vector3 lastSpawnPosition;
        private float lastSpawnZ = 0f;
        private readonly List<GameObject> spawnedCoins = new List<GameObject>();
        private Dictionary<int, float> lastLaneSpawnZ = new Dictionary<int, float>();

        private void Start()
        {
            lastSpawnPosition = playerTransform.position;
            InitializeLaneTracking();

            var turnHandler = playerTransform.GetComponent<PlayerTurnHandler>();
            if (turnHandler != null)
                turnHandler.OnPlayerTurn += OnPlayerTurn;
            else
                Debug.LogWarning("PlayerTurnHandler not found on player");

            SpawnCoinsAhead();
        }

        private void InitializeLaneTracking()
        {
            lastLaneSpawnZ[-1] = 0f;
            lastLaneSpawnZ[0] = 0f;
            lastLaneSpawnZ[1] = 0f;
        }

        private void Update()
        {
            var playerPosXZ = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            var lastSpawnPosXZ = new Vector3(lastSpawnPosition.x, 0, lastSpawnPosition.z);
            var distanceMoved = Vector3.Distance(playerPosXZ, lastSpawnPosXZ);

            if (distanceMoved >= 5f)
            {
                SpawnCoinsAhead();
                lastSpawnPosition = playerTransform.position;
            }

            CleanupCoins();
        }

        private void OnDestroy()
        {
            if (playerTransform != null)
            {
                var turnHandler = playerTransform.GetComponent<PlayerTurnHandler>();
                if (turnHandler != null)
                    turnHandler.OnPlayerTurn -= OnPlayerTurn;
            }
        }

        private void SpawnCoinsAhead()
        {
            var playerForward = playerTransform.forward;
            var playerRight = playerTransform.right;

            int pattern = Random.Range(0, 4);
            float spawnZ = 0f; // Start from near the player after turning
            float spawnLength = spawnDistance;
            int coinsSpawned = 0;

            // Debug visual to verify spawn direction
            Debug.DrawRay(playerTransform.position, playerForward * spawnDistance, Color.blue, 5f);
            Debug.DrawRay(playerTransform.position, playerRight * 5, Color.red, 5f);

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
                    var spawnPosition = playerTransform.position +
                                      playerForward * (spawnZ + 10f) + // Start a bit further from player
                                      laneOffset;

                    spawnPosition.y += 1.0f;

                    // Debug spawn position
                    Debug.DrawLine(playerTransform.position, spawnPosition, Color.green, 1f);

                    var coin = Instantiate(coinPrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
                    spawnedCoins.Add(coin);
                    coinsSpawned++;

                    lastLaneSpawnZ[selectedLane] = spawnZ;
                }

                spawnZ += coinSpawnInterval;
            }

            lastSpawnZ = spawnZ;
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
                var playerPos = playerTransform.position;
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
            // Wait a frame to ensure player transform is fully updated
            StartCoroutine(DelayedSpawnAfterTurn());
        }

        private System.Collections.IEnumerator DelayedSpawnAfterTurn()
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

            // Spawn new coins in the player's new direction
            SpawnCoinsAhead();
        }
    }
}