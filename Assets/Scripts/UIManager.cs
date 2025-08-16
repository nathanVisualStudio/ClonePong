using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Éléments d'UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Panneaux")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Boutons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;
    
    private GameManager gameManager;
    private bool isPaused = false;

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager non trouvé. Assurez-vous qu'il existe dans la scène.");
            return;
        }
        
        // S'abonner aux événements du GameManager
        SubscribeToGameEvents();
        
        // Initialiser les écrans
        InitializePanels();
        
        // Configurer les boutons
        SetupButtons();
        
        // Initialiser l'UI
        Invoke("UpdateUI", 0.1f); // Appel initial pour éviter les problèmes de timing
    }
    
    private void SubscribeToGameEvents()
    {
        gameManager.OnScoreChanged += UpdateScore;
        gameManager.OnLifeLost += UpdateLives;
        gameManager.OnGameOver += ShowGameOver;
        gameManager.OnLevelComplete += ShowLevelComplete;
        gameManager.OnGameStart += UpdateLevel; // Ajout d'un événement pour mettre à jour le niveau
    }
    
    private void InitializePanels()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }
    
    private void SetupButtons()
    {
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (resumeButton) resumeButton.onClick.AddListener(TogglePause);
    }
    
    private void UpdateUI()
    {
        UpdateScore();
        UpdateLives();
        UpdateTime();
        UpdateLevel(); // Mise à jour du texte du niveau
    }
    
    private void Update()
    {
        UpdateTime();
        
        // Vérifier la touche de pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    
    private void UpdateScore()
    {
        if (scoreText)
        {
            scoreText.text = "Score: " + gameManager.GetScore().ToString("D6");
        }
    }
    
    private void UpdateLives()
    {
        if (livesText)
        {
            livesText.text = "Vies: " + gameManager.GetLives().ToString();
        }
    }
    
    private void UpdateTime()
    {
        if (timeText && gameManager.IsGameRunning())
        {
            float time = gameManager.GetRemainingTime();
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = string.Format("Temps: {0:00}:{1:00}", minutes, seconds);
        }
    }
    
    private void UpdateLevel()
    {
        if (levelText)
        {
            levelText.text = "Niveau: " + gameManager.GetLevel();
        }
    }
    
    private void ShowGameOver()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    private void ShowLevelComplete()
    {
        if (levelCompletePanel)
        {
            levelCompletePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Masque le panel de fin de niveau
    /// </summary>
    public void HideLevelCompletePanel()
    {
        if (levelCompletePanel)
        {
            levelCompletePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Masque le panel de game over
    /// </summary>
    public void HideGameOverPanel()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pausePanel)
        {
            pausePanel.SetActive(isPaused);
        }
        
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    private void RestartGame()
    {
        // S'assurer que le jeu n'est pas en pause
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
            
            // Masquer le panneau de pause
            if (pausePanel) pausePanel.SetActive(false);
        }
        
        // Masquer les panneaux de game over et de niveau terminé
        HideGameOverPanel();
        HideLevelCompletePanel();
        
        // Appeler la fonction de redémarrage du GameManager
        gameManager.RestartGame();
        
        // Mettre à jour l'interface utilisateur
        Invoke("UpdateUI", 0.5f);
    }
    
    private void OnDestroy()
    {
        // Se désabonner des événements pour éviter les fuites de mémoire
        if (gameManager != null)
        {
            gameManager.OnScoreChanged -= UpdateScore;
            gameManager.OnLifeLost -= UpdateLives;
            gameManager.OnGameOver -= ShowGameOver;
            gameManager.OnLevelComplete -= ShowLevelComplete;
            gameManager.OnGameStart -= UpdateLevel; // Se désabonner de l'événement de démarrage
        }
        
        // Réinitialiser le timeScale en cas de destruction
        Time.timeScale = 1f;
    }
    public void GoBackToMenu()
    {
        // Charger la scène du menu principal
        UnityEngine.SceneManagement.SceneManager.LoadScene("PangMenu");  
    } 
}
