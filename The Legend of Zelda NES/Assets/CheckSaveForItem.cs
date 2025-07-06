using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CheckSaveForItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Events")]
    public UnityEvent m_selectableEvent;
    public UnityEvent m_notSelectableEvent;

    [Header("GameHudUpdatePanel")]
    public GameHudUpdater m_updater;

    [Header("ItemToCheckFor")]
    public string m_itemName;

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

        if (m_updater.IsItemActive(m_itemName))
        {
            m_selectableEvent.Invoke();
        }
        else
        {
            m_notSelectableEvent.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
