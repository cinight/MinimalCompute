using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructuredBufferWithCompute : MonoBehaviour
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
    private Particle[] particleArray;

    void Start()
    {
        //The actual number of particles
        int particleCount = warpCount * warpSize;

        // Init particles to same place
        particleArray = new Particle[particleCount];
        for (int i = 0; i < particleCount; ++i)
        {
            particleArray[i].position = transform.position;
        }
        
        //Initiate the objects
        objs = new GameObject[particleCount];
        for (int i = 0; i < objs.Length; ++i)
        {
            objs[i] = Instantiate(refObj, this.transform);
            objs[i].transform.position = particleArray[i].position;
        }

        //init compute buffer
        cBuffer = new ComputeBuffer(particleCount, 12); // 3*4bytes = sizeof(Particle)
        cBuffer.SetData(particleArray);

        //set compute buffer to compute shader
        computeShader.SetBuffer(0, "particleBuffer", cBuffer);
    }

    void Update()
    {
        //run the compute shader, the position of particles will be updated in GPU
        computeShader.Dispatch(0, warpCount, 1, 1);

        //Get data back from GPU to CPU
        cBuffer.GetData(particleArray);
        
        //Place the GameObjects
        for (int i = 0; i < particleArray.Length; ++i)
        {
            objs[i].transform.position = particleArray[i].position;
        }
    }

    void OnDestroy()
    {
        //remember to release for every compute buffer
        cBuffer.Release();
    }
}
