using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace obstacle.type
{
    public class ObstacleWall : MonoBehaviour
    {
        public GameObject[] wallBricks;
        private List<Rigidbody> wallBrickRb = new List<Rigidbody>();
        private Collider col;

        [Header("Physics Settings")] 
        public float explosionForce = 20f;
        public float upwardsModifier = 0.01f;
        public float randomizationFactor = 5f;
        
        [Range(0.1f, 1.0f)]
        public float affectedRadius = 0.5f;
        
        [Header("Collision Settings")]
        public float collisionDisableDelay = 1.5f;    // Time before bricks stop colliding with player
        public string playerLayerName = "Player";     // Name of your player layer
        public string debrisLayerName = "Debris";     // Create this layer in Unity Editor!
        
        [Header("Cleanup Settings")]
        public float fadeOutDelay = 3f;               // Time before bricks start fading
        public float fadeOutDuration = 2f;            // How long the fade takes
        
        // Cache the player layer number
        private int playerLayer;

        private void Start()
        {
            col = GetComponent<Collider>();
            playerLayer = LayerMask.NameToLayer(playerLayerName);

            foreach (var wallBrick in wallBricks)
            {
                var collider = wallBrick.GetComponent<BoxCollider>();
                collider.isTrigger = false;

                var rb = wallBrick.GetComponent<Rigidbody>();
                rb.mass = 1f;
                wallBrickRb.Add(rb);
            }
        }

        public bool IsIntact()
        {
            return col != null && col.enabled;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("Player") && IsIntact())
            {
                var playerMovement = hit.gameObject.GetComponent<Player.Player>();
                if (playerMovement != null && !playerMovement.IsDead)
                {
                    playerMovement.IsDead = true;

                    var playerAnimator = hit.gameObject.GetComponent<Animator>();
                    if (playerAnimator != null)
                        playerAnimator.SetTrigger("Death");
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Spell"))
            {
                col.enabled = false;

                var impactPoint = other.contacts[0].point;
                float maxWallDimension = CalculateMaxWallDimension();
                float effectiveRadius = maxWallDimension * affectedRadius;
                var forceDirection = other.gameObject.transform.forward;
                int affectedBricks = 0;

                foreach (var rb in wallBrickRb)
                {
                    var brickDirection = rb.position - impactPoint;
                    var distanceFromImpact = brickDirection.magnitude;
                    
                    if (distanceFromImpact <= effectiveRadius)
                    {
                        rb.isKinematic = false;
                        affectedBricks++;
                        
                        float distanceFactor = Mathf.Clamp01(1f - (distanceFromImpact / effectiveRadius));
                        
                        if (distanceFromImpact > 0.1f)
                            brickDirection = brickDirection.normalized;

                        var finalDirection = (forceDirection * 0.8f + brickDirection * 0.2f).normalized;
                        finalDirection += new Vector3(
                            Random.Range(-0.05f, 0.05f),
                            Random.Range(0f, 0.1f),
                            Random.Range(-0.05f, 0.05f)
                        );

                        rb.AddForce(
                            finalDirection * explosionForce * distanceFactor + 
                            Vector3.up * upwardsModifier * explosionForce,
                            ForceMode.Impulse
                        );

                        rb.AddTorque(Random.insideUnitSphere * randomizationFactor * distanceFactor, ForceMode.Impulse);
                        
                        // Start the process to handle the brick collision and fading
                        StartCoroutine(HandleBrickAfterCollision(rb.gameObject));
                    }
                }
                
                DeactivateSpell(other.gameObject);
                Destroy(gameObject, fadeOutDelay + fadeOutDuration + 1f);
            }
        }

        // coroutine to handle brick behavior after collision
        private IEnumerator HandleBrickAfterCollision(GameObject brick)
        {
            // Wait for the brick to settle a bit
            yield return new WaitForSeconds(collisionDisableDelay);
            
            // To make sure it does not collide with player
            var brickCollider = brick.GetComponent<Collider>();
            if (brickCollider != null)
            {
                brickCollider.isTrigger = true;
            }
            
            // Wait additional time before starting to fade out
            yield return new WaitForSeconds(fadeOutDelay - collisionDisableDelay);
            
            // Get all renderers (in case brick has multiple parts)
            var renderers = brick.GetComponentsInChildren<Renderer>();
            float fadeStartTime = Time.time;
            
            // Fade out all materials over time
            while (Time.time < fadeStartTime + fadeOutDuration)
            {
                float normalizedTime = (Time.time - fadeStartTime) / fadeOutDuration;
                float alpha = 1f - normalizedTime;
                
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        // Enable transparency on the material
                        material.SetFloat("_Mode", 2); // Fade mode
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        
                        // Set the alpha
                        Color color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }
                }
                
                yield return null;
            }
            
            // Destroy the brick after fading
            Destroy(brick);
        }
        
        private float CalculateMaxWallDimension()
        {
            if (wallBricks.Length == 0)
                return 1f;
                
            Vector3 min = wallBricks[0].transform.position;
            Vector3 max = wallBricks[0].transform.position;
            
            foreach (var brick in wallBricks)
            {
                min = Vector3.Min(min, brick.transform.position);
                max = Vector3.Max(max, brick.transform.position);
            }
            
            Vector3 size = max - min;
            return Mathf.Max(size.x, size.y, size.z);
        }
        
        private void DeactivateSpell(GameObject spellObject)
        {
            PlayerController playerController = null;
            var parent = spellObject.transform.parent;
            if (parent != null) playerController = parent.GetComponent<PlayerController>();

            if (playerController == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) playerController = player.GetComponent<PlayerController>();
            }

            if (playerController != null)
                playerController.destroyMagic();
            else
                spellObject.SetActive(false);
        }
    }
}
