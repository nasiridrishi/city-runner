using System.Collections.Generic;
using Player;
using UnityEngine;
using world;

namespace obstacle.type
{
    public class ObstacleFire : MonoBehaviour
    {
        [SerializeField] private GameObject[] obstaclePrefabs; // Array of obstacle prefabs
        [SerializeField] private PathDetector pathDetector;
        
        private Vector3 turnStartPos;
        private float laneWidth = 3f;
        private float spawnChance = 0.3f; // Lower chance than coins
        
        private List<GameObject> spawnedObstacles = new List<GameObject>();
        private bool listeningForTurns = false;
        
        private void Start()
        {
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
            if (!listeningForTurns)
            {
                GetComponent<PlayerTurnHandler>().OnPlayerTurnComplete += onPlayerTurned;
                listeningForTurns = true;
            }
        }
        
        private void SpawnObstacles()
        {
            // Use the same PathDetector to get path information
            PathInfo pathInfo = pathDetector.DetectPath(turnStartPos, transform.forward);
            
            // Use larger segments for obstacles to make them more sparse
            float[] segments = PathDetector.DividePathIntoSegments(pathInfo.PathLength, 15f, 30f);
            
            // Spawn obstacles for each segment based on spawn chance
            float currentPosition = 0f;
            
            foreach (float segmentLength in segments)
            {
                // Check if we should spawn an obstacle in this segment
                if (Random.value < spawnChance)
                {
                    // Choose a random lane (-1, 0, 1)
                    int lane = Random.Range(-1, 2);
                    
                    // Choose a random position within the segment
                    float posInSegment = Random.Range(0, segmentLength);
                    
                    // Choose a random obstacle prefab
                    GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                    
                    // Calculate position
                    Vector3 obstaclePosition = turnStartPos + 
                                            transform.forward * (currentPosition + posInSegment) + 
                                            transform.right * (lane * laneWidth);
                    
                    // Instantiate obstacle
                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    
                    // Add to tracking list
                    spawnedObstacles.Add(obstacle);
                }
                
                // Move to next segment
                currentPosition += segmentLength;
            }
        }
        
        private void onPlayerTurned()
        {
            turnStartPos = transform.position;
            SpawnObstacles();
        }
        
        public void ClearObstacles()
        {
            foreach (GameObject obstacle in spawnedObstacles)
            {
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }
            
            spawnedObstacles.Clear();
        }
    }
}
