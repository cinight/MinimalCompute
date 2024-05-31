using UnityEngine;

public class ResultSphereRotate : MonoBehaviour
{
    public float speed = 1.0f;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, speed);
    }
}
