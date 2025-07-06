#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class GoriyaEnemy : MonoBehaviour
{
    private PlayerController m_playerController;

    [Header("Movement Settings")]
    public float MoveSpeed = 4.0f;
    public float StepLength = 5.0f;
    public float FlipInterval = 0.08f;  // Time interval for toggling sprite flip while moving vertically
    public float MinDirectionChangeTime = 1f; // Min time before changing direction
    public float MaxDirectionChangeTime = 5f; // Max time before changing direction

    [Header("Boomerang Settings")]
    public GameObject BoomerangPrefab;      // Prefab for the boomerang
    public Transform BoomerangSpawnPoint;   // Spawn point for the boomerang
    public float MinBoomerangCooldown = 3f; // Min cooldown between boomerang throws
    public float MaxBoomerangCooldown = 6f; // Max cooldown between boomerang throws

    [Header("Other Settings")]
    public LayerMask ObstacleLayer; // Layer mask for detecting obstacles

    public event System.Action OnEnemyDestroyed; // Triggered when the enemy is destroyed

    [Header("Item Drop Settings")]
    [SerializeField] private GameObject m_heartPrefab;
    [SerializeField] private GameObject m_rupeePrefab;
    [SerializeField] private GameObject m_bombPrefab;

    // Variables
    private int m_health = 3;
    private Rigidbody2D m_pRb;
    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    private Vector2 m_randomDirection;
    private float m_nextDirectionChangeTime;
    private float m_nextBoomerangTime;
    private float m_nextFlipTime;
    private bool m_isThrowingBoomerang;
    private bool m_isStunned = false;
    private bool m_canMove = false;

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

        // Setting initial direction and timing
        ChooseValidRandomDirection();
        SetNextDirectionChangeTime();
        m_nextFlipTime = Time.time + FlipInterval;

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
        if (m_canMove && !m_isKnockedBack && !m_isThrowingBoomerang)
        {
            HandleDirectionChange();

            // Check if it's time to throw the boomerang
            if (Time.time >= m_nextBoomerangTime)
            {
                ThrowBoomerang();
            }
        }

        // Handle vertical sprite flip effect
        HandleUpDownTogglingEffect();
    }

    private void FixedUpdate()
    {
        // Move the enemy if not throwing a boomerang
        if (m_canMove && !m_isKnockedBack && !m_isThrowingBoomerang)
        {
            Move();
            ClampPositionWithinBounds();
        }
    }

    private void ThrowBoomerang()
    {
        if (BoomerangPrefab == null)
        {
            Debug.LogError("BoomerangPrefab is not assigned!");
            return;
        }

        m_isThrowingBoomerang = true;
        m_animator.SetTrigger("Throw");

        // Instantiating the boomerang
        GameObject boomerang = Instantiate(BoomerangPrefab, BoomerangSpawnPoint.position, Quaternion.identity, transform);
        Vector2 throwDirection = m_randomDirection == Vector2.zero ? Vector2.right : m_randomDirection;
        GoriyaBoomerang boomerangScript = boomerang.GetComponent<GoriyaBoomerang>();
        if (boomerangScript != null)
        {
            boomerangScript.Initialize(throwDirection, this);
        }
        else
        {
            Debug.LogError("BoomerangPrefab does not have a GoriyaBoomerang script attached!");
        }

        // Sets the next boomerang throw time
        m_nextBoomerangTime = Time.time + Random.Range(MinBoomerangCooldown, MaxBoomerangCooldown);
    }

    public void OnBoomerangReturned()
    {
        // Resets throwing flag when the boomerang returns
        m_isThrowingBoomerang = false;
    }

    private void HandleDirectionChange()
    {
        // Change direction if the time has come
        if (Time.time >= m_nextDirectionChangeTime)
        {
            ChooseValidRandomDirection();
            SetNextDirectionChangeTime();
        }
    }

    private void Move()
    {
        // Move in the current direction
        Vector2 moveDirection = m_randomDirection * MoveSpeed * Time.fixedDeltaTime;
        m_pRb.MovePosition(m_pRb.position + moveDirection);

        UpdateAnimatorAndSpriteDirection();
    }

    private void ChooseValidRandomDirection()
    {
        // Find a valid random direction
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 randomDirection = directions[Random.Range(0, directions.Length)];
            if (IsDirectionValid(randomDirection))
            {
                m_randomDirection = randomDirection;
                UpdateAnimatorAndSpriteDirection();
                return;
            }
        }

        m_randomDirection = Vector2.zero;
    }

    private bool IsDirectionValid(Vector2 direction)
    {
        // Check if the direction is valid
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, StepLength, ObstacleLayer);
        return hit.collider == null;
    }

    private void UpdateAnimatorAndSpriteDirection()
    {
        // Update animator parameters based on movement direction
        if (m_animator != null)
        {
            m_animator.SetFloat("MoveX", m_randomDirection.x);
            m_animator.SetFloat("MoveY", m_randomDirection.y);
        }

        // Flip the sprite horizontally
        if (m_randomDirection.x != 0)
        {
            m_spriteRenderer.flipX = m_randomDirection.x < 0;
        }
    }

    private void HandleUpDownTogglingEffect()
    {
        // Flip the sprite vertically if moving up or down
        if (m_randomDirection.y != 0 && Time.time >= m_nextFlipTime)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
            m_nextFlipTime = Time.time + FlipInterval;
        }
    }

    private void SetNextDirectionChangeTime()
    {
        m_nextDirectionChangeTime = Time.time + Random.Range(MinDirectionChangeTime, MaxDirectionChangeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Choose a new direction upon collision
        if (collision.collider != null)
        {
            ChooseValidRandomDirection();
        }
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
                ChooseValidRandomDirection();
                SetNextDirectionChangeTime();
            }

            transform.position = clampedPosition;
        }
    }

    // CHandler: Message Broadcast of the Player's death
    public void OnPlayerDeath()
    {
        // Destroy self when player dies
        Destroy(gameObject);
    }

    // CHandler: Message Broadcast of the Goriya taking damage
    public void OnHit(int damage)
    {
        Debug.Log("Goriya taking damage: " + damage);
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
        m_pRb.linearVelocity = Vector2.zero;
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        m_animator.SetFloat("MoveX", 0);
        m_animator.SetFloat("MoveY", 0);

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

        if (flag)
        {
            m_animator.SetFloat("MoveX", 0);
            m_animator.SetFloat("MoveY", 0);
        }
    }

    // Stun method to pause the entity for a specified duration
    public void Stun(float duration)
    {
        if (!m_isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        m_isStunned = true;
        PauseEntity(true);
        yield return new WaitForSeconds(duration);
        PauseEntity(false);
        m_isStunned = false;
    }

    // Drops a random item (heart, rupee, bomb, or nothing) when the enemy is destroyed
    private void DropItem()
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