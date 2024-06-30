using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMovement : MonoBehaviour
{
    public float moveTowardsCenter = 8f;
    private float originalSize;
    private float speed;
    private Vector3 originalPosition;
    private Vector3 directionToCenter;
    
    void Start()
    {
        speed = Random.Range(0.5f, 3f);
        originalSize = transform.localScale.x;
        originalPosition = transform.localPosition;
        directionToCenter = (Vector3.zero - originalPosition).normalized;
    }
    
    void FixedUpdate()
    {
        transform.localPosition = originalPosition + directionToCenter * (Mathf.Sin(Time.time * speed) * moveTowardsCenter);
        float wavingScale = (originalSize * (1f + Mathf.Sin(Time.time * speed) * 0.1f));
        transform.localScale = Vector3.one * Mathf.Lerp(wavingScale * 0.5f , wavingScale, transform.localPosition.magnitude);
        transform.Rotate(Vector3.up, Time.deltaTime * speed* 50f);
    }
}
