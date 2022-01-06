using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingCircles : MonoBehaviour
{
    public float frequency = 0f;
    public Transform tip_Par;
    public Transform tip; //for moving the child

    private static float repetition = 6f;

    void Start()
    {
        //Random
        tip.transform.localPosition = new Vector3( Random.Range( -1.5f, 1.5f ), 0 , 0);
    }

    void FixedUpdate()
    {
        //Follow the parent's tip position
        transform.position = tip_Par.position;

        //Self rotation
        Vector3 angle = transform.localRotation.eulerAngles;
        angle.y += (repetition * frequency + 1f) * Time.deltaTime * 1f/repetition * 100f ;
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
        UnityEditor.Handles.DrawWireDisc(transform.position ,Vector3.up, Vector3.Distance(tip.localPosition , Vector3.zero));
    }
    #endif
}
