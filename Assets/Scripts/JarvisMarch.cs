using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    public Button skipButton;
    public Animator animator;
    public TargetGroupAllPoints targetGroupAllPoints;
    public TargetGroupHull targetGroupHull;
    private bool isAddingPointsByClickingAllowed = true;


    // Define a point class representing 2D pointsa
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
    public List<Point> hullList = new List<Point>();    

    // LineRenderer component for drawing the convex hull
    private LineRenderer lineRenderer;

    // Index of the current edge being drawn
    private static int lastPointIndex = 0;
    private Point lastPoint = new Point(0, 0);
    private Point startPoint = new Point(0, 0);
    private int startPointIndex = 0;

    public Transform firstPointTransform;
    public Transform lastPointTransform;

    private bool IsCounterClockwise(Point a, Point b, Point c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) < 0;
    }
    private void Update()
    {
        nextButton.interactable = isButtonClickAllowed && points.Count>=2;
        skipButton.interactable = nextButton.interactable;
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
        if (isAddingPointsByClickingAllowed && !IsPointerOverUIObject(Input.mousePosition))
            AddPointsByClickingAllowed();
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

        //StartCoroutine(WaitForPoints());

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
        //while (!arePointsEntered)
        //{
            yield return null; // Yield until the next frame
        //}
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
    //hullList.Add(new Vector2(lastPoint.x, lastPoint.y));
    //targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
    isButtonClickAllowed = true;
    startPoint = lastPoint;
    startPointIndex = leftmostIndex;

    foreach (Point point in points)
    {
        pointList.Add(new Vector2(point.x, point.y));
    }
    //targetGroupAllPoints.GetComponent<TargetGroupAllPoints>().UpdatePoints();
}

    int nextIndex = lastPointIndex;
    bool hasStartBeenClicked = false;
    GameObject tempLine= null;
    public void OnButtonClick()
    {
        if(!hasStartBeenClicked)
        {
            hasStartBeenClicked = true;
            isAddingPointsByClickingAllowed = false;

            int leftmostIndex = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].x < points[leftmostIndex].x)
                    leftmostIndex = i;
            }

            lastPoint = points[leftmostIndex];
            lastPointIndex = leftmostIndex;
            hull.Add(lastPointIndex);
            hullList.Add(lastPoint);
            //targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
            UpdateHull(hullList);
            isButtonClickAllowed = true;
            startPoint = lastPoint;
            startPointIndex = leftmostIndex;

            foreach (Point point in points)
            {
                pointList.Add(new Vector2(point.x, point.y));
            }
            //targetGroupAllPoints.GetComponent<TargetGroupAllPoints>().UpdatePoints();
            UpdateAllPoints(points);

            if(points.Count == 2)
            {
                DrawLine(points[0], points[1], hullMaterial, width);
                foreach(Point p in points)
                {
                    if (!hullList.Contains(p))
                    {
                        hullList.Add(p);
                    }
                    Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
                }
                UpdateHull(hullList);
                endAlgo();
                return;
            }
        }
        if(clickNumber%3==0)
        {
            animator.SetBool("focusAll", false);
            animator.SetBool("focusLastPoint", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusFirstPoint", true);
            if (clickNumber==0)
                setStatus("We start with the leftmost point P which is sure to be on the hull.", points[lastPointIndex]);
            else
                setStatus("The last point Q added to the hull is our starting point P for the next iteration.", points[lastPointIndex]);
        }
        else if(clickNumber%3==1)
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
        else if (clickNumber % 3 == 2)
        {
            isButtonClickAllowed = false;
            animator.SetBool("focusAll", false);
            animator.SetBool("focusFirstPoint", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusLastPoint", true);
            setStatus("This is the valid point Q.", points[nextIndex]);
            StartCoroutine(ShowHull());
        }
        clickNumber++;
    }

    IEnumerator ShowHull()
    {
        yield return new WaitForSeconds(1.5f);

        animator.SetBool("focusAll", false);
        animator.SetBool("focusFirstPoint", false);
        animator.SetBool("focusLastPoint", false);
        animator.SetBool("focusHull", true);
        if (tempLine != null)
            Destroy(tempLine);
        kpsEdgeLines.Add(DrawLine(points[lastPointIndex], points[nextIndex], hullMaterial, width));
        lastPointIndex = nextIndex;
        setStatus("We add the edge PQ to our convex hull.", points[lastPointIndex]);
        if (lastPointIndex == startPointIndex)
        {
            endAlgo();
        }

        isButtonClickAllowed = true;
    }

    public GameObject DrawLine(Point startPoint1, Point endPoint1, Material material, float width)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.material = material;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = 10;

        Vector3[] positions = { new Vector3(startPoint1.x, startPoint1.y, 0), new Vector3(endPoint1.x, endPoint1.y, 0) };
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        return lineObject;
    }
    public GameObject DrawLine1(Point startPoint1, Point endPoint1, Material material, float width)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.material = material;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = 5;

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
    public float delayDuration = 0.1f;
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
            hullList.Add(points[nextIndex]);
            //targetGroupHull.GetComponent<TargetGroupHull>().UpdateHull();
            UpdateHull(hullList);
        }
        isButtonClickAllowed = true;
        tempLine = DrawLine1(points[lastPointIndex], points[nextPointIndex], prospectMaterial, width);

        Destroy(line1); // Destroy the edge between A and B
    }

    private void endAlgo()
    {
        if (wasKpsSkipped)
            UpdateHull(directlyComputedHullPoints);
        animator.SetBool("focusAll", false);
        animator.SetBool("focusFirstPoint", false);
        animator.SetBool("focusLastPoint", false);
        animator.SetBool("focusHull", true);

        //nextButton.interactable = false;
        statusText.text = "Convex Hull Complete";
        //buttonText.text = "Restart";
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    private float temp_x = -9999, temp_y = -9999;
    string str = "";
    public void InitializePoints(int value)
    {
        if (!isAddingPointsByClickingAllowed)
            return;

        str += value.ToString() + " ";
        if (temp_x == -9999)
            temp_x = value;
        else
        {
            temp_y = value;
            if (!points.Exists(p => p.x == (temp_x / 100) && p.y == (temp_y / 100)) && ((temp_x / 100) >= -25f) && ((temp_x / 100) <= 25f)
                && ((temp_y / 100) <= 10f && ((temp_y / 100) >= -10f)))
            {
                points.Add(new Point(temp_x / 100, temp_y / 100));
                Debug.Log("(" + temp_x / 100 + "," + temp_y / 100 + ")");
            }
            else
                Debug.Log("Invalid Point " + "(" + temp_x / 100 + "," + temp_y / 100 + ")");
            Instantiate(pointPrefab, new Vector3(temp_x/100, temp_y/100, 0), Quaternion.identity);
            temp_x = -9999;
            temp_y = -9999;
        }
    }
    bool wasKpsSkipped = false;
    List<GameObject>kpsEdgeLines = new List<GameObject>();
    public void OnSkip()
    {
        isAddingPointsByClickingAllowed = false;
        wasKpsSkipped = true;
        ComputeConvexHullDirectly();
        foreach (GameObject ob in kpsEdgeLines)
        {
            Destroy(ob);
        }
        endAlgo();
    }
    List<Point> directlyComputedHullPoints = new List<Point>();
    private void ComputeConvexHullDirectly()
    {
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
            directlyComputedHullPoints.Add(points[currentIndex]);

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

        DrawDirectlyComputedHull();
    }
    void DrawDirectlyComputedHull()
    {
        foreach(Point p in directlyComputedHullPoints)
        {
            Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
        }


        GameObject lineObject = new GameObject("Hull");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.material = hullMaterial;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = 10;
        lineRenderer.positionCount = directlyComputedHullPoints.Count;
        for (int i = 0; i < directlyComputedHullPoints.Count; i++)
        {
            Point p = directlyComputedHullPoints[i];
            lineRenderer.SetPosition(i, new Vector3(p.x, p.y, 0));
        }
        lineRenderer.loop = true;
    }
    public void Reload()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    void UpdateHull(List<Point> points)
    {
        List<Transform> transforms = new List<Transform>();
        List<GameObject> gameObjects = new List<GameObject>();
        foreach (Point p in points)
        {
            GameObject newGameObject = new GameObject("Temp Object");
            gameObjects.Add(newGameObject);
            Transform t = newGameObject.transform;
            t.position = new Vector3(p.x, p.y, 0);
            transforms.Add(t);
        }
        targetGroupHull.UpdateHull(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }
    void UpdateAllPoints(List<Point> points)
    {
        List<Transform> transforms = new List<Transform>();
        List<GameObject> gameObjects = new List<GameObject>();
        foreach (Point p in points)
        {
            GameObject newGameObject = new GameObject("Temp Object");
            gameObjects.Add(newGameObject);
            Transform t = newGameObject.transform;
            t.position = new Vector3(p.x, p.y, 0);
            transforms.Add(t);
        }
        targetGroupAllPoints.UpdateAllPoints(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }
    void AddPointsByClickingAllowed()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float roundedX = Mathf.Round(mousePosition.x * 100f) / 100f;
            float roundedY = Mathf.Round(mousePosition.y * 100f) / 100f;

            if (!points.Exists(p => p.x == roundedX && p.y == roundedY))
            {
                Debug.Log("Adding Point (" + roundedX + "," + roundedY);
                points.Add(new Point(roundedX, roundedY));
                Instantiate(pointPrefab, new Vector3(roundedX, roundedY, 0), Quaternion.identity);
            }
            else
            {
                Debug.Log("Redundant Point");
            }
        }
    }
    bool IsPointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touchPosition.x, touchPosition.y);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
}
