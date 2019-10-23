
// This is same as AsyncGPUReadbackMesh, but using new Mesh API introduced in 2019.3
// https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Mesh.SetVertexBufferData.html
//doesn't seem to have any further speedup using new API in this scenario

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class AsyncGPUReadbackMesh_NewMeshAPI : MonoBehaviour
{
    public ComputeShader computeShader;
    public MeshFilter mf;
    public MeshCollider mc;

    //Using 2019.3 new Mesh API
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct VertexData
    {
        public Vector3 pos;
        public Vector3 nor;
        public Vector2 uv;
    }

    private Mesh mesh;
    private ComputeBuffer cBuffer;
    private int _kernel;
    private int dispatchCount = 0;
    private NativeArray<VertexData> vertData;
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
        vertData = new NativeArray<VertexData>(mesh.vertexCount, Allocator.Temp);
        for (int i = 0; i < mesh.vertexCount; ++i)
        {
            VertexData v = new VertexData();
            v.pos = mesh.vertices[i];
            v.nor = mesh.normals[i];
            v.uv = mesh.uv[i];
            vertData[i] = v;
        }

        //Using 2019.3 new Mesh API
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
        };
        mesh.SetVertexBufferParams(mesh.vertexCount, layout);
        
        //init compute buffer
        cBuffer = new ComputeBuffer(mesh.vertexCount, 8*4); // 3*4bytes = sizeof(Vector3)
        if(vertData.IsCreated) cBuffer.SetData(vertData);
        computeShader.SetBuffer(_kernel, "vertexBuffer", cBuffer);

        //Request AsyncReadback
        request = AsyncGPUReadback.Request(cBuffer);

        Debug.Log("VertexCount = "+vertData.Length);
    }

    void Update()
    {
        //run the compute shader, the position of particles will be updated in GPU
        computeShader.SetFloat("_Time",Time.time);
        computeShader.Dispatch(_kernel, dispatchCount, 1, 1);
       
        if(request.done && !request.hasError)
        {
            //Readback and show result on texture
            vertData = request.GetData<VertexData>();

            //Update mesh
            mesh.MarkDynamic();
            mesh.SetVertexBufferData(vertData,0,0,vertData.Length);
            mesh.RecalculateNormals();

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