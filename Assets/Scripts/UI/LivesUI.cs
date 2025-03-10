using Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    [RequireComponent(typeof(PlayerLifeManager))]
    public class LivesUI : MonoBehaviour
    {
        [SerializeField] private Image[] lifeIcons; // Array of life icon images
        [SerializeField] private Sprite fullLifeSprite;
        [SerializeField] private Sprite emptyLifeSprite;
        
        private PlayerLifeManager lifeManager;
        
        private void Start()
        {
            // Find player and get life manager
            lifeManager = FindObjectOfType<Player.PlayerLifeManager>();
            
            if (lifeManager != null)
            {
                // Subscribe to events
                lifeManager.OnLifeLost += UpdateLivesDisplay;
                lifeManager.OnRespawn += UpdateLivesDisplay;
            }
            else
            {
                Debug.LogError("LivesUI: Could not find PlayerLifeManager in the scene!");
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (lifeManager != null)
            {
                lifeManager.OnLifeLost -= UpdateLivesDisplay;
                lifeManager.OnRespawn -= UpdateLivesDisplay;
            }
        }
        
        private void UpdateLivesDisplay()
        {
            Debug.Log(">>>>>>>> UpdateLivesDisplay");
            int currentLives = lifeManager.CurrentLives;
            Debug.Log("Current Lives: " + currentLives);
            // Update life icons if available
            if (lifeIcons != null && lifeIcons.Length > 0)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    if (lifeIcons[i] != null)
                    {
                        if (i < currentLives)
                        {
                            lifeIcons[i].sprite = fullLifeSprite;
                            lifeIcons[i].color = Color.white;
                        }
                        else
                        {
                            if (emptyLifeSprite != null)
                                lifeIcons[i].sprite = emptyLifeSprite;
                            else
                                lifeIcons[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dimmed
                        }
                    }
                }
            }
        }
    }
}
