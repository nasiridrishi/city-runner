using System.Collections.Generic;
using Player;
using UnityEngine;
using world;

namespace coin
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject coinPrefab; // Add reference to your coin prefab
        [SerializeField] private PathDetector pathDetector; // Reference to path detector
        
        private Vector3 turnStartPos;
        private float laneWidth = 3f;
        private float spawnChance = 0.5f;
        
        // List to track spawned coins for cleanup if needed
        private List<GameObject> spawnedCoins = new List<GameObject>();
        private bool listeningForTurns = false;
        
        private void Start()
        {
            // If pathDetector isn't set in inspector, try to find or add one
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
        
        private void SpawnCoin()
        {
            // Use PathDetector to get path information
            PathInfo pathInfo = pathDetector.DetectPath(turnStartPos, transform.forward);
            
            // Get path segments
            float[] segments = PathDetector.DividePathIntoSegments(pathInfo.PathLength);
            
            // Spawn coins for each segment based on spawn chance
            float currentPosition = 0f;
            
            foreach (float segmentLength in segments)
            {
                // Check if we should spawn a coin in this segment
                if (Random.value < spawnChance)
                {
                    // Choose a random lane (-1, 0, 1)
                    int lane = Random.Range(-1, 2); // Range is inclusive of min, exclusive of max
                    
                    for (int i = 0; i < segmentLength; i++)
                    {
                        Vector3 coinPosition = turnStartPos + transform.forward * (currentPosition + i) 
                                             + transform.right * (lane * laneWidth);
                        
                        // Instantiate coin at calculated position
                        GameObject coin = Instantiate(coinPrefab, coinPosition + Vector3.up, Quaternion.Euler(90, 0, 0));
                        
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
            foreach (GameObject coin in spawnedCoins)
            {
                if (coin != null)
                {
                    Destroy(coin);
                }
            }
            
            spawnedCoins.Clear();
        }
        public List<GameObject> GetSpawnedCoins()
        {
            return spawnedCoins;
        }
    }
}
