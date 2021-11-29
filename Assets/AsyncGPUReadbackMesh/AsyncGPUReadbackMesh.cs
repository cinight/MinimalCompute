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
        asyncGPUReadbackCallback -= AsyncGPUReadbackCallback;
        asyncGPUReadbackCallback += AsyncGPUReadbackCallback;
        request = AsyncGPUReadback.Request(cBuffer,asyncGPUReadbackCallback);
    }

    void Update()
    {
        //run the compute shader, the position of particles will be updated in GPU
        computeShader.SetFloat("_Time",Time.time);
        computeShader.Dispatch(_kernel, dispatchCount, 1, 1);
    }

    //The callback will be run when the request is ready
    private static event System.Action<AsyncGPUReadbackRequest> asyncGPUReadbackCallback;
    public void AsyncGPUReadbackCallback(AsyncGPUReadbackRequest request)
    {
        if(!mesh) return;

        //Readback and show result on texture
        vertData = request.GetData<Vector3>();

        //Update mesh
        mesh.MarkDynamic();
        mesh.SetVertices(vertData);
        mesh.RecalculateNormals();

        //Update to collider
        mc.sharedMesh = mesh;

        //Request AsyncReadback again
        request = AsyncGPUReadback.Request(cBuffer,asyncGPUReadbackCallback);
    }

    private void CleanUp()
    {
        if(cBuffer != null) cBuffer.Release();
        asyncGPUReadbackCallback -= AsyncGPUReadbackCallback;
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