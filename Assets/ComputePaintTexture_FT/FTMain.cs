// Fourier Transform sites
// https://www.jezzamon.com/fourier/index.html
// https://alex.miller.im/posts/fourier-series-spinning-circles-visualization/
// https://betterexplained.com/articles/an-interactive-guide-to-the-fourier-transform/
// https://www.myfourierepicycles.com/
// 3Blue1Brown Fourier Series https://www.youtube.com/watch?v=r6sGWTCMz2k
// 3Blue1Brown Fourier Transform https://www.youtube.com/watch?v=spUNpyF58BY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Numerics;

public class FTMain : MonoBehaviour
{
    public FTRotation[] fts;
    public Texture2D tex;
    public bool isRow = true;

    void Awake()
    {
        
      

        fts = GetComponentsInChildren<FTRotation>();
        for(int f=0; f<fts.Length; f++ )
        {
            //Amplitude and Phase for current freq
            Complex Xk = new Complex(0, 0);

            for(int row=0; row<tex.width; row++ )
            {
                for(int col=0; col<tex.height; col++ )
                {
                    float N = isRow? tex.width : tex.height; //resolution
                    float n = isRow? row : col; //current pixel
                    Color Xn = tex.GetPixel(row,col); //current pixel color
                    float k = fts[f].frequency; //filter

                    Complex com = new Complex(0, - 2f * Mathf.PI * k * n);
                    com /= N;
                    Complex exp = new Complex(Math.E, 0);
                    Xk += col * Complex.Pow(exp, com);

                }
            }

            Xk /= isRow? tex.width : tex.height;
            Debug.Log(Xk);


        }



    }

    void Update()
    {
        
    }
}
