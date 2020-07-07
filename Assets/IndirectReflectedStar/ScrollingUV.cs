using UnityEngine;

public class ScrollingUV : MonoBehaviour
{
    public Vector2 speed;
    public Material material;

    private Vector2 uvOffset = Vector2.zero;

    void Update()
    {
        uvOffset += Time.deltaTime*speed;
        material.SetTextureOffset("_MainTex",uvOffset);
    }
}
