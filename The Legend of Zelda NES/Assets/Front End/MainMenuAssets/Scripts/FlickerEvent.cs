using UnityEngine;
using UnityEngine.Events;

public class FlickerEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    public UnityEvent m_flickerEvent;

    [Header("Flicker Attributes")]
    public float m_timeBeforeFlicker;

    float m_flickerCounter;
    void Start()
    {
        if (m_flickerEvent == null)
        {
            m_flickerEvent = new UnityEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_timeBeforeFlicker > 0)
        {
            if (m_flickerCounter > m_timeBeforeFlicker)
            {
                m_flickerEvent.Invoke();
                m_flickerCounter = 0;
            }
            else
                m_flickerCounter += Time.deltaTime;
        }
    }
}
