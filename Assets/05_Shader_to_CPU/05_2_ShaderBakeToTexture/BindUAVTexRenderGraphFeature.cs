using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

[CreateAssetMenu]
public class BindUAVTexRenderGraphFeature : ScriptableRendererFeature
{
    private BindUAVTexRenderGraphPass pass;

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass = new BindUAVTexRenderGraphPass();
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	class BindUAVTexRenderGraphPass : ScriptableRenderPass
	{
        private ProfilingSampler sampler;
        private static int targetID = 6; //match with shader "register(u6)"
        private static string passName = "BindUAVTexRenderGraphPass";

        public BindUAVTexRenderGraphPass()
        {
            this.renderPassEvent = RenderPassEvent.BeforeRendering;
            sampler = new ProfilingSampler(passName);
        }

        internal class PassData
        {
            public RenderTargetIdentifier texId;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            RenderTexture tex = ShaderBakeToTexture.tex;
            if (tex == null || !tex.IsCreated()) return;
            RenderTargetIdentifier texId = new RenderTargetIdentifier(tex);
            
            //Set RadomWriteTarget
            using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData, sampler))
            {
                //Make sure the pass will not be culled
                builder.AllowPassCulling(false);

                //Setup passData
                passData.texId = texId;

                //Render function
                builder.SetRenderFunc((PassData data, UnsafeGraphContext rgContext) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
                    cmd.ClearRandomWriteTargets();
                    cmd.SetRandomWriteTarget(targetID, data.texId);
                });
            }
        }

    }
}