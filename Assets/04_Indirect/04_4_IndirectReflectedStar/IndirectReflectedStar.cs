using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

[CreateAssetMenu]
public class IndirectReflectedStar : ScriptableRendererFeature
{
	public int maxCount = 10000;
	public Mesh mesh;
	public Material mat;
    public RenderPassEvent evt;

    private IndirectReflectedStarPass pass;
    private GraphicsBuffer cbDrawArgs;
    private GraphicsBuffer cbPoints;
    private int[] args;
    private bool reinit = false;
    
	public IndirectReflectedStar()
	{
        reinit = true;
	}

	public override void Create()
	{
        if(mesh == null)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(gameObject);
        }

        //args
        if(args == null)
        {
            args = new int[]
            {
                (int)mesh.GetIndexCount(0),
                1,
                (int)mesh.GetIndexStart(0),
                (int)mesh.GetBaseVertex(0),
                0
            };
        }

        //Create resources
        CleanUp();
        if (cbDrawArgs == null)
        {
            cbDrawArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, args.Length * 4); //each int is 4 bytes
            cbDrawArgs.SetData (args);
        }
        if (cbPoints == null)
        {
            cbPoints = new GraphicsBuffer (GraphicsBuffer.Target.Append, maxCount, 12); //pointBuffer is 3 floats so 3*4bytes = 12, see shader
            mat.SetBuffer ("pointBuffer", cbPoints); //Bind the buffer wwith material
        }
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
		if(reinit)
		{
			reinit = false;
			Create();
		}
        pass = new IndirectReflectedStarPass(evt, maxCount,mesh,mat,cbDrawArgs,cbPoints);
        renderer.EnqueuePass(pass);
    }

    public void CleanUp()
    {
        //Clean up
        if (cbDrawArgs != null)
        {
            cbDrawArgs.Release ();
            cbDrawArgs = null;
        } 
        if (cbPoints != null)
        {
            cbPoints.Release(); 
            cbPoints = null;
        }
    }

    public void OnDisable()
    {
		CleanUp();
		reinit = true;
    }

    //-------------------------------------------------------------------------

	class IndirectReflectedStarPass : ScriptableRenderPass
	{
        private int maxCount;
        private Mesh mesh;
        private Material mat;
        private Vector2Int size;

        private GraphicsBuffer cbDrawArgs;
        private GraphicsBuffer cbPoints;
        private int m_ColorRTid = Shader.PropertyToID("_CameraScreenTexture");
        private string passName;
        private ProfilingSampler sampler;

        public IndirectReflectedStarPass(RenderPassEvent renderPassEvent,int count,Mesh mesh,Material material,GraphicsBuffer cbDrawArgs,GraphicsBuffer cbPoints)
        {
            this.maxCount = count;
            this.mesh = mesh;
            this.mat = material;
            this.renderPassEvent = renderPassEvent;
            this.cbDrawArgs = cbDrawArgs;
            this.cbPoints = cbPoints;

            passName = GetType().Name;
            sampler = new ProfilingSampler(passName);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
            size = new Vector2Int(cameraTextureDescripor.width , cameraTextureDescripor.height);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            //Camera camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(passName);

            //This binds the buffer we want to store the filtered star positions
            //Match the id with shader
            cmd.SetRandomWriteTarget(1, cbPoints);
            cmd.GetTemporaryRT(m_ColorRTid,size.x,size.y,0);
            //This blit will send the screen texture to shader and do the filtering
            //If the pixel is bright enough we take the pixel position
            cmd.Blit(colorHandle ,m_ColorRTid, mat, 0);
            cmd.ClearRandomWriteTargets();
            cmd.ReleaseTemporaryRT(m_ColorRTid);
            cmd.SetRenderTarget(colorHandle);
            //Tells actually how many stars we need to draw
            //Copy the filtered star count to cbDrawArgs[1], which is at 4bytes int offset
            cmd.CopyCounterValue(cbPoints, cbDrawArgs, 4);
            //Draw the stars
            cmd.DrawMeshInstancedIndirect(mesh,0,mat,1,cbDrawArgs,0);

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

        internal class PassData_SetUAV
        {
            public GraphicsBuffer cbPoints;
            public GraphicsBuffer cbDrawArgs;
        }

        internal class PassData_BrightnessFilter
        {
            public GraphicsBuffer cbPoints;
            public GraphicsBuffer cbDrawArgs;
            public TextureHandle colorTex;
            public TextureHandle tempTex;
            public Material material;
        }

        internal class PassData_RenderParticles
        {
			public TextureHandle colorHandle;
            public Material material;
            public Mesh mesh;
            public GraphicsBuffer cbDrawArgs;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, FrameResources frameResources, ref RenderingData renderingData)
        {
            //Color handle
            var renderer = renderingData.cameraData.renderer as UniversalRenderer;
            TextureHandle colorTex = renderer.activeColorTexture;

            //Set RadomWriteTarget
			using (var builder = renderGraph.AddLowLevelPass<PassData_SetUAV>(passName+"_SetUAV", out var passData, sampler))
			{
				//Make sure the pass will not be culled
				builder.AllowPassCulling(false);

                //Setup passData
                passData.cbPoints = cbPoints;

				//Render function
				builder.SetRenderFunc((PassData_SetUAV data, LowLevelGraphContext rgContext) =>
				{
					rgContext.legacyCmd.SetRandomWriteTarget(1, cbPoints); //match the id with shader
				});
			}
            
            //Get temp RT
            RenderTextureDescriptor rtDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            TextureDesc destinationDescriptor = new TextureDesc(rtDescriptor.width, rtDescriptor.height, false, false)
            {
                colorFormat = rtDescriptor.graphicsFormat, 
                name = "_CameraScreenTexture"
            };
            TextureHandle tempTex = renderGraph.CreateTexture(destinationDescriptor);

            //Brightness filter - don't do with AddRasterRenderPass because we are not really writing to a texture, but to the append buffer
            using (var builder = renderGraph.AddLowLevelPass<PassData_BrightnessFilter>(passName+"_BrightnessFilter", out var passData, sampler))
            {
                //Make sure the pass will not be culled
                builder.AllowPassCulling(false);

                //Setup passData
                passData.cbPoints = cbPoints;//builder.UseBuffer(cbPointsHandle, IBaseRenderGraphBuilder.AccessFlags.Read);
                passData.cbDrawArgs = cbDrawArgs;//builder.UseBuffer(cbDrawArgsHandle, IBaseRenderGraphBuilder.AccessFlags.Write);
                passData.material = mat;
                passData.colorTex = builder.UseTexture(colorTex, IBaseRenderGraphBuilder.AccessFlags.Read);
                passData.tempTex = builder.UseTexture(tempTex, IBaseRenderGraphBuilder.AccessFlags.Write);

                //Render function
                builder.SetRenderFunc((PassData_BrightnessFilter data, LowLevelGraphContext rgContext) =>
                {
                    rgContext.legacyCmd.Blit(data.colorTex ,data.tempTex, data.material, 0);
                    rgContext.legacyCmd.ClearRandomWriteTargets();
                    rgContext.legacyCmd.CopyCounterValue(data.cbPoints, data.cbDrawArgs, 4);
                });
            }

            //Render stars
            using (var builder = renderGraph.AddRasterRenderPass<PassData_RenderParticles>(passName+"_RenderParticles", out var passData, sampler))
            {   
				//Make sure the pass will not be culled
				builder.AllowPassCulling(false);
                
                //Setup passData
                passData.colorHandle = builder.UseTextureFragment(colorTex, 0, IBaseRenderGraphBuilder.AccessFlags.Write);
                passData.material = mat;
                passData.mesh = mesh;
                passData.cbDrawArgs = cbDrawArgs;

                //Render function
                builder.SetRenderFunc((PassData_RenderParticles data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawMeshInstancedIndirect(data.mesh,0,data.material,1,data.cbDrawArgs,0);
                });
            }

            //Can debug by reading the count after filtering
            /*
            Array data = new int[5];
            cbDrawArgs.GetData(data);
            string t = "";
            for(int i = 0; i < data.Length; i++)
            {
                t+=data.GetValue(i)+" ---- ";
            }
            Debug.Log(t);
            */
        }
    }
}