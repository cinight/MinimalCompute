using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public Transform target;
    private Vector3 pos;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        if (target != null)
        {
            pos = target.position;
        }
        else
        {
            pos = transform.position;
            pos.y = 0;
        }
        Gizmos.DrawLine(transform.position, pos);
    }
}
