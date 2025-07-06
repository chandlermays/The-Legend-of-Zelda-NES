#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class CaveTransition : MonoBehaviour
{
    [SerializeField] private GameObject m_caveSection;
    [SerializeField] private GameObject m_startingSection;
    [SerializeField] private Camera m_mainCamera;
    [SerializeField] private MessageDisplay m_messageDisplay;

    static private bool m_swordPickedUp = false;
    private bool m_dialogueDisplayed = false;
    private GameObject m_cave;
    private Transform m_swordPickupTransform;
    private Transform m_oldManTransform;
    private Transform m_dialogueTransform;
    private Transform m_caveEntryPoint;
    private Transform m_caveExitPoint;

    static private bool m_swordObtained = false;

    private void Start()
    {
        // Uncomment the following line to reset the PlayerPrefs for testing
        PlayerPrefs.DeleteAll();

        m_cave = m_caveSection.transform.Find("Cave").gameObject;
        m_swordPickupTransform = m_cave.transform.Find("Sword Pickup");
        m_oldManTransform = m_cave.transform.Find("Old Man");
        m_dialogueTransform = m_cave.transform.Find("Cave Dialogue");
        m_caveEntryPoint = m_cave.transform.Find("Cave Entry Point");
        m_caveExitPoint = m_startingSection.transform.Find("Cave Exit Point");
    }

    private void Update()
    {
        if (!m_swordObtained)
        {
            if (!m_swordPickedUp && m_swordPickupTransform == null)
            {
                HandleSwordPickup();
            }
        }
        else if (m_swordObtained && !m_swordPickedUp)
        {
            HandleSwordPickup();
        }
    }

    private void HandleSwordPickup()
    {
        m_swordPickedUp = true;

        if (m_swordPickupTransform != null)
        {
            Destroy(m_swordPickupTransform.gameObject);
        }

        if (m_oldManTransform != null)
        {
            Destroy(m_oldManTransform.gameObject);
        }

        if (m_dialogueTransform != null)
        {
#if DEBUG_LOG
            Debug.Log("Destroying Dialogue");
#endif
            Destroy(m_dialogueTransform.gameObject);
            m_messageDisplay = null; // Set m_messageDisplay to null to avoid further use
        }

        // Save the state that the sword has been picked up
        PlayerPrefs.SetInt("SwordPickedUp", 1);
        PlayerPrefs.Save();

#if DEBUG_LOG
        Debug.Log("Old Man and Dialogue have been destroyed");
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on player");
            return;
        }

        if (gameObject.name == "Cave Entry")
        {
            m_cave.SetActive(true); // Activate the Cave
            StartCoroutine(HandleCaveEntry(other, playerController));
        }
        else if (gameObject.name == "Cave Exit")
        {
            StartCoroutine(HandleCaveExit(other, playerController));
        }
    }

    private IEnumerator HandleCaveEntry(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has entered a cave");
#endif

        if (m_caveEntryPoint != null)
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

            Vector3 finalStartPosition = new Vector3(m_caveEntryPoint.position.x, m_caveEntryPoint.position.y - 4.0f, m_caveEntryPoint.position.z);
            other.transform.position = finalStartPosition; // Set the position to 4 units lower than the cave entry point

            m_mainCamera.transform.position = new Vector3(m_caveSection.transform.position.x, m_caveSection.transform.position.y, m_mainCamera.transform.position.z); // Update camera position

            elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(finalStartPosition, m_caveEntryPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

#if DEBUG_LOG
            Debug.Log("Reached the cave entry point");
#endif
            playerController.PauseEntity(true, true);   // Keep the movement disabled but stop the animation, too
            m_startingSection.SetActive(false); // Deactivate the Starting Section

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
            Debug.LogError("Cave Entry Point not found in cave section");
        }
    }

    private IEnumerator HandleCaveExit(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has exited the cave");
#endif

        if (m_caveExitPoint != null)
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
            Vector3 startPosition = new Vector3(m_caveExitPoint.position.x, m_caveExitPoint.position.y - 3.0f, m_caveExitPoint.position.z);
            other.transform.position = startPosition; // Set the position to 3 units below the cave exit point

            m_mainCamera.transform.position = new Vector3(m_startingSection.transform.position.x, m_startingSection.transform.position.y, m_mainCamera.transform.position.z); // Update camera position

            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;

#if DEBUG_LOG
            Debug.Log("Starting movement up the stairs");
#endif
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(startPosition, m_caveExitPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            other.transform.position = m_caveExitPoint.position; // Ensure the final position is set
#if DEBUG_LOG
            Debug.Log("Reached the cave exit point");
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

            m_cave.SetActive(false); // Deactivate the Cave
        }
        else
        {
            Debug.LogError("Cave Exit Point not found in starting section");
        }
    }

    static public void DisableStarterSwordArea(bool status)
    {
        Debug.Log("Sword Already Obtained!");
        m_swordObtained = status;
    }
}