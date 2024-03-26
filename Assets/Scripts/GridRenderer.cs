using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public int gridSize = 10;
    public float gridSpacing = 1f;
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    public float axisLineWidth = 0.05f;

    void Start()
    {
        RenderGrid();
    }

    void RenderGrid()
    {
        /*GameObject gridObject = new GameObject("Grid");
        LineRenderer lineRenderer = gridObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;*/

        // Draw horizontal lines
        for (float y = -gridSize; y <= gridSize; y += gridSpacing)
        {
            GameObject gridObject = new GameObject("Grid"); // Reset the GameObject for re-use
            LineRenderer lineRenderer = gridObject.AddComponent<LineRenderer>(); // Reset the LineRenderer for re-use
            lineRenderer.material = lineMaterial;
            lineRenderer.startWidth = ( y == 0)?axisLineWidth:lineWidth;
            lineRenderer.endWidth = (y == 0) ? axisLineWidth : lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.sortingOrder = 1;
            lineRenderer.SetPosition(0, new Vector3(-gridSize, y, 0));
            lineRenderer.SetPosition(1, new Vector3(gridSize, y, 0));
        }

        // Draw vertical lines
        /*gridObject = new GameObject("Grid"); // Reset the GameObject for re-use
        lineRenderer = gridObject.AddComponent<LineRenderer>(); // Reset the LineRenderer for re-use
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;*/
        for (float x = -gridSize; x <= gridSize; x += gridSpacing)
        {
            GameObject gridObject = new GameObject("Grid"); // Reset the GameObject for re-use
            LineRenderer lineRenderer = gridObject.AddComponent<LineRenderer>(); // Reset the LineRenderer for re-use
            lineRenderer.material = lineMaterial;
            lineRenderer.startWidth = (x == 0) ? axisLineWidth : lineWidth;
            lineRenderer.endWidth = (x == 0) ? axisLineWidth : lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.sortingOrder = 1;
            lineRenderer.SetPosition(0, new Vector3(x, -gridSize, 0));
            lineRenderer.SetPosition(1, new Vector3(x, gridSize, 0));
        }
    }
}
