using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameHudUpdater : MonoBehaviour
{
    [Header("Panel Variables")]
    public GameObject[] m_heartArray;
    public GameObject[] m_rupeeArray;
    public GameObject[] m_keyCountArray;
    public GameObject[] m_bombCountArray;
    public GameObject[] m_triforceArray;
    public GameObject[] m_boomerangs;
    public GameObject[] m_arrows;
    public GameObject[] m_potions;
    public GameObject[] m_candles;
    public GameObject[] m_rings;
    public Sprite[] m_swords;
    public GameObject m_raft;
    public GameObject m_bookOfMagic;
    public GameObject m_stepLadder;
    public GameObject m_magicalkey;
    public GameObject m_powerBracelet;
    public GameObject m_bomb;
    public GameObject m_food;
    public GameObject m_recorder;
    public GameObject m_magicalRod;
    public GameObject m_bow;

    [Header("Item Slots")]
    public GameObject m_aContainer;
    public GameObject m_bContainer;
    public GameObject m_inventoryItemContainer;

    [Header("Map")]
    public GameObject m_mapContainer;

    [Header("Text Sprites")]
    public Sprite[] m_numberSpriteArray;
    public Sprite m_xSprite;

    [Header("Heart Images")]
    public Sprite m_fullHeartSprite;
    public Sprite m_halfHeartSprite;
    public Sprite m_emptyHeartSprite;

    [Header("Link Controller")]
    public PlayerController m_link;

    [Header("Starter Sword Pickup")]
    public GameObject[] m_starterSwordPickupArea;

    static bool m_updateVisibility = false;
    static bool m_updateEquipVisibility = false;
    static bool m_saveGame = true;

    // placeholder location
    static bool m_pauseGame = false;

    static private ZeldaSaveData m_currentProfile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (FileManager.GetCurrentProfile() == null)
        {
            m_currentProfile = new ZeldaSaveData();
            Debug.LogError("Null save profile!");
        }
        else
            m_currentProfile = FileManager.GetCurrentProfile();

        if (m_currentProfile == null)
        {
            Debug.LogError("Current profile is null!");
        }
        if (m_link == null)
        {
            Debug.LogError("Link is null!");
        }

        InitHeartPanel();
        m_link.SetHearts(m_currentProfile.m_maxHeartCount, m_currentProfile.m_heartCount);
        InitStackables(m_rupeeArray, m_currentProfile.m_rupeeCount);
        InitStackables(m_bombCountArray, m_currentProfile.m_bombCount);
        InitStackables(m_keyCountArray, m_currentProfile.m_keyCount);
        m_link.LoadStackable("Rupee", m_currentProfile.m_rupeeCount);
        m_link.LoadStackable("Bomb", m_currentProfile.m_bombCount);
        m_link.LoadStackable("Key", m_currentProfile.m_keyCount);

        SetItemVisibility(true);
        SetEquippedItemVisibility();
    }

    // Update is called once per frame
    private void OnEnable()
    {
        CurtainTransition.StartTransition();
    }
    void Update()
    {
        if (m_updateVisibility)
        {
            SetItemVisibility();
            m_updateVisibility = false;
        }
        if (m_updateEquipVisibility)
        {
            SetEquippedItemVisibility();
            m_updateEquipVisibility = false;
        }
        if (m_saveGame)
        {
            SaveGame();
            m_saveGame = false;
        }
    }

    private void SetArrayNumbers(GameObject[] array, int count)
    {
        if (count > 99)
        {
            array[0].SetActive(true);
            array[1].SetActive(true);
            array[2].SetActive(true);

            int firstDigit = count / 100;
            array[0].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[firstDigit];
            array[0].GetComponent<UnityEngine.UI.Image>().enabled = true;
            count -= firstDigit * 100;

            int secondDigit = count / 10;
            array[1].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[secondDigit];
            array[1].GetComponent<UnityEngine.UI.Image>().enabled = true;

            int thirdDigit = count % 10;
            array[2].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[thirdDigit];
            array[2].GetComponent<UnityEngine.UI.Image>().enabled = true;
            return;
        }
        else // if not greater than 99, then we set the first digit to be the x
        {
            array[0].SetActive(true);
            array[0].GetComponent<UnityEngine.UI.Image>().sprite = m_xSprite;
        }

        if (count > 9)
        {
            array[1].SetActive(true);
            array[2].SetActive(true);
            int firstDigit = count / 10;
            array[1].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[firstDigit];
            array[1].GetComponent<UnityEngine.UI.Image>().enabled = true;
            int secondDigit = count % 10;
            array[2].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[secondDigit];
            array[2].GetComponent<UnityEngine.UI.Image>().enabled = true;
            return;
        }
        else
        {
            array[1].SetActive(true);
            array[1].GetComponent<UnityEngine.UI.Image>().sprite = m_numberSpriteArray[count];
            array[1].GetComponent<UnityEngine.UI.Image>().enabled = true;
        }
    }

    private void SetHearts()
    {
        if (m_heartArray.Length > 0)
        {
            for (int i = 0; i < m_heartArray.Length && i < m_currentProfile.m_maxHeartCount; ++i)
            {
                m_heartArray[i].SetActive(true);

                var heartImage = m_heartArray[i].GetComponent<UnityEngine.UI.Image>();

                if (i < m_currentProfile.m_heartCount)
                {
                    // Full or half heart
                    heartImage.sprite = (m_currentProfile.m_heartCount - i) == 1
                        ? m_fullHeartSprite : (m_currentProfile.m_heartCount - i) == 0 
                        ? m_emptyHeartSprite : m_halfHeartSprite;
                }
                else
                {
                    // Empty heart
                    heartImage.sprite = m_emptyHeartSprite;
                }
            }
        }

        m_link.SetHearts(m_currentProfile.m_maxHeartCount, m_currentProfile.m_heartCount);
    }

    private void SetTriforce()
    {
        if (m_triforceArray.Length > 0)
        {
            for (int i = 0; i < m_triforceArray.Length && i < m_currentProfile.m_triforceCount; ++i)
            {
                Debug.Log("Obtained Triforce Piece: " + i);
                m_triforceArray[i].SetActive(true);
            }
        }
    }

    private void SetItemVisibility(bool value, GameObject obj, string name)
    {
        if (obj == null)
        {
            Debug.LogError(name + " Is null!");
            return;
        }
        obj.SetActive(value);
        if (value == true)
        {
            m_link.LoadItem(name);
        }
    }

    private void SetItemVisibility(bool loading = false)
    {
        if (m_currentProfile == null)
        {
            Debug.LogError("Current profile is null!");
            return;
        }
        SetItemVisibility(m_currentProfile.m_raft, m_raft, "Raft");
        SetItemVisibility(m_currentProfile.m_bookOfMagic, m_bookOfMagic, "BookOfMagic");

        if (m_currentProfile.m_redRing == false)
            SetItemVisibility(m_currentProfile.m_blueRing, m_rings[0], "BlueRing");
        else
            SetItemVisibility(m_currentProfile.m_redRing, m_rings[1], "RedRing");

        SetItemVisibility(m_currentProfile.m_stepLadder, m_stepLadder, "StepLadder");
        SetItemVisibility(m_currentProfile.m_magicalKey, m_magicalkey, "MagicalKey");
        SetItemVisibility(m_currentProfile.m_powerBracelet, m_powerBracelet, "PowerBracelet");

        SetItemVisibility(m_currentProfile.m_bomb, m_bomb, "Bomb");

        if (m_currentProfile.m_magicalBoomerang == false)
            SetItemVisibility(m_currentProfile.m_boomerang, m_boomerangs[0], "Boomerang");
        else
            SetItemVisibility(m_currentProfile.m_magicalBoomerang, m_boomerangs[1], "MagicalBoomerang");

        if (m_currentProfile.m_silverArrow == false)
            SetItemVisibility(m_currentProfile.m_standardArrow, m_arrows[0], "Arrow");
        else
            SetItemVisibility(m_currentProfile.m_silverArrow, m_arrows[1], "SilverArrow");

        SetItemVisibility(m_currentProfile.m_bow, m_bow, "Bow");

        if (m_currentProfile.m_redCandle == false)
            SetItemVisibility(m_currentProfile.m_blueCandle, m_candles[1], "BlueCandle");
        else
            SetItemVisibility(m_currentProfile.m_redCandle, m_candles[0], "RedCandle");

        SetItemVisibility(m_currentProfile.m_recorder, m_recorder, "Recorder");
        SetItemVisibility(m_currentProfile.m_food, m_food, "Food");

        if (m_currentProfile.m_secondPotion == false)
            SetItemVisibility(m_currentProfile.m_lifePotion, m_potions[0], "LifePotion");
        else
            SetItemVisibility(m_currentProfile.m_secondPotion, m_potions[1], "SecondPotion");

        SetItemVisibility(m_currentProfile.m_magicalRod, m_magicalRod, "MagicalRod");

        // in descending order so we dont use the standard sword over the magical or white sword
        bool swordLoaded = false;
        if (m_currentProfile.m_magicalSword == true)
        {
            swordLoaded = true;
            m_aContainer.GetComponent<UnityEngine.UI.Image>().sprite = m_swords[2];
            m_link.LoadItem("MagicalSword");
            m_aContainer.SetActive(true);
        }
        else if (m_currentProfile.m_whiteSword == true)
        {
            swordLoaded = true;
            m_aContainer.GetComponent<UnityEngine.UI.Image>().sprite = m_swords[1];
            m_link.LoadItem("WhiteSword");
            m_aContainer.SetActive(true);
        }
        else if (m_currentProfile.m_sword == true)
        {            swordLoaded = true;
            m_aContainer.GetComponent<UnityEngine.UI.Image>().sprite = m_swords[0];
            m_link.LoadItem("Sword");
            m_aContainer.SetActive(true);
        }
        if (swordLoaded)
        {
            if (loading == true)
            {
                DeactivateStarterCave();
            }
        }
    }

    public bool IsItemActive(string name)
    {
        switch (name)
        {
            case "Boomerang":
                return m_currentProfile.m_boomerang;
            case "MagicalBoomerang":
                return m_currentProfile.m_magicalBoomerang;
            case "LifePotion":
                return m_currentProfile.m_lifePotion;
            case "SecondPotion":
                return m_currentProfile.m_secondPotion;
            case "RedCandle":
                return m_currentProfile.m_redCandle;
            case "BlueCandle":
                return m_currentProfile.m_blueCandle;
            case "RedRing":
                return m_currentProfile.m_redRing;
            case "BlueRing":
                return m_currentProfile.m_blueRing;
            case "Sword":
                return m_currentProfile.m_sword;
            case "WhiteSword":
                return m_currentProfile.m_whiteSword;
            case "MagicalSword":
                return m_currentProfile.m_magicalSword;
            case "Bomb":
                return m_currentProfile.m_bomb;
            case "BookOfMagic":
                return m_currentProfile.m_bookOfMagic;
            case "StepLadder":
                return m_currentProfile.m_stepLadder;
            case "PowerBracelet":
                return m_currentProfile.m_powerBracelet;
            case "Food":
                return m_currentProfile.m_food;
            case "Recorder":
                return m_currentProfile.m_recorder;
            case "MagicalRod":
                return m_currentProfile.m_magicalRod;
            case "Bow":
                return m_currentProfile.m_bow;
            case "MagicalKey":
                return m_currentProfile.m_magicalKey;
            case "Raft":
                return m_currentProfile.m_raft;
            default:
                return false;
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        switch (name)
        {
            case "Boomerang":
                return m_boomerangs[0].GetComponent<Image>().sprite;
            case "MagicalBoomerang":
                return m_boomerangs[1].GetComponent<Image>().sprite;
            case "LifePotion":
                return m_potions[0].GetComponent<Image>().sprite;
            case "SecondPotion":
                return m_potions[1].GetComponent<Image>().sprite;
            case "RedCandle":
                return m_candles[0].GetComponent<Image>().sprite;
            case "BlueCandle":
                return m_candles[1].GetComponent<Image>().sprite;
            case "Bomb":
                return m_bomb.GetComponent<Image>().sprite;
            case "PowerBracelet":
                return m_powerBracelet.GetComponent<Image>().sprite;
            case "Food":
                return m_food.GetComponent<Image>().sprite;
            case "Recorder":
                return m_recorder.GetComponent<Image>().sprite;
            case "MagicalRod":
                return m_magicalRod.GetComponent<Image>().sprite;
            case "Bow":
                return m_bow.GetComponent<Image>().sprite;
            default:
                return null;
        }
    }
    public void NewItemAcquired(string name)
    {
        m_updateVisibility = true;
        m_saveGame = true;
        switch (name)
        {
            case "Boomerang":
                m_currentProfile.m_boomerang = true;
                Debug.Log("Boomerange is picked up = " + m_currentProfile.m_boomerang);
                return;
            case "MagicalBoomerang":
                m_currentProfile.m_magicalBoomerang = true;
                return;
            case "LifePotion":
                m_currentProfile.m_lifePotion = true;
                return;
            case "SecondPotion":
                m_currentProfile.m_secondPotion = true;
                return;
            case "RedCandle":
                m_currentProfile.m_redCandle = true;
                return;
            case "BlueCandle":
                m_currentProfile.m_blueCandle = true;
                return;
            case "RedRing":
                m_currentProfile.m_redRing = true;
                return;
            case "BlueRing":
                m_currentProfile.m_blueRing = true;
                return;
            case "Sword":
                m_currentProfile.m_sword = true;
                return;
            case "WhiteSword":
                m_currentProfile.m_whiteSword = true;
                return;
            case "MagicalSword":
                m_currentProfile.m_magicalSword = true;
                return;
            case "Bomb":
                m_currentProfile.m_bomb = true;
                return;
            case "BookOfMagic":
                m_currentProfile.m_bookOfMagic = true;
                return;
            case "StepLadder":
                m_currentProfile.m_stepLadder = true;
                return;
            case "PowerBracelet":
                m_currentProfile.m_powerBracelet = true;
                return;
            case "Food":
                m_currentProfile.m_food = true;
                return;
            case "Recorder":
                m_currentProfile.m_recorder = true;
                return;
            case "MagicalRod":
                m_currentProfile.m_magicalRod = true;
                return;
            case "Bow":
                m_currentProfile.m_bow = true;
                return;
            case "MagicalKey":
                m_currentProfile.m_magicalKey = true;
                return;
            case "Raft":
                m_currentProfile.m_raft = true;
                return;
            default:
                return;
        }
    }

    private void SetEquippedItemVisibility()
    {
        if (m_currentProfile.m_currentHeldItemName != string.Empty)
        {
            if (m_link != null)
                m_link.EquipItem(m_currentProfile.m_currentHeldItemName);
            Sprite sprite = GetSpriteByName(m_currentProfile.m_currentHeldItemName);
            if (sprite != null)
            {
                m_bContainer.GetComponent<Image>().sprite = sprite;
                m_inventoryItemContainer.GetComponent<Image>().sprite = sprite;
                m_inventoryItemContainer.SetActive(true);
                m_bContainer.SetActive(true);
            }
        }
    }

    public void ChangedEquippedItem(string name)
    {
        m_currentProfile.m_currentHeldItemName = name;
        m_saveGame = true;
        m_updateEquipVisibility = true;
    }

    public void SaveGame()
    {
        FileManager.SaveCurrentGame();
    }

    private void DeactivateStarterCave()
    {
        Debug.Log("Disabling Starter Cave");
        CaveTransition.DisableStarterSwordArea(true);
        for (int i = 0; i < m_starterSwordPickupArea.Length; ++i)
        {
            m_starterSwordPickupArea[i].SetActive(false);
        }
    }

    private void InitHeartPanel()
    {
        if (m_heartArray.Length > 0)
        {
            UpdateHearts(m_currentProfile.m_heartCount, false);
        }
        else Debug.LogError("No GameHud Heart Array Set!");
    }

    private void InitStackables(GameObject[] stackableArray, int stackableCount)
    {
        if (stackableArray.Length > 0)
        {
            SetArrayNumbers(stackableArray, stackableCount);
        }
        else Debug.LogError("Stackable Array NOT Set!", this);
    }

    public void UpdateHearts(float currentHeartCount, bool save = false)
    {
        if (currentHeartCount < 0) currentHeartCount = 0;
            m_currentProfile.m_heartCount = currentHeartCount;

  //      Debug.Log("Updating hearts");
        if (m_heartArray.Length > 0)
        {
            for (int i = 0; i < m_currentProfile.m_maxHeartCount; ++i)
            {
                if (i < currentHeartCount)
                {
                    m_heartArray[i].SetActive(true);
                    m_heartArray[i].GetComponent<UnityEngine.UI.Image>().sprite = m_fullHeartSprite;
                }
                else
                {
                    m_heartArray[i].SetActive(true);
                    m_heartArray[i].GetComponent<UnityEngine.UI.Image>().sprite = m_emptyHeartSprite;
                }
            }
        }
        if (Mathf.Floor(currentHeartCount) != currentHeartCount)
        {
            m_heartArray[(int)currentHeartCount].GetComponent<UnityEngine.UI.Image>().sprite = m_halfHeartSprite;
        }
        if (save)
            m_saveGame = true;
    }

    public void AddMaxHealth(float addValue)
    {
        ++m_currentProfile.m_maxHeartCount;
        ++m_currentProfile.m_heartCount;
        UpdateHearts(m_currentProfile.m_heartCount);
    }

    // use this to add this amount to the current rupee count (use negative if your losing rupees)
    public void AddRupeeAmount(int amount)
    {
        m_currentProfile.m_rupeeCount += amount;
        InitStackables(m_rupeeArray, m_currentProfile.m_rupeeCount);
        m_saveGame = true;
    }
    public void AddKeyAmount(int amount)
    {
        m_currentProfile.m_keyCount += amount;
        InitStackables(m_keyCountArray, m_currentProfile.m_keyCount);
        m_saveGame = true;
    }

    public void AddBombAmount(int amount)
    {
        m_currentProfile.m_bombCount += amount;
        InitStackables(m_bombCountArray, m_currentProfile.m_bombCount);
        m_saveGame = true;
    }

    public void AddTriforcePiece()
    {
        m_currentProfile.m_triforceCount++;
        SetTriforce();
        m_saveGame = true;
    }

    static public bool GetPause()
    {
        return m_pauseGame;
    }

    static public void SetPause(bool pause)
    {
    //    Debug.Log("pause = " + pause);
        m_pauseGame = pause;
    }

    public void DeathContinue()
    {
        m_currentProfile.m_heartCount = 3;
        UpdateHearts(m_currentProfile.m_heartCount);
        SaveGame();
        SceneManager.LoadScene("Gameplay");
    }
    public void DeathSave()
    {
        m_currentProfile.m_heartCount = 3;
        UpdateHearts(m_currentProfile.m_heartCount);
        SaveGame();
        SceneManager.LoadScene("MainMenu");
    }
    public void DeathRetry()
    {
        FileManager.ResetSave();
        SceneManager.LoadScene("MainMenu");
    }
}