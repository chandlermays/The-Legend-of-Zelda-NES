using UnityEngine;
using System.Collections;

public class AquamentusEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float leftLimit = 2.0f;
    public float rightLimit = 5.0f;

    [Header("Direction Change Settings")]
    public float minDirectionChangeInterval = 2.0f;
    public float maxDirectionChangeInterval = 5.0f;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20.0f;
    public float minShootCooldown = 2.0f;
    public float maxShootCooldown = 5.0f;
    public Transform shootOrigin;

    [Header("Target Settings")]
    public Transform player;

    private int m_health = 4;
    private Vector2 startPosition;
    private bool movingRight = true;
    private Rigidbody2D rb;
    [SerializeField] private GameObject m_heartContainerPrefab;

    private bool m_isKnockedBack = false;
    public bool IsKnockedBack
    {
        get { return m_isKnockedBack; }
        set { m_isKnockedBack = value; }
    }

    private bool m_isStunned = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing");
        }

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        startPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player not found in the scene. Assign the player in AquamentusEnemy");
            }
        }

        if (projectilePrefab != null && shootOrigin != null && player != null)
        {
            StartCoroutine(ShootingRoutine());
        }
        else
        {
            Debug.LogWarning("ProjectilePrefab, ShootOrigin, or Player is not assigned.");
        }

        StartCoroutine(DirectionChangeRoutine());
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        float moveDelta = moveSpeed * Time.deltaTime * (movingRight ? 1 : -1);
        transform.Translate(moveDelta, 0, 0);

        float distanceFromStart = transform.position.x - startPosition.x;
        if (movingRight && distanceFromStart >= rightLimit)
        {
            movingRight = false;
        }
        else if (!movingRight && distanceFromStart <= -leftLimit)
        {
            movingRight = true;
        }
    }

    private IEnumerator DirectionChangeRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minDirectionChangeInterval, maxDirectionChangeInterval);
            yield return new WaitForSeconds(waitTime);
            movingRight = !movingRight;
        }
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            ShootProjectiles();
            float cooldown = Random.Range(minShootCooldown, maxShootCooldown);
            yield return new WaitForSeconds(cooldown);
        }
    }

    private void ShootProjectiles()
    {
        if (projectilePrefab == null || shootOrigin == null || player == null)
        {
            Debug.LogWarning("Cannot shoot projectiles. Prefab, origin, or player not assigned.");
            return;
        }
        StartCoroutine(ShootProjectilesCoroutine());
    }

    private IEnumerator ShootProjectilesCoroutine()
    {
        // Instantiate top and bottom projectiles
        GameObject topProj = Instantiate(projectilePrefab, shootOrigin.position, Quaternion.identity);
        GameObject bottomProj = Instantiate(projectilePrefab, shootOrigin.position, Quaternion.identity);

        // Flags to check if movement is done
        bool topDone = false;
        bool bottomDone = false;

        IEnumerator MoveTop()
        {
            yield return StartCoroutine(MoveProjectile(topProj, shootOrigin.position + Vector3.up * 0.5f, 0.1f));
            topDone = true;
        }

        IEnumerator MoveBottom()
        {
            yield return StartCoroutine(MoveProjectile(bottomProj, shootOrigin.position + Vector3.down * 0.5f, 0.1f));
            bottomDone = true;
        }

        // Start both movement
        StartCoroutine(MoveTop());
        StartCoroutine(MoveBottom());

        // Wait until both top and bottom projectiles have moved
        yield return new WaitUntil(() => topDone && bottomDone);

        // Set directions for top and bottom projectiles
        AquamentusProjectile topScript = topProj.GetComponent<AquamentusProjectile>();
        AquamentusProjectile bottomScript = bottomProj.GetComponent<AquamentusProjectile>();

        Vector2 directionToPlayer = (player.position - shootOrigin.position).normalized;

        if (topScript != null)
        {
            topScript.direction = Quaternion.Euler(0, 0, 15f) * directionToPlayer;
            topScript.moveSpeed = projectileSpeed;
        }

        if (bottomScript != null)
        {
            bottomScript.direction = Quaternion.Euler(0, 0, -15f) * directionToPlayer;
            bottomScript.moveSpeed = projectileSpeed;
        }

        GameObject middleProj = Instantiate(projectilePrefab, shootOrigin.position, Quaternion.identity);
        AquamentusProjectile middleScript = middleProj.GetComponent<AquamentusProjectile>();

        if (middleScript != null)
        {
            middleScript.direction = directionToPlayer;
            middleScript.moveSpeed = projectileSpeed;
        }

        float rotationZMiddle = Mathf.Atan2(middleScript.direction.y, middleScript.direction.x) * Mathf.Rad2Deg;
        middleProj.transform.rotation = Quaternion.Euler(0, 0, rotationZMiddle);
    }

    private IEnumerator MoveProjectile(GameObject proj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = proj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            proj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        proj.transform.position = targetPos;
    }

    public void TakeDamage(int damage)
    {
        m_health -= damage;
        if (m_health <= 0)
        {
            DropHeartContainer();
            Destroy(gameObject);

#if DEBUG_LOG
            Debug.Log("Aquamentus destroyed.");
#endif
        }
    }

    // Message Broadcast of the Aquamantus taking damage
    public void OnHit(int damage)
    {
        Debug.Log("Aquamantus taking damage: " + damage);
        TakeDamage(damage);
    }

    // Pause the Entity
    public void PauseEntity(bool flag)
    {
        m_isKnockedBack = flag;
        if (flag)
        {
            rb.linearVelocity = Vector2.zero;
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

    // Message Broadcast of the Player's death
    public void OnPlayerDeath()
    {
        // Destroy self when player dies
        Destroy(gameObject);
    }

    private void DropHeartContainer()
    {
        if (m_heartContainerPrefab != null)
        {
            Instantiate(m_heartContainerPrefab, transform.position, Quaternion.identity);
        }
    }
}