#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class SkeletonEnemy : MonoBehaviour
{
    private PlayerController m_playerController;

    [Header("Movement Settings")]
    public float MoveSpeed = 1.5f;      // Speed of the skeleton's movement
    public float DetectionRadius = 5f;  // Radius for detecting the player
    public float StepLength = 1.0f;     // Distance covered in each step
    public float FlipInterval = 0.5f;   // Time interval for flipping sprite
    public LayerMask PlayerLayer;       // Layer mask to detect the player
    public LayerMask ObstacleLayer;     // Layer mask to detect obstacles
    public float MinDirectionChangeTime = 1f;  // Min time before changing direction
    public float MaxDirectionChangeTime = 10f; // Max time before changing direction

    public event System.Action OnEnemyDestroyed; // Triggered when the skeleton is destroyed

    [Header("Item Drop Settings")]
    [SerializeField] private GameObject m_heldItem;
    [SerializeField] private GameObject m_heartPrefab;
    [SerializeField] private GameObject m_rupeePrefab;
    [SerializeField] private GameObject m_bombPrefab;

    // Variables
    private int m_health = 2;
    private Transform m_target;
    private Rigidbody2D m_pRb;
    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    private bool m_isFlipped = false;
    private Vector2 m_randomDirection;
    private float m_nextFlipTime;
    private float m_nextDirectionChangeTime;
    private bool m_isMovingToStep = false;
    private Vector2 m_stepTarget;
    private Vector2 m_lastDirection;
    private bool m_isStunned = false;

    private BoxCollider2D m_sectionBounds;

    private bool m_isKnockedBack = false;
    public bool IsKnockedBack
    {
        get { return m_isKnockedBack; }
        set { m_isKnockedBack = value; }
    }

    private bool m_isInvulnerable = false;
    public bool IsInvulnerable
    {
        get { return m_isInvulnerable; }
        set { m_isInvulnerable = value; }
    }

    private bool m_canMove = false;

    private void Start()
    {
        m_pRb = GetComponent<Rigidbody2D>();
        m_pRb.gravityScale = 0;
        m_pRb.freezeRotation = true;
        m_pRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // This method will be called by the animation event at the end of the "Transition" animation
    public void OnTransitionAnimationComplete()
    {
        m_animator.SetBool("canMove", true);

        // Proceed with the rest of the initialization logic
        Vector3 position = transform.position;
        position.z = 10;
        transform.position = position;

        ChooseRandomDirection();
        SetNextDirectionChangeTime();
        m_nextFlipTime = Time.time + FlipInterval;

        // Set the Sprite of the Held Item to appear as if they're carrying it
        if (m_heldItem != null)
        {
            SpriteRenderer heldItemSpriteRenderer = m_heldItem.GetComponent<SpriteRenderer>();
            if (heldItemSpriteRenderer != null)
            {
                Transform childTransform = transform.Find("Held Item"); // Replace with the actual name of the child GameObject
                if (childTransform != null)
                {
                    SpriteRenderer childSpriteRenderer = childTransform.GetComponent<SpriteRenderer>();
                    if (childSpriteRenderer != null)
                    {
                        childSpriteRenderer.sprite = heldItemSpriteRenderer.sprite;
                    }
                }
            }
        }

        // Find the BoxCollider2D in the parent Section
        m_sectionBounds = GetComponentInParent<BoxCollider2D>();
        if (m_sectionBounds == null)
        {
            Debug.LogError("No BoxCollider2D found in parent Section.");
        }

        // Cache the PlayerController reference
        m_playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        m_canMove = true;
    }

    private void Update()
    {
        if (m_canMove && !m_isKnockedBack)
        {
            DetectPlayer();
            HandleSpriteFlip();
            HandleDirectionChange();
        }
    }

    private void FixedUpdate()
    {
        if (m_canMove && !m_isKnockedBack)
        {
            Move();
            ClampPositionWithinBounds();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // React to collisions with non-player objects
        if (!collision.collider.CompareTag("Player"))
        {
            m_isMovingToStep = false;
            ChooseNewDirection();
        }
    }

    private void Move()
    {
        if (m_isKnockedBack)
        {
            return;
        }

        // Move to step target if set
        if (m_isMovingToStep)
        {
            Vector2 currentPosition = transform.position;
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, m_stepTarget, MoveSpeed * Time.fixedDeltaTime);
            m_pRb.MovePosition(newPosition);

            // Check if the step target has been reached
            if (Vector2.Distance(currentPosition, m_stepTarget) <= 0.01f)
            {
                m_isMovingToStep = false;
            }
            return;
        }

        // Determine movement direction
        Vector2 movementDirection;
        if (m_target != null)
        {
            // Move towards the player if detected
            Vector2 directionToTarget = (Vector2)m_target.position - (Vector2)transform.position;

            // Choose the dominant direction (horizontal or vertical)
            if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
            {
                movementDirection = directionToTarget.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                movementDirection = directionToTarget.y > 0 ? Vector2.up : Vector2.down;
            }
        }
        else
        {
            // Move in the current random direction
            movementDirection = m_randomDirection;
        }

        // Attempt to move in the chosen direction
        if (CanMoveInDirection(movementDirection, 0.1f))
        {
            m_stepTarget = (Vector2)transform.position + movementDirection * (StepLength / 2);
            m_isMovingToStep = true;
            m_lastDirection = movementDirection;
        }
        else
        {
            // Adjust direction if blocked
            HandleCollisionOrStuck(movementDirection);
        }
    }

    private void HandleCollisionOrStuck(Vector2 currentDirection)
    {
        // Trying other directions if the current direction is blocked
        Vector2[] possibleDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var direction in possibleDirections)
        {
            if (direction != -currentDirection && CanMoveInDirection(direction, 0.1f))
            {
                m_randomDirection = direction;
                m_stepTarget = (Vector2)transform.position + direction * (StepLength / 2);
                m_isMovingToStep = true;
                return;
            }
        }

        ChooseNewDirection();
    }

    private void DetectPlayer()
    {
        // Check for player
        Collider2D player = Physics2D.OverlapCircle(transform.position, DetectionRadius, PlayerLayer);
        m_target = player != null ? player.transform : null;
    }

    private void HandleSpriteFlip()
    {
        // Flip the sprite horizontally
        if (Time.time >= m_nextFlipTime)
        {
            m_isFlipped = !m_isFlipped;
            m_spriteRenderer.flipX = m_isFlipped;
            m_nextFlipTime = Time.time + FlipInterval;
        }
    }

    private void HandleDirectionChange()
    {
        if (m_target == null && Time.time >= m_nextDirectionChangeTime)
        {
            ChooseRandomDirection();
            SetNextDirectionChangeTime();
        }
    }

    private void ChooseRandomDirection()
    {
        // Randomly choose a valid movement direction
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 chosenDirection;
        do
        {
            // Weighted preference based on the last direction
            float verticalWeight = (m_lastDirection == Vector2.left || m_lastDirection == Vector2.right) ? 0.7f : 0.3f;
            float horizontalWeight = (m_lastDirection == Vector2.up || m_lastDirection == Vector2.down) ? 0.7f : 0.3f;

            float randomValue = Random.value;
            if (randomValue < verticalWeight / 2f)
            {
                chosenDirection = Vector2.up;
            }
            else if (randomValue < verticalWeight)
            {
                chosenDirection = Vector2.down;
            }
            else if (randomValue < verticalWeight + horizontalWeight / 2f)
            {
                chosenDirection = Vector2.left;
            }
            else
            {
                chosenDirection = Vector2.right;
            }
        } while (!CanMoveInDirection(chosenDirection, StepLength / 2));

        m_randomDirection = chosenDirection;
    }

    private void ChooseNewDirection()
    {
        // Immediately choose a new random direction
        ChooseRandomDirection();
        m_nextDirectionChangeTime = Time.time;
    }

    private bool CanMoveInDirection(Vector2 direction, float stepSize)
    {
        // Check if the direction is clear
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, stepSize, ObstacleLayer);
        return hit.collider == null;
    }

    private void SetNextDirectionChangeTime()
    {
        // Set the next time the direction can change
        m_nextDirectionChangeTime = Time.time + Random.Range(MinDirectionChangeTime, MaxDirectionChangeTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }

    // Clamps the enemy's position within the section bounds
    private void ClampPositionWithinBounds()
    {
        if (m_sectionBounds != null)
        {
            Vector3 clampedPosition = transform.position;
            bool changedDirection = false;

            if (clampedPosition.x < m_sectionBounds.bounds.min.x || clampedPosition.x > m_sectionBounds.bounds.max.x)
            {
                clampedPosition.x = Mathf.Clamp(clampedPosition.x, m_sectionBounds.bounds.min.x, m_sectionBounds.bounds.max.x);
                changedDirection = true;
            }

            if (clampedPosition.y < m_sectionBounds.bounds.min.y || clampedPosition.y > m_sectionBounds.bounds.max.y)
            {
                clampedPosition.y = Mathf.Clamp(clampedPosition.y, m_sectionBounds.bounds.min.y, m_sectionBounds.bounds.max.y);
                changedDirection = true;
            }

            if (changedDirection)
            {
                ChooseRandomDirection();
            }

            transform.position = clampedPosition;
        }
    }

    // Chandler: Message Broadcast of the Player's death
    public void OnPlayerDeath()
    {
        // Destroy self when player dies
        Destroy(gameObject);
    }

    // Chandler: Message Broadcast of the Skeleton taking damage
    public void OnHit(int damage)
    {
        Debug.Log("Skeleton taking damage: " + damage);
        TakeDamage(damage);
    }

    // Chandler: Takes damage and destroys the enemy if health is zero
    public void TakeDamage(int damage)
    {
        m_health -= damage;
        if (m_health <= 0)
        {
            HandleDeath();
        }
        else
        {
            // Start the coroutine to handle the color change only if the enemy is still alive
            StartCoroutine(FlashDamageColor());
        }
    }

    // Chandler: Quickly switch between two colors for an event
    private IEnumerator FlashDamageColor()
    {
        m_isInvulnerable = true;
        Color originalColor = m_spriteRenderer.color;
        Color damageColor = new Color(1f, 0.5137f, 0.5137f); // FF8383 in RGB
        float duration = 1f; // Duration of the color flash
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            m_spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            m_spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.2f;
        }

        // Ensure the color is set back to the original color
        m_spriteRenderer.color = originalColor;
        m_isInvulnerable = false;
    }

    // Chandler: Returns the current health of the enemy
    public int GetCurrentHealth()
    {
        return m_health;
    }

    // Chandler: Handle the death behavior of the enemy
    public void HandleDeath()
    {
        // Disable movement and components
        m_canMove = false;
        m_pRb.linearVelocity = Vector2.zero;            // Ensure the Rigidbody2D velocity is set to zero

        // Disable all colliders to prevent further collisions
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Deactivate the child GameObject
        Transform childTransform = transform.Find("Held Item");     // If the Skeleton is holding an item
        if (childTransform != null)
        {
            childTransform.gameObject.SetActive(false);
        }

        // Trigger the death animation
        m_animator.SetTrigger("Death");
    }

    // Chandler: Animation event to finalize the enemy's death behavior
    public void OnDeath()
    {
        DropItem();
        Destroy(gameObject);
    }

    // Chandler: Pause the Entity during the Knockback
    public void PauseEntity(bool flag)
    {
        m_canMove = !flag;
    }

    // Chandler: Stun method to pause the entity for a specified duration
    public void Stun(float duration)
    {
        if (!m_isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    // Chandler: The duration of being stunned
    private IEnumerator StunCoroutine(float duration)
    {
        m_isStunned = true;
        PauseEntity(true);
        yield return new WaitForSeconds(duration);
        PauseEntity(false);
        m_isStunned = false;
    }

    // Chandler: Drops a random item (heart, rupee, bomb, or nothing) when the enemy is destroyed
    private void DropItem()
    {
        // If the Enemy is holding an item, drop it
        if (m_heldItem != null)
        {
            Instantiate(m_heldItem, transform.position, Quaternion.identity);
        }
        else
        {
            if (m_playerController != null && m_playerController.AtFullHealth())
            {
                // Player is at full health, exclude heart from drop chance
                float dropChance = Random.value; // Random value between 0 and 1

                if (dropChance < 0.33f) // 1/3 chance
                {
                    Instantiate(m_rupeePrefab, transform.position, Quaternion.identity);
                }
                else if (dropChance < 0.66f) // 1/3 chance
                {
                    Instantiate(m_bombPrefab, transform.position, Quaternion.identity);
                }

                // 1/3 chance for 'nothing'
            }
            else
            {
                // Normal drop chance including heart
                float dropChance = Random.value; // Random value between 0 and 1

                if (dropChance < 0.25f) // 1/4 chance
                {
                    Instantiate(m_heartPrefab, transform.position, Quaternion.identity);
                }
                else if (dropChance < 0.5f) // 1/4 chance
                {
                    Instantiate(m_rupeePrefab, transform.position, Quaternion.identity);
                }
                else if (dropChance < 0.75f) // 1/4 chance
                {
                    Instantiate(m_bombPrefab, transform.position, Quaternion.identity);
                }

                // 1/4 chance for 'nothing'
            }
        }
    }
}