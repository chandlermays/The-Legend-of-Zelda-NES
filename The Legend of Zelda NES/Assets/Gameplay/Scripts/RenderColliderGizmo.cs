using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderColliderGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.red;        // Change this to your desired color
    public float lineThickness = 0.01f;         // Adjust this to change the thickness

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;              // Set the gizmo color

        // Iterate through all Collider2D components attached to this GameObject
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                DrawBoxColliderGizmo(boxCollider);
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                DrawCircleColliderGizmo(circleCollider);
            }
            else if (collider is CapsuleCollider2D capsuleCollider)
            {
                DrawCapsuleColliderGizmo(capsuleCollider);
            }
            else if (collider is PolygonCollider2D polygonCollider)
            {
                DrawPolygonColliderGizmo(polygonCollider);
            }
        }
    }

    private void DrawBoxColliderGizmo(BoxCollider2D boxCollider)
    {
        Vector2 size = boxCollider.size;
        Vector2 center = boxCollider.offset + (Vector2)transform.position;

        // Draw thicker lines by drawing multiple lines close to each other
        for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
        {
            Gizmos.DrawLine(new Vector3(center.x - size.x / 2, center.y - size.y / 2 + i, 0), new Vector3(center.x + size.x / 2, center.y - size.y / 2 + i, 0));
            Gizmos.DrawLine(new Vector3(center.x - size.x / 2, center.y + size.y / 2 + i, 0), new Vector3(center.x + size.x / 2, center.y + size.y / 2 + i, 0));
            Gizmos.DrawLine(new Vector3(center.x - size.x / 2 + i, center.y - size.y / 2, 0), new Vector3(center.x - size.x / 2 + i, center.y + size.y / 2, 0));
            Gizmos.DrawLine(new Vector3(center.x + size.x / 2 + i, center.y - size.y / 2, 0), new Vector3(center.x + size.x / 2 + i, center.y + size.y / 2, 0));
        }
    }

    private void DrawCircleColliderGizmo(CircleCollider2D circleCollider)
    {
        Vector2 center = circleCollider.offset + (Vector2)transform.position;
        float radius = circleCollider.radius;

        // Draw thicker lines by drawing multiple circles close to each other
        for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
        {
            Gizmos.DrawWireSphere(center, radius + i);
        }
    }

    private void DrawCapsuleColliderGizmo(CapsuleCollider2D capsuleCollider)
    {
        Vector2 center = capsuleCollider.offset + (Vector2)transform.position;
        float height = capsuleCollider.size.y;
        float radius = capsuleCollider.size.x / 2;

        // Draw thicker lines by drawing multiple capsules close to each other
        for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
        {
            // Draw the top and bottom circles
            Gizmos.DrawWireSphere(center + Vector2.up * (height / 2 - radius), radius + i);
            Gizmos.DrawWireSphere(center - Vector2.up * (height / 2 - radius), radius + i);

            // Draw the lines connecting the circles
            Gizmos.DrawLine(new Vector3(center.x - radius - i, center.y + height / 2 - radius, 0), new Vector3(center.x - radius - i, center.y - height / 2 + radius, 0));
            Gizmos.DrawLine(new Vector3(center.x + radius + i, center.y + height / 2 - radius, 0), new Vector3(center.x + radius + i, center.y - height / 2 + radius, 0));
        }
    }

    private void DrawPolygonColliderGizmo(PolygonCollider2D polygonCollider)
    {
        Vector2[] points = polygonCollider.points;
        Vector2 offset = polygonCollider.offset + (Vector2)transform.position;

        // Draw thicker lines by drawing multiple lines close to each other
        for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
        {
            for (int j = 0; j < points.Length; j++)
            {
                Vector2 startPoint = points[j] + offset;
                Vector2 endPoint = points[(j + 1) % points.Length] + offset;
                Gizmos.DrawLine(new Vector3(startPoint.x + i, startPoint.y + i, 0), new Vector3(endPoint.x + i, endPoint.y + i, 0));
            }
        }
    }
}