// using System.Collections.Generic;
// using UnityEngine;
//
// namespace World
// {
//     public abstract class WorldObjectSpawner : MonoBehaviour
//     {
//         [Header("Spawn Settings")]
//         [SerializeField] protected GameObject prefab;
//         [SerializeField] protected float laneWidth = 3f;
//         [SerializeField] protected float spawnChance = 0.5f;
//         
//         [Header("Path Detection")]
//         [SerializeField] protected PathDetector pathDetector;
//         
//         protected Vector3 spawnStartPos;
//         protected List<GameObject> spawnedObjects = new List<GameObject>();
//         
//         protected virtual void Awake()
//         {
//             // Ensure there's a PathDetector component
//             if (pathDetector == null)
//             {
//                 pathDetector = GetComponent<PathDetector>();
//                 if (pathDetector == null)
//                 {
//                     pathDetector = gameObject.AddComponent<PathDetector>();
//                     Debug.LogWarning($"PathDetector not assigned to {GetType().Name}, creating one automatically");
//                 }
//             }
//         }
//         
//         // To be implemented by child classes
//         public abstract void SpawnObjects(Vector3 startPos, Vector3 direction);
//         
//         // Clear spawned objects
//         public virtual void ClearObjects()
//         {
//             foreach (var obj in spawnedObjects)
//             {
//                 if (obj != null)
//                     Destroy(obj);
//             }
//             spawnedObjects.Clear();
//         }
//     }
// }