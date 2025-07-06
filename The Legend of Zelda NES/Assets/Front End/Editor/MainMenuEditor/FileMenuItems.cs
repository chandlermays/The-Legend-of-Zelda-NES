using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using static FileManager;

public class FileMenuItems : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [MenuItem("Custom/Find Sprite")]
    public static void FindSprite()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;
        var renderer = selected.GetComponent<SpriteRenderer>();
        if (renderer == null) return;
        Debug.Log(AssetDatabase.GetAssetPath(renderer.sprite));
        Debug.Log(renderer.sprite);
    }

    [MenuItem("Custom/Delete All Saves")]
    public static void DeleteAllSaves()
    {
        var formatter = new XmlSerializer(typeof(ZeldaSaveData));
        string path = Application.persistentDataPath + "/UserSaveFiles";
        Directory.CreateDirectory(path); // incase the directory doesnt exist

        string pathOne = GetSaveSlotFromIndex(0);
        string pathTwo = GetSaveSlotFromIndex(1);
        string pathThree = GetSaveSlotFromIndex(2);

        for (int i = 0; i < 3; ++i)
        {
            string currentPath = path;
            if (i == 0)
                currentPath += pathOne;
            else if (i == 1)
                currentPath += pathTwo;
            else
                currentPath += pathThree;

            FileStream saveStream = new FileStream(currentPath, FileMode.Create);
            Debug.Log("Deleting " + currentPath);
            ZeldaSaveData data = new ZeldaSaveData { m_index = i };
            formatter.Serialize(saveStream, data);
            saveStream.Close();
        }
    }

    [MenuItem("Custom/Create Test Saves")]
    public static void CreateTestSaves()
    {
        var formatter = new XmlSerializer(typeof(ZeldaSaveData));
        string path = Application.persistentDataPath + "/UserSaveFiles";
        Directory.CreateDirectory(path); // incase the directory doesnt exist

        string pathOne = GetSaveSlotFromIndex(0);
        string pathTwo = GetSaveSlotFromIndex(1);
        string pathThree = GetSaveSlotFromIndex(2);

        int index = 0;
        float maxHearts = 3f;
        float currentHearts = 3f;
        int rupees = 0;
        int bombs = 0;
        int keys = 0;
        int deaths = 0;
        int trifrocePieces = 0;

        bool raft = false;
        bool bookOfMagic = false;
        bool blueRing = false;
        bool redRing = false;
        bool stepLadder = false;
        bool magicalKey = false;
        bool powerBracelet = false;
        bool sword = false;
        bool whiteSword = false;
        bool magicalSword = false;
        bool magicalShield = false;
        bool boomerang = false;
        bool magicalBoomerang = false;
        bool recorder = false;
        bool bomb = false;
        bool food = false;
        bool standardArrow = false;
        bool silverArrow = false;
        bool bow = false;
        bool lifePotion = false;
        bool secondPotion = false;
        bool redCandle = false;
        bool blueCandle = false;
        bool magicalRod = false;
        string equippedItem = string.Empty;
        string[] sprites = new string[8];
        for (int i = 0; i < 3; ++i)
        {
            string currentPath = path;
            if (i == 0)
            {
                index = 0;
                maxHearts = 3f;
                currentHearts = 3f;
                rupees = 0;
                bombs = 0;
                keys = 0;
                sprites[0] = "ZeldaKeyboardFont_11";
                sprites[1] = "ZeldaKeyboardFont_27";
                sprites[2] = "ZeldaKeyboardFont_30";
                sprites[3] = "ZeldaKeyboardFont_12";
                sprites[4] = "ZeldaKeyboardFont_14";
                currentPath += pathOne;
            }
            else if (i == 1)
            {
                equippedItem = "InventoryIcons_8";
                index = 1;
                maxHearts = 6f;
                currentHearts = 4.5f;
                rupees = 340;
                bombs = 6;
                keys = 2;
                trifrocePieces = 3;
                sword = true;
                boomerang = true;
                raft = true;
                bow = true;
                standardArrow = true;
                bomb = true;
                deaths = 10;
                sprites[0] = "ZeldaKeyboardFont_19";
                sprites[1] = "ZeldaKeyboardFont_24";
                sprites[2] = "ZeldaKeyboardFont_27";
                sprites[3] = "ZeldaKeyboardFont_13";
                sprites[4] = "ZeldaKeyboardFont_10";
                sprites[5] = "ZeldaKeyboardFont_23";
                currentPath += pathTwo;
            }
            else
            {
                equippedItem = "InventoryIcons_6";
                index = 2;
                maxHearts = 6f;
                currentHearts = 4.5f;
                rupees = 560;
                bombs = 9;
                keys = 3;
                trifrocePieces = 7;
                deaths = 100;
                sword = true;
                raft = true;
                bow = true;
                boomerang = true;
                standardArrow = true;
                bomb = true;
                redCandle = true;
                sword = true;
                standardArrow = true;
                blueRing = true;
                bookOfMagic = true;
                food = true;
                lifePotion = true;
                recorder = true;
                magicalRod = true;

                sprites[0] = "ZeldaKeyboardFont_12";
                sprites[1] = "ZeldaKeyboardFont_17";
                sprites[2] = "ZeldaKeyboardFont_10";
                sprites[3] = "ZeldaKeyboardFont_23";
                sprites[4] = "ZeldaKeyboardFont_13";
                sprites[5] = "ZeldaKeyboardFont_21";
                sprites[6] = "ZeldaKeyboardFont_14";
                sprites[7] = "ZeldaKeyboardFont_27";
                currentPath += pathThree;
            }

            FileStream saveStream = new FileStream(currentPath, FileMode.Create);
            Debug.Log("Creating new save " + currentPath);
            ZeldaSaveData data = new ZeldaSaveData
            {
                m_currentHeldItemName = "InventoryIcons_6",
                m_index = index,
                m_maxHeartCount = maxHearts,
                m_triforceCount = trifrocePieces,
                m_heartCount = currentHearts,
                m_rupeeCount = rupees,
                m_bombCount = bombs,
                m_keyCount = keys,
                m_nameArray = sprites,
                m_active = true,
                m_blueCandle = blueCandle,
                m_bomb = bomb,
                m_bookOfMagic = bookOfMagic,
                m_blueRing = blueRing,
                m_boomerang = boomerang,
                m_magicalBoomerang = magicalBoomerang,
                m_food = food,
                m_powerBracelet = powerBracelet,
                m_bow = bow,
                m_lifePotion = lifePotion,
                m_magicalKey = magicalKey,
                m_magicalRod = magicalRod,
                m_magicalShield = magicalShield,
                m_magicalSword = magicalSword,
                m_raft = raft,
                m_recorder = recorder,
                m_redCandle = redCandle,
                m_redRing = redRing,
                m_secondPotion = secondPotion,
                m_silverArrow = silverArrow,
                m_standardArrow = standardArrow,
                m_stepLadder = stepLadder,
                m_sword = sword,
                m_whiteSword = whiteSword,
                m_deathTotal = deaths
            };
            formatter.Serialize(saveStream, data);
            saveStream.Close();
        }
    }
}