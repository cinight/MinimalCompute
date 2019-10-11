// Around 60 FPS if we do not use AsyncGPUReadback
// Around 145 FPS if we use AsyncGPUReadback!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class AsyncGPUReadbackMesh : MonoBehaviour
{
    public ComputeShader computeShader;
    public MeshFilter mf;
    public MeshCollider mc;

    private Mesh mesh;
    private ComputeBuffer cBuffer;
    private int _kernel;
    private int dispatchCount = 0;
    private NativeArray<Vector3> vertData;
    private AsyncGPUReadbackRequest request;

    private void Start()
    {
        if(!SystemInfo.supportsAsyncGPUReadback) { this.gameObject.SetActive(false); return;}

        //The Mesh
        mesh = mf.mesh;
        mesh.name = "My Mesh";

        //compute shader
        _kernel = computeShader.FindKernel ("CSMain");
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount = Mathf.CeilToInt(mesh.vertexCount / threadX +1);

        // Init mesh vertex array
        Vector3[] meshVerts = mesh.vertices;
        vertData = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Temp);
        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            vertData[i] = meshVerts[i];
        }
        
        //init compute buffer
        cBuffer = new ComputeBuffer(mesh.vertexCount, 12); // 3*4bytes = sizeof(Vector3)
        if(vertData.IsCreated) cBuffer.SetData(vertData);
        computeShader.SetBuffer(_kernel, "vertexBuffer", cBuffer);

        //Request AsyncReadback
        request = AsyncGPUReadback.Request(cBuffer);
    }

    void Update()
    {
        //run the compute shader, the position of particles will be updated in GPU
        computeShader.SetFloat("_Time",Time.time);
        computeShader.Dispatch(_kernel, dispatchCount, 1, 1);
       
        if(request.done && !request.hasError)
        {
            //Readback and show result on texture
            vertData = request.GetData<Vector3>();

            //Update mesh
            mesh.MarkDynamic();
            mesh.vertices = vertData.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            //Update to collider
            mc.sharedMesh = mesh;

            //Request AsyncReadback again
            request = AsyncGPUReadback.Request(cBuffer);
        }
    }

    private void CleanUp()
    {
        if(cBuffer != null) cBuffer.Release();
    }

    void OnDisable()
    {
        CleanUp();
    }

    void OnDestroy()
    {
        CleanUp();
    }
}