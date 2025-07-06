using UnityEngine;
using UnityEngine.InputSystem;

public class NextCursor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Cursors")]
    public GameObject[] m_cursors;

    InputAction m_moveAction;
    int m_cursorCount;
    int m_currentIndex = 0;
    void Start()
    {
        m_moveAction = InputSystem.actions.FindAction("ArrowsAndWASD");
        m_cursorCount = m_cursors.Length;

        if (m_cursorCount > m_currentIndex)
            m_cursors[m_currentIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_moveAction.WasPerformedThisFrame())
        {
            Vector2 moveInput = m_moveAction.ReadValue<Vector2>();
            if (moveInput.x > 0)
            {
                IncrementCursor();
            }
        }
    }
    public void IncrementCursor()
    {
        if (m_cursorCount > 0)
        {
            if (m_currentIndex < m_cursorCount - 1)
            {
                m_cursors[m_currentIndex].SetActive(false);
                ++m_currentIndex;
                m_cursors[m_currentIndex].SetActive(true);
            }
            else
            {
                m_cursors[m_currentIndex].SetActive(false);
                m_currentIndex = 0;
                m_cursors[m_currentIndex].SetActive(true);
            }
        }
    }
}
