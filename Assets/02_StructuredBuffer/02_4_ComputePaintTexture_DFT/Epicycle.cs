using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Epicycle : MonoBehaviour
{
    public float frequency = 0f; //the time to finish 1 cycle
    public float amplitude = 0f;
    public float phaseAngle = 0f;
    public Transform tip_Par;
    public Transform tip; //for moving the child

    void FixedUpdate()
    {
        //Follow the parent's tip position
        transform.position = tip_Par.position;

        //Self rotation
        Vector3 angle = transform.localRotation.eulerAngles;
        angle.y += frequency * DFTMain.timeSpeed;
        transform.localRotation = Quaternion.Euler(0,angle.y, 0);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;

        if (tip_Par != null && tip != null)
        {
            from = tip_Par.position;
            to = tip.position;
        }

        Gizmos.DrawLine(from, to);
        UnityEditor.Handles.DrawWireDisc(transform.position ,Vector3.up, amplitude);
    }
    #endif
}
