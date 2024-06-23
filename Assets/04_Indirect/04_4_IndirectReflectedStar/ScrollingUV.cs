using UnityEngine;

public class ScrollingUV : MonoBehaviour
{
    public Vector2 speed;
    public Material material;
    public string propname = "_BaseMap";

    private Vector2 _uvOffset = Vector2.zero;
    private float _timeIterator = 0;

    void Update()
    {
        _uvOffset.x += speed.x * Time.deltaTime;
        _uvOffset.y += speed.y * Mathf.Sin(_timeIterator) * Time.deltaTime;
        material.SetTextureOffset(propname,_uvOffset);
        
        _timeIterator += Time.deltaTime;
        _timeIterator = Mathf.Repeat(_timeIterator, 2f*Mathf.PI);
    }
}
