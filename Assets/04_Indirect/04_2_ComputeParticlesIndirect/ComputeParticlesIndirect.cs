using UnityEngine;
using System.Collections;

public class ComputeParticlesIndirect : MonoBehaviour 
{
	//The stride passed when constructing the buffer must match structure size, be a multiple of 4 and less than 2048
	struct Particle
	{
		public Vector3 position;
		public uint idx;
		public Color color;
	};

	public int particleCount = 5000;
	public Material material;
	public ComputeShader computeShader;

	private int _kernelDirect;
	private ComputeBuffer particleBuffer;
	private ComputeBuffer particleFilteredResultBuffer;
	private ComputeBuffer argsBuffer;
	private int[] args;
	private Particle[] plists;


	private Bounds bounds;

	void Start ()
	{
		//just to make sure the buffer are clean
		release();
		
		//kernels
        _kernelDirect = computeShader.FindKernel("main1");
		
		// Init particles position
		plists = new Particle[particleCount];
		for (int i = 0; i < particleCount; ++i)
		{
			plists[i].idx = (uint)i;
            plists[i].position = Random.insideUnitSphere * 4f;
			plists[i].color = Color.yellow;
        }
		
		//particleBuffer, for rendering
		particleBuffer = new ComputeBuffer(particleCount, 4+12+16); // 4+12+16 = sizeof(Particle)
		particleBuffer.SetData(plists);

		//filtered result buffer, storing only the idx value of a particle
		particleFilteredResultBuffer = new ComputeBuffer(particleCount, sizeof(uint), ComputeBufferType.Append);
		
		//bind buffer to computeShader and Material
		computeShader.SetBuffer(_kernelDirect, "particleFiltered", particleFilteredResultBuffer);
		computeShader.SetBuffer(_kernelDirect, "particleBuffer", particleBuffer);
		material.SetBuffer ("particleBuffer", particleBuffer);
		material.SetBuffer ("particleResult", particleFilteredResultBuffer);

		//Args for indirect draw
		args = new int[]
		{
			(int)1, //vertex count per instance
			(int)particleCount, //instance count
			(int)0, //start vertex location
			(int)0 //start instance location
		};
		argsBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

		//just a big enough bounds for drawing
		bounds = new Bounds(Vector3.zero,Vector3.one*400f);
	}

	void Update () 
	{
		//Reset count
		particleFilteredResultBuffer.SetCounterValue(0);

		//Direct dispatch to do filter
		computeShader.SetFloat("_Time",Time.time);
		computeShader.Dispatch(_kernelDirect, particleCount, 1, 1);

		//Copy Count - visually no change but this is necessary in terms of performance!
		//because without this, shader will draw full amount of particles, just overlapping
		//Check Profiler > GPU > Hierarchy search Graphics.DrawProcedural > GPU time
		//4 is the offset byte. "particleCount" is the second int in args[], and 1 int = 4 bytes
		ComputeBuffer.CopyCount(particleFilteredResultBuffer, argsBuffer, 4);

		//Draw
		//3*4 is the offset byte, where the indirect draw in args starts
		Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Points,argsBuffer, 0);
	}

    private void release()
    {
        if (particleFilteredResultBuffer != null)
        {
            particleFilteredResultBuffer.Dispose();
            particleFilteredResultBuffer.Release();
            particleFilteredResultBuffer = null;
        }
        if (particleBuffer != null)
        {
            particleBuffer.Dispose();
            particleBuffer.Release();
            particleBuffer = null;
        }
        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
	void OnDestroy()
	{
        release();
    }

	void OnApplicationQuit()
	{
        release();
    }
}
