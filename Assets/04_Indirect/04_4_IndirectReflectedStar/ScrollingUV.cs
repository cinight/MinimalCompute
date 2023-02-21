using UnityEngine;

public class ScrollingUV : MonoBehaviour
{
    public Vector2 speed;
    public Material material;
    public string propname = "_BaseMap";

    private Vector2 uvOffset = Vector2.zero;

    void Update()
    {
        uvOffset += Time.deltaTime*speed;
        material.SetTextureOffset(propname,uvOffset);
    }
}
