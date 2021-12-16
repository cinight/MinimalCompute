using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTRotation : MonoBehaviour
{
    public bool isXaxis = true;
    public float frequency = 0f;
    public float amplitude = 0f;
    public float phaseAngle = 0f;

    void Start()
    {
        Vector3 pos = transform.localPosition;
        if(isXaxis)
        {
            pos.x += amplitude;
        }
        else
        {
            pos.z += amplitude;
        }
        transform.localPosition = pos;
    }

    void Update()
    {
        phaseAngle += frequency * Time.deltaTime * FTMain.timeSpeed;
        transform.localRotation = Quaternion.Euler(0,phaseAngle, 0);
    }
}
