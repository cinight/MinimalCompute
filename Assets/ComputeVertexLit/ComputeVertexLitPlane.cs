using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public MeshCollider mc;
    private Mesh mesh;

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

        //Compute Shader
        _kernel = shader.FindKernel ("CSMain");
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        shader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount = Mathf.CeilToInt(meshVertData.Length / threadX);
        shader.SetBuffer(_kernel, "vertexBuffer", vertexBuffer);
        shader.SetInt("_VertexCount",meshVertData.Length);

        //The Material
        //MeshRenderer ren = this.GetComponent<MeshRenderer>();
        //mat = ren.material;
        mat.name = "My Mat";
        mat.SetBuffer("vertexBuffer", vertexBuffer);
    }

    void Update()
    {
        //Getting mouse position
        if (Input.GetMouseButton(0))
        {
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit)) return;
                
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == mc)
            {
                if (mousePos != hit.point) mousePos = hit.point;
            }
        }
        else
        {
            if (mousePos != defaultposition) mousePos = defaultposition;
        }

        //Run compute shader
        shader.SetVector("_MousePos", mousePos);
        shader.SetFloat("_Time",Time.time);
        shader.Dispatch (_kernel, dispatchCount , 1, 1);
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
    }
}
