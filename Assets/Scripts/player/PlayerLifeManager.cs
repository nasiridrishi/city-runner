using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    [RequireComponent(typeof(PlayerTurnHandler))]
    public class PlayerLifeManager : MonoBehaviour
    {
        [Header("Life Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private float gameOverDelay = 3f;
        
        private string gameOverSceneName = "GameOverScene";
        
        [Header("UI References")]
        [SerializeField] private GameObject livesDisplay; // Optional UI to show lives

        private int currentLives;
        private Vector3 respawnPosition;
        private Quaternion respawnRotation;
        private Player playerController;
        private PlayerTurnHandler turnHandler;
        
        // Events
        public event Action OnLifeLost;
        public event Action OnGameOver;
        public event Action OnRespawn;
        
        public int CurrentLives => currentLives;

        private void Start()
        {
            // Initialize
            currentLives = maxLives;
            Debug.Log("...Current lives:" + currentLives);
            playerController = GetComponent<Player>();
            turnHandler = GetComponent<PlayerTurnHandler>();
            
            // Set initial respawn point
            SaveRespawnPoint();
            
            // Subscribe to turn events
            if (turnHandler != null)
            {
                turnHandler.OnPlayerTurnComplete += OnPlayerTurnComplete;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (turnHandler != null)
            {
                turnHandler.OnPlayerTurnComplete -= OnPlayerTurnComplete;
            }
        }
        
        // Event handler for when player completes a turn
        private void OnPlayerTurnComplete()
        {
            // Save the position after a turn as a respawn point
            SaveRespawnPoint();
            Debug.Log($"Respawn point saved at {respawnPosition}");
        }
        
        // Call this to save the current position as a respawn point
        public void SaveRespawnPoint()
        {
            respawnPosition = transform.position;
            respawnRotation = transform.rotation;
        }
        
        public void HandlePlayerDeath()
        {
            if (playerController.IsDead)
            {
                currentLives--;
                OnLifeLost?.Invoke();
                
                Debug.Log($"Player died. Lives remaining: {currentLives}");
                
                if (currentLives > 0)
                {
                    // Player has remaining lives, respawn after delay
                    StartCoroutine(RespawnAfterDelay());
                }
                else
                {
                    // Game over
                    StartCoroutine(TransitionToGameOver());
                }
            }
        }
        
        private IEnumerator RespawnAfterDelay()
        {
            // Wait for death animation to play
            yield return new WaitForSeconds(respawnDelay);
            
            // Reset player state
            playerController.ResetPlayerState(respawnPosition, respawnRotation);
            
            Debug.Log($"Player respawned at {respawnPosition}");
            OnRespawn?.Invoke();
        }
        
        private IEnumerator TransitionToGameOver()
        {
            // Save any necessary game data to PlayerPrefs
            int currentScore = ScoreManager.instance != null ? ScoreManager.instance.CurrentScore : 0;
            PlayerPrefs.SetInt("FinalScore", currentScore);
            PlayerPrefs.Save();
            
            // Notify any listeners that the game is over
            OnGameOver?.Invoke();
            
            // Wait for death animation and a bit more for dramatic effect
            yield return new WaitForSeconds(gameOverDelay);
            
            // Load the Game Over scene
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
