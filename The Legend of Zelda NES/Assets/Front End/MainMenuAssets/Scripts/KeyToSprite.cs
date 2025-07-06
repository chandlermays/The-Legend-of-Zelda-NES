using UnityEngine;

public class KeyToSprite : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Sprite[] m_fontIcons;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Sprite GetKey(int index)
    {
        return m_fontIcons[index];
    }
}