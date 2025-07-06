using UnityEngine;
using UnityEngine.Events;

public class SelectPanel : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent m_selectEvent;
    public UnityEvent m_deselectEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (m_selectEvent == null)
        {
            m_selectEvent = new UnityEvent();
        }
    }

    private void OnEnable()
    {
        m_selectEvent.Invoke();
    }

    private void OnDisable()
    {
        m_deselectEvent.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
