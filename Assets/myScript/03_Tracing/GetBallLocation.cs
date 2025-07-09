using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBallLocation : MonoBehaviour
{
    public List<GameObject> balls;
    public List<GameObject> curretShapes;

    public static int index { get; set; }

    public string GetShapeName()
    {
        return curretShapes[index].name.ToString();
    }

    public Vector3 GetBallPosition()
    {
        return balls[index].transform.position;
    }
}
