#define DEBUG_LOG

using System.Collections;
using UnityEngine;

public class ChamberTransition : MonoBehaviour
{
    [SerializeField] private GameObject m_chamberSection;
    [SerializeField] private GameObject m_startingSection;
    [SerializeField] private Camera m_mainCamera;

    static private bool m_bowPickedUp = false;
    private GameObject m_chamber;
    private Transform m_bowPickupTransform;
    private Transform m_chamberEntryPoint;
    private Transform m_chamberExitPoint;

    static private bool m_bowObtained = false;

    private float m_playerYOffset = 1.5f;
    private float m_cameraXOffset = 0.1f;
    private float m_chamberCameraYOffset = 6.0f;
    private float m_returningCameraYOffset = 5.25f;

    private void Start()
    {
        // Uncomment the following line to reset the PlayerPrefs for testing
        PlayerPrefs.DeleteAll();

        m_chamber = m_chamberSection.transform.Find("Treasure Chamber").gameObject;
        m_bowPickupTransform = m_chamber.transform.Find("Bow Pickup");
        m_chamberEntryPoint = m_chamber.transform.Find("Chamber Entry Point");
        m_chamberExitPoint = m_startingSection.transform.Find("Chamber Exit Point");
    }

    private void Update()
    {
        if (!m_bowObtained)
        {
            if (!m_bowPickedUp && m_bowPickupTransform == null)
            {
                HandleBowPickup();
            }
        }
        else if (m_bowObtained && !m_bowPickedUp)
        {
            HandleBowPickup();
        }
    }

    private void HandleBowPickup()
    {
        m_bowPickedUp = true;

        if (m_bowPickupTransform != null)
        {
            Destroy(m_bowPickupTransform.gameObject);
        }

        // Save the state that the bow has been picked up
        PlayerPrefs.SetInt("BowPickedUp", 1);
        PlayerPrefs.Save();
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

        if (gameObject.name == "Chamber Entry")
        {
            m_chamber.SetActive(true); // Activate the Chamber
            StartCoroutine(HandleChamberEntry(other, playerController));
        }
        else if (gameObject.name == "Chamber Exit")
        {
            StartCoroutine(HandleChamberExit(other, playerController));
        }
    }

    private IEnumerator HandleChamberEntry(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has entered a chamber");
#endif

        if (m_chamberEntryPoint != null)
        {
            CircleCollider2D[] playerCircleColliders = other.GetComponents<CircleCollider2D>();

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = false; // Disable all player's circle colliders
            }

            playerController.PauseEntity(true, false); // Disable movement but not the animation
            AccessInventory.DisableInventory(true);

            // Directly set the position to 4 units higher than the chamber entry point
            Vector3 finalStartPosition = new Vector3(m_chamberEntryPoint.position.x, m_chamberEntryPoint.position.y + m_playerYOffset, m_chamberEntryPoint.position.z);
            other.transform.position = finalStartPosition;

            // Update camera position
            m_mainCamera.transform.position = new Vector3(m_chamberSection.transform.position.x - m_cameraXOffset, m_chamberSection.transform.position.y + m_chamberCameraYOffset, m_mainCamera.transform.position.z);

            // Set the player's facing direction to Down
            playerController.SetFacingDirection(PlayerController.FacingDirection.kDown);

            // Set the player's movement mode to Platformer
            playerController.SetMovementMode(PlayerController.MovementMode.kPlatformer);

            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(finalStartPosition, m_chamberEntryPoint.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

#if DEBUG_LOG
            Debug.Log("Reached the chamber entry point");
#endif
            playerController.PauseEntity(true, true);   // Keep the movement disabled but stop the animation, too
            m_startingSection.SetActive(false); // Deactivate the Starting Section

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = true; // Re-enable all player's circle colliders
            }

            playerController.PauseEntity(false, true); // Re-enable movement and animation
            AccessInventory.DisableInventory(false);
        }
        else
        {
            Debug.LogError("Chamber Entry Point not found in cave section");
        }
    }

    private IEnumerator HandleChamberExit(Collider2D other, PlayerController playerController)
    {
#if DEBUG_LOG
        Debug.Log("Link has exited the chamber");
#endif

        if (m_chamberExitPoint != null)
        {
            CircleCollider2D[] playerCircleColliders = other.GetComponents<CircleCollider2D>();

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = false; // Disable all player's circle colliders
            }

            playerController.PauseEntity(true, false); // Disable movement but not the animation
            AccessInventory.DisableInventory(true);

            // Set the starting position to the chamber exit point
            Vector3 startPosition = m_chamberExitPoint.position;

            // Set the target position to 3 units to the left of the chamber exit point
            Vector3 targetPosition = new Vector3(m_chamberExitPoint.position.x - 3.0f, m_chamberExitPoint.position.y, m_chamberExitPoint.position.z);
            other.transform.position = startPosition;

            // Set the player's facing direction to Left
            playerController.SetFacingDirection(PlayerController.FacingDirection.kLeft);

            // Update camera position
            m_mainCamera.transform.position = new Vector3(m_startingSection.transform.position.x, m_startingSection.transform.position.y + m_returningCameraYOffset, m_mainCamera.transform.position.z);

            float duration = 1.0f; // Duration of the movement
            float elapsedTime = 0.0f;

#if DEBUG_LOG
            Debug.Log("Starting movement to the left");
#endif
            while (elapsedTime < duration)
            {
                other.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            other.transform.position = targetPosition; // Ensure the final position is set
#if DEBUG_LOG
            Debug.Log("Reached the target position");
#endif

            foreach (var collider in playerCircleColliders)
            {
                collider.enabled = true; // Re-enable all player's circle colliders
            }

            playerController.PauseEntity(false, true); // Re-enable movement and animation
            AccessInventory.DisableInventory(false);
#if DEBUG_LOG
            Debug.Log("Movement and animation re-enabled");
#endif

            // Set the player's movement mode to TopDown
            playerController.SetMovementMode(PlayerController.MovementMode.kTopDown);

            m_chamber.SetActive(false); // Deactivate the chamber
            m_startingSection.SetActive(true); // Activate the Starting Section
        }
        else
        {
            Debug.LogError("Chamber Exit Point not found in starting section");
        }
    }

    static public void DisableStarterBowArea(bool status)
    {
        Debug.Log("Bow Already Obtained!");
        m_bowObtained = status;
    }
}