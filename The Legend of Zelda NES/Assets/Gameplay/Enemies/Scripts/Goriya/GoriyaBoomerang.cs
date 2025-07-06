#define DEBUG_LOG

using UnityEngine;

public class GoriyaBoomerang : MonoBehaviour
{
    public float moveSpeed = 5f;           // Speed of the boomerang
    public float maxDistance = 10f;        // Maximum distance the boomerang can travel
    public float returnSpeed = 7f;         // Speed of the boomerang when returning

    private Vector2 m_startingPosition;    // Starting position of the boomerang
    private Vector2 m_direction;           // Direction of the boomerang
    private bool m_returning = false;      // Flag to indicate if the boomerang is returning
    private GoriyaEnemy m_goriya;          // Reference to the GoriyaEnemy

    public void Initialize(Vector2 direction, GoriyaEnemy goriya)
    {
        m_direction = direction.normalized;
        m_startingPosition = transform.position;
        m_goriya = goriya;
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
            // Move the boomerang back to the Goriya
            Vector2 directionToGoriya = ((Vector2)m_goriya.transform.position - (Vector2)transform.position).normalized;
            transform.Translate(directionToGoriya * returnSpeed * Time.deltaTime);

            // Check if the boomerang has reached the Goriya
            if (Vector2.Distance(m_goriya.transform.position, transform.position) < 0.5f)
            {
                Destroy(gameObject);
                m_goriya.OnBoomerangReturned();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the boomerang collides with an enemy or interactable object
        if (collision.gameObject.CompareTag("Player"))
        {
#if DEBUG_LOG
            Debug.Log("Boomerang collided with: " + collision.gameObject.name);
#endif
        }
        // Check if the boomerang collides with an item
        else if (collision.gameObject.CompareTag("Item"))
        {
#if DEBUG_LOG
            Debug.Log("Boomerang collided with item: " + collision.gameObject.name);
#endif
            // Attach the item to the boomerang
            collision.transform.SetParent(transform);
            collision.transform.localPosition = Vector2.zero; // Center the item on the boomerang
        }
    }
}
