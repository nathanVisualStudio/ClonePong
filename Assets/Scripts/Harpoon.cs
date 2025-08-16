using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float width = 0.33f;

    private Vector2 startPosition;

    private const float INITIAL_HEIGHT = 0.1f;

    void Start()
    {
        startPosition = transform.position;
        transform.localScale = new Vector3(width, INITIAL_HEIGHT, 1f);
    }

    void Update() => ExtendHarpoon();

    private void ExtendHarpoon()
    {
        // Étendre le grapin vers le haut
        float newY = transform.localScale.y + speed * Time.deltaTime;
        transform.localScale = new Vector3(width, newY, 1f);
        
        // Ajuster la position pour qu'il s'étende vers le haut
        transform.position = startPosition + Vector2.up * (newY / 2f);
        
        // Vérifier si le grapin a atteint sa longueur maximale
        if (transform.localScale.y >= maxDistance || transform.position.y >= Camera.main.orthographicSize)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            AudioManager.Instance.PlayBubbleSound(); // Jouer le son de la bulle

            // Diviser la balle en deux
            Ball ball = collision.GetComponent<Ball>();
            if (ball != null)
            {
                ball.Split();
            }
            
            // Détruire le grapin
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ceiling"))
        {
            // Si le grapin touche le plafond, le détruire
            Destroy(gameObject);
        }
    }   

}
