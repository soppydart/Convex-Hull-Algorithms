using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupHull : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public JarvisMarch jarvisMarchScript;
    

    public void UpdateHull()
    {
        List<Transform> targetTransforms = new List<Transform>();
        List<Vector2> hullList = jarvisMarchScript.hullList;
        foreach (Vector2 point in hullList)
        {
            GameObject newTarget = new GameObject("Target");
            newTarget.transform.position = new Vector3(point.x, point.y, 0);
            targetTransforms.Add(newTarget.transform);
        }

        if (hullList != null && targetGroup != null)
        {
            List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
            foreach (Vector2 point in hullList)
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

        Debug.Log("Number of points in the hull = " + targetTransforms.Count);
    }
}
