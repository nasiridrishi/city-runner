using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText; // New field for high score display
    
    [Header("Score Settings")]
    [SerializeField] private int pointsPerCoin = 10;
    [SerializeField] private int pointsPerMeter = 1;
    [SerializeField] private string highScoreKey = "HighScore"; // PlayerPrefs key
    
    private int currentScore = 0;
    private int highScore = 0;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;
    
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Initialize position tracking
        lastPosition = FindObjectOfType<Player.Player>().transform.position;
        
        // Load the high score
        LoadHighScore();
        
        // Update UI
        UpdateScoreDisplay();
    }
    
    public void AddCoinScore()
    {
        AddScore(pointsPerCoin);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        UpdateScoreDisplay();
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(highScoreKey, 0);
    }
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(highScoreKey, highScore);
        PlayerPrefs.Save(); // Force immediate save
    }
    
    public void ResetCurrentScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
        
        if (highScoreText != null)
        {
            highScoreText.text = $"Highest: {highScore}";
        }
    }
    
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
        UpdateScoreDisplay();
    }
    
    public void GameOver()
    {
        // Save high score one last time in case it wasn't saved during gameplay
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
    }
}
