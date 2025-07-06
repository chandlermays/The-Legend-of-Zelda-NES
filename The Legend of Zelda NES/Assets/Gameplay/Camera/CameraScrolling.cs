using System.Collections;
using UnityEngine;

public class CameraScrolling : MonoBehaviour
{
    [SerializeField] private Camera m_mainCamera; // Reference to the main camera
    [SerializeField] private Transform m_newPlayerPosition; // Reference to the new player position

    private const float m_cameraXIncrement = 48f; // The amount to move the camera on the X-axis
    private const float m_cameraYIncrement = 33f; // The amount to move the camera on the Y-axis
    private const float m_cameraMoveSpeed = 25f; // The speed at which the camera moves

    private bool m_isTransitioning = false; // Flag to indicate if the camera is transitioning
    private SectionManager m_sectionManager; // Reference to the SectionManager

    private enum Direction
    {
        North,
        South,
        East,
        West
    }

    private void Start()
    {
        // Get the SectionManager component from the parent GameObject
        m_sectionManager = GetComponentInParent<SectionManager>();
        if (m_sectionManager == null)
        {
            Debug.LogError("SectionManager component not found on parent object");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !m_isTransitioning)
        {
#if DEBUG_LOG
            Debug.Log("Transitioning to a new section");
#endif

            m_isTransitioning = true; // Prevents multiple triggers while the player is within the collider
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController component not found on player object");
                m_isTransitioning = false;
                return;
            }

            playerController.PauseEntity(true); // Disable player movement while the camera is moving
            AccessInventory.DisableInventory(true);

            // Disable the player's CircleCollider2D components
            CircleCollider2D[] playerCircleColliders = other.GetComponents<CircleCollider2D>();
            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = false;
            }

