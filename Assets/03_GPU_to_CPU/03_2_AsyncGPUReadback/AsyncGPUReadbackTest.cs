using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class AsyncGPUReadbackTest : MonoBehaviour
{
    struct Particle
    {
        public Vector3 position;
    };
    
    public int warpCount = 5;
    public ComputeShader computeShader;
    public GameObject refObj;

    private GameObject[] objs;
    private const int warpSize = 32; //match with compute numOfThread X
    private ComputeBuffer cBuffer;
    
    private NativeArray<Particle> plists;
    private AsyncGPUReadbackRequest request;

    private void Start()
    {
        if(!SystemInfo.supportsAsyncGPUReadback) { this.gameObject.SetActive(false); return;}

        //The actual number of particles
        int particleCount = warpCount * warpSize;

        // Init particles to same place
        plists = new NativeArray<Particle>(particleCount, Allocator.Temp);
        for (int i = 0; i < particleCount; ++i)
        {
            var c = new Particle();
            c.position = transform.position;
            plists[i] = c;
        }
        
        //Initiate the objects
        objs = new GameObject[particleCount];
        for (int i = 0; i < objs.Length; ++i)
        {
            objs[i] = Instantiate(refObj, this.transform);
            objs[i].transform.position = plists[i].position;
        }

        //init compute buffer
        cBuffer = new ComputeBuffer(particleCount, 12); // 3*4bytes = sizeof(Particle)
        if(plists.IsCreated) cBuffer.SetData(plists);

        //set compute buffer to compute shader
        computeShader.SetBuffer(0, "particleBuffer", cBuffer);
        
        //Request AsyncReadback
        request = AsyncGPUReadback.Request(cBuffer); //here we can also use the callback method. see AsyncGPUReadbackMesh.cs
    }

    void Update()
    {
        //run the compute shader, the position of particles will be updated in GPU
        computeShader.Dispatch(0, warpCount, 1, 1);
        
        if(request.done && !request.hasError)
        {
            //Readback And show result on texture
            plists = request.GetData<Particle>();
            
            //Place the GameObjects
            for (int i = 0; i < plists.Length; ++i)
            {
                objs[i].transform.position = plists[i].position;
            }

            //Request AsyncReadback again
            request = AsyncGPUReadback.Request(cBuffer); //here we can also use the callback method. see AsyncGPUReadbackMesh.cs
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