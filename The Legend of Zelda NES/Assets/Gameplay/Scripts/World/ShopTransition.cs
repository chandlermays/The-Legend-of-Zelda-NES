#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class ShopTransition : MonoBehaviour
{
    [SerializeField] private GameObject m_shopSection;
    [SerializeField] private GameObject m_startingSection;
    [SerializeField] private Camera m_mainCamera;
    [SerializeField] private MessageDisplay m_messageDisplay;

    private GameObject m_shop;
    private Transform m_dialogueTransform;
    private Transform m_shopEntryPoint;
    private Transform m_shopExitPoint;
    private bool m_dialogueDisplayed = false;

    private void Start()
    {
        m_shop = m_shopSection.transform.Find("Shop").gameObject;
        m_dialogueTransform = m_shop.transform.Find("Shop Dialogue");
        m_shopEntryPoint = m_shop.transform.Find("Shop Entry Point");
        m_shopExitPoint = m_startingSection.transform.Find("Shop Exit Point");
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

        if (gameObject.name == "Shop Entry")
        {
            m_shop.SetActive(true); // Activate the Shop
            StartCoroutine(HandleShopEntry(other, playerController));
        }
        else if (gameObject.name == "Shop Exit")
        {
            StartCoroutine(HandleShopExit(other, playerController));
        }
    }

    private IEnumerator HandleShopEntry(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has entered a shop");
#endif

        if (m_shopEntryPoint != null)
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

            Vector3 finalStartPosition = new Vector3(m_shopEntryPoint.position.x, m_shopEntryPoint.position.y - 4.0f, m_shopEntryPoint.position.z);
            other.transform.position = finalStartPosition; // Set the position to 4 units lower than the shop entry point

            m_mainCamera.transform.position = new Vector3(m_shopSection.transform.position.x, m_shopSection.transform.position.y, m_mainCamera.transform.position.z); // Update camera position

            elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(finalStartPosition, m_shopEntryPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerController.PauseEntity(true, true);   // Keep the movement disabled but stop the animation, too
            m_startingSection.SetActive(false); // Deactivate the Starting Section
            AccessInventory.DisableInventory(true);

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = true; // Re-enable all player's circle colliders
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sortingOrder = 1; // Restore Link's sorting order
            }

            if (!m_dialogueDisplayed && m_messageDisplay != null)
            {
#if DEBUG_LOG
                Debug.Log("Displaying dialogue");
#endif
                m_messageDisplay.DisplayDialogue(0.1f, () =>
                {
#if DEBUG_LOG
                    Debug.Log("Dialogue displayed");
#endif
                    playerController.PauseEntity(false); // Unpause the player once the message display is complete
                    AccessInventory.DisableInventory(false);
                });
                m_dialogueDisplayed = true; // Set the flag to true after displaying the dialogue
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("No message display found");
#endif
                playerController.PauseEntity(false, true); // Re-enable movement and animation if no message display
                AccessInventory.DisableInventory(false);
            }
        }
        else
        {
            Debug.LogError("Shop Entry Point not found in shop section");
        }
    }

    private IEnumerator HandleShopExit(Collider2D other, PlayerController playerController)
    {
        const float cameraYOffset = 5.25f;

#if DEBUG_LOG
        Debug.Log("Link has exited the shop");
#endif

        if (m_shopExitPoint != null)
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
            AccessInventory.DisableInventory(true);
            Vector3 startPosition = new Vector3(m_shopExitPoint.position.x, m_shopExitPoint.position.y - 3.0f, m_shopExitPoint.position.z);
            other.transform.position = startPosition; // Set the position to 3 units below the shop exit point

            m_mainCamera.transform.position = new Vector3(m_startingSection.transform.position.x, m_startingSection.transform.position.y + cameraYOffset, m_mainCamera.transform.position.z); // Update camera position

            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;

#if DEBUG_LOG
            Debug.Log("Starting movement up the stairs");
#endif
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(startPosition, m_shopExitPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            other.transform.position = m_shopExitPoint.position; // Ensure the final position is set
#if DEBUG_LOG
            Debug.Log("Reached the shop exit point");
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
            m_startingSection.SetActive(true); // Activate the Starting Section
            AccessInventory.DisableInventory(false);
#if DEBUG_LOG
            Debug.Log("Movement and animation re-enabled");
#endif

            m_shop.SetActive(false); // Deactivate the Shop
        }
        else
        {
            Debug.LogError("Shop Exit Point not found in starting section");
        }
    }
}