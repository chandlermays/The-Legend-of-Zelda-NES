using System.Collections;
using UnityEngine;
using static PlayerController;

public class RockProjectile : MonoBehaviour, IProjectile
{
    public float MoveSpeed = 5f;                            // Speed projectile moves
    public float m_damage = 1f;                             // Damage the projectile deals
    public float fadeDuration = 2f;                       // Duration of the fade-out effect

    public Vector2 Direction => m_pRb != null ? m_pRb.linearVelocity.normalized : Vector2.zero;

    private Rigidbody2D m_pRb;
    private Collider2D m_collider;
    private SpriteRenderer m_spriteRenderer;
    private bool m_directionSet = false;

    private void Awake()
    {
        m_pRb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();

        if (m_pRb != null)
        {
            m_pRb.gravityScale = 0;
            m_pRb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (m_directionSet && m_pRb != null)
        {
            m_pRb.linearVelocity = m_pRb.linearVelocity.normalized * MoveSpeed;
        }
    }

    public void SetDirection(Vector2 direction)
    {
        if (m_pRb != null)
        {
            // Normalize the direction and set the initial velocity
            m_pRb.linearVelocity = direction.normalized * MoveSpeed;
            m_directionSet = true;

            // Adjust the rotation based on the direction
            if (direction == Vector2.up)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (direction == Vector2.left)
            {
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (direction == Vector2.right)
            {
                transform.rotation = Quaternion.Euler(0, 0, -90);
            }
            else if (direction == Vector2.down)
            {
                transform.rotation = Quaternion.identity;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Rock projectile hit the player.");

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                FacingDirection playerFacing = player.GetFacingDirection();
                Vector2 projectileDirection = m_pRb.linearVelocity.normalized;

                // Determine if the projectile is moving in the opposite direction of the player's facing direction
                if (IsOppositeDirection(playerFacing, projectileDirection))
                {
                    BounceOff(collision.transform.position);
                }
                else
                {
                    // Deal damage to the player
                    player.TakeDamage(m_damage);
                    Destroy(gameObject);            // Destroy the projectile after dealing damage
                }
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

    private bool IsOppositeDirection(FacingDirection playerFacing, Vector2 projectileDirection)
    {
        // Map FacingDirection to a vector
        Vector2 playerFacingVector = Vector2.zero;
        switch (playerFacing)
        {
            case FacingDirection.kUp: playerFacingVector = Vector2.up; break;
            case FacingDirection.kDown: playerFacingVector = Vector2.down; break;
            case FacingDirection.kLeft: playerFacingVector = Vector2.left; break;
            case FacingDirection.kRight: playerFacingVector = Vector2.right; break;
        }

        // Check if the dot product is approximately -1, indicating opposite directions
        float dotProduct = Vector2.Dot(playerFacingVector, projectileDirection);
        return dotProduct < -0.9f; // Adjust tolerance if needed
    }

    private void BounceOff(Vector2 playerPosition)
    {
        Debug.Log("Bouncing off the player");

        if (m_pRb != null)
        {
            m_collider.enabled = false; // Disable the collider to prevent further collisions

            // Set the layer to Default
            gameObject.layer = LayerMask.NameToLayer("Default");

            Vector2 direction = (transform.position - (Vector3)playerPosition).normalized;

            // Randomly choose between 45 or -45 degrees
            float angle = Random.value > 0.5f ? 45f : -45f;
            Vector2 bounceDirection = Quaternion.Euler(0, 0, angle) * direction;

            m_pRb.linearVelocity = bounceDirection * MoveSpeed;

            // Start the fade-out coroutine
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Debug.LogWarning("Rigidbody2D component not found.");
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        Debug.Log("Fading out and destroying the projectile");

        if (m_spriteRenderer != null)
        {
            float startAlpha = m_spriteRenderer.color.a;
            float rate = 1.0f / fadeDuration;
            float progress = 0.0f;

            while (progress < 1.0f)
            {
                Color tmpColor = m_spriteRenderer.color;
                m_spriteRenderer.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));
                progress += rate * Time.deltaTime;

                yield return null;
            }

            m_spriteRenderer.color = new Color(m_spriteRenderer.color.r, m_spriteRenderer.color.g, m_spriteRenderer.color.b, 0);
        }

        Destroy(gameObject);
    }
}