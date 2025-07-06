using UnityEngine;
using UnityEngine.InputSystem;

public class RegistrationNameChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public UnityEvent m_changeTextEvent;
    [Header("Name Setting Attributes")]
    public GameObject[] m_textBoxes;
    public GameObject[] m_cursors;
    public Sprite m_emptySprite;
    public GameObject m_keyboard;

    [Header("Save Slot")]
    public SaveSlot m_saveSlot;

    int m_totalBoxCount = 0;
    int m_totalCursorCount = 0;
    int m_currentIndex = 0;
    InputAction m_moveAction;
    InputAction m_backspaceAction;
    bool m_selected = false;
    bool m_flickerToggle = true;

    bool m_stopFlickering = false;
    float m_stopFlickeringTimer = 1f;
    float m_stopFlickeringCounter = .0f;

    void Start()
    {
        m_totalBoxCount = m_textBoxes.Length;
        m_totalCursorCount = m_cursors.Length;
        if (m_totalBoxCount == 0)
        {
            Debug.LogError("No text boxes set!", this);
        }
        else if (m_totalCursorCount == 0)
        {
            Debug.LogError("No cursors set!", this);
        }
        else
        {
            if (m_selected)
            {
                m_cursors[m_currentIndex].SetActive(true);
            }
        }
        m_moveAction = InputSystem.actions.FindAction("OnlyWASD");
        m_backspaceAction = InputSystem.actions.FindAction("UIKeyboardBackspace");
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
        if (m_selected)
        {
            if (m_moveAction.WasPerformedThisFrame())
            {
                m_stopFlickering = true;
                m_stopFlickeringCounter = 0;
                Vector2 moveInput = m_moveAction.ReadValue<Vector2>();
                if (moveInput.x > 0)
                {
                    IncrementCursor();
                }
                else if (moveInput.x < 0)
                {
                    DecrementCursor();
                }
            }
            if (m_backspaceAction.WasPerformedThisFrame())
            {
                m_stopFlickering = true;
                m_stopFlickeringCounter = 0;
                Backspace();
            }
        }
    }

    private void Backspace()
    {
        if (m_currentIndex != 0)
        {
            m_cursors[m_currentIndex].SetActive(false);
            --m_currentIndex;
            m_cursors[m_currentIndex].SetActive(true);
            m_textBoxes[m_currentIndex].GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public void AddText(int index)
    {
        Sprite sprite = m_keyboard.GetComponent<KeyToSprite>().GetKey(index);
        if (m_selected)
        {
            if (sprite.Equals(m_emptySprite))
            {
                m_textBoxes[m_currentIndex].GetComponent<SpriteRenderer>().sprite = null;
            }
            else
                m_textBoxes[m_currentIndex].GetComponent<SpriteRenderer>().sprite = sprite;
            IncrementCursor();
        }
    }
    void IncrementCursor()
    {
        if (m_selected)
        {
            if (m_totalBoxCount > 0)
            {
                if (m_currentIndex < m_totalBoxCount - 1)
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

    void DecrementCursor()
    {
        if (m_selected)
        {
            if (m_totalBoxCount > 0)
            {
                if (m_currentIndex > 0)
                {
                    m_cursors[m_currentIndex].SetActive(false);
                    --m_currentIndex;
                    m_cursors[m_currentIndex].SetActive(true);
                }
                else
                {
                    m_cursors[m_currentIndex].SetActive(false);
                    m_currentIndex = m_totalBoxCount - 1;
                    m_cursors[m_currentIndex].SetActive(true);
                }
            }
        }
    }

    public void Selected()
    {
        m_cursors[m_currentIndex].SetActive(true);
        m_selected = true;
    }

    public void UnSelected()
    {
        m_cursors[m_currentIndex].SetActive(false);
        m_selected = false;
        m_currentIndex = 0;
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
