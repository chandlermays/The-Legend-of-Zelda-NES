using UnityEngine;

public class Pause : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public MonoBehaviour[] m_scriptsToDisable;
    public Animator[] m_animatorsToDisable;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameHudUpdater.GetPause())
        {
            foreach (var script in m_scriptsToDisable)
            {
                script.enabled = false;
            }
            foreach (var animation in m_animatorsToDisable)
            {
                animation.enabled = false;
            }
        }
        else
        {
            foreach (var script in m_scriptsToDisable)
            {
                script.enabled = true;
            }
            foreach (var animation in m_animatorsToDisable)
            {
                animation.enabled = true;
            }
        }
    }
}
