#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class SwordProjectile : MonoBehaviour, IProjectile
{
    public float speed = 10f;
    public int damage = 1;

    private Vector2 direction;
    private Animator m_animator;                        // Reference to the Animator
    private bool isExploding = false;                   // Flag to prevent multiple triggers
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

        // Deactivate all child GameObjects at the beginning
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only move if not exploding
        if (!isExploding)
        {
            transform.Translate(speed * Time.deltaTime * direction, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploding) return; // Prevent further actions once exploding

        if (other.CompareTag("Enemy"))
        {
#if DEBUG_LOG
            Debug.Log("Sword projectile hit an enemy.");
#endif

            transform.position = other.transform.position; // Move the projectile to the enemy's position
            StartExplosion(); // Start explosion at the current position

            // Call OnHit on the appropriate enemy component
            other.GetComponent<OctorokEnemy>()?.OnHit(damage);
            other.GetComponent<SkeletonEnemy>()?.OnHit(damage);
            other.GetComponent<GoriyaEnemy>()?.OnHit(damage);
            other.GetComponent<AquamentusEnemy>()?.OnHit(damage);

            // Apply knockback
            other.GetComponent<Knockback>()?.OnTriggerEnter2D(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isExploding) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Sections"))
        {
#if DEBUG_LOG
            Debug.Log("Sword projectile exited the section.");
#endif
            StartExplosion();
        }
    }

    private void StartExplosion()
    {
        isExploding = true; // Set the exploding flag

        // Hide the sword by disabling its SpriteRenderer and BoxCollider2D
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Activate all child GameObjects
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // Trigger the explosion animation on the parent GameObject
        if (m_animator != null)
        {
            m_animator.SetTrigger("Explode");
        }

        // Trigger child animations with a slight delay
        foreach (Transform child in transform)
        {
            Animator childAnimator = child.GetComponent<Animator>();
            if (childAnimator != null)
            {
                StartCoroutine(TriggerChildExplosion(childAnimator));
            }
            else
            {
                Debug.LogWarning("No Animator component found on child: " + child.gameObject.name);
            }
        }

        // Destroy the parent object after a short delay (allowing animations to play)
        Destroy(gameObject, 0.5f);
    }

    private IEnumerator TriggerChildExplosion(Animator animator)
    {
        yield return null; // Wait for one frame to ensure Animator is ready
        animator.SetTrigger("Explode");
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnSwordProjectileDestroyed();
        }
    }
}