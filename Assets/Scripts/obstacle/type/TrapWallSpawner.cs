using Player;
using System.Collections.Generic;
using UnityEngine;
using world;

namespace trap
{
    public class TrapWallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject trapWallPrefab; // Trap wall prefab
        [SerializeField] private PathDetector pathDetector; // Path detector
        [SerializeField] private float laneWidth = 3f; // Lane width
        [SerializeField] private int maxTraps = 5; // Number of trap walls generated per turn
        [SerializeField] private float safeZoneDistance = 10f; // Safe zone distance, head and tail reserved range
        private Vector3 turnStartPos; // Record the starting point of the turn
        private bool listeningForTurns = false; // Avoid duplicate subscription events

        private List<GameObject> spawnedTraps = new List<GameObject>(); // Track the generated trap walls

        private void Start()
        {
            // Make sure the path detector exists
            if (pathDetector == null)
            {
                pathDetector = GetComponent<PathDetector>();
                if (pathDetector == null)
                {
                    pathDetector = gameObject.AddComponent<PathDetector>();
                }
            }
        }

        private void Update()
        {
            // Listen for player turn events (only subscribe once)
            if (!listeningForTurns)
            {
                GetComponent<PlayerTurnHandler>().OnPlayerTurnComplete += OnPlayerTurned;
                listeningForTurns = true;
            }
        }

        private void SpawnTrapWalls()
        {
            // Get path information using PathDetector
            PathInfo pathInfo = pathDetector.DetectPath(turnStartPos, transform.forward);

            // Make sure the path segment is not empty and set the trap spawning logic
            if (pathInfo.PathLength > 0)
            {
                float currentPosition = safeZoneDistance; // Start spawning outside the safe zone
                float usablePathLength = pathInfo.PathLength - 2 * safeZoneDistance; // Excluding the length of the head and tail safe zones

                if (usablePathLength <= 0)
                {
                    Debug.LogWarning("The path is too short to generate a trap wall! ");
                    return;
                }

                // Limit the generation of maxTraps trap walls at a time
                for (int i = 0; i < maxTraps; i++)
                {
                    // Make sure the generated position is within the safe path range
                    float segmentLength = Random.Range(8f, 12f); // The distance of each trap
                    if (currentPosition + segmentLength > safeZoneDistance + usablePathLength)
                    {
                        break; // If the path range is exceeded, stop generating
                    }

                    // Randomly select a lane (-1, 0, 1)
                    int lane = Random.Range(-1, 2);
                    Vector3 trapPosition = turnStartPos + transform.forward * currentPosition
                    + transform.right * (lane * laneWidth);

                    // Generate a trap wall and record it
                    GameObject trapWall = Instantiate(trapWallPrefab, trapPosition + Vector3.up, Quaternion.identity);
                    spawnedTraps.Add(trapWall);

                    // Advance to next segment
                    currentPosition += segmentLength;
                }
            }
        }

        private void OnPlayerTurned()
        {
            // When the player completes a turn
            turnStartPos = transform.position; // Record the start of the turn
            SpawnTrapWalls(); // Call the method that spawns the trap wall
        }

        // Optional: Clear the spawned trap wall (for testing)
        public void ClearTraps()
        {
            foreach (GameObject trap in spawnedTraps)
            {
                if (trap != null)
                {
                    Destroy(trap);
                }
            }
            spawnedTraps.Clear();
        }
    }
}