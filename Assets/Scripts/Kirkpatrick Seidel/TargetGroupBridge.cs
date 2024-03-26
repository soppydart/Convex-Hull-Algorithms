using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupBridge : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public void UpdateBridge(List<Transform> targetTransforms)
    {
        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        foreach (Transform t in targetTransforms)
        {
            GameObject emptyObject = new GameObject("Bridge GameObject");
            emptyObject.transform.position = t.position;
            CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target();
            target.target = emptyObject.transform;
            target.weight = 1;
            target.radius = 2;
            targets.Add(target);
            Debug.Log("Addign Bridge point at " + t.position.x + "," + t.position.y);
        }
        targetGroup.m_Targets = targets.ToArray();
    }
}
