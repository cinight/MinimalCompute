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

    void Start()
    {
        bufferA = smrA.GetVertexBuffer();
        bufferB = smrB.GetVertexBuffer();

        for(int i=0; i<materials.Length; i++)
        {
            materials[i].SetBuffer("bufVerticesA", bufferA);
            materials[i].SetBuffer("bufVerticesB", bufferB);
        }
    }

    void Update()
    {
        
    }
}
