using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.Events;
using FileMode = System.IO.FileMode;
public enum SaveSlot // to prevent saving at a different index
{
    kSaveOne,
    kSaveTwo,
    kSaveThree
}

[XmlRoot("ZeldaSaveData")]
[System.Serializable]
public class ZeldaSaveData
{
    [XmlAttribute("Index")]
    public int m_index = 0;

    public float m_maxHeartCount = 3f;
    public float m_heartCount = 3f;

    [XmlElement("Bombs")]
    public int m_bombCount = 0;

    [XmlElement("Rupees")]
    public int m_rupeeCount = 0;

    [XmlElement("Keys")]
    public int m_keyCount = 0;

    [XmlElement("TriforcePieces")]
    public int m_triforceCount = 0;

    public bool m_raft = false;
    public bool m_bookOfMagic = false;
    public bool m_blueRing = false;
    public bool m_redRing = false;
    public bool m_stepLadder = false;
    public bool m_magicalKey = false;
    public bool m_powerBracelet = false;

    public bool m_sword = false;
    public bool m_whiteSword = false;
    public bool m_magicalSword = false;
    public bool m_magicalShield = false;
    public bool m_boomerang = false;
    public bool m_magicalBoomerang = false;
    public bool m_recorder = false;
    public bool m_bomb = false;
    public bool m_food = false;
    public bool m_standardArrow = false;
    public bool m_silverArrow = false;
    public bool m_bow = false;
    public bool m_lifePotion = false;
    public bool m_secondPotion = false;
    public bool m_redCandle = false;
    public bool m_blueCandle = false;
    public bool m_magicalRod = false;

    [XmlElement("CurrentHeldItem")]
    public string m_currentHeldItemName = string.Empty;

    [XmlArray("NameSprites"),
    XmlArrayItem("LetterSprite")]
    public string[] m_nameArray = new string[8];

    [XmlElement("Active")]
    public bool m_active = false;

    [XmlElement("DeathCount")]
    public int m_deathTotal = 0;
}
public class FileManager : MonoBehaviour
{

    static string m_path;
    static string m_saveSlotOne = "/SaveOne.xml";
    static string m_saveSlotTwo = "/SaveTwo.xml";
    static string m_saveSlotThree = "/SaveThree.xml";
    static int m_currentProfile = 0;
    static bool m_loaded = false;

    [Header("Events")]
    static public UnityEvent m_updateSaveDataEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    static private ZeldaSaveData[] m_data = new ZeldaSaveData[3];
    void Awake()
    {
        m_path = Application.persistentDataPath + "/UserSaveFiles";
        Directory.CreateDirectory(m_path); // incase the directory doesnt exist
        if (m_updateSaveDataEvent == null)
        {
            m_updateSaveDataEvent = new UnityEvent();
        }
    }

    static public void ResetSave()
    {
        DestroySave((SaveSlot)m_currentProfile);
    }

