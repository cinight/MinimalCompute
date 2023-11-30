using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
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

	private GraphicsBuffer buffer;
	private Particle[] plists;
	private bool reinit = false;
	public string bufferName = "particleBuffer";
    
	public ComputeParticlesDirect()
	{
		reinit = true;
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
		CleanUp();
		buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, 12); // 12 = sizeof(Particle)
		buffer.SetData(plists);
		
		//Set buffer to computeShader and Material
		computeShader.SetBuffer(0, bufferName, buffer);
		mat.SetBuffer (bufferName, buffer);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
		if(reinit)
		{
			reinit = false;
			Create();
		}
		
        var pass = new ComputeParticlesDirectPass(evt,count,mat,computeShader,buffer);
        renderer.EnqueuePass(pass);
    }

	private void CleanUp()
	{
        if (buffer != null)
        {
			buffer.Release();
            buffer = null;
        }
	}

    public void OnDisable()
    {
		CleanUp();
		reinit = true;
    }

    //-------------------------------------------------------------------------

	class ComputeParticlesDirectPass : ScriptableRenderPass
	{
		private int count;
		private Material mat;
		private ComputeShader computeShader;
		private GraphicsBuffer buffer;
		private string passName;
		private ProfilingSampler sampler;

        public ComputeParticlesDirectPass(RenderPassEvent renderPassEvent, int count,Material material,ComputeShader computeShader,GraphicsBuffer buffer)
        {
            this.count = count;
            this.mat = material;
            this.renderPassEvent = renderPassEvent;
            this.computeShader = computeShader;
            this.buffer = buffer;

			passName = GetType().Name;
			sampler = new ProfilingSampler(passName);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
			var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
			CommandBuffer cmd = CommandBufferPool.Get(passName);
            cmd.SetRenderTarget(colorHandle);
			cmd.DispatchCompute(computeShader,0,Mathf.CeilToInt(count / 32),1,1);
			cmd.DrawProcedural(Matrix4x4.identity,mat,0,MeshTopology.Points,1,count);

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

        #region RenderGraph
        internal class PassData
        {
        }
        
       public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
       {
	       UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
	       
	       //shouldn't blit from the backbuffer
	       if (resourceData.isActiveTargetBackBuffer)
		       return;
	       
			//Dispatch compute
			using (var builder = renderGraph.AddComputePass<PassData>(passName+"_DispatchCompute", out var passData, sampler))
			{
				//The compute will be culled because attachment dimensions is 0x0x0, so here we make sure it is not culled
				builder.AllowPassCulling(false);

				//Render function
				builder.SetRenderFunc((PassData data, ComputeGraphContext rgContext) =>
				{
					rgContext.cmd.DispatchCompute(computeShader,0,Mathf.CeilToInt(count / 32),1,1);
				});
			}

			//Render particles
			TextureHandle colorHandle = resourceData.activeColorTexture;
			
			//To avoid error from material preview in the scene
			if(!colorHandle.IsValid())
				return;

			using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName+"_RenderParticles", out var passData, sampler))
			{   
				//Setup builder
				builder.SetRenderAttachment(colorHandle, 0, AccessFlags.Write);

				//Render function
				builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
				{
					rgContext.cmd.DrawProcedural(Matrix4x4.identity,mat,0,MeshTopology.Points,1,count);
				});
			}
	   }
       #endregion
    }
}
