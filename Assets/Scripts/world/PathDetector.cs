using System.Collections.Generic;
using UnityEngine;

namespace world
{
    public class PathDetector : MonoBehaviour
    {
        [SerializeField] private float maxRaycastDistance = 500f; // Maximum distance to check

        // Method to detect path from a position in a given direction
        public PathInfo DetectPath(Vector3 startPosition, Vector3 direction)
        {
            PathInfo pathInfo = new PathInfo();
            RaycastHit hit;
            
            // Initialize path info
            pathInfo.StartPosition = startPosition;
            pathInfo.Direction = direction;
            
            // Perform raycast
            if (Physics.Raycast(startPosition, direction, out hit, maxRaycastDistance))
            {
                pathInfo.EndPosition = hit.point;
                pathInfo.PathLength = hit.distance;
                pathInfo.HitObject = hit.collider.gameObject;
                pathInfo.HitTurnPoint = hit.collider.CompareTag("TurnPoint");
            }
            else
            {
                // If no hit, use maximum distance
                pathInfo.PathLength = maxRaycastDistance;
                pathInfo.EndPosition = startPosition + (direction * maxRaycastDistance);
            }
            
            return pathInfo;
        }
        
        // Helper method to divide path into segments
        public static float[] DividePathIntoSegments(float pathLength, float minSegmentSize = 8f, float maxSegmentSize = 12f)
        {
            // Split the path into smaller segments with random lengths
            List<float> segments = new List<float>();
            float currentPosition = 0f;
            
            while (currentPosition < pathLength)
            {
                float segmentLength = Random.Range(minSegmentSize, maxSegmentSize);
                
                // Make sure we don't exceed the path length
                if (currentPosition + segmentLength > pathLength)
                {
                    segmentLength = pathLength - currentPosition;
                }
                
                segments.Add(segmentLength);
                currentPosition += segmentLength;
            }
            
            return segments.ToArray();
        }
    }
    
    // Data structure to hold path detection results
    public class PathInfo
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float PathLength { get; set; }
        public GameObject HitObject { get; set; }
        public bool HitTurnPoint { get; set; }
    }
}
