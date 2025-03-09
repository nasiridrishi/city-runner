using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int pointsPerCoin = 10;
    [SerializeField] private int pointsPerMeter = 1;
    
    private int currentScore = 0;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;
    
    public int CurrentScore => currentScore;
    
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
        UpdateScoreDisplay();
    }
    
    private void Update()
    {
        // Track distance traveled
        Player.Player player = FindObjectOfType<Player.Player>();
        if (player != null && !player.IsDead)
        {
            float distance = Vector3.Distance(lastPosition, player.transform.position);
            distanceTraveled += distance;
            
            // Add points for every meter traveled
            if (distanceTraveled >= 1f)
            {
                AddScore(Mathf.FloorToInt(distanceTraveled) * pointsPerMeter);
                distanceTraveled %= 1f; // Keep only the remainder
            }
            
            lastPosition = player.transform.position;
        }
    }
    
    public void AddCoinScore()
    {
        AddScore(pointsPerCoin);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
}