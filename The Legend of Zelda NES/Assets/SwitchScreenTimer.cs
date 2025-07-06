using UnityEngine;

public class SwitchScreenTimer : MonoBehaviour
{
    [Header("Screen Movement")]
    public GameObject[] m_nextScreensToActivate;
    public GameObject[] m_screensToDisable;

    public float m_timeBeforeSwitch = 0f;
    private float m_currentTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentTime < m_timeBeforeSwitch)
        {
            m_currentTime += Time.deltaTime;
        }
        else
            SwitchScreen();
    }

    public void SwitchScreen()
    {
        m_currentTime = 0f;
        foreach (GameObject screen in m_nextScreensToActivate)
        {
            screen.SetActive(true);
        }
        foreach (GameObject screen in m_screensToDisable)
        {
            screen.SetActive(false);
        }
    }
}
