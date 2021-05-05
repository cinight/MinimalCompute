using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Camera cam;
    private Vector3 lastPos;

    void Start()
    {
        cam = Camera.main;
    }

    private Vector3 GetPosition()
    {
        float camPlaneDistance = 13.99f;
        return cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camPlaneDistance));
    }

    public void OnMouseDown()
    {
        lastPos = GetPosition();
    }

    public void OnMouseDrag()
    {
        Vector3 delta = GetPosition()-lastPos;

        Vector3 pos = this.transform.position;
        pos.x += delta.x;
        pos.y += delta.y;
        this.transform.position = pos;

        lastPos = GetPosition();
    }
}

