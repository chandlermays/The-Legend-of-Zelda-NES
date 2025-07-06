using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathOptions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameHudUpdater m_gameHudUpdater;
    public enum Options
    {
        kContinue,
        kSave,
        kRetry,
        kNone
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PickOption(Options options)
    {
        switch (options)
        {
            case Options.kContinue:
                Continue();
                break;
            case Options.kSave:
                Save();
                break;
            case Options.kRetry:
                Retry();
                break;
            default:
                break;
        }
    }
    void Continue()
    {
        //
    }

    void Save()
    {
        //
    }

    void Retry()
    {
        //
    }
}
