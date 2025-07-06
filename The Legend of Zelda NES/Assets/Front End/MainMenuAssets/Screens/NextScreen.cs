using UnityEngine;
using UnityEngine.InputSystem;

public class NextScreen : MonoBehaviour
{
    [Header("Screen Movement")]
    public GameObject[] m_nextScreensToActivate;
    public GameObject[] m_screensToDisable;

    InputAction m_nextScreenAction;
    bool m_switchNextFrame = false;
    public bool m_switchOnFirstUpdate = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_nextScreenAction = InputSystem.actions.FindAction("UISelect");
    }

    // Update is called once per frame
    void Update()
    {
        if (m_switchNextFrame)
        {
            ActivateScreen();
        }
        if (m_nextScreenAction.WasPerformedThisFrame())
        {
            if (m_switchOnFirstUpdate)
                m_switchNextFrame = true;
            else
                ActivateScreen();

        }
    }

    public void ActivateScreen()
    {
        foreach (GameObject screen in m_nextScreensToActivate)
        { 
            screen.SetActive(true);
        }
        foreach (GameObject screen in m_screensToDisable)
        {
            screen.SetActive(false);
        }
        m_switchNextFrame= false;
    }
}