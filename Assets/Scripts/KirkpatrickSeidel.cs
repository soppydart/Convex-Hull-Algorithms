using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

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
    [SerializeField] float width = 0.1f;
    [SerializeField] TMP_Text statusText;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Button nextButton;
    private bool isButtonClickAllowed = false;
    private bool arePointsEntered = false;


    private List<Point> points = new List<Point>();
    private List<Point> invertedPoints = new List<Point>();
    private List<Edge> edges = new List<Edge>();

    void Start()
    {
        //points.Add(new Point(-3f, 0f));
        //points.Add(new Point(-1f, 1.5f));
        //points.Add(new Point(0.5f, 2f));
        //points.Add(new Point(3f, 0.5f));
        //points.Add(new Point(1.5f, 0f));
        //points.Add(new Point(4f, -1f));
        //points.Add(new Point(2f, -3f));

        buttonText.text = "Start";
        statusText.text = "";

        /*for (int i = 0; i < 5; i++)
        {
            float randomX = Random.Range(-6f, 6f);
            float randomY = Random.Range(-4f, 4f);
            randomX = Mathf.Round(randomX * 100f) / 100f;
            randomY = Mathf.Round(randomY * 100f) / 100f;
            points.Add(new Point(randomX, randomY));
            Instantiate(pointPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity);
        }*/

        /*foreach (Point point in points)
        {
            //Debug.Log("(" + point.x + "," + point.y + ")");
            GameObject newPoint = Instantiate(pointPrefab, new Vector3(point.x, point.y, 0), Quaternion.identity);
            //pointGameObjects.Add(newPoint);
        }
        arePointsEntered = true;

        invertedPoints = ReflectAboutOrigin(points);*/

        StartCoroutine(WaitForPoints());

        /*List<Point> maxMinPoints = findMaxMinX(points);

        Point pumin1 = new Point(maxMinPoints[0].x, maxMinPoints[0].y);
        Point pumax1 = new Point(maxMinPoints[1].x, maxMinPoints[1].y);

        List<Point> lhPoints = new List<Point>();

        foreach (Point it in points)
        {
            if ((it.x == pumin1.x && it.y != pumin1.y) || (it.x == pumax1.x && it.y != pumax1.y))
            {
            }
            else
                lhPoints.Add(it);
        }*/

        //ksStack.Push(new KsStackObject(invertedPoints, false));
        /*Debug.Log("lh points are");
        foreach(Point p in lhPoints)
        {
            Debug.Log(p.x + "," + p.y);
        }

        List<Point> minMaxPoints = findMinMaxX(points);

        Point pumin = new Point(minMaxPoints[0].x, minMaxPoints[0].y);
        Point pumax = new Point(minMaxPoints[1].x, minMaxPoints[1].y);

        List<Point> uhPoints = new List<Point>();

        foreach (Point it in points)
        {
            if ((it.x == pumin.x && it.y != pumin.y) || (it.x == pumax.x && it.y != pumax.y))
            {
            }
            else
                uhPoints.Add(it);
        }*/

        //ksStack.Push(new KsStackObject(points, true));
    }
    void Update()
    {
        if (buttonClicks > 0)
            buttonText.text = "Next";

    }
    IEnumerator WaitForPoints()
    {
        while (!arePointsEntered)
        {
            yield return null;
        }
        invertedPoints = ReflectAboutOrigin(points);
        ksStack.Push(new KsStackObject(invertedPoints, false));
        ksStack.Push(new KsStackObject(points, true));
        isButtonClickAllowed = true;
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

    void ComputeHull()
    {
        List<Point> minMaxPoints = new List<Point>();
            //findMinMaxX(points);


        Point pumin = new Point(minMaxPoints[0].x, minMaxPoints[0].y);
        Point pumax = new Point(minMaxPoints[1].x, minMaxPoints[1].y);

        //Debug.Log("point with lowest x coord = " + pumin.x + "," + pumin.y);
        //Debug.Log("point with highest x coord = " + pumax.x + "," + pumax.y);

        List<Point> uhPoints = new List<Point>();


        foreach (Point it in points)
        {
            if ((it.x == pumin.x && it.y != pumin.y) || (it.x == pumax.x && it.y != pumax.y))
            {
            }
            else
                uhPoints.Add(it);
        }

        //Debug.Log("Finding Upper Hull for the following points");
        foreach(Point point in uhPoints)
        {
            //Debug.Log(point.x + " " + point.y);
        }
        UpperHull(pumin, pumax, uhPoints);

        //Debug.Log("Printing Upper Hull Edges");
        foreach (Edge edge in edges)
        {
            //Debug.Log("(" + edge.point1.x + "," + edge.point1.y + ") - (" + edge.point2.x + "," + edge.point2.y + ")");
        }

        List<Point> maxMinPoints = findMaxMinX(points);

        Point pumin1 = new Point(maxMinPoints[0].x, maxMinPoints[0].y);
        Point pumax1 = new Point(maxMinPoints[1].x, maxMinPoints[1].y);
        //Debug.Log("point with lowest x coord = " + pumin1.x + "," + pumin1.y);
        //Debug.Log("point with highest x coord = " + pumax1.x + "," + pumax1.y);
        List<Point> lhPoints = new List<Point>();

        foreach (Point it in points)
        {
            if ((it.x == pumin1.x && it.y != pumin1.y) || (it.x == pumax1.x && it.y != pumax1.y))
            {
            }
            else
                lhPoints.Add(it);
        }

        LowerHull(pumin1, pumax1, lhPoints);

        if (pumin != pumin1)
        {
            edges.Add(new Edge(pumin, pumin1));
        }
        if (pumax != pumax1)
        {
            edges.Add(new Edge(pumax, pumax1));
        }

        Debug.Log("After lower hull");
        foreach (Edge ub in edges)
        {
            Debug.Log("(" + ub.point1.x + ',' + ub.point1.y + "),(" + ub.point2.x + "," + ub.point2.y + ")");
            DrawLine(ub.point1, ub.point2, hullMaterial, width);
        }
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

    List<Point> findMaxMinX(List<Point> points)
    {
        List<Point> res = new List<Point>();
        float puminx = points[0].x;
        float puminy = points[0].y;
        float pumaxx = points[0].x;
        float pumaxy = points[0].y;

        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < puminx || (points[i].x == puminx && points[i].y < puminy))
            {
                puminx = points[i].x;
                puminy = points[i].y;
            }
            if (points[i].x > pumaxx || (points[i].x == pumaxx && points[i].y < pumaxy))
            {
                pumaxx = points[i].x;
                pumaxy = points[i].y;
            }
        }
        res.Add(new Point(puminx, puminy));
        res.Add(new Point(pumaxx, pumaxy));
        return res;
    }

    void UpperHull(Point pumin, Point pumax, List<Point> points)
    {
        points.Sort(new XComparer());
        int n = points.Count;
        if (n == 2)
        {
            edges.Add(new Edge(points[0], points[1]));
            //Debug.Log("Addign edge "+ points[0].x+ "," + points[0].y + " - " + points[1].x + "," + points[1].y);
            return;
        }

        float medianX;
        if (n % 2 == 0)
            medianX = (points[n / 2].x + points[n / 2 - 1].x) / 2.0f;
        else
            medianX = points[n / 2].x;

        //Debug.Log("Median line : x = " + medianX);

        Edge ub = UpperBridge(points, medianX);

        //Debug.Log("Printing Upper Bridge Edge ");
        //Debug.Log("(" + ub.point1.x + ',' + ub.point1.y + "),(" + ub.point2.x + "," + ub.point2.y + ")");
        edges.Add(ub);

        Point pl = ub.point1;
        Point pr = ub.point2;
        Point pmin = pumin;
        Point pmax = pumax;

        List<Point> T_left = new List<Point>(), T_right = new List<Point>();
        T_left.Add(pl);
        T_left.Add(pmin);

        foreach (Point point in points)
        {
            if (pointPosition(pmin, pl, point) > 0)
            {
                T_left.Add(point);
            }
        }
        T_right.Add(pr);
        T_right.Add(pmax);
        foreach (Point point in points)
        {
            if (pointPosition(pmax, pr, point) < 0)
            {
                T_right.Add(point);
            }
        }
        //Debug.Log("Contents of T_left");
        foreach(Point p in T_left)
        {
            //Debug.Log(p.x + "," + p.y);
        }
        //Debug.Log("Contents of T_right");
        foreach (Point p in T_right)
        {
            //Debug.Log(p.x + "," + p.y);
        }

        if (pl.x != pmin.x && pl.y!= pmin.y)
        {
            UpperHull(pmin, pl, T_left);
        }
        if (pr.x != pmax.x && pr.y != pmax.y)
        {
            UpperHull(pr, pmax, T_right);
        }
    }

    void LowerHull(Point pumin, Point pumax, List<Point> points)
    {
        points.Sort(new XComparer());
        int n = points.Count;
        if (n == 2)
        {
            edges.Add(new Edge(points[0], points[1]));
            return;
        }

        float medianX;
        if (n % 2 == 0)
            medianX = (points[n / 2].x + points[n / 2 - 1].x) / 2.0f;
        else
            medianX = points[n / 2].x;

        //Debug.Log("Median line : x = " + medianX);

        Edge ub = LowerBridge(points, medianX);

        //Debug.Log("Printing Lower Bridge Edge ");
        //Debug.Log("(" + ub.point1.x + ',' + ub.point1.y + "),(" + ub.point2.x + "," + ub.point2.y + ")");
        edges.Add(ub);

        Point pl = ub.point1;
        Point pr = ub.point2;
        Point pmin = pumin;
        Point pmax = pumax;

        List<Point> T_left = new List<Point>(), T_right = new List<Point>();
        T_left.Add(pl);
        T_left.Add(pmin);

        foreach (Point point in points)
        {
            if (pointPosition(pl, pmin, point) > 0)
            {
                T_left.Add(point);
            }
        }
        T_right.Add(pr);
        T_right.Add(pmax);
        foreach (Point point in points)
        {
            if (pointPosition(pr, pmax, point) < 0)
            {
                T_right.Add(point);
            }
        }

        if (pl.x != pmin.x && pl.y != pmin.y)
        {
            LowerHull(pmin, pl, T_left);
        }
        if (pr.x != pmax.x && pr.y != pmax.y)
        {
            LowerHull(pr, pmax, T_right);
        }
    }

    Edge UpperBridge(List<Point> points, float a)
    {
        //Debug.Log("a = "+a);
        List<Point> candidates = new List<Point>();
        int n = points.Count;
        if (n == 2)
        {
            if (points[0].x < points[1].x)
                return new Edge(points[0], points[1]);
            else
                return new Edge(points[1], points[0]);
        }
        //else
            //Debug.Log("number of pints = " + n);
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
        //Debug.Log("Printing candidates1");
        foreach (Point p in candidates)
        {
            //Debug.Log(p.x + "," + p.y);
        }

        float medianK = findMedian(K);
        //Debug.Log("Median is " + medianK);
        List<Edge> smaller = new List<Edge>();
        List<Edge> equal = new List<Edge>();
        List<Edge> larger = new List<Edge>();

        for (i = 0; i < pairs.Count; i++)
        {
            Point p1 = pairs[i].point1;
            Point p2 = pairs[i].point2;

            float slopeVal = (float)(p1.y - p2.y) / (p1.x - p2.x);
            //Debug.Log("Slope is " + slopeVal);
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
            //Debug.Log("The point at this point is "+x + "," + y+" "+maximumIntercept);
            //Debug.Log("Maximum intercept is " + maximumIntercept);
            //Debug.Log("y - medianK * x " + (y - medianK * x));

            //if ((y - medianK * x) == maximumIntercept)
            //Debug.Log("They are equal");
            //else
            //Debug.Log((y - medianK * x) + " and " + maximumIntercept+" are not equal");

            if (Mathf.Approximately(y - medianK * x, maximumIntercept))
            {
                //Debug.Log("Change here " + maximumIntercept);
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

        //Debug.Log("pk is " + pk.x + "," + pk.y);
        //Debug.Log("pm is " + pm.x + "," + pm.y);
        //Debug.Log("a is " + a);


        if (pk.x <= a && pm.x > a)
        {
            return new Edge(pk, pm);
        }

        if (pm.x <= a)
        {
            foreach (Edge pair in larger)
            {
                candidates.Add(pair.point2);
                //Debug.Log("Added on line 504");
            }
            foreach (Edge pair in equal)
            {
                candidates.Add(pair.point2);
                //Debug.Log("ok"+pair.point2.x);
                //Debug.Log("ok1"+pair.point2.y);

            }
            foreach (Edge pair in smaller)
            {
                candidates.Add(pair.point2);
                //Debug.Log("Added on line 514");
                candidates.Add(pair.point1);
                //Debug.Log("Added on line 516");
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
        //Debug.Log("Printing candidates2");
        foreach(Point p in candidates)
        {
            //Debug.Log(p.x + "," + p.y);
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
        if (tempList.Count == 0)
        {
            //Debug.LogError("Cannot find median of empty list");
        }

        if(n % 2 == 1)
        {
            return tempList[n / 2];
        }
        else
        {
            return (tempList[n / 2] + tempList[n / 2 - 1]) / 2.0f;
        }

        /*int n = nums.Count;
        int mid = n / 2;
        float median = quickselect(nums, 0, n - 1, mid);

        // If the size is even, we need to find the average of the two middle numbers
        if (n % 2 == 0)
        {
            float lowerMedian = quickselect(nums, 0, mid - 1, mid - 1);
            median = (median + lowerMedian) / 2.0f;
        }*/

        //return median;
    }

    float quickselect(List<float> nums, int left, int right, int k)
    {
        if (left == right)
        {
            return nums[left];
        }

        int pivotIndex = left + (right - left) / 2;
        pivotIndex = partition(nums, left, right, pivotIndex);

        if (k == pivotIndex)
        {
            return nums[k];
        }
        else if (k < pivotIndex)
        {
            return quickselect(nums, left, pivotIndex - 1, k);
        }
        else
        {
            return quickselect(nums, pivotIndex + 1, right, k);
        }
    }

    int partition(List<float> nums, int left, int right, int pivotIndex)
    {
        float pivotValue = nums[pivotIndex];
        //swap(nums[pivotIndex], nums[right]); // Move pivot to end

        float temp = nums[pivotIndex];
        nums[pivotIndex] = nums[right];
        nums[right] = temp;

        int storeIndex = left;
        for (int i = left; i < right; i++)
        {
            if (nums[i] < pivotValue)
            {
                //std::swap(nums[i], nums[storeIndex]);
                float temp1 = nums[i]; nums[i] = nums[storeIndex]; nums[storeIndex] = temp1;
                storeIndex++;
            }
        }
        //std::swap(nums[storeIndex], nums[right]); // Move pivot to its final place
        temp = nums[storeIndex]; nums[storeIndex] = nums[right]; nums[right] = temp;
        return storeIndex;
    }

    float pointPosition(Point A, Point B, Point C)
    {
        // Calculate the cross product of vectors AB and AC
        float crossProd = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);

        // If cross product is positive or 0, C is to the left of AB, return 1
        // If cross product is negative or zero, C is to the right of AB, return 0
        return crossProd;
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
            points.Add(new Point(temp_x / 100, temp_y / 100));
            Debug.Log("(" + temp_x / 100 + "," + temp_y / 100 + ")");
            Instantiate(pointPrefab, new Vector3(temp_x / 100, temp_y / 100, 0), Quaternion.identity);
            temp_x = -9999;
            temp_y = -9999;
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
    public void OnButtonClick()
    {
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

            foreach(GameObject pointObject in pointGameObjects)
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
                setStatus("Only 2 points in subproblem, base case");
                if(stackObject.isUpperHull)
                    DrawLine(stackObject.points[0], stackObject.points[1], hullMaterial, width);
                else
                    DrawLine(ReflectAboutOrigin(stackObject.points[0]), ReflectAboutOrigin(stackObject.points[1]), hullMaterial, width);
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
            setStatus("At the start, we find Xmax and Xmin");
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

            /*bool isPointOnMedian = false;
            foreach(Point p in stackObject.points)
            {
                if(p.x == medianX)
                {
                    isPointOnMedian = true;
                    break;
                }
            }
            Point firstPointAfterMedianX = stackObject.points[0];
            foreach (Point p in stackObject.points)
            {
                if (p.x > medianX)
                {
                    firstPointAfterMedianX = p;
                    break;
                }
            }
            if(!isPointOnMedian)
                medianInX = medianX;
            else
                medianInX = (medianX+firstPointAfterMedianX.x)/2.0f;*/
            medianInX = medianX;

            if (!stackObject.isUpperHull)
               medianX *= -1;

            GameObject newLine = DrawLine1(new Point(medianX, 10), new Point(medianX, -10), lineMaterial, width);
            lineGameObjects.Add(newLine);

            setStatus("The median line is x = " + medianX);
        }
        else if (buttonClicks % 4 == 2)
        {
            stackObject = ksStack.Peek();
            stackObject.points.Sort(new XComparer());
            Edge ub;
            ub = UpperBridge(stackObject.points, medianInX);

            edges.Add(ub);
            if (stackObject.isUpperHull)
            {
                DrawLine(ub.point1, ub.point2, hullMaterial, width);
            }
            else 
            {
                Edge lb = ReflectAboutOrigin(ub);
                DrawLine(lb.point1, lb.point2, hullMaterial, width);
            }
            setStatus("Drawing the upper/lower bridge");
            bridge = ub;
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
                //connect(pj, pm, S_right, edges);
                ksStack.Push(new KsStackObject(S_right, stackObject.isUpperHull));
            }
            if (pi.x != pk.x || pi.y != pk.y)
            {
                //connect(pk, pi, S_left, edges);
                ksStack.Push(new KsStackObject(S_left, stackObject.isUpperHull));
            }
            string hull = stackObject.isUpperHull ? "Upper hull" : "Lower Hull";
            setStatus("Going into 2 subproblems for "+hull);
        }
        Debug.Log("The number of objects in the stack = " + ksStack.Count);
        buttonClicks++;
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
        foreach(Point p in points)
        {
            Instantiate(bigPointPrefab, new Vector3(p.x, p.y, 0), Quaternion.identity);
        }
        setStatus("Convex Hull Complete");
        nextButton.gameObject.SetActive(false);
    }
}
