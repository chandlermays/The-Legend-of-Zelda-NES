#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class DungeonTransition : MonoBehaviour
{
    [SerializeField] private GameObject m_dungeonSection;
    [SerializeField] private GameObject m_startingSection;
    [SerializeField] private Camera m_mainCamera;

    private Transform m_dungeonEntryPoint;
    private Transform m_dungeonExitPoint;

    private void Start()
    {
        m_dungeonEntryPoint = m_dungeonSection.transform.Find("Dungeon Entry Point");
        m_dungeonExitPoint = m_startingSection.transform.Find("Dungeon Exit Point");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on player object");
            return;
        }

        if (gameObject.name == "Dungeon Entry")
        {
            m_dungeonSection.SetActive(true); // Activate the Dungeon
            StartCoroutine(HandleDungeonEntry(other, playerController));
        }
        else if (gameObject.name == "Dungeon Exit")
        {
            StartCoroutine(HandleDungeonExit(other, playerController));
        }
    }

    private IEnumerator HandleDungeonEntry(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has entered a dungeon");
#endif

        if (m_dungeonEntryPoint != null)
        {
            CircleCollider2D[] playerCircleColliders = other.GetComponents<CircleCollider2D>();
            SpriteRenderer playerSpriteRenderer = other.GetComponent<SpriteRenderer>();

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = false; // Disable all player's circle colliders
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = -1; // Set Link behind the map
            }

            playerController.PauseEntity(true, false); // Disable movement but not the animation
            Debug.Log("pause link");
            AccessInventory.DisableInventory(true);
            Vector3 startPosition = other.transform.position;
            Vector3 intermediatePosition = new Vector3(startPosition.x, startPosition.y - 3.0f, startPosition.z);
            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(startPosition, intermediatePosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Vector3 finalStartPosition = new Vector3(m_dungeonEntryPoint.position.x, m_dungeonEntryPoint.position.y - 4.0f, m_dungeonEntryPoint.position.z);
            other.transform.position = finalStartPosition; // Set the position to 4 units lower than the dungeon entry point

            m_mainCamera.transform.position = new Vector3(m_dungeonSection.transform.position.x, m_dungeonSection.transform.position.y, m_mainCamera.transform.position.z); // Update camera position
            AccessInventory.SetInDungeon(true);
            CurtainTransition.StartTransition();
            SectionManager sectionManager = m_dungeonSection.GetComponent<SectionManager>();
            if (sectionManager != null)
            {
                sectionManager.SetChildrenActive(true); // Activate the Dungeon
            }
            else
            {
                Debug.LogError("SectionManager component not found on dungeon section");
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = 1; // Set Link between the map and overlay
            }

            yield return new WaitForSeconds(3.0f); // Wait for the curtain transition to complete

            elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(finalStartPosition, m_dungeonEntryPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            other.transform.position = m_dungeonEntryPoint.position; // Ensure the final position is set
            playerController.PauseEntity(true, true);   // Keep the movement disabled but stop the animation, too

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = true; // Re-enable all player's circle colliders
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = 1; // Restore Link's sorting order
            }

            playerController.PauseEntity(false, true); // Re-enable movement and animation
            playerController.m_isInDungeon = true; // Set the flag to indicate Link is in the dungeon
            AccessInventory.DisableInventory(false);
        }
        else
        {
            Debug.LogError("Dungeon Entry Point not found in dungeon section");
        }
    }

    private IEnumerator HandleDungeonExit(Collider2D other, PlayerController playerController)
    {
        const float cameraYOffset = 5.25f;

#if DEBUG_LOG
        Debug.Log("Link has exited the dungeon");
#endif

        if (m_dungeonExitPoint != null)
        {
            CircleCollider2D[] playerCircleColliders = other.GetComponents<CircleCollider2D>();
            SpriteRenderer playerSpriteRenderer = other.GetComponent<SpriteRenderer>();

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = false; // Disable all player's circle colliders
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = -1; // Set Link behind the map
            }

            playerController.PauseEntity(false, false); // Disable movement but not the animation
            AccessInventory.DisableInventory(true);
            Vector3 startPosition = new Vector3(m_dungeonExitPoint.position.x, m_dungeonExitPoint.position.y - 3.0f, m_dungeonExitPoint.position.z);
            other.transform.position = startPosition; // Set the position to 3 units below the dungeon exit point

            m_mainCamera.transform.position = new Vector3(m_startingSection.transform.position.x, m_startingSection.transform.position.y + cameraYOffset, m_mainCamera.transform.position.z); // Update camera position
            AccessInventory.SetInDungeon(false);
            CurtainTransition.StartTransition();
            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;

            yield return new WaitForSeconds(3.0f); // Wait for the curtain transition to complete

            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(startPosition, m_dungeonExitPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            other.transform.position = m_dungeonExitPoint.position; // Ensure the final position is set
#if DEBUG_LOG
            Debug.Log("Reached the dungeon exit point");
#endif

            playerController.PauseEntity(true, true);   // Keep the movement disabled but stop the animation, too

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = true; // Re-enable all player's circle colliders
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = 1; // Restore Link's sorting order
            }

            playerController.PauseEntity(false, true); // Re-enable movement and animation
            AccessInventory.DisableInventory(false);
#if DEBUG_LOG
            Debug.Log("Movement and animation re-enabled");
#endif

            m_dungeonSection.SetActive(false); // Deactivate the Dungeon
            playerController.m_isInDungeon = false; // Set the flag to indicate Link is not in the dungeon
        }
        else
        {
            Debug.LogError("Dungeon Exit Point not found in starting section");
        }
    }
}