    private void Start()
    {
        if (m_loaded == false)
        {
            LoadSaveData(SaveSlot.kSaveOne);
            LoadSaveData(SaveSlot.kSaveTwo);
            LoadSaveData(SaveSlot.kSaveThree);
            m_updateSaveDataEvent.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {  
        //
    }

    public void LoadSaveData(SaveSlot slot)
    {
        try
        {
            int index = (int)slot;
            string path = GetPathFromIndex(index);

            if (File.Exists(path))
            {
                using (var loadStream = new FileStream(path, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(ZeldaSaveData));

                    try
                    {

                        var data = formatter.Deserialize(loadStream) as ZeldaSaveData;

                        if (data == null)
                        {
                            Debug.LogError("Deserialized data is null or invalid.");
                            return;
                        }
                        m_data[index] = data;

                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.LogError($"Failed to deserialize save data: {ex.Message}");
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception and return null
            Debug.LogError($"Loading save data has failed!: {ex.Message}");
            return;
        }
        return;
    }


    public ZeldaSaveData GetSaveData(SaveSlot slot)
    {
        try
        {
            int index = (int)slot;
            string path = GetPathFromIndex(index);

            if (File.Exists(path))
            {
                using (var loadStream = new FileStream(path, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(ZeldaSaveData));

                    try
                    {

                        var data = formatter.Deserialize(loadStream) as ZeldaSaveData;

                        if (data == null)
                        {
                            Debug.LogError("Deserialized data is null or invalid.");
                            return null;
                        }

                        return data;

                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.LogError($"Failed to deserialize save data: {ex.Message}");
                        return null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception and return null
            Debug.LogError($"Getting save data has failed!: {ex.Message}");
            return null;
        }
        return null;
    }


    public void Save(SaveSlot slot)
    {
        int index = (int)slot;
        string path = GetPathFromIndex(index);
        var formatter = new XmlSerializer(typeof(ZeldaSaveData));
        if (path != null)
        {
            using (var saveStream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(saveStream, m_data[index]);
                Debug.Log("Saving...: " + path);
                saveStream.Close();
                m_updateSaveDataEvent.Invoke();
            }
        }
    }

    public void CreateNewSave(SaveSlot slot, GameObject[] nameArray)
    {
        try
        {
            int index = (int)slot;
            string[] array = null;
            if (nameArray != null)
            {
                array = new string[nameArray.Length];
            }
            else
            {
                Debug.LogError("CreateNewSave was given invalid name array!");
                return;
            }

            int i = 0;
            foreach (var name in nameArray)
            {
                Sprite sprite = name.GetComponent<SpriteRenderer>().sprite;
                if (sprite != null)
                {
                    string spritePath = sprite.name;
                    array[i] = spritePath;
                }
                else
                {
                    array[i] = null;
                }
                ++i;
            }
            var data = new ZeldaSaveData { m_index = index, m_nameArray = array, m_active = true };

            string path = GetPathFromIndex(index);
            if (path != null)
            {
                using (var saveStream = new FileStream(path, FileMode.Create))
                {
                    var formatter = new XmlSerializer(typeof(ZeldaSaveData));
                    formatter.Serialize(saveStream, data);
                    Debug.Log("New Save Created!: " + path);
                    saveStream.Close();
                }
            }
            m_updateSaveDataEvent.Invoke();
            switch (slot)
            {
                case SaveSlot.kSaveOne:
                    m_data[0] = data;
                    break;
                case SaveSlot.kSaveTwo:
                    m_data[1] = data;
                    break;
                case SaveSlot.kSaveThree:
                    m_data[2] = data;
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log the exception and return null
            Debug.LogError($"Create new save has failed!: {ex.Message}");
        }
    }

    static public void DestroySave(SaveSlot slot)
    {
        int index = (int)slot;
        string path = GetPathFromIndex(index);
        var formatter = new XmlSerializer(typeof(ZeldaSaveData));
        var data = new ZeldaSaveData { m_index = (int)slot };
        m_data[(int)slot] = data;

        if (path != null)
        {
            using (var saveStream = new FileStream(path, FileMode.Truncate))
            {
                formatter.Serialize(saveStream, data);
                Debug.Log("DestroyedSave " + path);
                saveStream.Close();
                m_updateSaveDataEvent.Invoke();
            }
        }
    }

    static public string GetPathFromIndex(int index)
    {
        string path = null;
        if (index == 0)
            path = m_path + m_saveSlotOne;
        else if (index == 1)
            path = m_path + m_saveSlotTwo;
        else
            path = m_path + m_saveSlotThree;
        return path;
    }

    // for editor files that only need the end file name
    static public string GetSaveSlotFromIndex(int index) 
    {
        string path = null;
        if (index == 0)
            path = m_saveSlotOne;
        else if (index == 1)
            path = m_saveSlotTwo;
        else
            path = m_saveSlotThree;
        return path;
    }

    static public ZeldaSaveData GetCurrentProfile()
    {
        return m_data[m_currentProfile];
    }
    static public void SetCurrentProfile(SaveSlot slot)
    {
        m_currentProfile = (int)slot;
    }

    static public void SaveCurrentGame()
    {
        string path = GetPathFromIndex(m_currentProfile);
        var formatter = new XmlSerializer(typeof(ZeldaSaveData));
        if (path != null)
        {
            using (var saveStream = new FileStream(path, FileMode.Create))
            {
                // since everything is passed by reference, this will contain the newest data already
                formatter.Serialize(saveStream, m_data[m_currentProfile]);
                // Debug.Log("Saving...: " + path);
                saveStream.Close();
                m_updateSaveDataEvent.Invoke();
            }
        }
    }
}
