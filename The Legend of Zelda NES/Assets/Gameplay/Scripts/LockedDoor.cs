#define DEBUG_LOG

using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private BlockPuzzle m_associatedBlockPuzzle;   // Optional: Complete a Block Puzzle

    private bool m_isOpen = false;  // Flag to indicate if the door is open

    private void Start()
    {
        if (m_associatedBlockPuzzle != null)
        {
            m_associatedBlockPuzzle.OnPuzzleComplete += OpenDoor; // Subscribe to the puzzle completion event
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.HasKey())
            {
                OpenDoor();
                player.UseKey();
            }
            else if (m_associatedBlockPuzzle != null && m_associatedBlockPuzzle.IsComplete())
            {
                OpenDoor();
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Door is locked. Player does not have a key and the block puzzle is not complete.");
#endif
            }
        }
    }

    private void OpenDoor()
    {
        if (!m_isOpen)
        {
            m_isOpen = true;
            gameObject.SetActive(false); // Example: Deactivate the door GameObject

#if DEBUG_LOG
            Debug.Log("Door opened.");
#endif
        }
    }
}