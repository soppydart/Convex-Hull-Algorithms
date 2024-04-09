using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class KirkpatrickSeidelConvexHull : MonoBehaviour
{
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

    public class Edge
    {
        public Point point1;
        public Point point2;

        public Edge(Point point1, Point point2)
        {
            this.point1 = point1;
            this.point2 = point2;
        }
    }
    class XComparer : IComparer<Point>
    {
        public int Compare(Point a, Point b)
        {
            if (a.x < b.x)
                return -1;
            else if (a.x > b.x)
                return 1;
            else
                return 0;
        }
    }
    public class KsStackObject
    {
        public List<Point> points;
        public bool isUpperHull;
        public KsStackObject(List<Point> points, bool isUpperHull)
        {
            this.points = points;
            this.isUpperHull = isUpperHull;
        }
    }
    public class PairPointsIndices
    {
        public List<Point> points;
        public List<int> indices;
        public PairPointsIndices(List<Point> points, List<int> indices)
        {
            this.points = points;
            this.indices = indices;
        }
    }
    Stack<KsStackObject> ksStack = new Stack<KsStackObject>();

    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject bigPointPrefab;
    [SerializeField] Material hullMaterial;
    [SerializeField] Material lineMaterial;
    [SerializeField] Material tempMaterial;
    [SerializeField] Material tempMaterial1;
    [SerializeField] Material tempMaterial2;
    [SerializeField] float width = 0.1f;
    [SerializeField] TMP_Text statusText;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button nextButton;
    [SerializeField] Button skipButton;
    [SerializeField] Animator animator;
    private bool isButtonClickAllowed = true;
    private bool isAddingPointsByClickingAllowed = true;
    private bool hasStackBeenInitialized = false;
    private bool wasKpsSkipped = false;

    [SerializeField] TargetGroupSubproblem subproblemScript;
    [SerializeField] TargetGroupSubproblem duplicateSubproblemScript;
    [SerializeField] TargetGroupLeftSubproblem leftSubproblemScript;
    [SerializeField] TargetGroupRightSubproblem rightSubproblemScript;
    [SerializeField] TargetGroupBridge bridgeScript;
    [SerializeField] TargetGroupHull hullScript;

    private List<Point> points = new List<Point>();
    private List<Point> invertedPoints = new List<Point>();
    private List<Edge> edges = new List<Edge>();
    private List<Point> directlyComputedHullPoints = new List<Point>();
    private List<GameObject> kpsEdgeLines= new List<GameObject>();

    void Start()
    {
        buttonText.text = "Start";
        statusText.text = "";
    }
    void Update()
    {
        if (buttonClicks > 0)
            buttonText.text = "Next";

        nextButton.interactable = isButtonClickAllowed && points.Count>=2;
        skipButton.interactable = nextButton.interactable;

        if (isAddingPointsByClickingAllowed && !IsPointerOverUIObject(Input.mousePosition))
            AddPointsByClickingAllowed();

    }

    private GameObject DrawLine(Point startPoint1, Point endPoint1, Material material, float width)
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

    private GameObject DrawLine1(Point startPoint1, Point endPoint1, Material material, float width)
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

    PairPointsIndices findMinMaxX(List<Point> points)
    {
        List<Point> res = new List<Point>();
        int minIndex = 0, maxIndex = 0;
        float puminx = points[0].x;
        float puminy = points[0].y;
        float pumaxx = points[0].x;
        float pumaxy = points[0].y;

        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < puminx || points[i].x == puminx && points[i].y > puminy)
            {
                puminx = points[i].x;
                puminy = points[i].y;
                minIndex = i;
            }
            if (points[i].x > pumaxx || points[i].x == pumaxx && points[i].y > pumaxy)
            {
                pumaxx = points[i].x;
                pumaxy = points[i].y;
                maxIndex = i;
            }
        }
        res.Add(new Point( puminx, puminy ));
        res.Add(new Point(pumaxx, pumaxy));
        List<int> ind = new List<int> { minIndex , maxIndex };
        return new PairPointsIndices(res, ind);
    }

    List<GameObject>plottedLines = new List<GameObject>();
    IEnumerator UpperBridge1(List<Point> points, float a, bool isUpperHull)
    {
        isButtonClickAllowed = false;
        yield return new WaitForSeconds(0.2f);

        foreach(GameObject ob in plottedLines)
        {
            Destroy(ob);
        }

        List<Point> candidates = new List<Point>();
        int n = points.Count;
        if (n == 2)
        {
            if (points[0].x < points[1].x)
                bridge =  new Edge(points[0], points[1]);
            else
                bridge =  new Edge(points[1], points[0]);

            isButtonClickAllowed = true;

            if (isUpperHull)
            {
                kpsEdgeLines.Add(DrawLine(bridge.point1, bridge.point2, hullMaterial, width));
                edges.Add(bridge);
                UpdateBridge(bridge);
            }
            else
            {
                Edge lb = ReflectAboutOrigin(bridge);
                kpsEdgeLines.Add(DrawLine(lb.point1, lb.point2, hullMaterial, width));
                edges.Add(lb);
                UpdateBridge(lb);
            }
            if (isUpperHull)
                setStatus("Drawing the upper bridge");
            else
                setStatus("Drawing the lower bridge");

            animator.SetBool("focusLeft", false);
            animator.SetBool("focusRight", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusSubproblem", false);
            animator.SetBool("focusBridge", true);

            yield break;
        }

        List<Edge> pairs = new List<Edge>();

        List<GameObject>initialPairs = new List<GameObject>();

        List<float> K = new List<float>();
        int i = 0;
        if (n % 2 == 1)
        {
            candidates.Add(points[0]);
            i++;
        }
        for (; i <= n - 2; i += 2)
        {
            float x1 = points[i].x;
            float x2 = points[i + 1].x;

            if (x1 <= x2)
            {
                pairs.Add(new Edge(points[i], points[i + 1]));
            }
            else
            {
                pairs.Add(new Edge(points[i + 1], points[i]));
            }
        }
        foreach(Edge e in pairs)
        {
            GameObject tempLine;
            if (isUpperHull)
                tempLine = DrawLine1(e.point1, e.point2, tempMaterial, width / 2.0f);
            else
                tempLine = DrawLine1(ReflectAboutOrigin(e.point1), ReflectAboutOrigin(e.point2), tempMaterial, width / 2.0f);
            plottedLines.Add(tempLine);
        }

        yield return new WaitForSeconds(0.2f);

        foreach(GameObject ob in plottedLines)
        {
            Destroy(ob);
        }

        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            if (p1.x == p2.x)
            {
                if (p1.y > p2.y)
                {
                    candidates.Add(p1);
                }
                else
                {
                    candidates.Add(p2);
                }
            }
            else
            {
                K.Add((float)(p1.y - p2.y) / (p1.x - p2.x));
            }
        }

        List<GameObject>candidateGameObjects = new List<GameObject>();
        foreach(Edge e in pairs)
        {
            if(candidates.Contains(e.point1) && candidates.Contains(e.point2))
            {
                if(isUpperHull)
                    plottedLines.Add(DrawLine1(e.point1, e.point2, tempMaterial, width / 0.2f));
                else
                    plottedLines.Add(DrawLine1(ReflectAboutOrigin(e.point1), ReflectAboutOrigin(e.point2), tempMaterial, width / 0.2f));
            }
        }

        float medianK = findMedian(K);


        Point maxYinterceptPoint = points[0];
        float maxYintercept = int.MinValue;
        // draw all lines with a particular slope
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject ob in plottedLines)
        {
            Destroy(ob);
        }

        foreach(Point p in points)
        {
            float c = p.y - medianK* p.x;
            float startX = -25f;
            float startY = medianK * startX + c;

            float endX = 25f;
            float endY = medianK * endX + c;

            if(c > maxYintercept)
            {
                maxYintercept = c;
                maxYinterceptPoint = p;
            }

            if(isUpperHull)
                plottedLines.Add(DrawLine1(new Point(startX, startY), new Point(endX, endY), tempMaterial1, width / 2.0f));
            else
                plottedLines.Add(DrawLine1(new Point(-startX, -startY), new Point(-endX, -endY), tempMaterial1, width / 2.0f));
        }

        List<Edge> smaller = new List<Edge>();
        List<Edge> equal = new List<Edge>();
        List<Edge> larger = new List<Edge>();

        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            if (p1.x == p2.x && p1.y == p2.y)
            {
                continue;
            }

            float slopeVal = (float)(p1.y - p2.y) / (p1.x - p2.x);
            if (slopeVal < medianK)
            {
                smaller.Add(new Edge(p1, p2));
            }
            else if (Mathf.Approximately(slopeVal, medianK))
            {
                equal.Add(new Edge(p1, p2));
            }
            else
            {
                larger.Add(new Edge(p1, p2));
            }
        }

        float maximumIntercept = int.MinValue;

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (maximumIntercept < y - medianK * x)
            {
                maximumIntercept = y - medianK * x;
            }
        }
        // Drawing line with max y-Intercept

        yield return new WaitForSeconds(0.4f);
        foreach(GameObject ob in plottedLines)
        {
            Destroy(ob);
        }

        float c1 = maxYinterceptPoint.y - medianK * maxYinterceptPoint.x;
        float startX1 = -25f;
        float startY1 = medianK * startX1 + c1;

        float endX1 = 25f;
        float endY1 = medianK * endX1 + c1;

        if(isUpperHull)
            plottedLines.Add(DrawLine1(new Point(startX1, startY1), new Point(endX1, endY1), tempMaterial2, width / 2.0f));
        else
            plottedLines.Add(DrawLine1(new Point(-startX1, -startY1), new Point(-endX1, -endY1), tempMaterial2, width / 2.0f));


        Point pk = new Point(int.MaxValue, int.MaxValue);
        Point pm = new Point(int.MinValue, int.MinValue);

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (Mathf.Approximately(y - medianK * x, maximumIntercept))
            {
                if (x < pk.x)
                {
                    pk = new Point(x, y);
                }
                if (x > pm.x)
                {
                    pm = new Point(x, y);
                }
            }
        }

        yield return new WaitForSeconds(0.4f);
        if(isUpperHull)
            plottedLines.Add(DrawLine1(pk, pm, lineMaterial, width));
        else
            plottedLines.Add(DrawLine1(ReflectAboutOrigin(pk), ReflectAboutOrigin(pm), lineMaterial, width));

        if (pk.x <= a && pm.x > a)
        {
            bridge =  new Edge(pk, pm);
            isButtonClickAllowed = true;

            foreach (GameObject ob in plottedLines)
            {
                if(ob != null)
                Destroy(ob);
            }

            if (isUpperHull)
            {
                kpsEdgeLines.Add(DrawLine(bridge.point1, bridge.point2, hullMaterial, width));
                edges.Add(bridge);
                UpdateBridge(bridge);
            }
            else
            {
                Edge lb = ReflectAboutOrigin(bridge);
                kpsEdgeLines.Add(DrawLine(lb.point1, lb.point2, hullMaterial, width));
                edges.Add(lb);
                UpdateBridge(lb);
            }
            if (isUpperHull)
                setStatus("Drawing the upper bridge");
            else
                setStatus("Drawing the lower bridge");

            animator.SetBool("focusLeft", false);
            animator.SetBool("focusRight", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusSubproblem", false);
            animator.SetBool("focusBridge", true);

            yield break;
        }

        if (pm.x <= a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point2);

            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point2);
                candidates.Add(pair.point1);
            }
        }

        if (pk.x > a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point1);
            }
        }

        yield return new WaitForSeconds(0.2f);
        foreach(GameObject ob in plottedLines)
        {
            Destroy(ob);
        }

        List<GameObject> finalCandidateList = new List<GameObject>();
        foreach(Edge e in pairs)
        {
            if(candidates.Contains(e.point1) && candidates.Contains(e.point2))
            {
                if(isUpperHull)
                    plottedLines.Add(DrawLine1(e.point1, e.point2, tempMaterial, width / 2.0f));
                else
                    plottedLines.Add(DrawLine1(ReflectAboutOrigin(e.point1), ReflectAboutOrigin(e.point2), tempMaterial, width / 2.0f));
            }
        }

        StartCoroutine(UpperBridge1(candidates, a, isUpperHull));
    }

    Edge UpperBridge(List<Point> points, float a)
    {
        List<Point> candidates = new List<Point>();
        int n = points.Count;
        if (n == 2)
        {
            if (points[0].x < points[1].x)
                return new Edge(points[0], points[1]);
            else
                return new Edge(points[1], points[0]);
        }
        List<Edge> pairs = new List<Edge>();

        List<float> K = new List<float>();
        int i = 0;
        if (n % 2 == 1)
        {
            candidates.Add(points[0]);
            i++;
        }
        for (; i <= n - 2; i += 2)
        {
            float x1 = points[i].x;
            float x2 = points[i + 1].x;

            if (x1 <= x2)
            {
                pairs.Add(new Edge(points[i], points[i + 1]));
            }
            else
            {
                pairs.Add(new Edge(points[i + 1], points[i]));
            }
        }
        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            if (p1.x == p2.x)
            {
                if (p1.y > p2.y)
                {
                    candidates.Add(p1);
                }
                else
                {
                    candidates.Add(p2);
                }
            }
            else
            {
                K.Add((float)(p1.y - p2.y) / (p1.x - p2.x));
            }
        }

        float medianK = findMedian(K);
        List<Edge> smaller = new List<Edge>();
        List<Edge> equal = new List<Edge>();
        List<Edge> larger = new List<Edge>();

        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            float slopeVal = (float)(p1.y - p2.y) / (p1.x - p2.x);
            if (slopeVal < medianK)
            {
                smaller.Add(new Edge(p1, p2));
            }
            else if (Mathf.Approximately(slopeVal, medianK))
            {
                equal.Add(new Edge(p1, p2));
            }
            else
            {
                larger.Add(new Edge(p1, p2));
            }
        }

        float maximumIntercept = int.MinValue;

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (maximumIntercept < y - medianK * x)
            {
                maximumIntercept = y - medianK * x;
            }
        }

        Point pk = new Point(int.MaxValue, int.MaxValue);
        Point pm = new Point(int.MinValue, int.MinValue);

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (Mathf.Approximately(y - medianK * x, maximumIntercept))
            {
                if (x < pk.x)
                {
                    pk = new Point(x, y);
                }
                if (x > pm.x)
                {
                    pm = new Point(x, y);
                }
            }
        }


        if (pk.x <= a && pm.x > a)
        {
            return new Edge(pk, pm);
        }

        if (pm.x <= a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point2);
            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point2);
                candidates.Add(pair.point1);
            }
        }

        if (pk.x > a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point1);
            }
        }

        return UpperBridge(candidates, a);
    }

    Edge LowerBridge(List<Point> points, float a)
    {
        List<Point> candidates = new List<Point>();
        int n = points.Count;
        if (n == 2)
        {
            return new Edge(points[0], points[1]);
        }
        List<Edge> pairs = new List<Edge>();

        List<float> K = new List<float>();
        int i = 0;
        if (n % 2 == 1)
        {
            candidates.Add(points[0]);
            i++;
        }
        for (; i <= n - 2; i += 2)
        {
            float x1 = points[i].x;
            float x2 = points[i + 1].x;

            if (x1 <= x2)
            {
                pairs.Add(new Edge(points[i], points[i + 1]));
            }
            else
            {
                pairs.Add(new Edge(points[i + 1], points[i]));
            }
        }
        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            if (p1.x == p2.x)
            {
                if (p1.y > p2.y)
                {
                    candidates.Add(p2);
                }
                else
                {
                    candidates.Add(p1);
                }
            }
            else
            {
                K.Add((float)(p1.y - p2.y) / (p1.x - p2.x));
            }
        }

        float medianK = findMedian(K);
        List<Edge> smaller = new List<Edge>();
        List<Edge> equal = new List<Edge>();
        List<Edge> larger = new List<Edge>();

        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            float slopeVal = (float)(p1.y - p2.y) / (p1.x - p2.x);
            if (slopeVal < medianK)
            {
                smaller.Add(new Edge(p1, p2));
            }
            else if (Mathf.Approximately(slopeVal, medianK))
            {
                equal.Add(new Edge(p1, p2));
            }
            else
            {
                larger.Add(new Edge(p1, p2));
            }
        }

        float minimumIntercept = int.MaxValue;

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (minimumIntercept > y - medianK * x)
            {
                minimumIntercept = y - medianK * x;
            }
        }

        Point pk = new Point(int.MaxValue, int.MaxValue);
        Point pm = new Point(int.MinValue, int.MinValue);

        foreach (Point point in points)
        {
            float y = point.y;
            float x = point.x;

            if (Mathf.Approximately(y - medianK * x, minimumIntercept))
            {
                if (x < pk.x)
                {
                    pk = new Point(x, y);
                }
                if (x > pm.x)
                {
                    pm = new Point(x, y);
                }
            }
        }

        if (pk.x <= a && pm.x > a)
        {
            return new Edge(pk, pm);
        }

        if (pm.x <= a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point2);
            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point2);
            }
        }

        if (pk.x > a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point1);
            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point1);
                candidates.Add(pair.point2);
            }
        }

        return LowerBridge(candidates, a);
    }

    float findMedian(List<float> nums)
    {
        List<float> tempList = nums;
        int n = tempList.Count;
        if(n==1)
            return nums[0];
        tempList.Sort();

        if(n % 2 == 1)
        {
            return tempList[n / 2];
        }
        else
        {
            return (tempList[n / 2] + tempList[n / 2 - 1]) / 2.0f;
        }
    }

    float pointPosition(Point A, Point B, Point C)
    {
        float crossProd = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);
        return crossProd;
    }

    private float temp_x = int.MinValue, temp_y = int.MinValue;
    public void InitializePoints(int value)
    {
        if (!isAddingPointsByClickingAllowed)
            return;

        if (temp_x == int.MinValue)
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
                Debug.Log("Invalid Point "+ "(" + temp_x / 100 + "," + temp_y / 100 + ")");
            Instantiate(pointPrefab, new Vector3(temp_x / 100, temp_y / 100, 0), Quaternion.identity);
            temp_x = int.MinValue;
            temp_y = int.MinValue;
        }
    }
    private void setStatus(string str)
    {
        statusText.text = str;
    }
    
    private int buttonClicks = 0;
    private float medianInX = 0;
    Edge bridge;
    List<GameObject>pointGameObjects= new List<GameObject>();
    List<GameObject>lineGameObjects= new List<GameObject>();
    bool isLeftActive = true;
    bool isFirstSubproblem = true;
    public void OnButtonClick()
    {
        if (!hasStackBeenInitialized)
        {
            hasStackBeenInitialized = true;
            isAddingPointsByClickingAllowed = false;

            invertedPoints = ReflectAboutOrigin(points);
            ksStack.Push(new KsStackObject(invertedPoints, false));
            ksStack.Push(new KsStackObject(points, true));
            isButtonClickAllowed = true;

            if(points.Count == 2)
            {
                edges.Add(new Edge(points[0], points[1]));
                DrawLine(points[0], points[1], hullMaterial, width);
                endAlgo();
                return;
            }
        }
        if (!isButtonClickAllowed)
        {
            return;
        }
        if(ksStack.Count == 0)
        {
            foreach (GameObject pointObject in pointGameObjects)
            {
                Destroy(pointObject);
            }
            foreach (GameObject lineObject in lineGameObjects)
            {
                Destroy(lineObject);
            }

            endAlgo();
            return;
        }
        KsStackObject stackObject;
        if (buttonClicks % 4 == 0)
        {
            stackObject = ksStack.Peek();

            foreach (GameObject pointObject in pointGameObjects)
            {
                Destroy(pointObject);
            }
            foreach(Point p in stackObject.points)
            {
                GameObject newPoint;
                if (stackObject.isUpperHull)
                    newPoint = Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
                else
                    newPoint = Instantiate(bigPointPrefab, new Vector3(-p.x, -p.y, 0), Quaternion.identity);
                pointGameObjects.Add(newPoint);
            }
            foreach(GameObject lineObject in lineGameObjects)
            {
                Destroy(lineObject);
            }

            Debug.Log("In this subproblem there are " + stackObject.points.Count + " points. They are");
            foreach (Point p in stackObject.points)
            {
                Debug.Log(p.x + "," + p.y);
            }

            if (stackObject.points.Count == 1)
            {
                setStatus("Edge already drawn");
                return;
            }
            else if (stackObject.points.Count == 2)
            {
                setStatus("Only 2 points in subproblem [base case]");

                if (isLeftActive)
                {
                    if (stackObject.isUpperHull)
                        UpdateRightSubproblem(stackObject.points);
                    else
                        UpdateRightSubproblem(ReflectAboutOrigin(stackObject.points));

                    animator.SetBool("focusLeft", false);
                    animator.SetBool("focusHull", false);
                    animator.SetBool("focusBridge", false);
                    animator.SetBool("focusSubproblem", false);
                    animator.SetBool("focusDuplicate", false);
                    animator.SetBool("focusRight", true);
                }
                else
                {
                    if(stackObject.isUpperHull)
                        UpdateLeftSubproblem(stackObject.points);
                    else
                        UpdateLeftSubproblem(ReflectAboutOrigin(stackObject.points));

                    animator.SetBool("focusRight", false);
                    animator.SetBool("focusHull", false);
                    animator.SetBool("focusBridge", false);
                    animator.SetBool("focusSubproblem", false);
                    animator.SetBool("focusDuplicate", false);
                    animator.SetBool("focusLeft", true);
                }
                isLeftActive = !isLeftActive;

                if (stackObject.isUpperHull)
                {
                    kpsEdgeLines.Add(DrawLine(stackObject.points[0], stackObject.points[1], hullMaterial, width));
                    edges.Add(new Edge(stackObject.points[0], stackObject.points[1]));
                }
                else
                {
                    kpsEdgeLines.Add(DrawLine(ReflectAboutOrigin(stackObject.points[0]), ReflectAboutOrigin(stackObject.points[1]), hullMaterial, width));
                    edges.Add(new Edge(ReflectAboutOrigin(stackObject.points[0]), ReflectAboutOrigin(stackObject.points[1])));
                }
                ksStack.Pop();
                buttonClicks = 0;
                return;
            }

            stackObject.points.Sort(new XComparer());
            PairPointsIndices temp = findMinMaxX(stackObject.points);

            Point pmin = temp.points[0];
            Point pmax = temp.points[1];

            int minIndex = temp.indices[0];
            int maxIndex = temp.indices[1];

            if (minIndex == maxIndex)
            {
                return;
            }

            List<Point> uhPoints = new List<Point> { pmin, pmax };

            foreach (Point point in stackObject.points)
            {
                if (point.x > pmin.x && point.x < pmax.x)
                {
                    uhPoints.Add(point);
                }
            }
            uhPoints.Sort(new XComparer());

            ksStack.Pop();
            ksStack.Push(new KsStackObject(uhPoints, stackObject.isUpperHull));

            stackObject = ksStack.Peek();

            foreach (Point p in stackObject.points)
            {
                GameObject newPoint;
                if(stackObject.isUpperHull)
                    newPoint = Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
                else
                    newPoint = Instantiate(bigPointPrefab, new Vector3(-p.x, -p.y, 0), Quaternion.identity);
                pointGameObjects.Add(newPoint);
            }
            foreach (GameObject lineObject in lineGameObjects)
            {
                Destroy(lineObject);
            }
            GameObject baseLine;
            if (stackObject.isUpperHull)
                baseLine = DrawLine1(pmin, pmax, lineMaterial, width);
            else
                baseLine = DrawLine1(ReflectAboutOrigin(pmin), ReflectAboutOrigin(pmax), lineMaterial, width);
            if (isFirstSubproblem)
            {
                setStatus("At the start, we find Xmax and Xmin");
                isFirstSubproblem= false;
            }
            else
                setStatus("For the next sub-problem, we find Xmax and Xmin");

            animator.SetBool("focusDuplicate", false);
            if(stackObject.isUpperHull)
                UpdateSubproblem(stackObject.points);
            else
                UpdateSubproblem(ReflectAboutOrigin(stackObject.points));

            animator.SetBool("focusLeft", false);
            animator.SetBool("focusRight", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusBridge", false);
            animator.SetBool("focusDuplicate", false);
            animator.SetBool("focusSubproblem", true);

            lineGameObjects.Add(baseLine);
        }
        else if (buttonClicks % 4 == 1)
        {
            stackObject = ksStack.Peek();
            stackObject.points.Sort(new XComparer());
            
            Point pmin = stackObject.points[0];
            Point pmax = stackObject.points[stackObject.points.Count - 1];
            int n = stackObject.points.Count;
            float medianX;
            if (n % 2 == 0)
                medianX = (stackObject.points[n / 2].x + stackObject.points[n / 2 - 1].x) / 2.0f;
            else
                medianX = stackObject.points[n / 2].x;

            foreach(Point p in stackObject.points)
            {
                Debug.Log("Debug median " + p.x + "," + p.y);
            }

            medianInX = medianX;

            if (!stackObject.isUpperHull)
               medianX *= -1;

            GameObject newLine = DrawLine1(new Point(medianX, 13), new Point(medianX, -13), lineMaterial, width);
            lineGameObjects.Add(newLine);

            setStatus("The median line is x = " + medianX);
        }
        else if (buttonClicks % 4 == 2)
        {
            stackObject = ksStack.Peek();
            stackObject.points.Sort(new XComparer());

            if (stackObject.isUpperHull)
                setStatus("Computing the Upper Bridge");
            else
                setStatus("Computing the Lower Bridge");

            StartCoroutine(UpperBridge1(stackObject.points, medianInX, stackObject.isUpperHull));

            buttonClicks++;
            return;
        }
        else if(buttonClicks % 4 == 3)
        {
            stackObject = ksStack.Peek();
            stackObject.points.Sort(new XComparer());
            Point pi = bridge.point1;
            Point pj = bridge.point2;

            List<Point> S_left = new List<Point> { pi }, S_right = new List<Point> { pj };

            foreach (Point point in stackObject.points)
            {
                if (point.x < pi.x)
                {
                    S_left.Add(point);
                }
                if (point.x > pj.x)
                {
                    S_right.Add(point);
                }
            }

            ksStack.Pop();
            Point pk = stackObject.points[0], pm = stackObject.points[stackObject.points.Count - 1];
            if (pj.x != pm.x || pj.y != pm.y)
            {
                ksStack.Push(new KsStackObject(S_right, stackObject.isUpperHull));
            }
            if (pi.x != pk.x || pi.y != pk.y)
            {
                ksStack.Push(new KsStackObject(S_left, stackObject.isUpperHull));
            }
            string hull = stackObject.isUpperHull ? "Upper hull" : "Lower Hull";
            setStatus("Going into 2 subproblems for "+hull);

            StartCoroutine(DivideIntoSubproblems(S_left, S_right, stackObject.points, stackObject.isUpperHull));
            isButtonClickAllowed = false;
        }
        Debug.Log("The number of objects in the stack = " + ksStack.Count);
        buttonClicks++;
    }

    List<GameObject> tempPointGameObjects = new List<GameObject>();
    IEnumerator DivideIntoSubproblems(List<Point>S_left, List<Point>S_right, List<Point>allPoints, bool isupperHull)
    {
        if (isupperHull)
        {
            UpdateLeftSubproblem(S_left);
            UpdateRightSubproblem(S_right);
        }
        else
        {
            UpdateLeftSubproblem(ReflectAboutOrigin(S_left));
            UpdateRightSubproblem(ReflectAboutOrigin(S_right));
        }

        foreach (GameObject ob in pointGameObjects)
        {
            if(ob != null)
                ob.GameObject().SetActive(false);
        }

        if (S_left.Count > 0)
        {
            yield return new WaitForSeconds(1f);

            foreach (Point p in S_left)
            {
                if (isupperHull)
                    tempPointGameObjects.Add(Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity));
                else
                    tempPointGameObjects.Add(Instantiate(bigPointPrefab, new Vector3(-p.x, -p.y, 0), Quaternion.identity));
            }
            if (isupperHull)
            {
                if (S_left.Count == 1)
                    setStatus("This is the left sub-problem, which has only 1 point [trivial case]");
                else
                    setStatus("This is the left sub-problem");
            }
            else
            {
                if (S_left.Count == 1)
                    setStatus("This is the right sub-problem, which has only 1 point [trivial case]");
                else
                    setStatus("This is the right sub-problem");
            }
            animator.SetBool("focusBridge", false);
            animator.SetBool("focusRight", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusSubproblem", false);
            animator.SetBool("focusLeft", true);
        }

        if(S_right.Count > 0)
        {
            yield return new WaitForSeconds(1f);

            foreach (GameObject ob in tempPointGameObjects)
            {
                Destroy(ob);
            }
            foreach (Point p in S_right)
            {
                if(isupperHull)
                    tempPointGameObjects.Add(Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity));
                else
                    tempPointGameObjects.Add(Instantiate(bigPointPrefab, new Vector3(-p.x, -p.y, 0), Quaternion.identity));
            }
            if (isupperHull)
            {
                if (S_right.Count == 1)
                    setStatus("This is the right sub-problem, which has only 1 point [trivial case]");
                else
                    setStatus("This is the right sub-problem");
            }
            else
            {
                if (S_right.Count == 1)
                    setStatus("This is the left sub-problem, which has only 1 point [trivial case]");
                else
                    setStatus("This is the left sub-problem");
            }
            animator.SetBool("focusBridge", false);
            animator.SetBool("focusLeft", false);
            animator.SetBool("focusHull", false);
            animator.SetBool("focusSubproblem", false);
            animator.SetBool("focusRight", true);
        }

        yield return new WaitForSeconds(1f);
        foreach (GameObject ob in tempPointGameObjects)
        {
            Destroy(ob);
        }
        foreach (GameObject ob in pointGameObjects)
        {
            if(ob != null)
                ob.GameObject().SetActive(true);
        }

        if(isupperHull)
            UpdateDuplicateSubproblem(allPoints);
        else
            UpdateDuplicateSubproblem(ReflectAboutOrigin(allPoints));
        setStatus("");
        animator.SetBool("focusBridge", false);
        animator.SetBool("focusHull", false);
        animator.SetBool("focusRight", false);
        animator.SetBool("focusLeft", false);
        animator.SetBool("focusSubproblem", false);
        animator.SetBool("focusDuplicate", true);

        isButtonClickAllowed = true;
        yield break;
    }

    List<Point> ReflectAboutOrigin(List<Point> points)
    {
        List<Point> ans = new List<Point>();
        foreach(Point p in points)
        {
            Point newP = new Point(-1*p.x, -1*p.y);
            ans.Add(newP);
        }
        return ans;
    }
    Edge ReflectAboutOrigin(Edge edge)
    {
        Point newPoint1 = new Point(-1 * edge.point1.x, -1 * edge.point1.y);
        Point newPoint2 = new Point(-1 * edge.point2.x, -1 * edge.point2.y);

        return new Edge(newPoint1, newPoint2);
    }
    Point ReflectAboutOrigin(Point p)
    {
        return new Point(-1 * p.x, -1 * p.y);
    }
    void endAlgo()
    {
        List<Point> hullPoints = new List<Point>();
        if (!wasKpsSkipped)
        {
            PairPointsIndices pupper = findMinMaxX(points);
            PairPointsIndices plower = findMinMaxX(invertedPoints);

            Point pmin = pupper.points[0];
            Point pmax = pupper.points[1];

            Point pmin1 = plower.points[0];
            Point pmax1 = plower.points[1];

            Point pmin2 = ReflectAboutOrigin(pmax1);
            Point pmax2 = ReflectAboutOrigin(pmin1);

            pmin1 = pmin2;
            pmax1 = pmax2;

            if (pmin.x != pmin1.x || pmin.y != pmin1.y)
            {
                edges.Add(new Edge(pmin, pmin1));
                kpsEdgeLines.Add(DrawLine(pmin, pmin1, hullMaterial, width));
            }
            if (pmax.x != pmax1.x || pmax.y != pmax1.y)
            {
                edges.Add(new Edge(pmax, pmax1));
                kpsEdgeLines.Add(DrawLine(pmax, pmax1, hullMaterial, width));
            }

            foreach (Edge e in edges)
            {
                hullPoints.Add(e.point1);
                hullPoints.Add(e.point2);
            }
        }
        else
        {
            hullPoints = directlyComputedHullPoints;
        }

        foreach(GameObject ob in tempPointGameObjects)
        {
            if(ob != null)
            {
                Destroy(ob);
            }
        }
        foreach (GameObject ob in pointGameObjects)
        {
            if (ob != null)
            {
                Destroy(ob);
            }
        }

        foreach (Point p in hullPoints)
        {
            Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
        }
        
        UpdateHull(hullPoints);
        animator.SetBool("focusLeft", false);
        animator.SetBool("focusRight", false);
        animator.SetBool("focusBridge", false);
        animator.SetBool("focusHull", true);

        setStatus("Convex Hull Complete");
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
    }
    void UpdateSubproblem(List<Point> points)
    {
        List<Transform>transforms = new List<Transform>();
        List<GameObject>gameObjects = new List<GameObject>();
        foreach(Point p in points)
        {
            GameObject newGameObject = new GameObject("Temp Object");
            gameObjects.Add(newGameObject);
            Transform t = newGameObject.transform;
            t.position = new Vector3(p.x, p.y, 0);
            transforms.Add(t);
        }
        subproblemScript.UpdateSubproblem(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }
    void UpdateDuplicateSubproblem(List<Point> points)
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
        duplicateSubproblemScript.UpdateSubproblem(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }
    void UpdateBridge(Edge bridge)
    {
        List<Transform> transforms = new List<Transform>();

        Point p1 = bridge.point1;
        Point p2 = bridge.point2;

        GameObject newGameObject = new GameObject("Temp Object");
        Transform t = newGameObject.transform;
        t.position = new Vector3(p1.x, p1.y, 0);
        transforms.Add(t);
        Destroy(newGameObject);

        newGameObject = new GameObject("Temp Object");
        t = newGameObject.transform;
        t.position = new Vector3(p2.x, p2.y, 0);
        transforms.Add(t);
        Destroy(newGameObject);

        bridgeScript.UpdateBridge(transforms);
    }
    void UpdateLeftSubproblem(List<Point> points)
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
        leftSubproblemScript.UpdateLeftSubproblem(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }
    void UpdateRightSubproblem(List<Point> points)
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
        rightSubproblemScript.UpdateRightSubproblem(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
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
        hullScript.UpdateHull(transforms);
        foreach (GameObject ob in gameObjects)
        {
            Destroy(ob);
        }
    }

    public void Reload()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
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
                Debug.Log("Adding Point ("+ roundedX + ","+ roundedY);
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
    public void OnSkip()
    {
        isAddingPointsByClickingAllowed = false;
        wasKpsSkipped = true;
        ComputeConvexHullDirectly();
        foreach(GameObject ob in kpsEdgeLines)
        {
            Destroy(ob);
        }
        foreach (GameObject lineObject in lineGameObjects)
        {
            Destroy(lineObject);
        }
        endAlgo();
    }
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
                if (pointPosition(points[currentIndex], points[i], points[nextIndex]) > 0)
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
}
