using UnityEngine;

namespace Player
{
    public class PlayerTurnPoint : MonoBehaviour
    {
        public enum TurnDirection { Left, Right }
    
        public TurnDirection direction = TurnDirection.Right;
        public float turnAngle = 90f; // How many degrees to turn
        public float turnRadius = 10f; // Size of the turning arc
        public float turnSpeed = 1f; // How quickly to execute the turn (multiplier)
    
        // Visual for editor only
        private void OnDrawGizmos()
        {
            Gizmos.color = direction == TurnDirection.Left ? Color.blue : Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
        
            // Draw an arrow indicating turn direction
            Vector3 forward = transform.forward * 2;
            Vector3 turnDir = direction == TurnDirection.Left ? -transform.right : transform.right;
            Vector3 arrowEnd = transform.position + turnDir * 2;
        
            Gizmos.DrawLine(transform.position, arrowEnd);
            // Draw arrowhead
            Gizmos.DrawLine(arrowEnd, arrowEnd - turnDir + transform.forward);
            Gizmos.DrawLine(arrowEnd, arrowEnd - turnDir - transform.forward);
        }
    }
}
