using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class LivesUI : MonoBehaviour
    {
        [SerializeField] private Image[] lifeIcons; // Array of life icon images
        [SerializeField] private TextMeshProUGUI livesText; // Optional text display
        [SerializeField] private Sprite fullLifeSprite;
        [SerializeField] private Sprite emptyLifeSprite;
        
        private Player.PlayerLifeManager lifeManager;
    
        private void Start()
        {
              // Find player and get life manager
            lifeManager = FindObjectOfType<Player.PlayerLifeManager>();

            if (lifeManager != null)
            {
                // Subscribe to events
                lifeManager.OnLifeLost += UpdateLivesDisplay;
                lifeManager.OnRespawn += UpdateLivesDisplay;
                
                // Initial update
                Invoke(nameof(UpdateLivesDisplay), 0.1f);
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
            // Update life icons if available
            if (lifeIcons != null && lifeIcons.Length > 0)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    if (lifeIcons[i] != null)
                    {
                        if (i < lifeManager.CurrentLives)
                        {
                            lifeIcons[i].sprite = fullLifeSprite;
                            lifeIcons[i].color = Color.white;
                        }
                        else
                        {
                            if (emptyLifeSprite != null)
                                lifeIcons[i].sprite = emptyLifeSprite;
                            lifeIcons[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dimmed
                        }
                    }
                }
            }
            
            // Update text if available
            if (livesText != null)
            {
                livesText.text = $"Lives: {lifeManager.CurrentLives}";
            }
        }
    }
}
