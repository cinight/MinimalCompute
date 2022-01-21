using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class IndirectReflectedStar : ScriptableRendererFeature
{
	public int maxCount = 10000;
	public Mesh mesh;
	public Material mat;
    public RenderPassEvent evt;

    private IndirectReflectedStarPass pass;
    private ComputeBuffer cbDrawArgs;
    private ComputeBuffer cbPoints;
    private int[] args;
    
	public IndirectReflectedStar()
	{
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
        OnDisable();
        if (cbDrawArgs == null)
        {
            cbDrawArgs = new ComputeBuffer (1, args.Length * 4, ComputeBufferType.IndirectArguments); //each int is 4 bytes
            cbDrawArgs.SetData (args);
        }
        if (cbPoints == null)
        {
            cbPoints = new ComputeBuffer (maxCount, 12, ComputeBufferType.Append); //pointBuffer is 3 floats so 3*4bytes = 12, see shader
            mat.SetBuffer ("pointBuffer", cbPoints); //Bind the buffer wwith material
        }
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass = new IndirectReflectedStarPass(evt, maxCount,mesh,mat,cbDrawArgs,cbPoints);
        renderer.EnqueuePass(pass);
    }

    public void OnDisable()
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

    //-------------------------------------------------------------------------

	class IndirectReflectedStarPass : ScriptableRenderPass
	{
        private int maxCount;
        private Mesh mesh;
        private Material mat;
        private Vector2Int size;

        private ComputeBuffer cbDrawArgs;
        private ComputeBuffer cbPoints;
        private int m_ColorRTid = Shader.PropertyToID("_CameraScreenTexture");

        public IndirectReflectedStarPass(RenderPassEvent renderPassEvent,int count,Mesh mesh,Material material,ComputeBuffer cbDrawArgs,ComputeBuffer cbPoints)
        {
            this.maxCount = count;
            this.mesh = mesh;
            this.mat = material;
            this.renderPassEvent = renderPassEvent;
            this.cbDrawArgs = cbDrawArgs;
            this.cbPoints = cbPoints;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
            size = new Vector2Int(cameraTextureDescripor.width , cameraTextureDescripor.height);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            //Camera camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get("IndirectReflectedStarPass");

            //This binds the buffer we want to store the filtered star positions
            //Match the id with shader
            cmd.SetRandomWriteTarget(1, cbPoints);
            cmd.GetTemporaryRT(m_ColorRTid,size.x,size.y,24);
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
    }
}