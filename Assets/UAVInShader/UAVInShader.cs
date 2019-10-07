using UnityEngine;
using System.Collections;

public class UAVInShader : MonoBehaviour
{
    public Material mat;
    public TextMesh text;

    private int targetID = 6; //match with shader "register(u6)"
    private ComputeBuffer fieldbuf;
    private float[] fdata = new float[3];

    void OnEnable()
    {
        Setup();
    }

    void OnDisable()
    {
        if (fieldbuf != null)
        {
            fieldbuf.Release();
            fieldbuf.Dispose();
            fieldbuf = null;
        }
    }

    void Setup()
    {
        if (fieldbuf == null)
        {
            fieldbuf = new ComputeBuffer(3, sizeof(float), ComputeBufferType.Default);
        }
    }

	void OnRenderObject()
    {
        Graphics.ClearRandomWriteTargets();
        Setup();
        mat.SetPass(0);
        mat.SetBuffer("Field", fieldbuf);
        Graphics.SetRandomWriteTarget(targetID, fieldbuf);

        fieldbuf.GetData(fdata);
        text.text = "From shader, the RGB value of rainbow color \n";
        for (int i = 0; i < fdata.Length; i++)
        {
            text.text += i + ":  " + fdata[i] + "\n";
        }
    }
}
