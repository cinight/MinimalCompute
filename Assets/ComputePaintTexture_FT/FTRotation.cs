using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTRotation : MonoBehaviour
{
    private static float timeSpeed = 5.0f;
    public float frequency = 0f;
    public float amplitude = 0f;
    public float phaseAngle = 0f;

    void Start()
    {
        //temp
        if(frequency>0f) amplitude = (1f/frequency)*2.0f;


        Vector3 pos = transform.localPosition;
        pos.x += amplitude;
        transform.localPosition = pos;
    }

    void Update()
    {
        phaseAngle += frequency*Time.deltaTime*timeSpeed;
        transform.localRotation = Quaternion.Euler(0,phaseAngle, 0);
    }
}
