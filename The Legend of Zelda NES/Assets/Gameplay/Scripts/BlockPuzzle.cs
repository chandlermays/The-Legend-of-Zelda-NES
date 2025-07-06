using UnityEngine;
using System;

public class BlockPuzzle : MonoBehaviour
{
    [SerializeField] private bool m_isHorizontal = false; // Determines if the block moves horizontally
    private bool m_isComplete = false;
    private float m_targetPosition;
    private float m_speed = 10f; // Speed at which the block moves
    private bool m_isPushing = false;
    private Vector2 m_pushDirection;
    private float m_initialPosition;
    private float m_movementLimit = 3f; // Limit of movement from the initial position
    private float m_tolerance = 0.1f; // Tolerance for reaching the target position

    public event Action OnPuzzleComplete; // Event to notify when the puzzle is complete

    private void Start()
    {
        if (m_isHorizontal)
        {
            m_initialPosition = transform.position.x; // Store the initial X position
        }
        else
        {
            m_initialPosition = transform.position.y; // Store the initial Y position
        }
        m_targetPosition = m_initialPosition; // Initialize target position
    }

    private void Update()
    {
        if (m_isPushing)
        {
            // Move the block in the push direction
            float step = m_speed * Time.deltaTime;
            if (m_isHorizontal)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(m_targetPosition, transform.position.y), step);
                Debug.Log("transform.position.x: " + transform.position.x + ", target.position: " + m_targetPosition);
                // Check if the block has reached the target position
                if (Mathf.Abs(transform.position.x - m_targetPosition) < m_tolerance)
                {
                    transform.position = new Vector2(m_targetPosition, transform.position.y); // Snap to target position
                    m_isPushing = false; // Stop pushing
                    Debug.Log("Block reached target position horizontally.");
                    CheckCompletion(); // Check if the puzzle is complete
                }
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, m_targetPosition), step);
                // Check if the block has reached the target position
                if (Mathf.Abs(transform.position.y - m_targetPosition) < m_tolerance)
                {
                    transform.position = new Vector2(transform.position.x, m_targetPosition); // Snap to target position
                    m_isPushing = false; // Stop pushing
                    Debug.Log("Block reached target position vertically.");
                    CheckCompletion(); // Check if the puzzle is complete
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 contactPoint = collision.GetContact(0).point;
            Vector2 center = GetComponent<Collider2D>().bounds.center;
            Vector2 extents = GetComponent<Collider2D>().bounds.extents;

            if (m_isHorizontal)
            {
                // Check if the collision is at the left or right bounds
                if (contactPoint.x > center.x + extents.x - 0.1f && !m_isPushing)
                {
                    // Collision at the right bounds, move the box left if within limits
                    if (transform.position.x > m_initialPosition - m_movementLimit)
                    {
                        m_targetPosition = m_initialPosition - m_movementLimit;
                        m_isPushing = true;
                    }
                }
         //       else if (contactPoint.x < center.x - extents.x + 0.1f && !m_isPushing)
         //       {
         //           // Collision at the left bounds, move the box right if within limits
         //           if (transform.position.x < m_initialPosition + m_movementLimit)
         //           {
         //               m_targetPosition = m_initialPosition + m_movementLimit;
         //               m_isPushing = true;
         //           }
         //       }
            }
            else
            {
                // Check if the collision is at the top or bottom bounds
                if (contactPoint.y > center.y + extents.y - 0.1f && !m_isPushing)
                {
                    // Collision at the top bounds, move the box down if within limits
                    if (transform.position.y > m_initialPosition - m_movementLimit)
                    {
                        m_targetPosition = m_initialPosition - m_movementLimit;
                        m_isPushing = true;
                    }
                }
                else if (contactPoint.y < center.y - extents.y + 0.1f && !m_isPushing)
                {
                    // Collision at the bottom bounds, move the box up if within limits
                    if (transform.position.y < m_initialPosition + m_movementLimit)
                    {
                        m_targetPosition = m_initialPosition + m_movementLimit;
                        m_isPushing = true;
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_isPushing = false; // Stop pushing when the player stops colliding
        }
    }

    private void CheckCompletion()
    {
        Debug.Log("Checking completion");

        if (m_isHorizontal)
        {
            // Check if the block has reached the horizontal limit
            if (Mathf.Abs(transform.position.x - m_initialPosition) >= m_movementLimit)
            {
                Debug.Log("Puzzle Complete!");
                m_isComplete = true;
                OnPuzzleComplete?.Invoke(); // Notify that the puzzle is complete
            }
        }
        else
        {
            // Check if the block has reached the vertical limit
            if (Mathf.Abs(transform.position.y - m_initialPosition) >= m_movementLimit)
            {
                Debug.Log("Puzzle Complete!");
                m_isComplete = true;
                OnPuzzleComplete?.Invoke(); // Notify that the puzzle is complete
            }
        }
    }

    public bool IsComplete()
    {
        return m_isComplete;
    }
}