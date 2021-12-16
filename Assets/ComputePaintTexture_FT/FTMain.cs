// Fourier Transform tutorial sites
// https://www.jezzamon.com/fourier/index.html
// https://alex.miller.im/posts/fourier-series-spinning-circles-visualization/
// https://betterexplained.com/articles/an-interactive-guide-to-the-fourier-transform/
// https://www.myfourierepicycles.com/
// 3Blue1Brown Fourier Series https://www.youtube.com/watch?v=r6sGWTCMz2k
// 3Blue1Brown Fourier Transform https://www.youtube.com/watch?v=spUNpyF58BY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTMain : MonoBehaviour
{
    public MeshFilter mf;
    public GameObject fts_XaxisObj;
    public GameObject fts_YaxisObj;
    public float scale = 1.0f;
    public static float timeSpeed = 1.0f;

    private FTRotation[] fts_Xaxis;
    private FTRotation[] fts_Yaxis;
    private int N = 0; //no. of signals
    private Vector3[] vpos;


    public FTRotation[] DFT(FTRotation[] fts, bool isAxisX)
    {
        //Discrete fourier transform
        //Coding Challenge #130.1: Drawing with Fourier Transform and Epicycles by The coding train https://www.youtube.com/watch?v=MY4luNgGfms
        for(int k=0; k<fts.Length; k++) //k is the frequency
        {
            float re = 0f; //the amplitude or radius
            float im = 0f; //the phase angle

            for(int n=0; n<N; n++ )
            {
                float pos = isAxisX? vpos[n].x : vpos[n].y;
                float phi = 2f * Mathf.PI * k * n / N;
                re += pos * Mathf.Cos(phi);
                im -= pos * Mathf.Sin(phi);
            }
            
            //Average them
            re /= N;
            im /= N;

            //Assign the result to the epicycles
            fts[k].frequency = k;
            fts[k].amplitude = Mathf.Sqrt(re*re + im*im) * scale; //c^2 = a^2+b^2
            fts[k].phaseAngle = Mathf.Atan2(im,re) * Mathf.Rad2Deg;
        }

        return fts;
    }

    void Awake()
    {
        Camera cam = Camera.main;

        //Get Mesh Vertices position in screen space. Mesh Vertices will be used as signal
        vpos = mf.sharedMesh.vertices;
        N = vpos.Length; //no. of signals
        Debug.Log("No. of vertices = "+N);

        //Convert vertex position from object space to screen space
        for(int i=0; i< vpos.Length; i++)
        {
            vpos[i] = mf.transform.TransformPoint(vpos[i]); //to world space
            vpos[i] = cam.WorldToScreenPoint(vpos[i]); //to screen space

            //to -1 to 1 range
            vpos[i].x = (vpos[i].x / cam.pixelWidth - 0.5f)*2.0f;
            vpos[i].y = (vpos[i].y / cam.pixelHeight - 0.5f)*2.0f;
        }

        //The epicycles we have in the scene = the frequency (filter) increases in later ones
        //i.e. more epicycles = more detailed the drawing is
        fts_Xaxis = fts_XaxisObj.GetComponentsInChildren<FTRotation>();
        fts_Xaxis = DFT(fts_Xaxis,true);
        fts_Yaxis = fts_YaxisObj.GetComponentsInChildren<FTRotation>();
        fts_Yaxis = DFT(fts_Yaxis,false);
    }
}
