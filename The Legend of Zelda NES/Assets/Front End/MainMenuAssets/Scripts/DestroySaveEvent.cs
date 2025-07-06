using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DestroySaveEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    public UnityEvent m_registerEvent;

    InputAction m_deleteAction;
    void Start()
    {
        if (m_registerEvent == null)
        {
            m_registerEvent = new UnityEvent();
        }
        m_deleteAction = InputSystem.actions.FindAction("UIKeyboardSelect");
    }

    // Update is called once per frame
    void Update()
    {
        if (m_deleteAction.WasPerformedThisFrame())
        {
            m_registerEvent.Invoke();
        }
    }
}