            // Change Link's sorting layer and order
            SpriteRenderer playerSpriteRenderer = other.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = 1;
            }

            Vector3 newCameraPosition = m_mainCamera.transform.position;
            Direction direction;

            // Determine the direction based on the name of the GameObject
            switch (gameObject.name)
            {
                case "North Trigger":
                    newCameraPosition.y += m_cameraYIncrement;
                    direction = Direction.North;
                    break;

                case "South Trigger":
                    newCameraPosition.y -= m_cameraYIncrement;
                    direction = Direction.South;
                    break;

                case "East Trigger":
                    newCameraPosition.x += m_cameraXIncrement;
                    direction = Direction.East;
                    break;

                case "West Trigger":
                    newCameraPosition.x -= m_cameraXIncrement;
                    direction = Direction.West;
                    break;

                default:
                    Debug.LogWarning("Unrecognized trigger direction!");
                    m_isTransitioning = false;
                    playerController.PauseEntity(false);
                    foreach (var collider in playerCircleColliders)
                    {
                        collider.enabled = true;
                    }
                    return;
            }
            UpdateMap(direction);

            // Deactivate all child GameObjects of SectionManager except this one
            foreach (Transform child in m_sectionManager.transform)
            {
                if (child.gameObject != gameObject)
                {
                    child.gameObject.SetActive(false);
                }
            }

            // Call the appropriate method based on whether Link is in the dungeon
            if (playerController.m_isInDungeon)
            {
                StartCoroutine(HandleDungeonScrolling(newCameraPosition, direction, playerCircleColliders, playerSpriteRenderer, playerController));
            }
            else
            {
                StartCoroutine(HandleOverworldScrolling(newCameraPosition, direction, playerCircleColliders, playerSpriteRenderer, playerController));
            }
        }
    }

    private IEnumerator HandleOverworldScrolling(Vector3 targetPosition, Direction direction, CircleCollider2D[] playerCircleColliders, SpriteRenderer playerSpriteRenderer, PlayerController playerController)
    {
        bool playerOutsideBounds = false;
        float topBoundOffset = 5.75f;        // Offset for the GameHUD

        while (Vector3.Distance(m_mainCamera.transform.position, targetPosition) > 0.01f)
        {
            m_mainCamera.transform.position = Vector3.MoveTowards(m_mainCamera.transform.position, targetPosition, m_cameraMoveSpeed * Time.deltaTime);

            // Check if the player is outside the camera bounds
            Vector3 playerViewportPosition = m_mainCamera.WorldToViewportPoint(playerController.transform.position);

            // Adjust the top bound by subtracting the offset from the viewport y position
            float adjustedTopBound = 1 - (topBoundOffset / m_mainCamera.orthographicSize);

            if (!playerOutsideBounds && (playerViewportPosition.x < 0 || playerViewportPosition.x > 1 || playerViewportPosition.y < 0 || playerViewportPosition.y > adjustedTopBound))
            {
                playerOutsideBounds = true;
            }

            // Gradually move the player towards the new position if they are outside the camera bounds
            if (playerOutsideBounds && m_newPlayerPosition != null)
            {
                Vector3 newPlayerPosition = playerController.transform.position;
                switch (direction)
                {
                    case Direction.North:
                    case Direction.South:
                        newPlayerPosition.y = Mathf.MoveTowards(newPlayerPosition.y, m_newPlayerPosition.position.y, m_cameraMoveSpeed * Time.deltaTime);
                        break;
                    case Direction.East:
                    case Direction.West:
                        newPlayerPosition.x = Mathf.MoveTowards(newPlayerPosition.x, m_newPlayerPosition.position.x, m_cameraMoveSpeed * Time.deltaTime);
                        break;
                }
                playerController.SetPosition(newPlayerPosition);
            }

            yield return null;
        }

        // Ensure the camera is exactly at the target position
        m_mainCamera.transform.position = targetPosition;

        // Ensure the player is exactly at the new position
        if (m_newPlayerPosition != null)
        {
            Vector3 newPlayerPosition = playerController.transform.position;
            switch (direction)
            {
                case Direction.North:
                case Direction.South:
                    newPlayerPosition.y = m_newPlayerPosition.position.y;
                    break;
                case Direction.East:
                case Direction.West:
                    newPlayerPosition.x = m_newPlayerPosition.position.x;
                    break;
            }
            playerController.SetPosition(newPlayerPosition);
        }

        // Re-enable player movement
        playerController.PauseEntity(false);
        m_isTransitioning = false;
        AccessInventory.DisableInventory(false);

        // Re-enable the player's CircleCollider2D components
        foreach (var collider in playerCircleColliders)
        {
            collider.enabled = true;
        }

        // Restore Link's sorting layer and order
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = 1;
        }

        // Activate the new section's children
        m_sectionManager.SetChildrenActive(true);

        // Deactivate the GameObject that triggered CameraScrolling
        gameObject.SetActive(false);
    }

    private IEnumerator HandleDungeonScrolling(Vector3 targetPosition, Direction direction, CircleCollider2D[] playerCircleColliders, SpriteRenderer playerSpriteRenderer, PlayerController playerController)
    {
        // Hold Link's position while the camera scrolls
        Vector3 initialPlayerPosition = playerController.transform.position;

        while (Vector3.Distance(m_mainCamera.transform.position, targetPosition) > 0.01f)
        {
            m_mainCamera.transform.position = Vector3.MoveTowards(m_mainCamera.transform.position, targetPosition, m_cameraMoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure the camera is exactly at the target position
        m_mainCamera.transform.position = targetPosition;

        // Move Link towards the new position using Lerp
        float lerpDuration = 0.5f; // Duration of the Lerp
        float elapsedTime = 0.0f;

        // Start playing the animation
        playerController.PauseAnimation(false);
        AccessInventory.DisableInventory(true);
        while (elapsedTime < lerpDuration)
        {
            playerController.transform.position = Vector3.Lerp(initialPlayerPosition, m_newPlayerPosition.position, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure Link is exactly at the new position
        playerController.transform.position = m_newPlayerPosition.position;

        // Stop the animation
        playerController.PauseAnimation(true);

        // Re-enable player movement
        playerController.PauseEntity(false);
        m_isTransitioning = false;
        AccessInventory.DisableInventory(false);

        // Re-enable the player's CircleCollider2D components
        foreach (var collider in playerCircleColliders)
        {
            collider.enabled = true;
        }

        // Restore Link's sorting layer and order
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = 1;
        }

        // Activate the new section's children
        m_sectionManager.SetChildrenActive(true);

        // Deactivate the GameObject that triggered CameraScrolling
        gameObject.SetActive(false);
    }

    private void UpdateMap(Direction dir)
    {
        MapPositionManager.CompassDirection pos = MapPositionManager.CompassDirection.kNone;
        switch (dir)
        {
            case Direction.North:
                pos = MapPositionManager.CompassDirection.kNorth;
                break;
            case Direction.South:
                pos = MapPositionManager.CompassDirection.kSouth;
                break;
            case Direction.East:
                pos = MapPositionManager.CompassDirection.kEast;
                break;
            case Direction.West:
                pos = MapPositionManager.CompassDirection.kWest;
                break;
        }
        MapPositionManager.UpdateMapPositions(pos);
    }
}