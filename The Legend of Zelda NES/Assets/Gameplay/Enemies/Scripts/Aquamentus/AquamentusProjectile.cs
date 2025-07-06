using UnityEngine;
using static PlayerController;

public class AquamentusProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float moveSpeed = 5.0f;           
    public Vector2 direction = Vector2.right;
    public float m_damage = 1f;

    private CircleCollider2D m_pCollider;    
    private bool m_directionSet = false;     
    private Animator animator;               

    private void Awake()
    {
        m_pCollider = gameObject.GetComponent<CircleCollider2D>();
        if (m_pCollider == null)
        {
            m_pCollider = gameObject.AddComponent<CircleCollider2D>();
            m_pCollider.isTrigger = true;
            m_pCollider.radius = 1.5f;
        }

        // Initialize Animator
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("ProjectileAnimation");
        }
    }

    private void Start()
    {
        // Normalize the direction
        SetDirection(direction);
    }

    private void Update()
    {
        // Move the projectile each frame
        if (m_directionSet)
        {
            transform.Translate(direction.normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        m_directionSet = true;

        // Rotate projectile sprite based on the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Deal damage to the player
                player.TakeDamage(m_damage);
                Destroy(gameObject);            // Destroy the projectile after dealing damage
            }
        }
        // Check if the collision is with a Static Obstacle
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Static Obstacle"))
        {
            Destroy(gameObject); // Destroy the projectile on collision with a static obstacle
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the collision is with a GameObject on the "Sections" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sections"))
        {
            Destroy(gameObject); // Destroy the projectile when it exits the bounds of a GameObject on the "Sections" layer
        }
    }
}
