using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//[System.Serializable] public class _UnityEventSprite : UnityEvent<Sprite> { }
//[SerializeField] public class _UnityEventSprite : UnityEvent<Sprite> { /*Sprite m_sprite;*/ }

public class ActionEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    [SerializeField] public UnityEvent m_pressedEvent;
    [SerializeField] public string m_actionName;

    InputAction m_moveAction;
    InputAction m_selectAction;
    void Start()
    {
        if (m_pressedEvent == null)
        {
            m_pressedEvent = new UnityEvent();
        }
        m_selectAction = InputSystem.actions.FindAction(m_actionName);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_selectAction != null)
        {
            if (m_selectAction.WasPerformedThisFrame())
            {
                if (m_pressedEvent != null)
                {
                    m_pressedEvent.Invoke();
                }
            }
        }
    }
}
