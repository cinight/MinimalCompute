// Fourier Transform tutorial sites
// https://www.jezzamon.com/fourier/index.html
// https://alex.miller.im/posts/fourier-series-spinning-circles-visualization/
// https://betterexplained.com/articles/an-interactive-guide-to-the-fourier-transform/
// https://www.myfourierepicycles.com/
// 3Blue1Brown Fourier Series https://www.youtube.com/watch?v=r6sGWTCMz2k
// 3Blue1Brown Fourier Transform https://www.youtube.com/watch?v=spUNpyF58BY
// https://brettcvz.github.io/epicycles/

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
    public float timeMultiplier = 2f;
    public static float timeSpeed = 1f;
    public QuadDraw quadDraw;

    [Header("Final drawing sphere")]
    public Transform tip_Hor;
    public Transform tip_Ver;
    public Transform sphere;

    private Epicycle[] epicycles_Hor;
    private Epicycle[] epicycles_Ver;
    private int N = 0; //no. of signals

    public Epicycle[] DFT(Epicycle[] epicycles, bool isHorizontal)
    {
        //Discrete fourier transform
        //This is from the Coding Challenge #130.1: Drawing with Fourier Transform and Epicycles
        //by The Coding Train https://www.youtube.com/watch?v=MY4luNgGfms
        for(int k=0; k<epicycles.Length; k++) //k is the frequency
        {
            float freq = k;

            float re = 0f; //the amplitude or radius
            float im = 0f; //the phase angle

            for(int n=0; n<N; n++ )
            {
                float pos = isHorizontal? quadDraw.drawingPositions[n].x : quadDraw.drawingPositions[n].y;
                float phi = (2f * Mathf.PI * freq * n) / N;
                re += pos * Mathf.Cos(phi);
                im -= pos * Mathf.Sin(phi);
            }
            
            //Average them
            re /= (float)N;
            im /= (float)N;

            //Assign the result to the epicycles
            epicycles[k].frequency = freq;
            epicycles[k].amplitude = Mathf.Sqrt(re*re + im*im) * scale; //c^2 = a^2+b^2
            epicycles[k].phaseAngle = Mathf.Atan2(im,re) * Mathf.Rad2Deg;
            epicycles[k].transform.localRotation = Quaternion.Euler(0,epicycles[k].phaseAngle, 0);

            //Align the epicycles vertically, which is the Z axis on scene
            float position = epicycles[k].amplitude;
            epicycles[k].tip.localPosition = new Vector3(0,0,position); 
        }

        return epicycles;
    }

    void Start()
    {
		customButton = new GUIStyle("button");
		customButton.fontSize = 28;

        timeSpeed = timeMultiplier;

        epicycles_Hor = new Epicycle[epicycleCount];
        epicycles_Ver = new Epicycle[epicycleCount];

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

            //Set the moving parent so that the epicycle moves according to parent's tip position
            if(i==0)
            {
                eX.tip_Par = par_Hor;
                eY.tip_Par = par_Ver;
            }
            else
            {
                eX.tip_Par = epicycles_Hor[i-1].tip;
                eY.tip_Par = epicycles_Ver[i-1].tip;
            }

            epicycles_Hor[i] = eX;
            epicycles_Ver[i] = eY;
        }

        //The last tip is for moving the sphere
        tip_Hor = epicycles_Hor[epicycleCount-1].tip;
        tip_Ver = epicycles_Ver[epicycleCount-1].tip;
    }

    void Generate()
    {
        //Draw positions are used as signal
        N = quadDraw.drawingPositions.Count; //no. of signals
        Debug.Log("No. of recorded positions from QuadDraw= "+N);

        quadDraw.MakeSampledPositionEvenlySpaced();

        //The epicycles we have in the scene = the frequency (filter) increases in later ones
        //i.e. more epicycles = more detailed the drawing is
        epicycles_Hor = DFT(epicycles_Hor,true);
        epicycles_Ver = DFT(epicycles_Ver,false);
    }

    void Update()
    {
        //Moving the sphere
        if(sphere!=null && tip_Hor!=null && tip_Ver!=null)
        sphere.position = new Vector3(tip_Hor.position.x, 0 ,tip_Ver.position.z);
    }

    private GUIStyle customButton;
 	void OnGUI()
    {
        if (GUI.Button(new Rect(170, 10, 150, 100), "Generate", customButton))
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
