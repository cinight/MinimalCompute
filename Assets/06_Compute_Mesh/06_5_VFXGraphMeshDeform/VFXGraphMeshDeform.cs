using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VFXGraphMeshDeform : MonoBehaviour
{
    public VisualEffect vfx;
    public ComputeShader computeShader;

    public struct VertexData
    {
        public uint id;
        public Vector3 pos;
        public Vector3 nor;
        public Vector2 uv;
        public Color col;
    }

    private bool init = false;
    private Renderer vfxRen;
    private Mesh mesh;
    private Material vfxMat;
    private GraphicsBufferHandle gbh;
    private ComputeBuffer vertexBuffer;
    private VertexData[] meshVertData;
    private int dispatchCount = 0;
    private CommandBuffer cmd;

    [Header("For Bind Material Example")]
    public Material meshMaterial;

    [Header("For Readback Example")]
    public MeshFilter meshFilter;
    private Mesh newMesh;
    private Vector3[] newMesh_Vert;
    public bool SaveMeshAsset = false;

    void Start()
    {
        vfxRen = vfx.GetComponent<Renderer>();
    }

    private void Init()
    {
        if(init) return;

        //Get the material instance that vfx is using
        if(vfxRen.sharedMaterial != null && vfxMat == null)
        {
            vfxMat = vfxRen.sharedMaterial;
        }

        //Get GraphicsBufferHandle from the vfx material
        if(vfxMat != null && gbh.value == 0)
        {
            gbh = vfxMat.GetBuffer("attributeBuffer");
        }

        //Check if handle is ready
        if(gbh.value == 0)
        {
            return;
        }

        //Unity's sphere has no vertex color,
        //i.e. the array is 0 size, so we need to initiate it
        mesh = vfx.GetMesh("mesh");
        List<Color> meshColList = new List<Color>();
        for (int j=0; j< mesh.vertexCount; j++)
        {
            meshColList.Add(Color.red);
        }
        mesh.SetColors(meshColList);

        //MeshVertexData array
        meshVertData = new VertexData[mesh.vertexCount];
        for (int j=0; j< mesh.vertexCount; j++)
        {
            meshVertData[j].id = (uint)j;
            meshVertData[j].pos = mesh.vertices[j];
            meshVertData[j].nor = mesh.normals[j];
            meshVertData[j].uv = mesh.uv[j];
            meshVertData[j].col = mesh.colors[j];
        }

        //Construct a new mesh
        newMesh = CopyMesh(mesh);
        newMesh_Vert = new Vector3[meshVertData.Length];
        if(meshFilter != null) meshFilter.sharedMesh = newMesh;

        //Compute Buffer
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexData));
        vertexBuffer = new ComputeBuffer(mesh.vertexCount, size);
        vertexBuffer.SetData(meshVertData);

        //Compute Shader
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(0, out threadX, out threadY, out threadZ);
        dispatchCount = Mathf.CeilToInt(meshVertData.Length / threadX);

        //The Material
        meshMaterial.SetBuffer("vertexBuffer", vertexBuffer);

        //Setup CommandBuffer
        cmd = CommandBufferPool.Get("VFXGraphMeshDeform");
        cmd.SetComputeBufferParam(computeShader,0,"attributeBuffer",gbh);
        cmd.SetComputeBufferParam(computeShader,0,"vertexBuffer",vertexBuffer);
        cmd.DispatchCompute(computeShader, 0 , dispatchCount , 1, 1);

        init = true;
    }

    private Mesh CopyMesh(Mesh mesh)
    {
        Mesh nm = new Mesh();
        nm.name = mesh.name + " (Copy)";
        nm.vertices = mesh.vertices;
        nm.triangles = mesh.triangles;
        nm.uv = mesh.uv;
        nm.normals = mesh.normals;
        nm.colors = mesh.colors;
        nm.tangents = mesh.tangents;

        return nm;
    }

    void Update()
    {   
        if(!init)
        {
            Init();
            return;
        }
        else
        {
            //Run the compute shader to copy the vfx attribute buffer contents to our compute buffer
            Graphics.ExecuteCommandBuffer(cmd);

            //Update the mesh
            vertexBuffer.GetData(meshVertData);
            for (int j=0; j< meshVertData.Length; j++)
            {
                newMesh_Vert[j] = meshVertData[j].pos;
            }
            newMesh.SetVertices(newMesh_Vert);
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();

            #if UNITY_EDITOR
            if(SaveMeshAsset)
            {
                AssetDatabase.CreateAsset(newMesh, "Assets/06_Compute_Mesh/06_5_VFXGraphMeshDeform/NewMesh.asset");
                SaveMeshAsset = false;
            }
            #endif
        }
    }

    void OnDisable()
    {
        CleanUp();
    }
    
    void OnDestroy()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        if(vertexBuffer != null)
        {
            vertexBuffer.Release();
            vertexBuffer = null;
        }
        if(cmd != null)
        {
            CommandBufferPool.Release(cmd);
            cmd = null;
        }
    }
}
