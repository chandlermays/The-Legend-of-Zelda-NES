using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform m_target;

    // Bounds of the World
    [SerializeField] private Vector2 m_maxPosition;
    [SerializeField] private Vector2 m_minPosition;

    // Called once per frame after all Update methods
    void FixedUpdate()
    {
        // Calculate the desired camera position based on the target's position
        Vector3 targetPosition = new(m_target.position.x, m_target.position.y, transform.position.z);

        // Clamp the camera position within the specified min and max bounds
        float clampedX = Mathf.Clamp(targetPosition.x, m_minPosition.x, m_maxPosition.x);
        float clampedY = Mathf.Clamp(targetPosition.y, m_minPosition.y, m_maxPosition.y);

        // Apply the clamped position to the camera, keeping the Z position unchanged
        transform.position = new Vector3(clampedX, clampedY, targetPosition.z);
    }
}
