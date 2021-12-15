using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTDrawPosition : MonoBehaviour
{
    public Transform Xaxis;
    public Transform Yaxis;
    public Transform sphere;

    void Update()
    {
        sphere.position = new Vector3(Xaxis.position.x, 0 ,Yaxis.position.z);
    }
}
