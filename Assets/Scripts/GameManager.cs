using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Configurer les layers de collision dès le démarrage du jeu
            SetupCollisionLayers();
        }
        else
        {
            Destroy(gameObject);
        }

        string actualSceneName = SceneManager.GetActiveScene().name;
        if (actualSceneName == "PangMenu")
        {
            Debug.LogWarning("Le GameManager ne doit pas être présent dans cette scène. Veuillez le supprimer.");
        }
    }
    #endregion

    private const string BALL_LAYER_NAME = "Balls";
    private const int BALL_LAYER = 8; // Layer 8 est généralement le premier layer personnalisable dans Unity
    private const int MAX_LEVEL = 4; // Nombre total de niveaux dans le jeu

    [Header("Niveau")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float levelTime = 120f;
    [SerializeField] private int initialBallCount = 1;

    [Header("Joueur")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int lives = 3;

    [Header("Balles")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private List<Color> ballColors = new List<Color>();

    // État du jeu
    private bool isGameRunning = false;
    private int score = 0;
    private float remainingTime;
    private int remainingLives;

    // Événements du jeu
    public delegate void GameEvent();
    public event GameEvent OnGameStart;
    public event GameEvent OnGameOver;
    public event GameEvent OnLevelComplete;
    public event GameEvent OnScoreChanged;
    public event GameEvent OnLifeLost;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameRunning)
        {
            UpdateTimer();
            CheckLevelCompletion();
        }
    }

    private void InitializeGame()
    {
        score = 0;
        remainingLives = lives;
        remainingTime = levelTime;
        isGameRunning = true;
        
        SpawnPlayer();
        SpawnInitialBalls();
        
        OnGameStart?.Invoke();
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            GameOver();
        }
    }

    private void CheckLevelCompletion()
    {
        // Vérifier s'il reste des balles dans le niveau
        Ball[] balls = FindObjectsOfType<Ball>();
        
        if (balls.Length == 0)
        {
            LevelComplete();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(0, -4f, 0);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

    private void SpawnInitialBalls()
    {
        for (int i = 0; i < initialBallCount; i++)
        {
            SpawnBall(3, new Vector3(-3f + i * 6f, 2f, 0));
        }
    }

    #region Gestion des balles

    /// <summary>
    /// Crée une nouvelle balle du type spécifié à la position donnée
    /// </summary>
    /// <param name="size">Taille de la balle (3 = grande, 1 = petite)</param>
    /// <param name="position">Position où créer la balle</param>
    /// <returns>La référence au GameObject de la balle créée</returns>
    public GameObject SpawnBall(int size, Vector3 position)
    {
        GameObject ballObject = Instantiate(ballPrefab, position, Quaternion.identity);
        Ball ball = ballObject.GetComponent<Ball>();
        
        if (ball != null)
        {
            ball.Initialize(size, GetRandomBallColor());
        }
        
        return ballObject;
    }

    private Color GetRandomBallColor()
    {
        if (ballColors.Count > 0)
        {
            int randomIndex = Random.Range(0, ballColors.Count);
            return ballColors[randomIndex];
        }
        
        return Color.white;
    }

    #endregion

    #region Gestion du score et du joueur

    public void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke();
    }

    public void PlayerHit()
    {
        remainingLives--;
        OnLifeLost?.Invoke();
        
        if (remainingLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Réinitialiser la position du joueur
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.Reset();
            }
        }
    }

    #endregion

    #region Gestion du niveau

    private void LevelComplete()
    {
        isGameRunning = false;
        OnLevelComplete?.Invoke();
        
        // Passer au niveau suivant ou terminer le jeu
        currentLevel++;
        
        // Revenir au niveau 1 après avoir terminé le niveau MAX_LEVEL
        if (currentLevel > MAX_LEVEL)
        {
            currentLevel = 1;
        }
        
        Invoke(nameof(LoadNextLevel), 2f);
    }

    private void LoadNextLevel()
    {
        // Nettoyer les objets du niveau actuel
        CleanupCurrentLevel();
        
        // Masquer le panel de fin de niveau
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideLevelCompletePanel();
        }
        
        // Centrer le joueur
        CenterPlayer();
        
        // Réinitialiser le timer
        remainingTime = levelTime;
        
        // Changer l'arrière-plan du niveau (via le LevelGenerator)
        RefreshBackground();
        
        // Générer les balles pour le niveau
        SpawnBallsForLevel();
        
        // Démarrer le niveau
        isGameRunning = true;
        
        // Notifier le début du niveau
        OnGameStart?.Invoke();
    }
    
    private void CleanupCurrentLevel()
    {
        // Détruire toutes les balles restantes
        Ball[] balls = FindObjectsOfType<Ball>();
        foreach (Ball ball in balls)
        {
            Destroy(ball.gameObject);
        }
        
        // Détruire tous les harpons
        Harpoon[] harpoons = FindObjectsOfType<Harpoon>();
        foreach (Harpoon harpoon in harpoons)
        {
            Destroy(harpoon.gameObject);
        }
    }
    
    private void CenterPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.Reset();
        }
    }
    
    private void RefreshBackground()
    {
        // Trouver le LevelGenerator et lui demander de mettre à jour l'arrière-plan
        LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator != null)
        {
            levelGenerator.UpdateBackground(currentLevel);
        }
    }
    
    private void SpawnBallsForLevel()
    {
        // Le nombre de balles dépend du niveau actuel
        int ballsToSpawn = currentLevel;
        
        // Limiter le nombre maximum de balles à un niveau raisonnable
        if (ballsToSpawn < 1) ballsToSpawn = 1;
        
        // Espacer les balles horizontalement
        float spacing = 6f / ballsToSpawn;
        float startX = -3f + spacing/2;
        
        for (int i = 0; i < ballsToSpawn; i++)
        {
            Vector3 position = new Vector3(startX + i * spacing, 2f, 0);
            SpawnBall(3, position); // La taille 3 correspond aux grandes balles
        }
    }

    private void GameOver()
    {
        isGameRunning = false;
        OnGameOver?.Invoke();
        
        // Redémarrer le jeu après 2 secondes
        Invoke(nameof(RestartGame), 2f);
    }

    public void RestartGame()
    {
        // Nettoyer le niveau actuel
        CleanupCurrentLevel();
        
        // Réinitialiser les variables du jeu
        score = 0;
        remainingTime = levelTime;
        remainingLives = lives;
        currentLevel = 1;
        isGameRunning = false; // S'assurer que le jeu n'est pas en cours d'exécution pendant la réinitialisation
        
        // Masquer les panneaux d'UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideGameOverPanel();
            uiManager.HideLevelCompletePanel();
        }
        
        // Détruire le joueur existant s'il y en a un
        Player existingPlayer = FindObjectOfType<Player>();
        if (existingPlayer != null)
        {
            Destroy(existingPlayer.gameObject);
        }
        
        // Recharger le premier niveau
        Invoke(nameof(LoadFirstLevel), 0.5f);
    }
    
    /// <summary>
    /// Charge le premier niveau du jeu après réinitialisation
    /// </summary>
    private void LoadFirstLevel()
    {
        // Centrer le joueur
        SpawnPlayer();
        
        // Réinitialiser le timer
        remainingTime = levelTime;
        
        // Rafraîchir l'arrière-plan pour le niveau 1
        RefreshBackground();
        
        // Générer les balles pour le niveau 1
        SpawnBallsForLevel();
        
        // Démarrer le niveau
        isGameRunning = true;
        
        // Notifier le début du niveau
        OnGameStart?.Invoke();
    }

    #endregion

    #region Accesseurs

    // Getters publics pour l'UI et d'autres systèmes
    public int GetScore() => score;
    public int GetLives() => remainingLives;
    public float GetRemainingTime() => remainingTime;
    public bool IsGameRunning() => isGameRunning;
    public int GetLevel() => currentLevel;

    #endregion

    /// <summary>
    /// Configure les layers de collision du jeu pour que les balles s'ignorent entre elles
    /// </summary>
    private void SetupCollisionLayers()
    {
        // Obtenir l'index du layer Ball
        int ballLayer = LayerMask.NameToLayer(BALL_LAYER_NAME);
        
        // Si le layer n'existe pas, utiliser le layer par défaut
        if (ballLayer == -1)
        {
            ballLayer = BALL_LAYER;
            Debug.LogWarning($"Layer '{BALL_LAYER_NAME}' non trouvé. Vous devriez créer ce layer dans les paramètres du projet.");
        }
        
        // Configurer les balles pour qu'elles ne collisionnent pas entre elles
        // Cela fait que le layer "Balls" ignore les collisions avec lui-même
        Physics2D.IgnoreLayerCollision(ballLayer, ballLayer, true);
        
        Debug.Log($"Configuration des collisions: les balles (layer {ballLayer}) s'ignorent entre elles.");
    }
}
