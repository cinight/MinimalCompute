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

public class DFTMain : MonoBehaviour
{
    public int epicycleCount = 20;
    public GameObject epicyclePrefab;
    public Transform par_Hor;
    public Transform par_Ver;
    public float scale = 1.0f;
    public static float timeSpeed = 1f;
    public QuadDraw quadDraw;

    [Header("Final drawing sphere")]
    public Transform tip_Hor;
    public Transform tip_Ver;
    public Transform sphere;

    private Epicycle[] fts_Xaxis;
    private Epicycle[] fts_Yaxis;
    private int N = 0; //no. of signals

    public Epicycle[] DFT(Epicycle[] fts, bool isAxisX)
    {
        //Discrete fourier transform
        //This is from the Coding Challenge #130.1: Drawing with Fourier Transform and Epicycles
        //by The coding train https://www.youtube.com/watch?v=MY4luNgGfms
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
            float offset = 0f;//isAxisX? 0f: 90f;
            fts[k].phaseAngle = Mathf.Atan2(im,re) * Mathf.Rad2Deg + offset;
            fts[k].transform.localRotation = Quaternion.Euler(0,fts[k].phaseAngle, 0);

            //Set the positions
            float position = fts[k].amplitude;
            fts[k].tip.localPosition = new Vector3(position,0,0);
        }

        return fts;
    }

    void Start()
    {
        Camera cam = Camera.main;

        timeSpeed = (2f * Mathf.PI) / epicycleCount;

        fts_Xaxis = new Epicycle[epicycleCount];
        fts_Yaxis = new Epicycle[epicycleCount];

        //Generate epicycles
        for(int i=0; i<epicycleCount; i++)
        {
            GameObject newX = Instantiate(epicyclePrefab,par_Hor);
            GameObject newY = Instantiate(epicyclePrefab,par_Ver);

            newX.name = "Epicycle_"+i;
            newY.name = "Epicycle_"+i;

            newX.transform.localPosition = Vector3.zero;
            newY.transform.localPosition = Vector3.zero;

            Epicycle eX = newX.GetComponent<Epicycle>();
            Epicycle eY = newY.GetComponent<Epicycle>();

            //Set the moving parent so that the epicycle moves according to parent position
            if(i==0)
            {
                eX.tip_Par = par_Hor;
                eY.tip_Par = par_Ver;
            }
            else
            {
                eX.tip_Par = fts_Xaxis[i-1].tip;
                eY.tip_Par = fts_Yaxis[i-1].tip;
            }

            fts_Xaxis[i] = eX;
            fts_Yaxis[i] = eY;
        }

        //The last tip is for moving the sphere
        tip_Hor = fts_Xaxis[epicycleCount-1].tip;
        tip_Ver = fts_Yaxis[epicycleCount-1].tip;
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

    void Update()
    {
        //Moving the sphere
        if(sphere!=null && tip_Hor!=null && tip_Ver!=null)
        sphere.position = new Vector3(tip_Hor.position.x, 0 ,tip_Ver.position.z);
    }

 	void OnGUI()
    {
        if (GUI.Button(new Rect(170, 10, 150, 100), "Generate"))
        {
            Generate();
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(sphere!=null && tip_Hor!=null && tip_Ver!=null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(tip_Hor.position, sphere.position);
            Gizmos.DrawLine(tip_Ver.position, sphere.position);
        }
    }
    #endif
}
