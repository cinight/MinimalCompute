using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class AsyncGPUReadbackTest : MonoBehaviour
{
    struct Particle
    {
        public Vector4 color;
    };
    
    public ComputeShader computeShader;
    public int res = 128;
    public Material mat;

    private const int warpSize = 32;
    private ComputeBuffer particleBuffer;
    private int particleCount;
    private NativeArray<Particle> plists;
    private Texture2D tex;

    private AsyncGPUReadbackRequest request;
    private Color[] texturecolors;

    private void Setup()
    {
        if(!SystemInfo.supportsAsyncGPUReadback) { this.gameObject.SetActive(false); return;}

        particleCount = res*res;
        CleanUp();

        tex = new Texture2D(res,res);

        // Init particles
        plists = new NativeArray<Particle>(particleCount, Allocator.Temp);
        for (int i = 0; i < particleCount; ++i)
        {
            Color col = Color.HSVToRGB(Random.Range(0f,1f),1,0.5f);
            var c = new Particle();
            c.color = new Vector4(col.r,col.g,col.b,col.a);
            plists[i] = c;
        }

        // Init colors
        texturecolors = new Color[particleCount];

        //Set data to buffer
        particleBuffer = new ComputeBuffer(particleCount, 16); // 4floats * 4bytes
        if(plists.IsCreated) particleBuffer.SetData(plists);

        //Set buffer to computeShader
        computeShader.SetBuffer(0, "particleBuffer", particleBuffer);

        //Bind texture to material
        mat.mainTexture = tex;

        //Request AsyncReadback
        request = AsyncGPUReadback.Request(particleBuffer);
    }

    void Update()
    {
        computeShader.Dispatch(0, Mathf.CeilToInt(particleCount / 32f), 1, 1); //numthreads.x = 32 in compute
        
        //Readback And show result on texture
        if(request.done && !request.hasError)
        {
            plists = request.GetData<Particle>();
            for(int i=0; i<res; i++)
            {
                for(int j=0; j<res; j++)
                {
                    int id = i*res + j;
                    texturecolors[id] = plists[id].color;
                }
            }
            tex.SetPixels(texturecolors);
            tex.Apply();

            //Request AsyncReadback again
            request = AsyncGPUReadback.Request(particleBuffer);
        }
    }

    void OnEnable()
    {
        Setup();
    }

    private void CleanUp()
    {
        if(particleBuffer != null) particleBuffer.Release();
        //if(plists.IsCreated) plists.Dispose(); //Cannot be disposed because of invalid allocator...
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