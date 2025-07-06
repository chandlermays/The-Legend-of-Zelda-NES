using UnityEngine;
using UnityEngine.Events;

public class PanelController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Panel Images")]
    public GameObject[] m_nameArray;
    public GameObject[] m_deathCounterArray;
    public GameObject[] m_heartArray;
    public GameObject m_link;

    [Header("Events")]
    public UnityEvent m_selectableEvent;
    public UnityEvent m_notSelectableEvent;

    [Header("Save Info")]
    public SaveSlot m_slot;

    [Header("File Manager")]
    public GameObject m_fileManagerObject;

    ZeldaSaveData m_data;

    bool m_selected = false;
    bool m_selectable = false;
    bool m_activeProfile = false;

    [Header("Heart Images")]
    public Sprite m_fullHeartSprite;
    public Sprite m_halfHeartSprite;
    public Sprite m_emptyHeartSprite;

    [Header("Number Images")] // for death counter
    public Sprite[] m_numberSpriteArray;

    private void Start()
    {
        if (m_selectableEvent == null)
        {
            m_selectableEvent = new UnityEvent();
        }
        if (m_notSelectableEvent == null)
        {
            m_notSelectableEvent = new UnityEvent();
        }
    }

    public void GetSave()
    {
        if (m_fileManagerObject.TryGetComponent<FileManager>(out var fileManager))
        {
            ZeldaSaveData data = fileManager.GetSaveData(m_slot);
            if (data != null)
            {
                if (data.m_active)
                {
                    m_activeProfile = true;
                    m_selectable = true;
                    if (m_selectableEvent != null)
                        m_selectableEvent.Invoke();
                    SetVariablesFromData(data);
                    return;
                }
            }
        }
        m_selectable = false;
        m_activeProfile = false;
        if (m_notSelectableEvent != null)
            m_notSelectableEvent.Invoke();
    }

    private void OnEnable()
    {
        if (m_fileManagerObject.TryGetComponent<FileManager>(out var fileManager))
        {
            ZeldaSaveData data = fileManager.GetSaveData(m_slot);
            if (data != null)
            {
                if (data.m_active)
                {
                    m_selectable = true;
                    m_activeProfile = true;
                    if (m_selectableEvent != null)
                        m_selectableEvent.Invoke();
                    SetVariablesFromData(data);
                    return;
                }
            }
        }
        if (m_nameArray != null && m_nameArray.Length > 0)
        {
            foreach (GameObject letter in m_nameArray)
            {
                letter.GetComponent<SpriteRenderer>().sprite = null;
            }
        }
        if (m_heartArray != null && m_heartArray.Length > 0)
        {
            foreach (GameObject heart in m_heartArray)
            {
                heart.SetActive(false);
            }
        }
        if (m_deathCounterArray != null && m_deathCounterArray.Length > 0)
        {
            foreach (GameObject deathCounter in m_deathCounterArray)
            {
                deathCounter.SetActive(false);
            }
        }
        if (m_notSelectableEvent != null)
            m_notSelectableEvent.Invoke();
    }
    private void SetVariablesFromData(ZeldaSaveData data)
    {
        m_data = data;
        if (data.m_nameArray != null && m_nameArray != null)
        {
            int smallerLength = data.m_nameArray.Length > m_nameArray.Length ? m_nameArray.Length : data.m_nameArray.Length;
            for (int i = 0; i < smallerLength; ++i)
            {
                if (data.m_nameArray[i] != null)
                {
                    m_nameArray[i].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(data.m_nameArray[i]);
                    if (m_nameArray[i].GetComponent<SpriteRenderer>().sprite == null)
                    {
                        Debug.LogError("Sprite failed to load: " + data.m_nameArray[i]);
                    }
                }
            }
        }
        else
        {
            if (m_nameArray != null && m_nameArray.Length > 0)
            {
            }
        }
        if (m_deathCounterArray != null && m_numberSpriteArray != null)
        {
            if (m_deathCounterArray.Length > 0 && m_numberSpriteArray.Length > 0)
            {
                InitDeathArray(m_deathCounterArray, m_data.m_deathTotal);
            }
        }

        if (m_heartArray != null && m_heartArray.Length > 0)
        {
            if (m_heartArray.Length > 0)
            {
                for (int i = 0; i < m_data.m_maxHeartCount; ++i)
                {
                    m_heartArray[i].SetActive(true);
                }
            }
        }
        else
        {
            if (m_heartArray != null)
            {
                foreach (var heart in m_heartArray)
                {
                    heart.SetActive(false);
                }
            }
        }
    }

    public void Selected()
    {
        m_selected = true;
        FileManager.SetCurrentProfile(m_slot);
    }

    public void UnSelected()
    {
        m_selected = false;
    }

    public void DeleteSave()
    {
        if (m_selected)
        {
            if (m_fileManagerObject.TryGetComponent<FileManager>(out var fileManager))
            {
                FileManager.DestroySave(m_slot);
                SetVariablesFromData(fileManager.GetSaveData(m_slot)); // should be reset values
                for (int i = 0; i < m_nameArray.Length; ++i)
                {
                    m_nameArray[i].GetComponent<SpriteRenderer>().sprite = null;
                }
                m_activeProfile = false;
                m_selectable = false;
                m_notSelectableEvent.Invoke();
            }
            else
                Debug.LogError("Failed to get file manager from tag!");
        }
    }

    public void CreateNewPlayer()
    {
        if (!m_activeProfile)
        {
            bool m_createCharacter = false;
            foreach (var box in m_nameArray)
            {
                Sprite sprite = box.GetComponent<SpriteRenderer>().sprite;
                if (sprite != null)
                {
                    m_createCharacter = true;
                    break;
                }
            }
            if (m_createCharacter)
            {
                if (m_fileManagerObject.TryGetComponent<FileManager>(out var fileManager))
                {
                    fileManager.CreateNewSave(m_slot, m_nameArray);
                    SetVariablesFromData(fileManager.GetSaveData(m_slot)); // should be reset values
                    m_selectableEvent.Invoke();
                }
            }
        }
    }

    public bool GetSelectable() { return m_selectable; }

    private void InitDeathArray(GameObject[] array, int count)
    {
        if (count > 99)
        {
            array[0].SetActive(true);
            array[1].SetActive(true);
            array[2].SetActive(true);

            int firstDigit = count / 100;
            array[0].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[firstDigit];
            array[0].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;
            count -= firstDigit * 100;

            int secondDigit = count / 10;
            array[1].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[secondDigit];
            array[1].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;

            int thirdDigit = count % 10;
            array[2].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[thirdDigit];
            array[2].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;
            return;
        }

        if (count > 9)
        {
            array[1].SetActive(true);
            array[2].SetActive(true);
            int firstDigit = count / 10;
            array[1].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[firstDigit];
            array[1].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;
            int secondDigit = count % 10;
            array[2].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[secondDigit];
            array[2].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;
            return;
        }
        else
        {
            array[2].SetActive(true);
            array[2].GetComponent<UnityEngine.SpriteRenderer>().sprite = m_numberSpriteArray[count];
            array[2].GetComponent<UnityEngine.SpriteRenderer>().enabled = true;
        }
    }
}
