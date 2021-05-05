using UnityEngine;
using System.Collections;

public class ComputeParticlesDirect_Builtin : MonoBehaviour 
{
	struct Particle
	{
		public Vector3 position;
	};

	public int warpCount = 5;
	public Material material;
	public ComputeShader computeShader;

	private const int warpSize = 32;
	private ComputeBuffer particleBuffer;
	private int particleCount;
	private Particle[] plists;

	void Start () 
	{
		particleCount = warpCount * warpSize;
		
		// Init particles
		plists = new Particle[particleCount];
		for (int i = 0; i < particleCount; ++i)
		{
            plists[i].position = Random.insideUnitSphere * 4f;
        }
		
		//Set data to buffer
		particleBuffer = new ComputeBuffer(particleCount, 12); // 12 = sizeof(Particle)
		particleBuffer.SetData(plists);
		
		//Set buffer to computeShader and Material
		computeShader.SetBuffer(0, "particleBuffer", particleBuffer);
		material.SetBuffer ("particleBuffer", particleBuffer);
	}

	void Update () 
	{
		computeShader.Dispatch(0, warpCount, 1, 1);
	}

	void OnRenderObject()
	{
		material.SetPass(0);
		Graphics.DrawProceduralNow(MeshTopology.Points,1,particleCount);
	}

	void OnDestroy()
	{
		particleBuffer.Release();
	}
}
