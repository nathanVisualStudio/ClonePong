using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Dimensions du niveau")]
    [SerializeField] private float levelWidth = 16f;
    [SerializeField] private float levelHeight = 9f;
    
    [Header("Limites")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallThickness = 0.5f;
    
    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform obstaclesParent;
    
    [Header("Plateformes")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private List<Vector2> platformPositions = new List<Vector2>();
    
    [Header("Arrière-plan")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite[] backgroundSprites;
    
    private void Start()
    {
        // Configurer la caméra
        SetupCamera();
        
        // Générer les limites
        CreateBoundaries();
        
        // Placer les obstacles et les plateformes
        CreateObstacles();
        CreatePlatforms();
        
        // Configurer l'arrière-plan
        SetBackground();
    }
    
    private void SetupCamera()
    {
        if (Camera.main != null)
        {
            // Configurer la caméra pour qu'elle affiche correctement le niveau
            Camera.main.orthographicSize = levelHeight / 2f;
            Camera.main.transform.position = new Vector3(0, 0, -10);
        }
    }
    
    private void CreateBoundaries()
    {
        if (wallPrefab == null) return;
        
        // Créer les murs gauche et droite
        float wallHeight = levelHeight + wallThickness * 2;
        float sideX = levelWidth / 2f + wallThickness / 2f;
        
        // Mur gauche
        CreateWall(new Vector3(-sideX, 0, 0), new Vector3(wallThickness, wallHeight, 1));
        
        // Mur droit
        CreateWall(new Vector3(sideX, 0, 0), new Vector3(wallThickness, wallHeight, 1));
        
        // Sol
        float bottomY = -levelHeight / 2f - wallThickness / 2f;
        CreateWall(new Vector3(0, bottomY, 0), new Vector3(levelWidth + wallThickness * 2, wallThickness, 1));
        
        // Plafond
        float topY = levelHeight / 2f + wallThickness / 2f;
        GameObject ceiling = CreateWall(new Vector3(0, topY, 0), new Vector3(levelWidth + wallThickness * 2, wallThickness, 1));
        
        // Ajouter le tag "Ceiling" au plafond pour la détection du grapin
        if (ceiling != null)
        {
            ceiling.tag = "Ceiling";
        }
    }
    
    private GameObject CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.transform.localScale = scale;
        wall.transform.SetParent(transform);
        
        // Par défaut, tous les murs ont le tag "Wall"
        wall.tag = "Wall";
        
        return wall;
    }
    
    private void CreateObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0 || obstaclesParent == null) return;
        
        // Ici, vous pouvez implémenter une logique pour placer des obstacles
        // selon le niveau actuel dans GameManager.Instance.currentLevel
        
        // Exemple simple: placer quelques obstacles aléatoires
        int currentLevel = GameManager.Instance ? GameManager.Instance.GetLevel() : 1;
        int obstacleCount = Mathf.Min(currentLevel, 5);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            float x = Random.Range(-levelWidth / 2f + 1f, levelWidth / 2f - 1f);
            float y = Random.Range(-levelHeight / 2f + 2f, levelHeight / 2f - 1f);
            
            int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = Instantiate(obstaclePrefabs[prefabIndex], new Vector3(x, y, 0), Quaternion.identity);
            obstacle.transform.SetParent(obstaclesParent);
        }
    }
    
    private void CreatePlatforms()
    {
        if (platformPrefab == null || platformPositions.Count == 0) return;
        
        foreach (Vector2 position in platformPositions)
        {
            GameObject platform = Instantiate(platformPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
            platform.transform.SetParent(transform);
        }
    }
    
    private void SetBackground()
    {
        if (backgroundRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;
        
        // Choisir un arrière-plan aléatoire ou basé sur le niveau
        int currentLevel = GameManager.Instance ? GameManager.Instance.GetLevel() : 1;
        int backgroundIndex = (currentLevel - 1) % backgroundSprites.Length;
        
        backgroundRenderer.sprite = backgroundSprites[backgroundIndex];
        
        // Ajuster la taille de l'arrière-plan pour couvrir l'écran
        float width = levelWidth + 2;
        float height = levelHeight + 2;
        backgroundRenderer.size = new Vector2(width, height);
    }
    
    /// <summary>
    /// Met à jour l'arrière-plan en fonction du niveau actuel
    /// Cette méthode est appelée par le GameManager lors du changement de niveau
    /// </summary>
    /// <param name="level">Le niveau actuel</param>
    public void UpdateBackground(int level)
    {
        if (backgroundRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;
        
        // Calculer l'index du sprite d'arrière-plan à utiliser
        int backgroundIndex = (level - 1) % backgroundSprites.Length;
        
        // Assigner le nouveau sprite
        backgroundRenderer.sprite = backgroundSprites[backgroundIndex];
        
        // Ajuster la taille de l'arrière-plan pour couvrir l'écran
        float width = levelWidth + 2;
        float height = levelHeight + 2;
        backgroundRenderer.size = new Vector2(width, height);
        
        // Log pour le débogage
        Debug.Log($"Arrière-plan mis à jour pour le niveau {level} (index sprite: {backgroundIndex})");
    }
}