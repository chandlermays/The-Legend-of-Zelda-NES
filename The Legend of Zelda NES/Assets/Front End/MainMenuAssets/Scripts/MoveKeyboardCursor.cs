using UnityEngine;
using UnityEngine.InputSystem;

public class MoveKeyboardCursor : MonoBehaviour
{
    [Header("Keyboard Variables")]
    public GameObject m_keyboardPanel;
    public int m_totalRows;
    public int m_totalColumns;
    public int m_currentIndex = 0;

    bool m_flickerToggle = true;
    bool m_stopFlickering = false;
    float m_stopFlickeringTimer = 1f;
    float m_stopFlickeringCounter = .0f;

    int m_totalCursorCount;
    GameObject[] m_cursors;
    InputAction m_moveAction;
    InputAction m_selectAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_cursors = GameObject.FindGameObjectsWithTag("KeyboardCursor");
        m_totalCursorCount = m_cursors.Length;
        if (m_cursors.Length == 0)
        {
            Debug.LogError("No Cursors found with KeyboardCursor Tag!", m_keyboardPanel);
        }
        else
        {
            foreach (GameObject cursor in m_cursors)
            {
                cursor.SetActive(false);
            }
        }
        m_moveAction = InputSystem.actions.FindAction("UIFileSelectKeyboardMovement", m_keyboardPanel);

        if (m_moveAction == null)
        {
            Debug.LogError("Keyboard Move Action is not Set!", m_keyboardPanel);
        }

        if (m_totalCursorCount > 0)
            m_cursors[m_currentIndex].SetActive(true);
    }

    private void OnDisable()
    {
        m_cursors[m_currentIndex].SetActive(false);
        m_currentIndex = 0;
        m_cursors[m_currentIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_stopFlickering)
        {
            if (m_stopFlickeringCounter > m_stopFlickeringTimer)
            {
                m_stopFlickeringCounter = 0;
                m_stopFlickering = false;
            }
            else
                m_stopFlickeringCounter += Time.deltaTime;
        }
        if (m_totalCursorCount != 0)
        {
            if (m_moveAction.WasPerformedThisFrame())
            {
                m_stopFlickering = true;
                m_stopFlickeringCounter = 0;
                Vector2 moveInput = m_moveAction.ReadValue<Vector2>();
                if (moveInput.y < 0)
                {
                    MoveDown();
                }
                else if (moveInput.y > 0)
                {
                    MoveUp();
                }
                else if (moveInput.x < 0)
                {
                    MoveLeft();
                }
                else if (moveInput.x > 0)
                {
                    MoveRight();
                }
            }
        }
    }
    void MoveUp()
    {
        if (m_currentIndex - m_totalColumns > -1)
        {
            m_cursors[m_currentIndex].SetActive(false);
            m_currentIndex -= m_totalColumns;
            m_cursors[m_currentIndex].SetActive(true);
        }
    }
    void MoveDown()
    {
        if (m_currentIndex + m_totalColumns < m_totalCursorCount)
        {
            m_cursors[m_currentIndex].SetActive(false);
            m_currentIndex += m_totalColumns;
            m_cursors[m_currentIndex].SetActive(true);
        }
    }
    void MoveLeft()
    {
        if (m_currentIndex - 1 > -1)
        {
            m_cursors[m_currentIndex].SetActive(false);
            --m_currentIndex;
            m_cursors[m_currentIndex].SetActive(true);
        }
    }
    void MoveRight()
    {
        if (m_currentIndex + 1 < m_totalCursorCount)
        {
            m_cursors[m_currentIndex].SetActive(false);
            ++m_currentIndex;
            m_cursors[m_currentIndex].SetActive(true);
        }
    }

    public void Flicker()
    {
        if (m_stopFlickering == false)
        {
            m_flickerToggle = !m_flickerToggle;
            m_cursors[m_currentIndex].GetComponent<SpriteRenderer>().enabled = m_flickerToggle;
        }
    }
}
