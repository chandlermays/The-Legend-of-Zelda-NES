using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Scenes")]
    public string m_newSceneName;

    public void SwitchScenes()
    {
        SceneManager.LoadScene(m_newSceneName);
    }
}