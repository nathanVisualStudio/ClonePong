using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    [Header("Paramètres de la balle")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseSize = 1f;
    [SerializeField] private int basePoints = 100;
    
    [Header("Paramètres de rebond")]
    [SerializeField] private float baseJumpHeight = 3f;
    [SerializeField] private float sizeJumpMultiplier = 0.5f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float minHorizontalSpeed = 1f; // Vitesse horizontale minimale pour éviter l'immobilité
    [SerializeField] private float wallCheckDistance = 0.1f; // Distance pour détecter si la balle est collée au mur
    
    // Composants
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // État de la balle
    private int size;
    private float currentSpeed;
    private float jumpForce;
    private float stuckCheckTimer = 0f;
    private const float STUCK_CHECK_INTERVAL = 0.2f; // Intervalle pour vérifier si la balle est coincée
    
    private const string BALL_LAYER_NAME = "Balls";
    private const int BALL_LAYER = 8; // Layer 8 est généralement le premier layer personnalisable dans Unity

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurer cette balle pour qu'elle soit sur le layer "Balls"
        SetupCollisionLayer();
    }

    /// <summary>
    /// Configure le layer de la balle et s'assure que les balles s'ignorent entre elles
    /// </summary>
    private void SetupCollisionLayer()
    {
        // Vérifier si le layer existe, sinon utiliser un fallback sécurisé
        int ballLayer = LayerMask.NameToLayer(BALL_LAYER_NAME);
        if (ballLayer == -1)
        {
            // Si le layer n'existe pas, utiliser le layer par défaut prédéfini
            ballLayer = BALL_LAYER;
            Debug.LogWarning($"Layer '{BALL_LAYER_NAME}' non trouvé. Utilisation du layer {ballLayer} à la place. Créez ce layer dans les paramètres du projet.");
        }
        
        // Appliquer le layer à la balle
        gameObject.layer = ballLayer;
    }

    public void Initialize(int ballSize, Color ballColor)
    {
        size = ballSize;
        
        // Définir la taille physique et visuelle de la balle
        float scale = baseSize * (size * 0.5f);
        transform.localScale = new Vector3(scale, scale, 1f);
        
        // Configurer le collider
        GetComponent<CircleCollider2D>().radius = 0.5f;
        
        // Couleur
        spriteRenderer.color = ballColor;
        
        // Vitesse horizontale basée sur la taille (inversée pour que les petites balles soient plus rapides)
        currentSpeed = baseSpeed * (1f + (4 - size) * 0.25f);
        
        // Force de saut basée sur la taille (plus grandes balles = sauts plus hauts)
        jumpForce = baseJumpHeight * (1f + size * sizeJumpMultiplier);
        
        // Configurer la gravité
        rb.gravityScale = gravityScale;
        
        // Direction initiale aléatoire pour le mouvement horizontal
        SetInitialVelocity();
    }

    private void SetInitialVelocity()
    {
        // Direction horizontale aléatoire (gauche ou droite)
        float directionX = Random.Range(0, 2) * 2 - 1; // -1 ou 1
        
        // Appliquer la vitesse horizontale et une impulsion verticale initiale
        rb.linearVelocity = new Vector2(directionX * currentSpeed, jumpForce);
    }

    /// <summary>
    /// Initialise la balle avec une vitesse et une direction spécifiques
    /// </summary>
    /// <param name="directionX">Direction horizontale (-1 pour gauche, 1 pour droite)</param>
    /// <param name="initialVerticalVelocity">Vitesse verticale initiale</param>
    public void SetCustomVelocity(float directionX, float initialVerticalVelocity)
    {
        // S'assurer que la direction est normalisée à -1 ou 1
        directionX = Mathf.Sign(directionX);
        if (directionX == 0) directionX = 1; // Par défaut, aller à droite si 0
        
        // Appliquer la vitesse avec la direction spécifiée
        rb.linearVelocity = new Vector2(directionX * currentSpeed, initialVerticalVelocity);
    }

    public void Split()
    {
        // Si la balle n'est pas de la plus petite taille, la diviser
        if (size > 1)
        {
            // Créer deux nouvelles balles plus petites
            SpawnSplitBalls();
            
            // Ajouter des points au score
            int points = basePoints * (5 - size);
            GameManager.Instance.AddScore(points);
        }
        else
        {
            // Si c'est la plus petite balle, ajouter plus de points
            GameManager.Instance.AddScore(basePoints * 4);
        }
        
        // Détruire cette balle
        Destroy(gameObject);
    }

    private void SpawnSplitBalls()
    {
        // Récupérer la vitesse verticale actuelle pour les balles filles
        float currentVerticalVelocity = rb.linearVelocity.y;
        
        // Créer deux balles de taille inférieure qui partent dans des directions opposées
        for (int i = 0; i < 2; i++)
        {
            // Position légèrement décalée pour éviter la superposition
            Vector3 spawnPosition = transform.position;
            spawnPosition.x += (i == 0) ? -0.2f : 0.2f;
            
            // Utiliser le GameManager pour créer une nouvelle balle
            GameObject newBall = GameManager.Instance.SpawnBall(size - 1, spawnPosition);
            
            // Récupérer le composant Ball de la nouvelle balle
            Ball ballComponent = newBall.GetComponent<Ball>();
            if (ballComponent != null)
            {
                // Direction horizontale: première balle à gauche, deuxième à droite
                float directionX = (i == 0) ? -1f : 1f;
                
                // La vitesse verticale varie légèrement pour les deux balles
                float verticalVelocity = currentVerticalVelocity + Random.Range(-1f, 1f);
                if (verticalVelocity < 0) verticalVelocity = 1f; // Assurer un minimum de vitesse verticale positive
                
                // Définir la vitesse personnalisée
                ballComponent.SetCustomVelocity(directionX, verticalVelocity);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Rebond au contact du sol ou des murs
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ceiling"))
        {
            HandleBounce(collision);
        }
        
        // Effets sonores de rebond
        // AudioManager.Instance.PlayBounceSound();
    }

    private void HandleBounce(Collision2D collision)
    {
        // Déterminer si c'est un contact avec le sol
        bool isGroundContact = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Si la normale de contact pointe vers le haut, c'est probablement le sol
            if (contact.normal.y > 0.5f)
            {
                isGroundContact = true;
                break;
            }
        }
        
        // Si c'est un contact avec le sol, appliquer la force de saut
        if (isGroundContact)
        {
            // Conserver la vélocité horizontale, mais appliquer la force de saut verticale
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        // Sinon, c'est un mur, inverser la direction horizontale et s'assurer que la vitesse est suffisante
        else
        {
            // Déterminer la direction du rebond (inverse de la direction actuelle)
            float dirX = rb.linearVelocity.x > 0 ? -1 : 1;
            
            // Appliquer une vitesse horizontale minimale dans la nouvelle direction
            rb.linearVelocity = new Vector2(dirX * Mathf.Max(currentSpeed, minHorizontalSpeed), rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        // Vérifier périodiquement si la balle est coincée contre un mur
        CheckIfStuck();
        
        // Maintenir une vitesse horizontale minimum pour éviter l'immobilité
        EnsureMinimumHorizontalSpeed();
        
        // Limiter la position de la balle à l'écran
        LimitBallPosition();
    }

    private void CheckIfStuck()
    {
        // Incrémenter le timer
        stuckCheckTimer += Time.fixedDeltaTime;
        
        // Vérifier périodiquement si la balle est coincée
        if (stuckCheckTimer >= STUCK_CHECK_INTERVAL)
        {
            stuckCheckTimer = 0f;
            
            // Si la vitesse horizontale est trop faible, la balle est peut-être coincée
            if (Mathf.Abs(rb.linearVelocity.x) < minHorizontalSpeed)
            {
                // Obtenir les dimensions de l'écran en unités du monde
                float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
                float ballRadius = transform.localScale.x * 0.5f;
                
                // Vérifier si la balle est proche d'un mur
                bool nearLeftWall = transform.position.x < -screenWidth + ballRadius + wallCheckDistance;
                bool nearRightWall = transform.position.x > screenWidth - ballRadius - wallCheckDistance;
                
                if (nearLeftWall || nearRightWall)
                {
                    // Donner une impulsion horizontale dans la direction opposée au mur
                    float dirX = nearLeftWall ? 1f : -1f;
                    rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);
                }
            }
        }
    }

    private void EnsureMinimumHorizontalSpeed()
    {
        // S'assurer que la vitesse horizontale est toujours au moins égale à minHorizontalSpeed
        float currentXVelocity = rb.linearVelocity.x;
        
        if (Mathf.Abs(currentXVelocity) < minHorizontalSpeed)
        {
            // Garder le signe (direction) mais augmenter la magnitude
            float dirX = Mathf.Sign(currentXVelocity);
            // Si la vitesse est nulle, choisir une direction aléatoire
            if (dirX == 0f) dirX = Random.Range(0, 2) * 2 - 1;
            
            rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);
        }
    }

    private void LimitBallPosition()
    {
        // Obtenir les dimensions de l'écran en unités du monde
        float screenHeight = Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;
        
        // Obtenir le rayon de la balle pour les calculs de limites
        float ballRadius = transform.localScale.x * 0.5f;
        
        Vector2 position = transform.position;
        Vector2 velocity = rb.linearVelocity;
        
        // Appliquer les limites horizontales et verticales
        HandleHorizontalBoundaries(ref position, ref velocity, screenWidth, ballRadius);
        HandleVerticalBoundaries(ref position, ref velocity, screenHeight, ballRadius);
        
        // Appliquer les changements
        transform.position = position;
        rb.linearVelocity = velocity;
    }

    private void HandleHorizontalBoundaries(ref Vector2 position, ref Vector2 velocity, float screenWidth, float ballRadius)
    {
        const float OFFSET = 0.01f; // Petit décalage pour éviter de rester coincé
        
        // Limites horizontales
        if (position.x < -screenWidth + ballRadius)
        {
            position.x = -screenWidth + ballRadius + OFFSET;
            velocity.x = Mathf.Abs(velocity.x);
            
            // S'assurer que la vitesse est suffisante après rebond
            if (velocity.x < minHorizontalSpeed) velocity.x = currentSpeed;
        }
        else if (position.x > screenWidth - ballRadius)
        {
            position.x = screenWidth - ballRadius - OFFSET;
            velocity.x = -Mathf.Abs(velocity.x);
            
            // S'assurer que la vitesse est suffisante après rebond
            if (Mathf.Abs(velocity.x) < minHorizontalSpeed) velocity.x = -currentSpeed;
        }
    }
    
    private void HandleVerticalBoundaries(ref Vector2 position, ref Vector2 velocity, float screenHeight, float ballRadius)
    {
        const float OFFSET = 0.01f; // Petit décalage pour éviter de rester coincé
        
        // Limites verticales (uniquement pour éviter que la balle ne sorte de l'écran)
        if (position.y < -screenHeight + ballRadius)
        {
            position.y = -screenHeight + ballRadius + OFFSET;
            velocity.y = jumpForce; // Appliquer la force de saut si la balle touche le bas de l'écran
        }
        else if (position.y > screenHeight - ballRadius)
        {
            position.y = screenHeight - ballRadius - OFFSET;
            velocity.y = -Mathf.Abs(velocity.y);
        }
    }

}
