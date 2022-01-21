using UnityEngine;
using System.Collections;

public class ComputeSketch : MonoBehaviour 
{
	struct Particle
	{
		public Vector2 position;
		public float direction; //angle
		public float intensity;
	};

	public int particleCount;
	public Material material;
	public Mesh mesh;
	public Texture texRef;
	public ComputeShader computeShader;

	private int _kernel;
	private int dispatchCount;
	private ComputeBuffer particleBuffer;
	private ComputeBuffer argsBuffer;
	private Bounds bound;
	private Particle[] plists;


	void Start () 
	{
		//Compute kernel and dispatch size
		_kernel = computeShader.FindKernel ("CSMain");
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        computeShader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
		dispatchCount = Mathf.CeilToInt(particleCount / threadX);

		// Init particles
		plists = new Particle[particleCount];
		for (int i = 0; i < particleCount; ++i)
		{
            plists[i].position = new Vector2( Random.Range(0.00f,1.00f), Random.Range(0.00f,1.00f) );
			plists[i].direction = Random.Range(0.00f,2.00f*Mathf.PI); //angle
			plists[i].intensity = 0f;
        }
		
		//arg buffer
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)particleCount;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        bound = new Bounds(this.transform.position, Vector3.one*100f);
		
		//Set data to buffer
		particleBuffer = new ComputeBuffer(particleCount, 4*4); // 4 floats * 4 bytes = sizeof(Particle)
		particleBuffer.SetData(plists);
		
		//Set buffer to computeShader and Material
		computeShader.SetInt("texSize",texRef.width);
		computeShader.SetTexture(_kernel,"texRef",texRef);
		computeShader.SetBuffer(_kernel, "particleBuffer", particleBuffer);
		material.SetBuffer ("particleBuffer", particleBuffer);
	}

	void Update () 
	{
		computeShader.Dispatch(_kernel, dispatchCount, 1, 1);
		material.SetPass(0);
		Graphics.DrawMeshInstancedIndirect(mesh,0,material,bound,argsBuffer,0);
	}

	void OnRenderObject()
	{
		//material.SetPass(0);
		//Graphics.DrawMeshInstancedIndirect(mesh,0,material,bound,argsBuffer,0);
	}

	void OnDestroy()
	{
		CleanUp();
	}

    void OnDisable()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Release();
            particleBuffer = null;
        }

        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
}