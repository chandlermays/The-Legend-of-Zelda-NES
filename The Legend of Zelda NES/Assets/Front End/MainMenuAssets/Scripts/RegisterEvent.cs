using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RegisterEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    public UnityEvent m_registerEvent;
    public string m_selectActionName = string.Empty;
    InputAction m_selectAction;
    void Start()
    {
        if (m_registerEvent == null)
        {
            m_registerEvent = new UnityEvent();
        }
        if (m_selectActionName == string.Empty)
        {
            m_selectActionName = "UIKeyboardSelect";
        }
        m_selectAction = InputSystem.actions.FindAction(m_selectActionName);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_selectAction.WasPerformedThisFrame())
        {
            m_registerEvent.Invoke();
        }
    }
}
