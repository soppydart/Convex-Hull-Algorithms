using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupFirstPoint : MonoBehaviour
{
    public Transform followTarget; 
    public CinemachineVirtualCamera virtualCamera;
    public JarvisMarch jarvisMarch;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        followTarget = jarvisMarch.GetComponent<JarvisMarch>().firstPointTransform;
        virtualCamera.Follow = followTarget;
    }
}
