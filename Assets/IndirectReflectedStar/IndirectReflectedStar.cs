using UnityEngine;
using UnityEngine.Rendering;

public class IndirectReflectedStar : MonoBehaviour 
{
    struct Particle
    {
        public Vector2Int uv;
        public float intensity;
    };

	public int particleCount = 10;
    public float threshold = 1f;
	public Material material;
    public Mesh mesh;
	public ComputeShader computeShader;

	private int _kernelDirect;
	private ComputeBuffer particleBuffer;
	private ComputeBuffer particleFilteredResultBuffer;
	private ComputeBuffer argsBuffer;
	private int[] args;
	//private Particle[] plists;
    private CommandBuffer cmd;

	void Start ()
	{
		//just to make sure the buffer are clean
		release();
		
		//kernels
        _kernelDirect = computeShader.FindKernel("CSMainDirect");
		
		// Init particles position
		// plists = new Particle[particleCount];
		// for (int i = 0; i < particleCount; ++i)
		// {
		// 	plists[i].uv = Vector2Int.zero;
        //     plists[i].intensity = 1;
        // }
		
		//particleBuffer, for rendering
		//particleBuffer = new ComputeBuffer(particleCount, 12); // 2*int+1*float, 4 bytes each = sizeof(Particle)
		//particleBuffer.SetData(plists);

		particleFilteredResultBuffer = new ComputeBuffer(particleCount, 12, ComputeBufferType.Append);
		material.SetBuffer ("particleBuffer", particleFilteredResultBuffer);

		//Args for indirect draw
		args = new int[]
		{
			(int)mesh.GetIndexCount(0),
			(int)particleCount, //instance count
			(int)mesh.GetIndexStart(0),
			(int)mesh.GetBaseVertex(0),
            0
		};
		argsBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        //CommandBuffer
        cmd = new CommandBuffer();
        cmd.name = "Reflective star";
        cmd.SetComputeTextureParam(computeShader,_kernelDirect,"_CameraRenderTexture",BuiltinRenderTextureType.CameraTarget);
        cmd.SetComputeFloatParam(computeShader,"_Threshold",threshold);
        cmd.SetComputeBufferParam(computeShader,_kernelDirect,"particleFiltered", particleFilteredResultBuffer);
        cmd.DispatchCompute(computeShader,_kernelDirect,Mathf.CeilToInt(1920f / 8f), Mathf.CeilToInt(1080f / 8f), 1);
        cmd.CopyCounterValue(particleFilteredResultBuffer, argsBuffer, 4);
        cmd.DrawMeshInstancedIndirect(mesh,0,material,0,argsBuffer,0);
        Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha,cmd);
	}

	void Update () 
	{
		//Reset count
		particleFilteredResultBuffer.SetCounterValue(0);

		//Direct dispatch to do filter
       // computeShader.SetFloat("_Threshold",threshold);
        //computeShader.SetTexture(_kernelDirect, "screenTexture", tex);//////////////////////////////////////////
		//computeShader.Dispatch(_kernelDirect, Mathf.CeilToInt(tex.width / 8f), Mathf.CeilToInt(tex.height / 8f), 1);

		//Copy Count
		//ComputeBuffer.CopyCount(particleFilteredResultBuffer, argsBuffer, 4);
	}

    //void Update()
    //{
		//Draw
		//Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles,argsBuffer, 0);
    //}

    // void OnRenderImage (RenderTexture src, RenderTexture dst)
    // {
    //     Graphics.Blit(src,dst);

    //     //Run compute
    //     RunCompute (src);
    // }

    private void release()
    {
        if (particleFilteredResultBuffer != null)
        {
            particleFilteredResultBuffer.Dispose();
            particleFilteredResultBuffer.Release();
            particleFilteredResultBuffer = null;
        }
        // if (particleBuffer != null)
        // {
        //     particleBuffer.Dispose();
        //     particleBuffer.Release();
        //     particleBuffer = null;
        // }
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
