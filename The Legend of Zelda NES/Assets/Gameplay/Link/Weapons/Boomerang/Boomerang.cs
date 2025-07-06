#define DEBUG_LOG

using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;            // Speed of the boomerang
    [SerializeField] private float maxDistance = 10f;         // Maximum distance the boomerang can travel
    [SerializeField] private float returnSpeed = 7f;          // Speed of the boomerang when returning
    private int damage = 1;                                   // Damage dealt by the boomerang

    private Vector2 m_startingPosition;             // Starting position of the boomerang
    private Vector2 m_direction;                    // Direction of the boomerang
    private bool m_returning = false;               // Flag to indicate if the boomerang is returning
    private PlayerController m_player;              // Reference to the player

    public void Initialize(Vector2 direction, PlayerController player)
    {
        m_direction = direction.normalized;
        m_startingPosition = transform.position;
        m_player = player;
    }

    private void Update()
    {
        if (!m_returning)
        {
            // Move the boomerang forward
            transform.Translate(m_direction * moveSpeed * Time.deltaTime);

            // Check if the boomerang has reached its maximum distance
            if (Vector2.Distance(m_startingPosition, transform.position) >= maxDistance)
            {
                m_returning = true;
            }
        }
        else
        {
            // Move the boomerang back to the player
            Vector2 directionToPlayer = (m_player.transform.position - transform.position).normalized;
            transform.Translate(directionToPlayer * returnSpeed * Time.deltaTime);

            // Check if the boomerang has reached the player
            if (Vector2.Distance(m_player.transform.position, transform.position) < 0.5f)
            {
                Destroy(gameObject);
                m_player.OnBoomerangDestroyed();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the boomerang collides with an enemy
        if (other.gameObject.CompareTag("Enemy"))
        {
#if DEBUG_LOG
            Debug.Log("Boomerang collided with: " + other.gameObject.name);
#endif
            // Damage an Octorok
            other.GetComponent<OctorokEnemy>()?.TakeDamage(damage);

            // Stun a Skeleton, Goriya, or Aquamantus
            other.GetComponent<SkeletonEnemy>()?.Stun(2f);
            other.GetComponent<GoriyaEnemy>()?.Stun(2f);
            other.GetComponent<AquamentusEnemy>()?.Stun(2f);
        }
        // Check if the boomerang collides with an item
        else if (other.gameObject.CompareTag("Item"))
        {
#if DEBUG_LOG
            Debug.Log("Boomerang collided with item: " + other.gameObject.name);
#endif
            // Attach the item to the boomerang
            other.transform.SetParent(transform);
            other.transform.localPosition = Vector2.zero; // Center the item on the boomerang
        }
    }
}