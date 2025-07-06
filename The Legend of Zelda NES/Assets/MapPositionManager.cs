using UnityEngine;
using static MapPositionUpdater;

public class MapPositionManager : MonoBehaviour
{
    public enum CompassDirection
    {
        kNorth,
        kSouth,
        kWest,
        kEast,
        kNone
    }
    static CompassDirection m_newDirection = CompassDirection.kNone;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject[] m_maps;

    // Update is called once per frame
    void Update()
    {
        if (m_newDirection != CompassDirection.kNone)
        {
            foreach (GameObject map in m_maps)
            {
                map.GetComponent<MapPositionUpdater>().UpdatePosition(m_newDirection);
            }
            m_newDirection = CompassDirection.kNone;
        }
        
    }
    static public void UpdateMapPositions(MapPositionManager.CompassDirection dir)
    {
        m_newDirection = dir;
    }
}
