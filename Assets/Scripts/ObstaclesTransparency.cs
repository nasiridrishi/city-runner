using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesTransparency : MonoBehaviour
{
    public Transform player;       // player character position
    public LayerMask obstacleLayer; // layer to detect
    private Dictionary<Renderer, (Material material, Shader shader)> originalMaterials = new Dictionary<Renderer, (Material, Shader)>();

    void Update()
    {
        if (player == null) return;  // to ensure player is assigned

        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 playerPosition = player.position;
        Vector3 direction = (playerPosition - cameraPosition).normalized;
        float distance = Vector3.Distance(cameraPosition, playerPosition);

        RaycastHit[] hits = Physics.RaycastAll(cameraPosition, direction, distance, obstacleLayer);
        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend)
            {
                hitRenderers.Add(rend);

                // Record the original shader of the obstacles
                if (!originalMaterials.ContainsKey(rend))
                {
                    originalMaterials[rend] = (rend.material, rend.material.shader);
                }

                // Temporary change the material shader to standard to achieve transparency effect
                rend.material.shader = Shader.Find("Standard");

                // Adjust the transpapency of the material
                Color newColor = rend.material.color;
                newColor.a = 0.3f;  // 30% transparent
                rend.material.color = newColor;
                rend.material.SetFloat("_Mode", 3); // adjust the rendering mode
                rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                rend.material.SetInt("_ZWrite", 0);
                rend.material.DisableKeyword("_ALPHATEST_ON");
                rend.material.EnableKeyword("_ALPHABLEND_ON");
                rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                rend.material.renderQueue = 3000;
            }
        }

        // After the player leave the obstacles restore the original shader
        foreach (var rend in new List<Renderer>(originalMaterials.Keys))
        {
            if (!hitRenderers.Contains(rend))
            {
                if (originalMaterials.ContainsKey(rend))
                {
                    // Restore the original shader
                    rend.material.shader = originalMaterials[rend].shader;
                    rend.material = originalMaterials[rend].material;
                }
                originalMaterials.Remove(rend);
            }
        }
    }
}
