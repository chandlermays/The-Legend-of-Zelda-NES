using UnityEngine;

public class MapPositionUpdater : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject m_positionObject;
    public float m_verticalPosAddition = 0;
    public float m_horizontalPosAddition = 0;
    private MapPositionManager.CompassDirection m_updatePosition = MapPositionManager.CompassDirection.kNone;

    private Vector2 m_startPosition;

    // Update is called once per frame
    void Update()
    {
        if (m_updatePosition != MapPositionManager.CompassDirection.kNone)
        {
            PositionChanged(m_updatePosition);
            m_updatePosition = MapPositionManager.CompassDirection.kNone;
        }
    }

    private void PositionChanged(MapPositionManager.CompassDirection dir)
    {
        var pos = m_positionObject.transform.position;
        switch (dir)
        {
            case MapPositionManager.CompassDirection.kNorth:
                pos.y += m_verticalPosAddition;
                break;
            case MapPositionManager.CompassDirection.kSouth:
                pos.y -= m_verticalPosAddition;
                break;
            case MapPositionManager.CompassDirection.kWest:
                pos.x -= m_horizontalPosAddition;
                break;
            case MapPositionManager.CompassDirection.kEast:
                pos.x += m_horizontalPosAddition;
                break;
        }
        m_positionObject.transform.position = pos;
    }

    public void UpdatePosition(MapPositionManager.CompassDirection dir)
    {
        if (isActiveAndEnabled)
        {
            m_updatePosition = dir;
        }
    }
}
