using UnityEngine;

public class ActiveToggle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Objects To Enable - One")]
    public GameObject[] m_activeArrayOne;
    public bool m_activeOne = true;

    [Header("Objects To Enable - Two")]
    public GameObject[] m_activeArrayTwo;
    public bool m_activeTwo = false;

    void Start()
    {
        ToggleActives();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleActives()
    {
        foreach (GameObject one in m_activeArrayOne)
        {
            one.SetActive(m_activeOne);
        }
        foreach (GameObject two in m_activeArrayTwo)
        {
            two.SetActive(m_activeTwo);
        }
        m_activeOne = !m_activeOne;
        m_activeTwo = !m_activeTwo;
    }
}
