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
    public GameObject fts_XaxisObj;
    public GameObject fts_YaxisObj;
    public float scale = 1.0f;
    public static float timeSpeed = 10.0f;
    public QuadDraw quadDraw;

    private FTRotation[] fts_Xaxis;
    private FTRotation[] fts_Yaxis;
    private int N = 0; //no. of signals

    public FTRotation[] DFT(FTRotation[] fts, bool isAxisX)
    {
        //Discrete fourier transform
        //Coding Challenge #130.1: Drawing with Fourier Transform and Epicycles by The coding train https://www.youtube.com/watch?v=MY4luNgGfms
        for(int k=0; k<fts.Length; k++) //k is the frequency
        {
            float freq = k;

            float re = 0f; //the amplitude or radius
            float im = 0f; //the phase angle

            for(int n=0; n<N; n++ )
            {
                float pos = isAxisX? quadDraw.drawingPositions[n].x : quadDraw.drawingPositions[n].y;
                float phi = (2f * Mathf.PI * freq * n) / N;
                re += pos * Mathf.Cos(phi);
                im -= pos * Mathf.Sin(phi);
            }
            
            //Average them
            re /= (float)N;
            im /= (float)N;

            //Assign the result to the epicycles
            fts[k].frequency = freq;
            fts[k].amplitude = Mathf.Sqrt(re*re + im*im) * scale; //c^2 = a^2+b^2
            fts[k].phaseAngle = Mathf.Atan2(im,re) * Mathf.Rad2Deg;

            //Set the positions
            Vector3 tran_pos = fts[k].transform.localPosition;
            tran_pos.x = fts[k].amplitude;
            fts[k].transform.localPosition = tran_pos;
        }

        return fts;
    }

    void Start()
    {
        Camera cam = Camera.main;
        fts_Xaxis = fts_XaxisObj.GetComponentsInChildren<FTRotation>();
        fts_Yaxis = fts_YaxisObj.GetComponentsInChildren<FTRotation>();
    }

    void Generate()
    {
        //Draw positions are used as signal
        N = quadDraw.drawingPositions.Count; //no. of signals
        Debug.Log("No. of positions = "+N);

        //The epicycles we have in the scene = the frequency (filter) increases in later ones
        //i.e. more epicycles = more detailed the drawing is
        fts_Xaxis = DFT(fts_Xaxis,true);
        fts_Yaxis = DFT(fts_Yaxis,false);
    }

 	void OnGUI()
    {
        if (GUI.Button(new Rect(170, 10, 150, 100), "Generate"))
        {
            Generate();
        }
    }
}
