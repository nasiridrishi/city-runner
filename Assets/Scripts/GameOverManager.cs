using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Animation")]
    [SerializeField] private float fadeInTime = 1.0f;
    [SerializeField] private Animator panelAnimator;
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;
    
    private void Start()
    {
        // Set up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        // Get final score from PlayerPrefs if available
        if (scoreText != null)
        {
            int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
            scoreText.text = $"Final Score: {finalScore}";
        }
        
        // Play game over sound
        if (gameOverSound != null && SoundManager.instance != null)
        {
            //todo SoundManager.instance.PlayOneShot(gameOverSound);
        }
        
        // Start fade-in animation
        StartCoroutine(AnimateGameOver());
    }
    
    private IEnumerator AnimateGameOver()
    {
        float elapsedTime = 0f;
        // If we have an animator, trigger the animation
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("FadeIn");
        }
        // Otherwise, do a simple fade in
        else if (gameOverPanel != null)
        {
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
                
            canvasGroup.alpha = 0f;
            
            while (elapsedTime < fadeInTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        
        // Animate the Game Over text if available
        if (gameOverText != null)
        {
            gameOverText.transform.localScale = Vector3.zero;
            
            elapsedTime = 0f;
            while (elapsedTime < fadeInTime * 0.5f)
            {
                float scale = Mathf.Lerp(0f, 1.2f, elapsedTime / (fadeInTime * 0.5f));
                gameOverText.transform.localScale = new Vector3(scale, scale, scale);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0f;
            while (elapsedTime < fadeInTime * 0.5f)
            {
                float scale = Mathf.Lerp(1.2f, 1f, elapsedTime / (fadeInTime * 0.5f));
                gameOverText.transform.localScale = new Vector3(scale, scale, scale);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            gameOverText.transform.localScale = Vector3.one;
        }
    }
    
    public void RestartGame()
    {
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }
    
    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
