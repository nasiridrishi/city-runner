using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using world;

namespace trap
{
    [RequireComponent(typeof(PlayerTurnHandler))]
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Trap Wall Settings")] [SerializeField]
        private GameObject trapWallPrefab; // Trap wall prefab

        [SerializeField] private int maxTraps = 5; // Number of trap walls generated per turn

        [Header("Fire Settings")] [SerializeField]
        private GameObject firePrefab; // Fire prefab

        [SerializeField] private int maxFires = 8; // Number of fire obstacles generated per turn

        [Header("General Settings")] [SerializeField]
        private float laneWidth = 3f; // Lane width

        [SerializeField] private float safeZoneDistance = 10f; // Safe zone distance, head and tail reserved range
        [SerializeField] private int frameDelay = 3; // Number of frames to delay wall spawning

        private PathDetector pathDetector; // Path detector
        private Vector3 turnStartPos; // Record the starting point of the turn
        private List<GameObject> spawnedTraps = new List<GameObject>(); // Track the generated trap walls and fires

        private void Start()
        {
            // Make sure the path detector exists
            if (pathDetector == null)
            {
                pathDetector = GetComponent<PathDetector>();
                if (pathDetector == null) pathDetector = gameObject.AddComponent<PathDetector>();
            }

            GetComponent<PlayerTurnHandler>().OnPlayerTurnComplete += OnPlayerTurned;
        }

        private void SpawnObsWalls(PathInfo pathInfo)
        {
            // Ensure the path segment isn't empty and apply the trap spawning logic
            if (pathInfo.PathLength <= 0)
            {
                Debug.LogWarning("The path is too short to generate a trap wall!");
                return;
            }

            var usablePathLength =
                pathInfo.PathLength - 2 * safeZoneDistance; // Excluding both head and tail safe zones
            if (usablePathLength <= 0) return;

            // Decide on rotation based on forward direction, defaulting to identity rotation.
            var wallRotation = Quaternion.identity;
            if (Mathf.Abs(transform.forward.z) > Mathf.Abs(transform.forward.x))
                wallRotation = Quaternion.Euler(0, 90, 0);

            // Generate trap walls with positions biased toward the end of the path.
            for (var i = 0; i < maxTraps; i++)
            {
                // Compute a normalized fraction for this trap wall (ranging from 0 to 1)
                var t = (i + 1f) / (maxTraps + 1f);
                // Quadratic bias: squaring t pushes the value closer to 1 (i.e., toward the end).
                var biasedT = t * t;
                // Calculate the distance along the path from turnStartPos
                var distanceFromStart = safeZoneDistance + biasedT * usablePathLength;

                // Since the trap wall should be centered at the current lane (where the transform is),
                // we do not apply any horizontal offset.
                var trapPosition = turnStartPos + transform.forward * distanceFromStart;

                // Instantiate the trap wall at trapPosition
                var trapWall = Instantiate(trapWallPrefab, trapPosition + Vector3.up, wallRotation);
                spawnedTraps.Add(trapWall);
            }
        }

        private void SpawnFires(PathInfo pathInfo)
        {
            // Ensure the path segment isn't empty and apply the fire spawning logic
            if (pathInfo.PathLength <= 0) return;

            var usablePathLength =
                pathInfo.PathLength - 2 * safeZoneDistance; // Excluding both head and tail safe zones
            if (usablePathLength <= 0) return;

            // Calculate the right vector (perpendicular to forward)
            Vector3 rightVector;

            // Simplified right vector calculation
            if (Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.z))
                rightVector = new Vector3(-transform.forward.z, 0, transform.forward.x).normalized;
            else
                rightVector = new Vector3(transform.forward.z, 0, -transform.forward.x).normalized;

            // Set rotation based on direction
            var fireRotation = Quaternion.identity;
            if (Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.z))
                fireRotation = Quaternion.Euler(0, 90, 0);

            // Generate fires with more varied positions along the path
            for (var i = 0; i < maxFires; i++)
            {
                // More linear distribution for fires (less biased than walls)
                var t = (i + 0.5f) / maxFires;
                var distanceFromStart = safeZoneDistance + t * usablePathLength;

                // Randomly choose left, middle, or right lane
                // 0 = left, 1 = middle, 2 = right
                var laneChoice = Random.Range(0, 3);
                float laneMultiplier;

                switch (laneChoice)
                {
                    case 0: // Left
                        laneMultiplier = -1f;
                        break;
                    case 1: // Middle
                        laneMultiplier = 0f;
                        break;
                    case 2: // Right
                        laneMultiplier = 1f;
                        break;
                    default:
                        laneMultiplier = 0f;
                        break;
                }

                var laneOffset = rightVector * (laneWidth * laneMultiplier);

                // Calculate fire position (offset to selected lane)
                var firePosition = turnStartPos + transform.forward * distanceFromStart + laneOffset;

                // Instantiate the fire at firePosition with proper rotation
                var fire = Instantiate(firePrefab, firePosition + Vector3.up * 0.5f, fireRotation);
                spawnedTraps.Add(fire);
            }
        }


        private void OnPlayerTurned()
        {
            // When the player completes a turn, record the start of the turn
            turnStartPos = transform.position;

            // Start coroutine for delayed obstacle spawning
            StartCoroutine(DelayedObstacleSpawn());
        }

        private IEnumerator DelayedObstacleSpawn()
        {
            // Get path information using PathDetector (do this once)
            var pathInfo = pathDetector.DetectPath(turnStartPos, transform.forward);
            

            // Wait for specified number of frames before spawning walls
            for (var i = 0; i < frameDelay; i++) yield return new WaitForEndOfFrame();

            // Spawn the walls and fire after the delay
            SpawnFires(pathInfo);
            SpawnObsWalls(pathInfo);
        }

        // Clear all spawned obstacles (for testing)
        public void ClearTraps()
        {
            foreach (var obstacle in spawnedTraps)
                if (obstacle != null)
                    Destroy(obstacle);

            spawnedTraps.Clear();
        }
    }
}