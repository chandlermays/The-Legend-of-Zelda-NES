using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float m_explosionDelay;        // Delay before the bomb explodes
    [SerializeField] private float m_explosionRadius;       // Radius of the explosion
    private int m_damageAmount = 10;                        // Damage amount

    private Animator m_animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_animator = GetComponent<Animator>();
        Debug.Log("Bomb created");
        Invoke(nameof(Explode), m_explosionDelay); // Set the timer for the explosion
    }

    private void Explode()
    {
        m_animator.SetTrigger("Explode"); // Trigger the explosion animation
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, m_explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                hitCollider.GetComponent<OctorokEnemy>()?.TakeDamage(m_damageAmount);
                hitCollider.GetComponent<SkeletonEnemy>()?.TakeDamage(m_damageAmount);
                hitCollider.GetComponent<GoriyaEnemy>()?.TakeDamage(m_damageAmount);
                hitCollider.GetComponent<AquamentusEnemy>()?.TakeDamage(m_damageAmount);
            }
        }
    }

    // Animation event
    private void DestroyBomb()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the explosion radius in the editor for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_explosionRadius);
    }
}