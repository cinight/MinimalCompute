using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class ComputeParticlesDirect : ScriptableRendererFeature
{
	struct Particle
	{
		public Vector3 position;
	};

	public int count = 480000;
	public Material mat;
	public ComputeShader computeShader;
	public RenderPassEvent evt;

	private ComputeBuffer buffer;
	private Particle[] plists;
    
	public ComputeParticlesDirect()
	{
	}

	public override void Create()
	{
		// Init particles
		plists = new Particle[count];
		for (int i = 0; i < count; ++i)
		{
            plists[i].position = Random.insideUnitSphere * 4f;
        }
		
		//Set data to buffer
		OnDisable();
		buffer = new ComputeBuffer(count, 12); // 12 = sizeof(Particle)
		buffer.SetData(plists);
		
		//Set buffer to computeShader and Material
		computeShader.SetBuffer(0, "particleBuffer", buffer);
		mat.SetBuffer ("particleBuffer", buffer);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new ComputeParticlesDirectPass(evt,count,mat,computeShader,buffer);
        renderer.EnqueuePass(pass);
    }

    public void OnDisable()
    {
        //Clean up
        if (buffer != null)
        {
			buffer.Release();
            buffer = null;
        } 
    }

    //-------------------------------------------------------------------------

	class ComputeParticlesDirectPass : ScriptableRenderPass
	{
		private int count;
		private Material mat;
		private ComputeShader computeShader;
		private ComputeBuffer buffer;

        public ComputeParticlesDirectPass(RenderPassEvent renderPassEvent, int count,Material material,ComputeShader computeShader,ComputeBuffer buffer)
        {
            this.count = count;
            this.mat = material;
            this.renderPassEvent = renderPassEvent;
            this.computeShader = computeShader;
            this.buffer = buffer;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
			var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
			CommandBuffer cmd = CommandBufferPool.Get("ComputeParticlesDirectPass");
            cmd.SetRenderTarget(colorHandle);
			cmd.DispatchCompute(computeShader,0,Mathf.CeilToInt(count / 32),1,1);
			cmd.DrawProcedural(Matrix4x4.identity,mat,0,MeshTopology.Points,1,count);

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
    }
}
