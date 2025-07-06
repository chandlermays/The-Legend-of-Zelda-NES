using UnityEngine;
using UnityEngine.Events;

public class SelectableEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    public UnityEvent m_selectableEvent;
    public UnityEvent m_notSelectableEvent;

    void Start()
    {
        if (m_selectableEvent == null)
        {
            m_selectableEvent = new UnityEvent();
        }
        if (m_notSelectableEvent == null)
        {
            m_notSelectableEvent = new UnityEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IsSelectable()
    {
        if (m_selectableEvent != null)
        {
            m_selectableEvent.Invoke();
        }
    }

    public void NotSelectable()
    {
        if (m_notSelectableEvent != null)
        {
            m_notSelectableEvent.Invoke();
        }
    }
}
