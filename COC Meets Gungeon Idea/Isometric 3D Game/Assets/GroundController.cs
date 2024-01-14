using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    public float cellSize = 1.0f;

    public bool showgrid = false;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if(showgrid)
        {
            DrawIsometricGrid();

        }
#endif
    }

    private void DrawIsometricGrid()
    {
        MeshRenderer planeRenderer = GetComponent<MeshRenderer>();

        if (planeRenderer == null)
        {
            Debug.LogError("No MeshRenderer component found on the GameObject.");
            return;
        }

        // Get the mesh renderer bounds instead of the collider bounds
        Bounds bounds = planeRenderer.bounds;

        // Center of the mesh bounds
        Vector3 planeCenter = bounds.center;

        int gridSizeX = Mathf.RoundToInt(bounds.size.x / cellSize);
        int gridSizeY = Mathf.RoundToInt(bounds.size.z / cellSize);

        for (float x = bounds.min.x; x <= bounds.max.x; x += cellSize)
        {
            for (float y = bounds.min.z; y <= bounds.max.z; y += cellSize)
            {
                // Calculate the position of the grid cell
                float xPos = x + .5f;
                //float yPos = planeCenter.y + (x + y) % 2 * cellSize / 2;
                float zPos = y + .5f;

                // Draw a wire cube at the cell position using Gizmos
                Gizmos.DrawWireCube(new Vector3(xPos, 0, zPos), new Vector3(cellSize, 0.1f, cellSize));
                Gizmos.color = Color.blue;
            }
        }
    }
}
