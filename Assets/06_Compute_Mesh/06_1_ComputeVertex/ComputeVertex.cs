using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeVertex : MonoBehaviour
{
    public struct VertexData
    {
        public uint id;
        public Vector4 pos;
        public Vector3 nor;
        public Vector2 uv;
        public Color col;

        public Vector4 opos;
        public Vector3 velocity;
    }

    //The mesh
    private Mesh mesh;
    private Material mat;

    //For Mesh Color
    private Color col_default = Color.black;

    //Compute
    public ComputeShader shader;
    private int _kernel;
    private int dispatchCount = 0;
    private ComputeBuffer vertexBuffer;
    private VertexData[] meshVertData;

    void Start()
    {
        //The Mesh (instanced)
        MeshFilter filter = this.GetComponent<MeshFilter>();
        mesh = filter.mesh;
        mesh.name = "My Mesh";

        //Unity's sphere has no vertex color,
        //i.e. the array is 0 size, so we need to initiate it
        List<Color> meshColList = new List<Color>();
        for (int j=0; j< mesh.vertexCount; j++)
        {
            meshColList.Add(col_default);
        }
        mesh.SetColors(meshColList);

        //Random vector
        Vector3 s = Vector3.one;
        s.x = UnityEngine.Random.Range(0.1f,1f);
        s.y = UnityEngine.Random.Range(0.1f,1f);
        s.z = UnityEngine.Random.Range(0.1f,1f);

        //MeshVertexData array
        meshVertData = new VertexData[mesh.vertexCount];
        for (int j=0; j< mesh.vertexCount; j++)
        {
            meshVertData[j].id = (uint)j;
            meshVertData[j].pos = mesh.vertices[j];
            meshVertData[j].nor = mesh.normals[j];
            meshVertData[j].uv = mesh.uv[j];
            meshVertData[j].col = mesh.colors[j];

            meshVertData[j].opos = meshVertData[j].pos;
            meshVertData[j].velocity = s;
        }

        //Compute Buffer
        vertexBuffer = new ComputeBuffer(mesh.vertexCount, 21*4); // sizeof(VertexData) in bytes
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
        MeshRenderer ren = this.GetComponent<MeshRenderer>();
        mat = ren.material;
        mat.name = "My Mat";
        mat.SetBuffer("vertexBuffer", vertexBuffer);
    }

    void Update()
    {
        //Run compute shader
        shader.SetFloat("_Time",Time.time);
        shader.Dispatch (_kernel, dispatchCount , 1, 1);
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
    }
}
