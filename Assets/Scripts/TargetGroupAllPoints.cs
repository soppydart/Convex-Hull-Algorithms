using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupAllPoints : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public void UpdateAllPoints(List<Transform> targetTransforms)
    {
        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        foreach (Transform t in targetTransforms)
        {
            GameObject emptyObject = new GameObject("Point GameObject");
            emptyObject.transform.position = t.position;
            CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target();
            target.target = emptyObject.transform;
            target.weight = 1;
            target.radius = 2f;
            targets.Add(target);
        }
        targetGroup.m_Targets = targets.ToArray();
    }
}
