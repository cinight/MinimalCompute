using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshBuffer_DiffMesh : MonoBehaviour
{
    [Range(0f,1f)] public float progress = 0.5f;

    [Header("Object A")]
    public SkinnedMeshRenderer smrA;
    public Transform hipA;
    private GraphicsBuffer bufferA;
    private GraphicsBuffer bufferA_index;
    private int vertCountA;
    private int triangleCountA;

    [Header("Object B")]
    public SkinnedMeshRenderer smrB;
    public Transform hipB;
    private GraphicsBuffer bufferB;
    private GraphicsBuffer bufferB_index;
    private int vertCountB;
    private int triangleCountB;

    [Header("Object Mid")]
    public Material material;
    public Mesh mesh;

    private ComputeBuffer argsBuffer;
    private Bounds bound;
    private bool initialized = false;
    private int count = 0;

    void Start()
    {
        uint indexCountA = smrA.sharedMesh.GetIndexCount(0);
        uint indexCountB = smrB.sharedMesh.GetIndexCount(0);
        triangleCountA = (int) indexCountA/3;
        triangleCountB = (int) indexCountB/3;
        count = Mathf.Max(triangleCountA,triangleCountB);

        Debug.Log("IndexFormat of A="+smrA.sharedMesh.indexFormat+"  "+"IndexFormat of B="+smrB.sharedMesh.indexFormat);
        Debug.Log("IndexCount of A="+indexCountA+"  "+"IndexCount of B="+indexCountB);
        Debug.Log("TriangleCount of A="+triangleCountA+"  "+"TriangleCount of B="+triangleCountB+" "+"Will draw "+count+" triangles.");

        smrA.sharedMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        smrB.sharedMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

        //Index buffer is only available here. If this is put in Update(), IsValid returns false
        bufferA_index = smrA.sharedMesh.GetIndexBuffer();
        bufferB_index = smrB.sharedMesh.GetIndexBuffer();
        material.SetBuffer("bufVerticesA_index", bufferA_index);
        material.SetBuffer("bufVerticesB_index", bufferB_index);
    }

    void Update()
    {
        if( !initialized )
        {
            CleanUp();

            vertCountA = smrA.sharedMesh.vertexCount;
            vertCountB = smrB.sharedMesh.vertexCount;
            Debug.Log("VertexCount of A="+vertCountA+"  "+"VertexCount of B="+vertCountB);

            //skinned mesh buffer is not available at Start(). so need to do it here
            if(bufferA == null) bufferA = smrA.GetVertexBuffer();
            if(bufferB == null) bufferB = smrB.GetVertexBuffer();

            //args buffer & bound
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)count;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);
            bound = new Bounds(this.transform.position, Vector3.one*100f);

            //bind the buffer to materials
            if(bufferA != null && bufferB != null)
            {
                material.SetBuffer("bufVerticesA", bufferA);
                material.SetBuffer("bufVerticesB", bufferB);

                initialized = true;
                Debug.Log("initialized");
            }
        }
        else
        {
            //runtime values
            material.SetFloat("_Progress",progress);
            material.SetVector("_HipLocalPositionA", hipA.localPosition);
            material.SetVector("_HipLocalPositionB", hipB.localPosition);

            //Draw
            Graphics.DrawMeshInstancedIndirect(mesh,0,material,bound,argsBuffer,0);
        }
    }

    private void CleanUp()
    {
        if(bufferA != null) bufferA.Dispose();
        if(bufferB != null) bufferB.Dispose();
        if(bufferA_index != null) bufferA_index.Dispose();
        if(bufferB_index != null) bufferB_index.Dispose();

        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }

    void OnDisable()
    {
        CleanUp();
    }
}
