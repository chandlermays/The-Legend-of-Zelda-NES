#define DEBUG_LOG

using UnityEngine;
using System.Collections;

public class OctorokEnemy : MonoBehaviour
{
    private PlayerController m_playerController;

    public float MoveSpeed = 1f;
    public GameObject RockProjectilePrefab;
    public float StopDurationBeforeShoot = 1f;
    public float MinShootInterval = 2f;
    public float MaxShootInterval = 5f;
    public event System.Action OnEnemyDestroyed;

    [Header("Item Drop Settings")]
    [SerializeField] private GameObject m_heartPrefab;
    [SerializeField] private GameObject m_rupeePrefab;
    [SerializeField] private GameObject m_bombPrefab;

    private int m_health = 1;

    private Vector2 m_movementDirection;
    private float m_changeDirectionTime;
    private float m_shootTime;
    private Rigidbody2D m_pRb;
    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    private bool m_isShooting = false;

    private BoxCollider2D m_sectionBounds;

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
        m_pRb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
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
        m_changeDirectionTime = Time.time + Random.Range(0.5f, 5f);
        SetNextShootTime();

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
        if (m_canMove)
        {
            HandleDirectionChange();

            if (!m_isShooting && Time.time >= m_shootTime)
            {
                StartCoroutine(StopAndShoot());
                SetNextShootTime();
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_canMove && !m_isShooting)
        {
            Move();
            ClampPositionWithinBounds();
        }
        else
        {
            m_pRb.linearVelocity = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        m_pRb.linearVelocity = m_movementDirection * MoveSpeed;
    }

    private void OnDisable()
    {
        m_pRb.linearVelocity = m_movementDirection * 0;
    }

    private void OnDestroy()
    {
        OnEnemyDestroyed?.Invoke();
    }

    // Moves the enemy
    private void Move()
    {
        m_pRb.linearVelocity = m_movementDirection * MoveSpeed;

        UpdateSpriteDirection();
    }

    // Changes direction on a timer
    private void HandleDirectionChange()
    {
        if (!m_isShooting && Time.time > m_changeDirectionTime)
        {
            ChooseRandomDirection();
            m_changeDirectionTime = Time.time + Random.Range(1f, 3f);
        }
    }

    // Chooses a new random direction
    private void ChooseRandomDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        m_movementDirection = directions[Random.Range(0, directions.Length)];

        UpdateSpriteDirection();
    }

    // Updates the sprite and animator to match the movement direction
    private void UpdateSpriteDirection()
    {
        SetAnimatorDirection(m_movementDirection.x, m_movementDirection.y);

        if (m_movementDirection.x != 0)
        {
            m_spriteRenderer.flipX = m_movementDirection.x > 0;
            m_spriteRenderer.flipY = false;
        }
        else if (m_movementDirection.y != 0)
        {
            m_spriteRenderer.flipX = false;
            m_spriteRenderer.flipY = m_movementDirection.y > 0;
        }
    }

    // Sets the animator parameters based on movement direction
    private void SetAnimatorDirection(float moveX, float moveY)
    {
        if (m_animator != null)
        {
            m_animator.SetFloat("MoveX", Mathf.Abs(moveX));
            m_animator.SetFloat("MoveY", Mathf.Abs(moveY));
        }
    }

    // Stops the enemy, waits, and shoots a projectile
    private IEnumerator StopAndShoot()
    {
        if (!m_canMove) yield break;

        m_isShooting = true;
        m_pRb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(StopDurationBeforeShoot);
        ShootProjectile();
        m_isShooting = false;
    }

    // Shoots projectile in current direction
    private void ShootProjectile()
    {
        if (!m_canMove) return;

        GameObject projectile = Instantiate(RockProjectilePrefab, transform.position, Quaternion.identity);
        projectile.transform.SetParent(transform); // Set the projectile as a child of the Octorok
        projectile.GetComponent<RockProjectile>()?.SetDirection(m_movementDirection);
    }

    // Sets the time for the next shooting event
    private void SetNextShootTime()
    {
        m_shootTime = Time.time + Random.Range(MinShootInterval, MaxShootInterval);
    }

    // Chooses a new random direction
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null)
        {
            ChooseRandomDirection();
        }
    }

    // Chandler: Clamps the enemy's position within the section bounds
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

    // Chandler: Message Broadcast of the Octorok taking damage
    public void OnHit(int damage)
    {
        Debug.Log("Octorok taking damage: " + damage);
        TakeDamage(damage);
    }

    // Chandler: Takes damage and destroys the enemy if health is zero
    public void TakeDamage(int damage)
    {
        m_health -= damage;
        if (m_health <= 0)
        {
            HandleDeath();

#if DEBUG_LOG
            Debug.Log("Octorok destroyed.");
#endif
        }
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

        m_animator.SetTrigger("Death");
    }

    // Chandler: Animation event to finalize the enemy's death behavior
    public void OnDeath()
    {
        DropItem();
        Destroy(gameObject);
    }

    // Chandler: Drops a random item (heart, rupee, bomb, or nothing) when the enemy is destroyed
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