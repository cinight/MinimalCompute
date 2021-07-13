using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//This sample is using the new Mesh APIs. Official sample project: https://github.com/Unity-Technologies/MeshApiExamples

public class SkinnedMeshBuffer : MonoBehaviour
{
    [Range(0f,1f)] public float progress = 0.5f;

    [Header("Object A")]
    public SkinnedMeshRenderer smrA;
    public Transform hipA;
    private GraphicsBuffer bufferA;

    [Header("Object B")]
    public SkinnedMeshRenderer smrB;
    public Transform hipB;
    private GraphicsBuffer bufferB;

    [Header("Object Mid")]
    public Material[] materials;

    private bool initialized = false;

    void Update()
    {
        if( !initialized )
        {
            //skinned mesh buffer is not available at Start(). so need to do it here
            if(bufferA == null) bufferA = smrA.GetVertexBuffer();
            if(bufferB == null) bufferB = smrB.GetVertexBuffer();

            //bind the buffer to materials
            if(bufferA != null && bufferB != null )
            {
                for(int i=0; i<materials.Length; i++)
                {
                    materials[i].SetBuffer("bufVerticesA", bufferA);
                    materials[i].SetBuffer("bufVerticesB", bufferB);
                }
                initialized = true;
            }
        }
        else
        {
            //runtime values
            for(int i=0; i<materials.Length; i++)
            {
                materials[i].SetFloat("_Progress",progress);
                materials[i].SetVector("_HipLocalPositionA", hipA.localPosition);
                materials[i].SetVector("_HipLocalPositionB", hipB.localPosition);
            }
        }
    }

    void OnDisable()
    {
        if(bufferA != null) bufferA.Dispose();
        if(bufferB != null) bufferB.Dispose();
    }
}
