using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private string itemType;
    [SerializeField] private int m_itemCost;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (playerController.BuyItem(itemType, m_itemCost))
                {
                    Destroy(gameObject);  // Remove the item from the world
                    Debug.Log($"{itemType} picked up!");
                }
            }
        }
    }
}