using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSphereColor : MonoBehaviour
{
    public static Color color;
    public Material sphereMat;
    public Material planeMat;
    [Range(1f,20f)] public float timeSpeed = 10f;

    void FixedUpdate()
    {
        //Rainbow color
        color = Color.HSVToRGB( 0.5f*(Mathf.Sin( Time.time * Time.fixedDeltaTime * timeSpeed )+1f) , 1f, 1f);
        if(sphereMat != null) sphereMat.SetColor("_Color",color);
        if(planeMat != null) planeMat.SetColor("_Color",color);
    }
}
