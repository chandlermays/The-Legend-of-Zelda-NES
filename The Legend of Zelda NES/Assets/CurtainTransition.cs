using UnityEngine;

public class CurtainTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject m_leftCurtain;
    public GameObject m_rightCurtain;
    public int m_screenWidth = 768;
    public int m_speed = 10;

    bool m_activeCurtains = false;
    static bool m_startTransition = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_startTransition)
        {
            m_startTransition = false;
            ActivateTransition();
        }
        if (m_activeCurtains)
        {
            GameHudUpdater.SetPause(true);
            AccessInventory.DisableInventory(true);
            var leftPos = m_leftCurtain.GetComponent<Transform>().localPosition;
            var rightPos = m_rightCurtain.GetComponent<Transform>().localPosition;
            leftPos.x -= m_speed;
            rightPos.x += m_speed;
            m_leftCurtain.GetComponent<Transform>().localPosition = leftPos;
            m_rightCurtain.GetComponent<Transform>().localPosition = rightPos;
            if (leftPos.x < -m_screenWidth && rightPos.x > m_screenWidth)
            {
                m_activeCurtains = false;
                GameHudUpdater.SetPause(false);
                AccessInventory.DisableInventory(false);
                m_leftCurtain.SetActive(false);
                m_rightCurtain.SetActive(false);
            }
        }
    }

    public void ActivateTransition()
    {
   //     Debug.Log("Activate Transition");
        m_leftCurtain.SetActive(true);
        m_rightCurtain.SetActive(true);
        var leftPos = m_leftCurtain.GetComponent<Transform>().localPosition;
        var rightPos = m_rightCurtain.GetComponent<Transform>().localPosition;
        leftPos.x = -(m_screenWidth / 4);
        rightPos.x = (m_screenWidth / 4);
        m_leftCurtain.GetComponent<Transform>().localPosition = leftPos;
        m_rightCurtain.GetComponent<Transform>().localPosition = rightPos;
        m_activeCurtains = true;
    }
    static public void StartTransition()
    {
  //      Debug.Log("Start Transition");
        m_startTransition = true;
    }
}
