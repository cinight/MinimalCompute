using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructuredBufferWithCompute : MonoBehaviour
{
    struct Particle
    {
        public Vector3 position;
    };

    public int warpCount = 5; // The number particle /32.
    public ComputeShader computeShader;
    public GameObject refObj;

    private const int warpSize = 32; // GPUs process data by warp, 32 for every modern ones.
    private ComputeBuffer cBuffer;

    void Start()
    {
        //The actual number of particles
        int particleCount = warpCount * warpSize;

        // Init particles to same place
        Particle[] particleArray = new Particle[particleCount];
        for (int i = 0; i < particleCount; ++i)
        {
            particleArray[i].position = transform.position;
        }

        //init compute buffer
        cBuffer = new ComputeBuffer(particleCount, 12); // 3*4bytes = sizeof(Particle)
        cBuffer.SetData(particleArray);

        //set compute buffer to compute shader
        computeShader.SetBuffer(0, "particleBuffer", cBuffer);

        //run the compute shader, the position of particles will be updated in GPU
        computeShader.Dispatch(0, warpCount, 1, 1);

        //Get data back from GPU to CPU
        cBuffer.GetData(particleArray);

        //Place the new GameObjects
        for (int i = 0; i < particleArray.Length; ++i)
        {
            GameObject go = Instantiate(refObj, this.transform);
            go.transform.position = particleArray[i].position;
        }
    }

    void OnDestroy()
    {
        //remember to release for every compute buffer
        cBuffer.Release();
    }
}
