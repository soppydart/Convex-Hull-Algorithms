using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PointList", menuName = "ScriptableObjects/PointList", order = 1)]
public class PointListScriptableObject : ScriptableObject
{
    public List<Vector2> points = new List<Vector2>();
}