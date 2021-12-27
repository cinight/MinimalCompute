using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTRotation : MonoBehaviour
{
    public bool isXaxis = true;
    public float frequency = 0f; //the time to finish 1 cycle
    public float amplitude = 0f;
    public float phaseAngle = 0f;

    void Update()
    {
        phaseAngle += Time.deltaTime * frequency * FTMain.timeSpeed;
        transform.localRotation = Quaternion.Euler(0,phaseAngle, 0);
    }
}
