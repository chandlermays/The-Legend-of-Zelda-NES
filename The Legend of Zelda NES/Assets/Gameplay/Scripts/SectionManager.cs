#define DEBUG_LOG

using UnityEngine;

public class SectionManager : MonoBehaviour
{
    [SerializeField] private GameObject enemySpawner; // Reference to the EnemySpawner GameObject
    private static SectionManager currentSection; // Static reference to the current active section

    private void Awake()
    {
        // Hide all child objects at the start
        SetChildrenActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
#if DEBUG_LOG
            Debug.Log($"Link has entered a new section: {gameObject.name}");
#endif

            // Hide the previous section's children
            if (currentSection != null && currentSection != this)
            {
                currentSection.SetChildrenActive(false);
            }

            // Show the current section's children
            SetChildrenActive(true);

            // Update the current section reference
            currentSection = this;

            // Reset the enemy spawner if it exists
            if (enemySpawner != null)
            {
                var spawnerComponent = enemySpawner.GetComponent<EnemySpawner>();
                if (spawnerComponent != null)
                {
                    spawnerComponent.ResetSpawner();
                }
            }
        }
    }

    public void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }

    public bool ContainsEntryPoint(string entryPointName)
    {
        Transform entryPoint = transform.Find(entryPointName);
        return entryPoint != null;
    }

    // Message Broadcast of the Player's death
    public void OnPlayerDeath()
    {
        SetChildrenActive(false);
    }
}