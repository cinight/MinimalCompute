using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This sample is using the new Mesh APIs. Official sample project: https://github.com/Unity-Technologies/MeshApiExamples

public class SkinnedMeshBuffer : MonoBehaviour
{
    [Header("Object A")]
    public SkinnedMeshRenderer smrA;
    private GraphicsBuffer bufferA;

    [Header("Object B")]
    public SkinnedMeshRenderer smrB;
    private GraphicsBuffer bufferB;

    [Header("Object Mid")]
    public Material[] materials;

    void Update()
    {
        //skinned mesh buffer is not available at Start(). so need to do it here
        if(bufferA == null)
        {
            bufferA = smrA.GetVertexBuffer();
        }
        if(bufferB == null)
        {
            bufferB = smrB.GetVertexBuffer();
        }

        //bind the buffer to materials
        if(bufferA != null && bufferB != null)
        {
            for(int i=0; i<materials.Length; i++)
            {
                materials[i].SetBuffer("bufVerticesA", bufferA);
                materials[i].SetBuffer("bufVerticesB", bufferB);
            }
        }
        else
        {
            Debug.Log("Buffers are null");
        }
    }

    void OnDisable()
    {
        if(bufferA != null)
        {
            bufferA.Dispose();
        }
        if(bufferB != null)
        {
            bufferB.Dispose();
        }
    }
}
