using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [Header("Screen Movement")]
    public GameObject[] m_nextScreensToActivate;
    public GameObject[] m_screensToDisable;

    public float m_timeBeforeSwitch = 0f;
    private float m_currentTime = 0f;

    public float m_cameraSpeed = 1.0f;
    bool m_pan = false;
    Vector3 m_endPosition = new Vector3(0, -73, 0); // Example value for m_endPosition
    Vector3 m_startPosition;
    Camera m_mainCamera;

    void Start()
    {
        m_mainCamera = Camera.main;
        m_mainCamera.enabled = true;
        m_pan = true;
        m_startPosition = m_mainCamera.transform.position;
    }

    void Update()
    {
        if (m_pan)
        {
            Vector3 newPosition = m_mainCamera.transform.position;
            newPosition.y -= m_cameraSpeed * Time.deltaTime; // Added Time.deltaTime for frame rate independence
            m_mainCamera.transform.position = newPosition;

            if (m_mainCamera.transform.position.y <= m_endPosition.y)
            {
                m_pan = false;
            }
        }
        if (!m_pan)
        {
            if (m_currentTime < m_timeBeforeSwitch)
            {
                m_currentTime += Time.deltaTime;
            }
            else
            {
                m_currentTime = 0f;
                Reset();
                SwitchScreens();
            }
        }
    }

    private void SwitchScreens()
    {
        foreach (GameObject screen in m_nextScreensToActivate)
        {
            screen.SetActive(true);
        }
        foreach (GameObject screen in m_screensToDisable)
        {
            screen.SetActive(false);
        }
    }

    public void StartMoving()
    {
        m_pan = true;
    }

    public void Reset()
    {
        m_mainCamera.transform.position = m_startPosition;
        m_pan = true;
    }
}