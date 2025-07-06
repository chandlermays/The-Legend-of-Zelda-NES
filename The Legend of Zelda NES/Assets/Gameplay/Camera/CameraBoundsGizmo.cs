using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraBoundsDrawer : MonoBehaviour
{
    public Color boxColor = Color.green;            // Choose any color you like for the bounding box
    public float lineThickness = 0.01f;             // Adjust this to change the thickness

    private void OnDrawGizmos()
    {
        Camera cam = GetComponent<Camera>();
        Gizmos.color = boxColor;
        Gizmos.matrix = cam.transform.localToWorldMatrix;

        if (cam.orthographic)
        {
            float spread = cam.orthographicSize * 2;
            float aspect = cam.aspect;
            Vector3 size = new(spread * aspect, spread, cam.farClipPlane - cam.nearClipPlane);
            Vector3 center = (cam.nearClipPlane + cam.farClipPlane) * 0.5f * Vector3.forward;

            // Draw thicker lines by drawing multiple lines close to each other
            for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
            {
                Gizmos.DrawWireCube(center + new Vector3(i, i, 0), size);
            }
        }
        else
        {
            // Draw thicker lines by drawing multiple frustums close to each other
            for (float i = -lineThickness; i <= lineThickness; i += lineThickness / 2)
            {
                Gizmos.DrawFrustum(new Vector3(i, i, 0), cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
            }
        }
    }
}