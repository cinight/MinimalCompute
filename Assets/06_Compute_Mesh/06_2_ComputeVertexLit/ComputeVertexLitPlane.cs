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

    //For the 2 maps
    public int texResolution = 512;

    //Heightmap
    private RenderTexture tex;
    private int _kernelHeightMap;
    private Vector2Int dispatchCountHeightMap;
    public Material heightMapDebug;

    //Normalmap
    private RenderTexture texNor;
    private int _kernelNormalMap;
    private Vector2Int dispatchCountNormalMap;
    public Material normalMapDebug;
    public bool useMaterialNormal = true;

    //Compute
    public ComputeShader shader;
    private int _kernel;
    private int dispatchCount = 0;
    private ComputeBuffer vertexBuffer;
    private MyVertexData[] meshVertData;

    //Mouse input
    private Camera cam;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Vector2 defaultposition = new Vector2(-9, -9); //make it far away

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
        _kernelNormalMap = shader.FindKernel ("CSMainNormalMap");

        //Dispatch counts
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        //kernel
        shader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
        dispatchCount = Mathf.CeilToInt(meshVertData.Length / threadX)+1;
        //heightmap kernel
        shader.GetKernelThreadGroupSizes(_kernelHeightMap, out threadX, out threadY, out threadZ);
        dispatchCountHeightMap = Vector2Int.one;
        dispatchCountHeightMap.x = Mathf.CeilToInt(texResolution / threadX)+1;
        dispatchCountHeightMap.y = Mathf.CeilToInt(texResolution / threadY)+1;
        //normalmap kernel
        shader.GetKernelThreadGroupSizes(_kernelNormalMap, out threadX, out threadY, out threadZ);
        dispatchCountNormalMap = Vector2Int.one;
        dispatchCountNormalMap.x = Mathf.CeilToInt(texResolution / threadX)+1;
        dispatchCountNormalMap.y = Mathf.CeilToInt(texResolution / threadY)+1;

        //heightmap texture
 		tex = new RenderTexture (texResolution, texResolution, 0, GraphicsFormat.R8_UNorm);
		tex.enableRandomWrite = true;
        tex.wrapMode = TextureWrapMode.Clamp;
		tex.Create ();
		heightMapDebug.mainTexture = tex;

        //normalmap texture
 		texNor = new RenderTexture (texResolution, texResolution, 0, GraphicsFormat.R8G8B8A8_UNorm);
		texNor.enableRandomWrite = true;
        texNor.wrapMode = TextureWrapMode.Clamp;
		texNor.Create ();
		normalMapDebug.mainTexture = texNor;

        //SetBuffer
        shader.SetInt("_texResolution",texResolution);
        shader.SetTexture(_kernelHeightMap, "heightMap", tex); // read & write
        shader.SetTexture(_kernelNormalMap, "heightMapTex", tex); // readonly
        shader.SetTexture(_kernelNormalMap, "normalMap", texNor); // read & write
        shader.SetTexture(_kernel, "heightMapTex", tex); // readonly
        shader.SetTexture(_kernel, "normalMapTex", texNor); // readonly
        shader.SetBuffer(_kernel, "vertexBuffer", vertexBuffer); // read & write

        //The Material
        mat.name = "My Mat";
        mat.SetBuffer("vertexBuffer", vertexBuffer);
    }

    void Update()
    {
        if(useMaterialNormal)
        {
            //Because normal map is only generated in runtime, 
            //i.e. before playmode the material has no normal map, so Unity turns off this keyword.
            mat.SetTexture("_BumpMap", texNor);
            mat.EnableKeyword("_NORMALMAP");
        }
        else
        {
            //mat.SetTexture("_BumpMap", texNor);
            mat.DisableKeyword("_NORMALMAP");
        }
        
        //Getting mouse position. MeshCollider is needed for getting hit.textureCoord
        if (
            Input.GetMouseButton(0) &&
            Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) &&
            hit.collider == mc
        )
        {
            if (mousePos != hit.textureCoord) mousePos = hit.textureCoord;
        }
        else
        {
            if (mousePos != defaultposition) mousePos = defaultposition;
        }

        //Run compute shader
        shader.SetVector("_MousePos", mousePos);
        shader.Dispatch (_kernelHeightMap, dispatchCountHeightMap.x , dispatchCountHeightMap.y, 1);
        shader.Dispatch (_kernelNormalMap, dispatchCountNormalMap.x , dispatchCountNormalMap.y, 1);
        shader.Dispatch (_kernel, dispatchCount , 1, 1);
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
    }
}
