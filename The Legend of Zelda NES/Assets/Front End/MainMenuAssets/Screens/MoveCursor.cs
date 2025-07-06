using System;
using System.Linq;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveCursor : MonoBehaviour
{
    [Header("Cursors")]
    public GameObject[] m_cursors;
    public bool[] m_activeCursors;
    public int m_cursorStartIndex = 0;

    // this is here as a quick solution to the inventory displaying the first cursor when its empty
    [Header("Inventory Specific Settings")]
    public GameObject m_defaultCursor = null;
    public bool m_showDefaultIfEmpty = false;

    public bool m_enableVertical = true;
    public bool m_enableHorizontal = false;
    public bool m_wrapToSameRow = false;

    int m_currentCursorIndex = 0;
    InputAction m_moveAction;
    InputAction m_selectAction;
    [SerializeField] public string m_moveActionString = string.Empty;
    [SerializeField] public string m_selectActionString = string.Empty;

    private bool m_flickerToggle = true;
    private bool m_stopFlickering = false;
    private float m_stopFlickeringTimer = 1f;
    private float m_stopFlickeringCounter = .0f;

    public bool m_spriteRenderer = false;

    public int m_verticalStep = 1;
    public int m_horizontalStep = 1;
    bool m_repositionCursor = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_currentCursorIndex = m_cursorStartIndex;
        UnrenderAllCursors();
        if (m_moveActionString == string.Empty)
            m_moveAction = InputSystem.actions.FindAction("ArrowsAndWASD");
        else
            m_moveAction = InputSystem.actions.FindAction(m_moveActionString);

        if (m_selectActionString == string.Empty)
            m_selectAction = InputSystem.actions.FindAction("UIKeyboardSelect");
        else
            m_selectAction = InputSystem.actions.FindAction(m_selectActionString);

        if (m_cursors.Length > m_currentCursorIndex)
        {
            if (m_activeCursors[m_currentCursorIndex])
            {
                m_cursors[m_currentCursorIndex].SetActive(true);
            }
            else
            {
                m_currentCursorIndex = GetFirstActiveCursorIndex();
                m_cursors[m_currentCursorIndex].SetActive(true);
            }
        }
    }

    private int GetFirstActiveCursorIndex()
    {
        for (int i = 0; i < m_activeCursors.Length; ++i)
        {
            if (m_activeCursors[i])
            {
                return i;
            }
        }
        return 0;
    }
    private void OnDisable()
    {
        UnrenderAllCursors();
        m_currentCursorIndex = m_cursorStartIndex;
    }

    private void UnrenderAllCursors()
    {
        for (int i = 0; i < m_cursors.Length; ++i)
        {
            m_cursors[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        m_repositionCursor = true;
    }

    private void RepositionCursor()
    {
        if (!m_activeCursors.Contains(true) && m_showDefaultIfEmpty && m_defaultCursor != null)
        {
            m_defaultCursor.SetActive(true);
            return;
        }
        else
        {
            if (m_defaultCursor != null)
                m_defaultCursor.SetActive(false);
            if (m_activeCursors[m_cursorStartIndex])
            {
                m_cursors[m_cursorStartIndex].SetActive(true);
                m_currentCursorIndex = m_cursorStartIndex;
            }
            else
            {
                m_currentCursorIndex = GetFirstActiveCursorIndex();
                m_cursors[m_currentCursorIndex].SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_repositionCursor)
        {
            RepositionCursor();
            m_repositionCursor = false;
        }
        if (m_stopFlickering)
        {
            if (!m_spriteRenderer)
            {
                if (m_cursors[m_currentCursorIndex].GetComponent<Image>() != null)
                    m_cursors[m_currentCursorIndex].GetComponent<Image>().enabled = true;
            }
            else
                m_cursors[m_currentCursorIndex].GetComponent<SpriteRenderer>().enabled = true;

            if (m_stopFlickeringCounter > m_stopFlickeringTimer)
            {
                m_stopFlickeringCounter = 0;
                m_stopFlickering = false;
            }
            else
                m_stopFlickeringCounter += Time.deltaTime;
        }
        if (m_moveAction.WasPerformedThisFrame())
        {
            m_stopFlickering = true;
            m_stopFlickeringCounter = 0;
            Vector2 moveInput = m_moveAction.ReadValue<Vector2>();
            if (m_enableVertical && moveInput.y != 0f)
            {
                StepCursor((int)moveInput.y * -m_verticalStep);
            }
            if (m_enableHorizontal && moveInput.x != 0f)
            {
                StepCursor((int)moveInput.x * m_horizontalStep);
            }
        }
    }

    private void StepCursor(int step)
    {
        if (step == 0) return;
        int index = m_currentCursorIndex;
        int size = m_cursors.Length / Mathf.Abs(step);
        for (int i = 0; i < size; ++i)
        {
            index += step;
            if (m_wrapToSameRow)
            {
                if ((index + 1) % (m_verticalStep) == 0 && m_verticalStep > 1 && step == -1)
                {
                    int newVal = index + (m_verticalStep);
                    index += (m_verticalStep);
                }
                if (index % m_verticalStep == 0 && m_verticalStep > 1 && step == 1 && index != 0)
                {
                    int newVal = index - m_verticalStep;
                    index -= m_verticalStep;
                }
            }

            // these do nothing if index isnt out of bounds
            index = GetNextFromBottom(index, step);
            index = GetNextFromTop(index, step);
            if (m_activeCursors[index])
            {
                m_cursors[m_currentCursorIndex].SetActive(false);
                m_currentCursorIndex = index;
                m_cursors[m_currentCursorIndex].SetActive(true);
                return;
            }
        }
    }

    private int GetNextFromTop(int index, int step)
    {
        if (index > m_cursors.Length - 1)
        {
            index = index - m_cursors.Length;
        }
        return index;
    }
    private int GetNextFromBottom(int index, int step)
    {
        if (index < 0)
        {
            index = m_cursors.Length + index;
        }
        return index;
    }

    public void Selectable(int index)
    {
        if (m_activeCursors != null)
        {
            m_activeCursors[index] = true;
            if (m_defaultCursor != null)
                m_defaultCursor.SetActive(false);
        }
        else
        {
            Debug.LogError("Active cursor array is null!", this);
        }
    }

    public void NotSelectable(int index)
    {
        if (m_activeCursors != null)
        {
            m_activeCursors[index] = false;
            if (!m_activeCursors.Contains(true))
            {
                if (m_defaultCursor != null && m_showDefaultIfEmpty)
                    m_defaultCursor.SetActive(true);
            }
            if (m_currentCursorIndex == index)
            {
                m_cursors[m_currentCursorIndex].SetActive(false);
                m_currentCursorIndex = GetFirstActiveCursorIndex();
                m_cursors[m_currentCursorIndex].SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Active cursor array is null!", this);
        }
    }

    public void Flicker()
    {
        if (!m_stopFlickering)
        {
            m_flickerToggle = !m_flickerToggle;

            if (!m_activeCursors.Contains(true) && m_showDefaultIfEmpty)
            {
                if (!m_spriteRenderer)
                    m_defaultCursor.GetComponent<Image>().enabled = m_flickerToggle;
                else
                    m_defaultCursor.GetComponent<SpriteRenderer>().enabled = m_flickerToggle;
                return;
            }
            if (!m_spriteRenderer)
                m_cursors[m_currentCursorIndex].GetComponent<Image>().enabled = m_flickerToggle;
            else
                m_cursors[m_currentCursorIndex].GetComponent<SpriteRenderer>().enabled = m_flickerToggle;
        }
    }
}
