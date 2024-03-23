using UnityEngine;
using System.Collections.Generic;

public class JarvisMarchConvexHull : MonoBehaviour
{
    public GameObject pointPrefab;
    public Material hullMaterial;
    public float width = 0.1f;
    // Define a point class representing 2D points
    public class Point
    {
        public float x;
        public float y;

        public Point(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
    }

    // List to store all points
    private List<Point> points = new List<Point>();

    // LineRenderer component for drawing the convex hull
    private LineRenderer lineRenderer;

    // Function to compute the convex hull using the Jarvis March algorithm
    private void ComputeConvexHull()
    {
        if (points.Count < 3)
            return;

        List<Point> hull = new List<Point>();

        // Find the leftmost point
        int leftmostIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[leftmostIndex].x)
                leftmostIndex = i;
        }

        int currentIndex = leftmostIndex;
        int nextIndex;
        do
        {
            hull.Add(points[currentIndex]);

            nextIndex = (currentIndex + 1) % points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                if (IsCounterClockwise(points[currentIndex], points[i], points[nextIndex]))
                {
                    nextIndex = i;
                }
            }

            currentIndex = nextIndex;

        } while (currentIndex != leftmostIndex);

        // Set LineRenderer positions
        lineRenderer.positionCount = hull.Count;
        for (int i = 0; i < hull.Count; i++)
        {
            Point p = hull[i];
            lineRenderer.SetPosition(i, new Vector3(p.x, p.y, 0));
        }
        lineRenderer.loop = true;
    }

    // Function to determine if three points are in counter-clockwise order
    private bool IsCounterClockwise(Point a, Point b, Point c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
    }

    void Start()
    {
        // Create a LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Set LineRenderer properties
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = hullMaterial;
    }

    void Update()
    {
        // Check for user input to add new points
        if (Input.GetMouseButtonDown(0))
        {
            // Get the mouse position in world coordinates
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Round the coordinates to two decimal places
            float roundedX = Mathf.Round(mousePosition.x * 100f) / 100f;
            float roundedY = Mathf.Round(mousePosition.y * 100f) / 100f;

            // Check if the rounded position is already in the list of points
            if (!points.Exists(p => p.x == roundedX && p.y == roundedY))
            {
                Debug.Log("Adding Point");

                // Add a new point to the list
                points.Add(new Point(roundedX, roundedY));

                // Instantiate a point GameObject at the clicked position
                Instantiate(pointPrefab, new Vector3(roundedX, roundedY, 0), Quaternion.identity);

                // Recompute the convex hull
                ComputeConvexHull();
            }
            else
            {
                Debug.Log("Redundant Point");
            }
        }
    }
}
