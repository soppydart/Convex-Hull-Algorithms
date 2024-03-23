using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;

public class JarvisMarch : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject bigPointPrefab;
    public Material hullMaterial;
    public Material maybeMaterial;
    public Material prospectMaterial;
    public float width = 0.1f;
    private bool isButtonClickAllowed = true;
    public TMP_Text statusText;
    public TMP_Text buttonText;
    private int clickNumber = 0;
    public Button nextButton;
    public Animator animator;
    public TargetGroupAllPoints targetGroupAllPoints;
    public TargetGroupHull targetGroupHull;


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
    public List<Vector2> pointList = new List<Vector2>();

    private List<int> hull = new List<int>();
    public List<Vector2> hullList = new List<Vector2>();    

    // LineRenderer component for drawing the convex hull
    private LineRenderer lineRenderer;

    // Index of the current edge being drawn
    private static int lastPointIndex = 0;
    private Point lastPoint = new Point(0, 0);
    private Point startPoint = new Point(0, 0);
    private int startPointIndex = 0;
    private bool arePointsEntered = false;

    public Transform firstPointTransform;
    public Transform lastPointTransform;

    private bool IsCounterClockwise(Point a, Point b, Point c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) < 0;
    }
    private void Update()
    {
        nextButton.interactable = isButtonClickAllowed;
        if (clickNumber > 0)
        {
            buttonText.text = "Next";
        }   
        if(lastPointIndex < points.Count)
        {
            firstPointTransform.position = new Vector3(points[lastPointIndex].x, points[lastPointIndex].y, 0);
        }
        if(nextIndex < points.Count)
        {
            lastPointTransform.position = new Vector3(points[nextIndex].x, points[nextIndex].y, 0);
        }
    }
    void Start()
    {
        //DrawLine(new Point(0, 0), new Point(1, 1), hullMaterial, width*10);

        firstPointTransform = new GameObject("NewObject").transform;
        lastPointTransform = new GameObject("NewObject").transform;

        animator.SetBool("focusAll", true);
        statusText.text = "";
        buttonText.text = "Start";

        // Create a LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Set LineRenderer properties
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = hullMaterial;
        lineRenderer.positionCount = 0;

        // Initialize points with fixed number (10) of points
        /*for (int i = 0; i < 10; i++)
        {
            float randomX = UnityEngine.Random.Range(-6f, 6f);
            float randomY = UnityEngine.Random.Range(-4f, 4f);
            // Round the coordinates to two decimal places
            randomX = Mathf.Round(randomX * 100f) / 100f;
            randomY = Mathf.Round(randomY * 100f) / 100f;
            points.Add(new Point(randomX, randomY));
            Instantiate(pointPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity);
        }*/

        //points.Add(new Point(-2, 2));
        //points.Add(new Point(2, 2));
        //points.Add(new Point(-2, -2));
        //points.Add(new Point(2, -2));
        //points.Add(new Point(0, 0));
        //Debug.Log("Added points");
        //arePointsEntered= true;

        //for (int i=0;i<points.Count; i++)
        //{
        //    Debug.Log("Point " + i + " is " + points[i].x + "," + points[i].y);
        //}

        StartCoroutine(WaitForPoints());

        /*int leftmostIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[leftmostIndex].x)
                leftmostIndex = i;
        }

        lastPoint = points[leftmostIndex];
        lastPointIndex = leftmostIndex;
        hull.Add(lastPointIndex);
        hullList.Add(new Vector2(lastPoint.x, lastPoint.y));
        targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
        isButtonClickAllowed = true;
        startPoint = lastPoint;
        startPointIndex = leftmostIndex;

        foreach (Point point in points)
        {
            pointList.Add(new Vector2(point.x, point.y));
        }
        targetGroupAllPoints.GetComponent<TargetGroupAllPoints>().UpdatePoints();*/
    }

    IEnumerator WaitForPoints()
    {
        // Keep yielding until the value changes
        while (!arePointsEntered)
        {
            yield return null; // Yield until the next frame
        }
    //statusText.text = str;

    int leftmostIndex = 0;
    for (int i = 1; i < points.Count; i++)
    {
        if (points[i].x < points[leftmostIndex].x)
            leftmostIndex = i;
    }

    lastPoint = points[leftmostIndex];
    lastPointIndex = leftmostIndex;
    hull.Add(lastPointIndex);
    hullList.Add(new Vector2(lastPoint.x, lastPoint.y));
    targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
    isButtonClickAllowed = true;
    startPoint = lastPoint;
    startPointIndex = leftmostIndex;

    foreach (Point point in points)
    {
        pointList.Add(new Vector2(point.x, point.y));
    }
    targetGroupAllPoints.GetComponent<TargetGroupAllPoints>().UpdatePoints();
}

    int nextIndex = lastPointIndex;
    public void OnButtonClick()
    {
        if(clickNumber%4==0)
        {
            if (!arePointsEntered)
            {
                Debug.Log("Enter points first!");
                return;
            }
            animator.SetBool("focusAll", false);
            animator.SetBool("focusLastPoint", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusFirstPoint", true);
            if (clickNumber==0)
                setStatus("We start with the leftmost point P which is sure to be on the hull.", points[lastPointIndex]);
            else
                setStatus("The last point Q added to the hull is our starting point P for the next iteration.", points[lastPointIndex]);
        }
        else if(clickNumber%4==1)
        {
            animator.SetBool("focusFirstPoint", false);
            animator.SetBool("focusLastPoint", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusAll", true);
            if(hull.Count == points.Count)
            {
                DrawLine(points[lastPointIndex], startPoint, hullMaterial, width);
                endAlgo();
                return;
            }
            int j = lastPointIndex;
            j = j = (j + 1) % points.Count;
            while (hull.Contains(j))
            {
                j = (j + 1) % points.Count;
            }
            int nextPointIndex = j;

            statusText.text = "We find the point Q such that there is no other point R in the set of points such that PQR is anticlockwise.";
            FindNextPoint(lastPointIndex, nextPointIndex);
        }
        else if (clickNumber % 4 == 2)
        {
            animator.SetBool("focusAll", false);
            animator.SetBool("focusFirstPoint", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusLastPoint", true);
            setStatus("This is the valid point Q.", points[nextIndex]);
        }
        else if(clickNumber%4==3)
        {
            animator.SetBool("focusAll", false);
            animator.SetBool("focusFirstPoint", false);
            animator.SetBool("focusLastPoint", false);
            animator.SetBool("focusHull", true);
            DrawLine(points[lastPointIndex], points[nextIndex], hullMaterial, width);
            lastPointIndex = nextIndex;
            setStatus("We add the edge PQ to our convex hull.", points[lastPointIndex]);
            if(lastPointIndex == startPointIndex)
            {
                endAlgo();
            }
        }
        clickNumber++;
    }

    public GameObject DrawLine(Point startPoint1, Point endPoint1, Material material, float width)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.material = material;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        Vector3[] positions = { new Vector3(startPoint1.x, startPoint1.y, 0), new Vector3(endPoint1.x, endPoint1.y, 0) };
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        return lineObject;
    }

    private void setStatus(string str, Point nextPoint)
    {
        Instantiate(bigPointPrefab, new Vector3(nextPoint.x, nextPoint.y, 0), Quaternion.identity);
        statusText.text = str;
    }

    private void FindNextPoint(int prevPointIndex, int nextPointIndex)
    {
        StartCoroutine(DrawEdgesWithDelay(prevPointIndex, nextPointIndex));
    }
    public float delayDuration = 0.2f;
    private IEnumerator DrawEdgesWithDelay(int prevPointIndex, int nextPointIndex)
    {
        isButtonClickAllowed = false;
        // Draw edge between A and B
        GameObject line1 = DrawLine(points[prevPointIndex], points[nextPointIndex], prospectMaterial, width);
        yield return new WaitForSeconds(delayDuration);

        for (int i = 0; i < points.Count; i++)
        {
            if (i == prevPointIndex || i == nextPointIndex)
                continue;

            // Draw edge between B and C
            GameObject line2 = DrawLine(points[nextPointIndex], points[i], maybeMaterial, width);
            yield return new WaitForSeconds(delayDuration);

            if (IsCounterClockwise(points[prevPointIndex], points[i], points[nextPointIndex]))
            {
                nextPointIndex = i;
                Destroy(line2); // Destroy the edge between B and C
                Destroy(line1);
                yield return StartCoroutine(DrawEdgesWithDelay(prevPointIndex, nextPointIndex));
                yield break; // Exit the loop if a suitable C is found
            }
            
            Destroy(line2); // Destroy the edge between B and C if it's not suitable
        }
        nextIndex = nextPointIndex;
        // If no suitable C is found
        if (!hull.Contains(nextIndex))
        {
            hull.Add(nextIndex);
            hullList.Add(new Vector2(points[nextIndex].x, points[nextIndex].y));
            targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
        }
        isButtonClickAllowed = true;

        Destroy(line1); // Destroy the edge between A and B
    }

    private void endAlgo()
    {
        //nextButton.interactable = false;
        statusText.text = "Convex Hull Complete";
        //buttonText.text = "Restart";
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        nextButton.gameObject.SetActive(false);
    }
    private float temp_x = -9999, temp_y = -9999;
    string str = "";
    public void InitializePoints(int value)
    {
        if (value == -9999)
        {
            arePointsEntered = true;
            return;
        }
        str += value.ToString() + " ";
        if (temp_x == -9999)
            temp_x = value;
        else
        {
            temp_y = value;
            points.Add(new Point(temp_x/100, temp_y/100));
            Debug.Log("(" + temp_x / 100 + "," + temp_y / 100 + ")");
            Instantiate(pointPrefab, new Vector3(temp_x/100, temp_y/100, 0), Quaternion.identity);
            temp_x = -9999;
            temp_y = -9999;
        }
    }
}
