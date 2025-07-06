using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

// This class does not contain a ton of full proof error checking, please read through the class before attempting to use.
/// <summary>
/// Scrolls inventory downwards and then activates the UI options to select a new item from the inventory
/// </summary>
public class AccessInventory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private KeyCode m_inventoryAction = KeyCode.Escape;

    public float m_cameraPanSpeed = 20;
    bool m_inventoryOpen = false;
    bool m_cameraPanFinished = true;

    public GameObject m_hud;

    public MonoBehaviour m_inventoryCursorScript;

    public GameObject[] m_overworldInventory;
    public GameObject[] m_dungeonInventory;

    public float m_scrollDownAmount = 0;
    float m_startY;

    static bool m_disableInventory = false;

    static bool m_inDungeon = false;
    static bool m_updateBottomInventory = false;
    void Start()
    {
        m_startY = m_hud.GetComponent<Transform>().localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_updateBottomInventory)
        {
            if (m_inDungeon)
            {

                SetActiveArray(m_overworldInventory, false);
                SetActiveArray(m_dungeonInventory, true);
            }
            else
            {
                SetActiveArray(m_overworldInventory, true);
                SetActiveArray(m_dungeonInventory, false);
            }
            m_updateBottomInventory = false;
        }
        if (Input.GetKeyDown(m_inventoryAction) && m_cameraPanFinished && !m_disableInventory)
        {
            m_cameraPanFinished = false;
            m_inventoryOpen = !m_inventoryOpen;
        }
        if (!m_cameraPanFinished)
        {
            UpdateInventory();
        }
    }

    void UpdateInventory()
    {
        if (m_inventoryOpen) // if the inventory is trying to open
        {
            GameHudUpdater.SetPause(true);
            if (m_cameraPanFinished == false) // if the camera hasnt full panned
            {
                Vector3 yPos = m_hud.GetComponent<Transform>().localPosition; // gather current huds position
                yPos.y -= (m_cameraPanSpeed * Time.deltaTime); // add the camera pan speed to the y axis. NOTE: THIS DOES NOT RESTRICT THE UI TO THE DIMENSIONS OF THE SCREEN
                m_hud.GetComponent<Transform>().localPosition = yPos;
                // gather the position of the last object in the hud as we use that as our basis for alignment
                float pos = m_hud.GetComponent<Transform>().localPosition.y;
                if (m_scrollDownAmount >= pos)
                {
                    yPos = m_hud.GetComponent<Transform>().localPosition; // gather current huds position
                    yPos.y = m_scrollDownAmount;
                    m_hud.GetComponent<Transform>().localPosition = yPos;
                    m_cameraPanFinished = true;
                    m_inventoryCursorScript.enabled = true;
                }
            }
        }
        else
        {
            if (m_cameraPanFinished == false)
            {
                Vector3 yPos = m_hud.GetComponent<Transform>().localPosition;
                yPos.y += (m_cameraPanSpeed * Time.deltaTime);
                m_hud.GetComponent<Transform>().localPosition = yPos;
                // gather the position of the last object in the hud as we use that as our basis for alignment
                float pos = m_hud.GetComponent<Transform>().localPosition.y;
                if (m_startY <= pos)
                {
                    yPos = m_hud.GetComponent<Transform>().localPosition; // gather current huds position
                    yPos.y = m_startY; // add the camera pan speed to the y axis. NOTE: THIS DOES NOT RESTRICT THE UI TO THE DIMENSIONS OF THE SCREEN
                    m_hud.GetComponent<Transform>().localPosition = yPos;
                    m_cameraPanFinished = true;
                    m_inventoryCursorScript.enabled = false;
                    GameHudUpdater.SetPause(false);
                }
            }
        }
    }

    static public void DisableInventory(bool status)
    {
        m_disableInventory = status;
    }

    static public void SetInDungeon(bool status)
    {
        m_inDungeon = status;
        m_updateBottomInventory = true;
    }

    private void SetActiveArray(GameObject[] array, bool status)
    {
        foreach (GameObject item in array)
        {
            item.SetActive(status);
        }
    }
}