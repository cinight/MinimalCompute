using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ComputeVertexLitPlane : MonoBehaviour
{
    public struct MyVertexData
    {
        public uint id;
        public Vector4 pos;
        public Vector3 nor;
        public Vector4 tan;
        public Vector4 uv;
    }

    //The mesh
    public MeshFilter mf;
    public Material mat;
    public Collider mc;
    private Mesh mesh;

    //Heightmap
    private int sizeHeightMap = 256;
    private int _kernelHeightMap;
    private Vector2Int dispatchCountHeightMap;
    public Material heightMapDebug;

    //Compute
    public ComputeShader shader;
    private int _kernel;
    private int dispatchCount = 0;
    private ComputeBuffer vertexBuffer;
    private MyVertexData[] meshVertData;

    //Mouse input
    private Camera cam;
    private RaycastHit hit;
    private Vector3 mousePos;
    private Vector3 defaultposition = new Vector3(0, 0, -99);

    void Start()
    {
        //For mouse input
        cam = Camera.main;

        //The Mesh
        mesh = mf.mesh;
        mesh.name = "My Mesh";

        //MeshVertexData array
        meshVertData = new MyVertexData[mesh.vertexCount];
        for (int j=0; j< mesh.vertexCount; j++)
        {
            meshVertData[j].id = (uint)j;
            meshVertData[j].pos = mesh.vertices[j];
            meshVertData[j].nor = mesh.normals[j];
            meshVertData[j].uv = mesh.uv[j];
            meshVertData[j].tan = mesh.tangents[j];
        }

        //Compute Buffer
        vertexBuffer = new ComputeBuffer(mesh.vertexCount, 16*4); // sizeof(VertexData) in bytes
		vertexBuffer.SetData(meshVertData);

        //Compute Shader kernel
        _kernel = shader.FindKernel ("CSMain");
        _kernelHeightMap = shader.FindKernel ("CSMainHeightMap");

        //Dispatch counts
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        shader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount = Mathf.CeilToInt(meshVertData.Length / threadX)+1;
        shader.GetKernelThreadGroupSizes(_kernelHeightMap, out threadX, out threadY, out threadZ);
        dispatchCountHeightMap = Vector2Int.one;
        dispatchCountHeightMap.x = Mathf.CeilToInt(sizeHeightMap / threadX)+1;
        dispatchCountHeightMap.y = Mathf.CeilToInt(sizeHeightMap / threadY)+1;

        //heightmap texture
 		RenderTexture tex = new RenderTexture (sizeHeightMap, sizeHeightMap, 0, GraphicsFormat.R8_UNorm);
		tex.enableRandomWrite = true;
		tex.Create ();
		heightMapDebug.mainTexture = tex;

        //SetBuffer
        shader.SetInt("_heightMapSize",sizeHeightMap);
        shader.SetTexture(_kernelHeightMap, "heightMap", tex);
        shader.SetTexture(_kernel, "heightMapTex", tex);
        shader.SetBuffer(_kernel, "vertexBuffer", vertexBuffer);

        //The Material
        mat.name = "My Mat";
        mat.SetBuffer("vertexBuffer", vertexBuffer);
    }

    void Update()
    {
        //Getting mouse position
        if (
            Input.GetMouseButton(0) &&
            Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) &&
            hit.collider == mc
        )
        {
            if (mousePos != hit.point) mousePos = hit.point;
        }
        else
        {
            if (mousePos != defaultposition) mousePos = defaultposition;
        }

        //Run compute shader
        shader.SetVector("_MousePos", mousePos);
        shader.SetFloat("_Time",Time.time);
        shader.Dispatch (_kernelHeightMap, dispatchCountHeightMap.x , dispatchCountHeightMap.y, 1);
        shader.Dispatch (_kernel, dispatchCount , 1, 1);
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
    }
}
