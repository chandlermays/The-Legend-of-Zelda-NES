#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour, IProjectile
{
    public float speed = 10f;
    public int damage = 1;

    private Vector2 direction;
    private Animator m_animator;                        // Reference to the Animator
    private SpriteRenderer spriteRenderer;              // Reference to the SpriteRenderer
    private BoxCollider2D boxCollider;                  // Reference to the BoxCollider2D
    private PlayerController playerController;          // Reference to the PlayerController

    public Vector2 Direction => direction;

    public void Initialize(Vector2 direction, PlayerController playerController)
    {
        this.direction = direction.normalized;
        this.playerController = playerController;
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        transform.Translate(speed * Time.deltaTime * direction, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
#if DEBUG_LOG
            Debug.Log("Sword projectile hit an enemy.");
#endif
            // Call OnHit on the appropriate enemy component
            other.GetComponent<OctorokEnemy>()?.OnHit(damage);
            other.GetComponent<SkeletonEnemy>()?.OnHit(damage);
            other.GetComponent<GoriyaEnemy>()?.OnHit(damage);
            other.GetComponent<AquamentusEnemy>()?.OnHit(damage);

            // Apply knockback
            other.GetComponent<Knockback>()?.OnTriggerEnter2D(other);

            Destroy(gameObject); // Destroy the arrow projectile
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Sections"))
        {
            Destroy(gameObject); // Destroy the arrow projectile

#if DEBUG_LOG
            Debug.Log("Arrow projectile exited the section.");
#endif
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnArrowProjectileDestroyed();
        }
    }
}