using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class TargetGroupAllPoints : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public JarvisMarch jarvisMarchScript;
    public List<Transform> targetTransforms = new List<Transform>();

    public void UpdatePoints()
    {
        List<Vector2> pointList = jarvisMarchScript.pointList;
        foreach (Vector2 point in pointList)
        {
            GameObject newTarget = new GameObject("Target");
            newTarget.transform.position = new Vector3(point.x, point.y, 0);
            targetTransforms.Add(newTarget.transform);
        }

        if (pointList != null && targetGroup != null)
        {
            List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
            foreach (Vector2 point in pointList)
            {
                GameObject emptyObject = new GameObject(); // You might want to instantiate your targets differently
                emptyObject.transform.position = new Vector3(point.x, point.y, 0f);
                CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target();
                target.target = emptyObject.transform;
                target.weight = 1; // You may adjust the weight as needed
                targets.Add(target);
            }
            targetGroup.m_Targets = targets.ToArray();
        }
    }
}
