#define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PlayerController;

public class PlayerController : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent m_playerDeathEvent;

    private enum SpecialWeapon
    {
        kNone,
        kBoomerang,
        kBow,
        kBomb
    }

    private SpecialWeapon m_currentSpecialWeapon = SpecialWeapon.kNone;         // The Current Special Weapon

    private Rigidbody2D m_rigidbody2D;                                          // Reference to the Rigidbody2D component
    private Animator m_animator;                                                // Reference to the Animator component
    private SpriteRenderer m_spriteRenderer;                                    // Reference to the SpriteRenderer component
    Collider2D m_swordCollider;                                                 // Reference to the Sword Collider

    [SerializeField] private GameObject m_swordHitbox;                          // Reference to the Sword Hitbox GameObject
    [SerializeField] private ItemData m_swordData;                              // Reference to the Sword Item Data
    [SerializeField] private ItemData m_boomerangData;                          // Reference to the Boomerang Item Data
    [SerializeField] private ItemData m_bowData;                                // Reference to the Bow Item Data
    [SerializeField] private ItemData m_arrowData;                              // Reference to the Arrow Item Data
    [SerializeField] private ItemData m_bombData;                               // Reference to the Bomb Item Data
    [SerializeField] private GameObject m_key;                                  // Reference to the Key Prefab
    [SerializeField] private GameObject m_heartContainer;                       // Reference to the Heart Container Prefab
    [SerializeField] private GameObject m_triforce;                             // Reference to the Triforce Prefab

    [SerializeField] private Transform m_itemHoldPosition;

    [SerializeField] private float m_moveSpeed = 5.0f;                          // The Speed of the Player's Movement
    Vector2 m_movement;                                                         // The Movement Vector

    public enum FacingDirection
    {
        kUp,
        kDown,
        kLeft,
        kRight
    }

    private FacingDirection m_facingDirection = FacingDirection.kDown;

    private float m_lastHorizontal = 0;                                         // The Last Horizontal Movement
    private float m_lastVertical = 0;                                           // The Last Vertical Movement
    private bool m_isAttacking = false;                                         // The Flag for Attacking
    private bool m_isPaused = false;                                            // The Flag for Pausing
    private bool m_canMove = true;                                              // The Flag for Movement

    private bool m_hasSword = false;                                            // The Flag for having a Sword
    private bool m_swordProjectileActive = false;                               // The Flag for active sword projectile
    private bool m_hasBoomerang = false;                                        // The Flag for having a Boomerang
    private bool m_boomerangActive = false;                                     // The Flag for active boomerang
    private bool m_hasBow = false;                                              // The Flag for having a Bow
    private bool m_hasBomb = false;                                             // The Flag for having a Bomb
    private bool m_hasArrow = false;                                            // The Flag for having an Arrow
    private bool m_arrowProjectileActive = false;                               // The Flag for active arrow projectile

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

    private GameObject m_itemPickedUp;                                          // The Item Picked Up
    public GameHudUpdater m_hudUpdater;

    [SerializeField] private float m_maxHealth = 3;                             // The Maximum Health of the Player
    [SerializeField] private float m_currentHealth = 3;                         // The Current Health of the Player

    private bool m_isDead = false;

    private int m_rupees = 0;                                                   // The Amount of Rupees the Player has
    private int m_keys = 0;                                                     // The Amount of Keys the Player has
    private int m_bombs = 0;                                                    // The Amount of Bombs the Player has

    public bool m_isInDungeon { get; set; }                                     // The Flag for being in a Dungeon

    public enum MovementMode
    {
        kTopDown,
        kPlatformer
    }

    private MovementMode m_movementMode = MovementMode.kTopDown;                 // The Movement Mode of the Player
    private bool m_isOnStairs = false;                                           // The Flag for being on Stairs

    /* ----------------------------------- */

    ///////////////////////////
    /*-------------------------
    | --- Private Methods --- |
    -------------------------*/
    ///////////////////////////

    /*-----------------------------------------------
    | --- Start: Called just before any Updates --- |
    -----------------------------------------------*/

    private void Start()
    {
        // Get the Components for the Player
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_swordCollider = m_swordHitbox.GetComponent<Collider2D>();

        m_movementMode = MovementMode.kTopDown; // Ensure the default movement mode

        // Set true/false to enable/disable testing
        // bool m_saveTesting = true;

        //if (m_saveTesting)
        //{
        //    // Check if the player has the sword
        //    if (PlayerPrefs.GetInt("HasSword", 0) == 1)
        //    {
        //        m_hasSword = true;
        //    }
        //}
    }

    /*--------------------------------------------
    | --- Update: Called upon once per frame --- |
    --------------------------------------------*/

    private void Update()
    {
        if (m_canMove && !m_isDead)
        {
            switch (m_movementMode)
            {
                case MovementMode.kTopDown:
                    HandleTopDownMovement();
                    break;

                case MovementMode.kPlatformer:
                    HandlePlatformerMovement();
                    break;
            }
        }

        if (!m_isPaused && !m_isDead)
        {
            // Attack Handling
            if (m_hasSword && Input.GetKeyDown(KeyCode.Space) && !m_isAttacking)
            {
                m_isAttacking = true;       // Set attacking flag to true
                m_canMove = false;          // Disable movement while attacking
                m_animator.speed = 1;       // Ensure speed is normal for the attack animation

                m_animator.SetFloat("Horizontal", m_lastHorizontal);
                m_animator.SetFloat("Vertical", m_lastVertical);

                // Check if Link has full health and throw the sword if so
                if (IsAtFullHealth() && !m_swordProjectileActive)
                {
                    m_animator.SetTrigger("Throw");
                    ThrowSword();
                }
                else
                {
                    m_animator.SetTrigger("Attack");
                }
            }

            // Handle special weapon usage
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                UseSpecialWeapon();
            }
        }
    }

    /*--------------------------------------------------------------------------
    | --- HandleTopDownMovement: The Player's Movement for a Top-Down Mode --- |
    --------------------------------------------------------------------------*/

    private void HandleTopDownMovement()
    {
        // Input Handling
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical != 0)
        {
            m_movement.x = 0;
            m_movement.y = vertical;

            // Update facing direction
            m_facingDirection = vertical > 0 ? FacingDirection.kUp : FacingDirection.kDown;
        }
        else if (horizontal != 0)
        {
            m_movement.x = horizontal;
            m_movement.y = 0;

            // Update facing direction
            m_facingDirection = horizontal > 0 ? FacingDirection.kRight : FacingDirection.kLeft;
        }
        else
        {
            m_movement.x = 0;
            m_movement.y = 0;
        }

        // Animation Handling
        if (!m_isAttacking) // Only control movement animation if not attacking
        {
            if (horizontal != 0 || vertical != 0)
            {
                // Update the Animator parameters
                m_animator.SetFloat("Horizontal", horizontal);
                m_animator.SetFloat("Vertical", vertical);

                // Store the last direction of movement
                m_lastHorizontal = horizontal;
                m_lastVertical = vertical;

                // Resume animation playback
                m_animator.speed = 1;
            }
            else
            {
                // No movement input, so hold the last direction
                m_animator.SetFloat("Horizontal", m_lastHorizontal);
                m_animator.SetFloat("Vertical", m_lastVertical);

                // Pause animation playback to freeze on the last frame
                m_animator.speed = 0;
            }
        }
    }

    /*-------------------------------------------------------------------------------
    | --- HandlePlatformerMovement: The Player's Movement for a Platformer Mode --- |
    -------------------------------------------------------------------------------*/

    private void HandlePlatformerMovement()
    {
        // Input Handling
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = m_isOnStairs ? Input.GetAxisRaw("Vertical") : 0;

        m_movement.x = horizontal;
        m_movement.y = vertical; // Allow vertical movement if on stairs

        // Update facing direction
        if (horizontal != 0)
        {
            m_facingDirection = horizontal > 0 ? FacingDirection.kRight : FacingDirection.kLeft;
        }
        else if (vertical != 0)
        {
            m_facingDirection = vertical > 0 ? FacingDirection.kUp : FacingDirection.kDown;
        }

        // Animation Handling
        if (!m_isAttacking) // Only control movement animation if not attacking
        {
            if (horizontal != 0 || vertical != 0)
            {
                // Update the Animator parameters
                m_animator.SetFloat("Horizontal", horizontal);
                m_animator.SetFloat("Vertical", vertical);

                // Store the last direction of movement
                m_lastHorizontal = horizontal;
                m_lastVertical = vertical;

                // Resume animation playback
                m_animator.speed = 1;
            }
            else
            {
                // No movement input, so hold the last direction
                m_animator.SetFloat("Horizontal", m_lastHorizontal);
                m_animator.SetFloat("Vertical", m_lastVertical);

                // Pause animation playback to freeze on the last frame
                m_animator.speed = 0;
            }
        }
    }

    /*-----------------------------------------------------------------
    | --- IsAtFullHealth: Returns if the Player is at Full Health --- |
    -----------------------------------------------------------------*/

    private bool IsAtFullHealth()
    {
        return m_currentHealth == m_maxHealth;
    }

    /*---------------------------------------------------------------
    | --- ThrowSword: The Action of Throwing a Sword Projectile --- |
    ---------------------------------------------------------------*/

    private void ThrowSword()
    {
        if (m_hasSword)
        {
            // Get the direction based on the facing direction
            Vector2 direction = Vector2.zero;
            switch (m_facingDirection)
            {
                case FacingDirection.kUp:
                    direction = Vector2.up;
                    break;
                case FacingDirection.kDown:
                    direction = Vector2.down;
                    break;
                case FacingDirection.kLeft:
                    direction = Vector2.left;
                    break;
                case FacingDirection.kRight:
                    direction = Vector2.right;
                    break;
            }

            // Instantiate the projectile and initialize it with the direction
            GameObject projectile = Instantiate(m_swordData.m_weaponPrefab, transform.position, Quaternion.identity);

            // Set rotation based on direction
            if (direction == Vector2.up)
            {
                // Facing up (no rotation needed)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (direction == Vector2.down)
            {
                // Facing down (rotate 180 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (direction == Vector2.left)
            {
                // Facing left (rotate 90 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (direction == Vector2.right)
            {
                // Facing right (rotate -90 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, -90);
            }

            projectile.GetComponent<SwordProjectile>().Initialize(direction, this);
            m_swordProjectileActive = true; // Set the flag to indicate an active projectile
        }
    }

    /*------------------------------------------------------------
    | --- ThrowBoomerang: The Action of Throwing a Boomerang --- |
    ------------------------------------------------------------*/

    private void ThrowBoomerang()
    {
        // Get the direction based on the last movement
        Vector2 direction = new(m_lastHorizontal, m_lastVertical);

        // Instantiate the boomerang and initialize it with the direction
        GameObject boomerang = Instantiate(m_boomerangData.m_weaponPrefab, transform.position, Quaternion.identity);
        Boomerang boomerangScript = boomerang.GetComponent<Boomerang>();
        if (boomerangScript != null)
        {
            boomerangScript.Initialize(direction, this);
        }
        else
        {
            Debug.LogError("Boomerang script not found on the instantiated boomerang prefab.");
        }
        m_boomerangActive = true; // Set the flag to indicate an active boomerang
    }

    /*-----------------------------------------------------
    | --- ShootArrow: The Action of Shooting an Arrow --- |
    -----------------------------------------------------*/

    private void ShootArrow()
    {
        if (!m_hasArrow)
        {
            Debug.Log("Must have Arrow!");
            return;
        }
        if (m_rupees > 0)
        {
            // Deduct the cost of shooting an arrow
            m_rupees--;
            m_hudUpdater.AddRupeeAmount(-1);

            // Get the direction based on the last movement
            Vector2 direction = new(m_lastHorizontal, m_lastVertical);

            // Instantiate the projectile and initialize it with the direction
            GameObject projectile = Instantiate(m_bowData.m_weaponPrefab, transform.position, Quaternion.identity);

            // Set rotation based on direction
            if (direction == Vector2.up)
            {
                // Facing up (no rotation needed)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (direction == Vector2.down)
            {
                // Facing down (rotate 180 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (direction == Vector2.left)
            {
                // Facing left (rotate 90 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (direction == Vector2.right)
            {
                // Facing right (rotate -90 degrees)
                projectile.transform.rotation = Quaternion.Euler(0, 0, -90);
            }

            projectile.GetComponent<ArrowProjectile>().Initialize(direction, this);
            m_arrowProjectileActive = true; // Set the flag to indicate an active projectile
        }
        else
        {
#if DEBUG_LOG
            Debug.Log("Cannot shoot arrow. No rupees left.");
#endif
        }
    }

    /*-------------------------------------------------
    | --- PlaceBomb: The Action of Placing a Bomb --- |
    -------------------------------------------------*/

    private void PlaceBomb()
    {
        if (m_bombs > 0)
        {
            // Instantiate the bomb at the player's position
            Instantiate(m_bombData.m_weaponPrefab, transform.position, Quaternion.identity);
            m_bombs--; // Decrease the number of bombs
            m_hudUpdater.AddBombAmount(-1);
        }
        else
        {
#if DEBUG_LOG
            Debug.Log("Cannot place bomb. No bombs left.");
#endif
        }
    }

    /*--------------------------------------------------------------------------------------
    | --- OnTriggerEnter2D: Called when this Collider begins touching another Collider --- |
    --------------------------------------------------------------------------------------*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("Player collided with Enemy!");
            // Add your event logic here
        }

        if (other.gameObject.CompareTag("Stairs"))
        {
            m_isOnStairs = true;
        }
    }

    /*------------------------------------------------------------------------------------
    | --- OnTriggerExit2D: Called when this Collider stops touching another Collider --- |
    ------------------------------------------------------------------------------------*/

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Stairs"))
        {
            Debug.Log("Off the stairs.");
            m_isOnStairs = false;
        }
    }

    /*------------------------------------------------------------------
    | --- FixedUpdate: Called upon once per frame on a fixed timer --- |
    ------------------------------------------------------------------*/

    private void FixedUpdate()
    {
        if (m_canMove)
        {
            // Movement Handling
            m_rigidbody2D.MovePosition(m_rigidbody2D.position + m_moveSpeed * Time.fixedDeltaTime * m_movement);
        }
    }

    /*---------------------------------------------------------------------
    | --- UseSpecialWeapon: Handles using the equipped special weapon --- |
    ---------------------------------------------------------------------*/

    private void UseSpecialWeapon()
    {
        switch (m_currentSpecialWeapon)
        {
            case SpecialWeapon.kBoomerang:
                if (!m_boomerangActive)
                {
                    ThrowBoomerang();
                }
                break;

            case SpecialWeapon.kBow:
                if (!m_arrowProjectileActive)
                {
                    ShootArrow();
                }
                break;

            case SpecialWeapon.kBomb:
                PlaceBomb();
                break;

            case SpecialWeapon.kNone:
            default:
                // No special weapon equipped
                break;
        }
    }

    /*--------------------------------------------------------------------------
    | --- FlashDamageColor: Quickly switch between two colors for an event --- |
    --------------------------------------------------------------------------*/

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

    /*-----------------------------------------------------------------------------------------
    | --- BroadcastMessageToLayer: Sends a message to all GameObjects on a specific layer --- |
    -----------------------------------------------------------------------------------------*/

    private void BroadcastMessageToLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"Layer '{layerName}' not found.");
            return;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        GameObject[] objects = FindObjectsOfType<GameObject>();
#pragma warning restore CS0618 // Type or member is obsolete

        foreach (GameObject obj in objects)
        {
            if (obj.layer == layer)
            {
                obj.SendMessage("OnPlayerDeath", SendMessageOptions.DontRequireReceiver);
            }
        }
    }


    //////////////////////////
    /*------------------------
    | --- Public Methods --- |
    ------------------------*/
    //////////////////////////

    /*-------------------------------------------------------------------
    | --- PauseEntity: Toggle the Ability to Move and the Animation --- |
    -------------------------------------------------------------------*/

    public void PauseEntity(bool flag, bool stopAnimation = true)
    {
        Debug.Log("Pause link = " + flag);
        m_isPaused = flag;
        m_canMove = !flag;
        if (stopAnimation)
        {
            m_animator.speed = m_canMove ? 1 : 0;
        }
    }

    /*---------------------------------------------
    | --- PauseAnimation: Pause the Animation --- |
    ---------------------------------------------*/

    public void PauseAnimation(bool flag)
    {
        m_animator.speed = flag ? 0 : 1;
    }

    /*-----------------------------------------------------
    | --- SetPosition: Set the Position of the Player --- |
    -----------------------------------------------------*/

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /*-----------------------------------------------------------------------------
    | --- SetFacingDirection: Set the Direction to which the Player is facing --- |
    -----------------------------------------------------------------------------*/

    public void SetFacingDirection(FacingDirection direction)
    {
        m_facingDirection = direction;

        switch (direction)
        {
            case FacingDirection.kUp:
                m_lastHorizontal = 0;
                m_lastVertical = 1;
                m_animator.SetFloat("Horizontal", 0);
                m_animator.SetFloat("Vertical", 1);
                break;

            case FacingDirection.kDown:
                m_lastHorizontal = 0;
                m_lastVertical = -1;
                m_animator.SetFloat("Horizontal", 0);
                m_animator.SetFloat("Vertical", -1);
                break;

            case FacingDirection.kLeft:
                m_lastHorizontal = -1;
                m_lastVertical = 0;
                m_animator.SetFloat("Horizontal", -1);
                m_animator.SetFloat("Vertical", 0);
                break;

            case FacingDirection.kRight:
                m_lastHorizontal = 1;
                m_lastVertical = 0;
                m_animator.SetFloat("Horizontal", 1);
                m_animator.SetFloat("Vertical", 0);
                break;
        }
    }

    /*--------------------------------------------------------------
    | --- SetMovementMode: Set the Movement Mode of the Player --- |
    --------------------------------------------------------------*/

    public void SetMovementMode(MovementMode mode)
    {
        m_movementMode = mode;
    }

    /*------------------------------------------------------
    | --- PickupItem: The Action of Picking up an Item --- |
    ------------------------------------------------------*/

    public void PickupItem(string itemType)
    {
        bool playPickupAnimation = true;

        switch (itemType)
        {
            case "Sword":
                m_hasSword = true;
                m_itemPickedUp = Instantiate(m_swordData.m_itemPrefab, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                PlayerPrefs.SetInt("HasSword", 1); // Save the state that the player has the sword
                PlayerPrefs.Save();
                break;

            case "Boomerang":
                m_hasBoomerang = true;
                m_itemPickedUp = Instantiate(m_boomerangData.m_itemPrefab, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                break;

            case "Bow":
                m_hasBow = true;
                m_itemPickedUp = Instantiate(m_bowData.m_itemPrefab, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                break;

            case "Arrow":
                m_hasArrow = true;
                m_itemPickedUp = Instantiate(m_arrowData.m_itemPrefab, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                break;

            case "Bomb":
                if (!m_hasBomb)
                {
                    m_hasBomb = true;
                    m_itemPickedUp = Instantiate(m_bombData.m_itemPrefab, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                }
                else
                {
                    playPickupAnimation = false;
                }
                ++m_bombs;
                m_hudUpdater.AddBombAmount(1);
                break;

            case "Key":
                m_itemPickedUp = Instantiate(m_key, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                ++m_keys;
                m_hudUpdater.AddKeyAmount(1);
                break;

            case "HeartContainer":
                m_maxHealth += 1;
                m_currentHealth += 1;
                m_hudUpdater.AddMaxHealth(1);
                m_itemPickedUp = Instantiate(m_heartContainer, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                break;

            case "Triforce":
                m_itemPickedUp = Instantiate(m_triforce, m_itemHoldPosition.position, Quaternion.identity, m_itemHoldPosition);
                m_hudUpdater.AddTriforcePiece();
                break;

            case "Rupee":
                m_rupees += 1;
                m_hudUpdater.AddRupeeAmount(1);
                playPickupAnimation = false;
                break;

            case "Heart":
                Debug.Log("Heart Obtained!");
                if (m_currentHealth < m_maxHealth)
                {
                    m_currentHealth += 1;
                    m_hudUpdater.UpdateHearts(m_currentHealth, true);
                }
                playPickupAnimation = false;
                break;
        }

        m_hudUpdater.NewItemAcquired(itemType);

        if (playPickupAnimation)
        {
            m_animator.SetTrigger("Pickup");

#if DEBUG_LOG
            Debug.Log("Picking up item: " + itemType);
#endif
        }
        else
        {
#if DEBUG_LOG
            Debug.Log("Picked up item without animation: " + itemType);
#endif
        }
    }

    /*--------------------------------------------
    | --- LoadItem: Load the Item to the HUD --- |
    --------------------------------------------*/

    public void LoadItem(string itemType)
    {
        bool newItem = false;
        switch (itemType)
        {
            case "Sword":
                m_hasSword = true;
                newItem = true;
                break;

            case "Boomerang":
                m_hasBoomerang = true;
                newItem = true;
                break;

            case "Arrow":
                m_hasArrow = true;
                newItem = true;
                break;

            case "Bomb":
                m_hasBomb = true;
                newItem = true;
                break;

            case "Bow":
                m_hasBow = true;
                newItem = true;
                break;

            default:
                break;
        }

        if (newItem)
            m_hudUpdater.NewItemAcquired(itemType);
    }

    /*-------------------------------------------------
    | --- EquipItem: Switch current equipped item --- |
    -------------------------------------------------*/

    public void EquipItem(string itemType)
    {
        bool equipped = false;

        if (itemType == "Boomerang")
        {
            if (m_hasBoomerang)
            {
                m_currentSpecialWeapon = SpecialWeapon.kBoomerang;
                equipped = true;
            }
            else
            {
                m_currentSpecialWeapon = SpecialWeapon.kNone;
            }
        }
        else if (itemType == "Bomb")
        {
            if (m_hasBomb)
            {
                m_currentSpecialWeapon = SpecialWeapon.kBomb;
                equipped = true;
            }
            else
            {
                m_currentSpecialWeapon = SpecialWeapon.kNone;
            }
        }
        else if (itemType == "Bow")
        {
            if (m_hasBow)
            {
                m_currentSpecialWeapon = SpecialWeapon.kBow;
                equipped = true;
            }
            else
            {
                m_currentSpecialWeapon = SpecialWeapon.kNone;
            }
        }
        else
        {
            m_currentSpecialWeapon = SpecialWeapon.kNone;
        }

        if (equipped)
        {
            m_hudUpdater.ChangedEquippedItem(itemType);
        }
    }

    /*------------------------------------------------
    | --- HasKey: Checks if the Player has a Key --- |
    ------------------------------------------------*/

    public bool HasKey()
    {
        return m_keys > 0;
    }

    /*-------------------------------------------
    | --- UseKey: The Action of using a Key --- |
    -------------------------------------------*/

    public void UseKey()
    {
        if (m_keys > 0)
        {
            m_keys--;
            m_hudUpdater.AddKeyAmount(-1);
#if DEBUG_LOG
            Debug.Log("Key used. Keys remaining: " + m_keys);
#endif
        }
    }

    /*-----------------------------------------------------------------------------------
    | --- OnSwordProjectileDestroyed: Called when the sword projectile is destroyed --- |
    -----------------------------------------------------------------------------------*/

    public void OnSwordProjectileDestroyed()
    {
        m_swordProjectileActive = false; // Reset the flag when the projectile is destroyed
    }

    /*-----------------------------------------------------------------------------------
    | --- OnBoomerangDestroyed: Called when the boomerang is destroyed or returned --- |
    -----------------------------------------------------------------------------------*/

    public void OnBoomerangDestroyed()
    {
        m_boomerangActive = false; // Reset the flag when the boomerang is destroyed or returned
    }

    /*-----------------------------------------------------------------------------------
    | --- OnArrowProjectileDestroyed: Called when the arrow projectile is destroyed --- |
    -----------------------------------------------------------------------------------*/

    public void OnArrowProjectileDestroyed()
    {
        m_arrowProjectileActive = false; // Reset the flag when the projectile is destroyed
    }

    /*--------------------------------------------------------------------------------
    | --- OnInventoryStatusChanged: Message Broadcast when the Inventory is Open --- |
    --------------------------------------------------------------------------------*/

    public void OnInventoryStatusChanged(bool inventoryOpen)
    {
        PauseEntity(inventoryOpen, true);
    }

    /*--------------------------------------------------------------
    | --- AtFullHealth: Checks if the Player is at full health --- |
    --------------------------------------------------------------*/

    public bool AtFullHealth()
    {
        return m_currentHealth == m_maxHealth;
    }

    /*--------------------------------------------------------------
    | --- OnHit: Message Broadcast of the Player taking damage --- |
    --------------------------------------------------------------*/

    public void OnHit(int damage)
    {
        TakeDamage(damage);
    }

    /*---------------------------------------------------
    | --- TakeDamage: The Behavior of taking Damage --- |
    ---------------------------------------------------*/

    public void TakeDamage(float damage)
    {
        if (m_currentHealth <= 0 || m_isInvulnerable) return; // Prevent taking damage when already at zero health

        m_currentHealth -= damage; // Reduce health

#if DEBUG_LOG
        Debug.Log($"Player took {damage} damage! Current health: {m_currentHealth}");
#endif

        m_hudUpdater.UpdateHearts(m_currentHealth, true);

        if (m_currentHealth <= 0)
        {
            HandleDeath();
        }
        else
        {
            // Start the coroutine to handle the color change only if the player is still alive
            StartCoroutine(FlashDamageColor());
        }
    }

    /*----------------------------------------------------
    | --- HandleDeath: The Behavior when Player dies --- |
    ----------------------------------------------------*/

    public void HandleDeath()
    {
        m_isDead = true;
        Debug.Log("Player is dead!");

        BroadcastMessageToLayer("Enemy");
        BroadcastMessageToLayer("Enemy Projectile");
        BroadcastMessageToLayer("Player Projectile");
        BroadcastMessageToLayer("Sections");

        m_canMove = false;                              // Disable player movement
        m_rigidbody2D.linearVelocity = Vector2.zero;    // Stop the player from moving when dead (i.e. physics)
        m_animator.SetTrigger("Death");                 // Trigger death animation
        m_animator.speed = 1;                           // Ensure animation is playing (this fixed an issue I was having)
        StartCoroutine(FlashDamageColor());             // Flash the player sprite to indicate death
    }

    /*------------------------------------------------------------------
    | --- GetFacingDirection: Returns the current facing direction --- |
    ------------------------------------------------------------------*/

    public FacingDirection GetFacingDirection()
    {
        return m_facingDirection;
    }

    public float GetCurrentHealth()
    {
        return m_currentHealth;
    }

    public void SetHearts(float maxHearts, float totalHearts)
    {
        m_maxHealth = maxHearts;
        m_currentHealth = totalHearts;
    }

    public void LoadStackable(string name, int amount)
    {
        switch (name)
        {
            case "Rupee":
                m_rupees = amount;
                break;
            case "Bomb":
                m_bombs = amount;
                break;
            case "Key":
                m_keys = amount;
                break;
            default:
                break;
        }
    }

    public bool BuyItem(string name, int price)
    {
        if (m_rupees >= price)
        {
            m_hudUpdater.AddRupeeAmount(-price);
            m_rupees -= price;
            PickupItem(name);
            return true;
        }
        return false;
    }




    ///////////////////////////////////
    /*---------------------------------
    | --- Animation Event Methods --- |
    ---------------------------------*/
    ///////////////////////////////////

    /*---------------------------------------------------------
    | --- EnableMovement: Enable the Ability for Movement --- |
    ---------------------------------------------------------*/

    public void EnableMovement()
    {
#if DEBUG_LOG
        Debug.Log("Enable Movement");
#endif
        m_canMove = true;
    }

    /*-----------------------------------------------------------
    | --- DisableMovement: Disable the Ability for Movement --- |
    -----------------------------------------------------------*/

    public void DisableMovement()
    {
#if DEBUG_LOG
        Debug.Log("Disable Movement");
#endif
        m_canMove = false;
        m_movement = Vector2.zero;
    }

    /*--------------------------------------------------------------------
   | --- EndAttack: Event called at the End of the Attack Animation --- |
   --------------------------------------------------------------------*/

    public void EndAttack()
    {
#if DEBUG_LOG
        Debug.Log("Attack Ended");
#endif
        m_isAttacking = false;   // Reset the attacking flag when attack ends
        m_canMove = true;        // Enable movement after attack ends
    }

    /*-----------------------------------------------------------------------
    | --- StowAwayItem: Event called at the End of the Pickup Animation --- |
    -----------------------------------------------------------------------*/

    public void StowAwayItem()
    {
        if (m_itemPickedUp != null)
        {
            Destroy(m_itemPickedUp);
            m_itemPickedUp = null;
        }
    }

    /*------------------------------------------------------------------------------
    | --- OnDeath: Upon Death, at the end of Animation, Destroy the GameObject --- |
    ------------------------------------------------------------------------------*/

    public void OnDeath()
    {
        Debug.Log("OnDeath: Destroyed Player");
        m_playerDeathEvent.Invoke();
        Destroy(gameObject);
    }
}