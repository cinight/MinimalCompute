using UnityEngine;
using UnityEngine.Rendering;

public class DrawIndirect : MonoBehaviour
{
	public int maxCount = 10000;
	public Mesh mesh;
	public Material mat;
	private ComputeBuffer cbDrawArgs;
	private ComputeBuffer cbPoints;
	private CommandBuffer cmd;

	void Start()
	{
		Camera cam = Camera.main;
		int m_ColorRTid = Shader.PropertyToID("_CameraScreenTexture");

		//Create resources
		if (cbDrawArgs == null)
		{
			var args = new int[]
			{
				(int)mesh.GetIndexCount(0),
				1,
				(int)mesh.GetIndexStart(0),
				(int)mesh.GetBaseVertex(0),
				0
			};
			cbDrawArgs = new ComputeBuffer (1, args.Length * 4, ComputeBufferType.IndirectArguments); //each int is 4 bytes
			cbDrawArgs.SetData (args);
		}
		if (cbPoints == null)
		{
			cbPoints = new ComputeBuffer (maxCount, 12, ComputeBufferType.Append); //pointBuffer is 3 floats so 3*4bytes = 12, see shader
			mat.SetBuffer ("pointBuffer", cbPoints); //Bind the buffer wwith material
		}

        //The following workflow method comes from Aras's DX11Examples _DrawProceduralIndirect scene
        cmd = new CommandBuffer();
        cmd.name = "Reflective star";
		//This binds the buffer we want to store the filtered star positions
		//Match the id with shader
		cmd.SetRandomWriteTarget(1, cbPoints);
		cmd.GetTemporaryRT(m_ColorRTid,cam.pixelWidth,cam.pixelHeight,24);
		//This blit will send the screen texture to shader and do the filtering
		//If the pixel is bright enough we take the pixel position
		cmd.Blit(BuiltinRenderTextureType.CameraTarget,m_ColorRTid,mat, 0);
		cmd.ClearRandomWriteTargets();
		cmd.ReleaseTemporaryRT(m_ColorRTid);
		cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		//Tells actually how many stars we need to draw
		//Copy the filtered star count to cbDrawArgs[1], which is at 4bytes int offset
		cmd.CopyCounterValue(cbPoints, cbDrawArgs, 4);
		//Draw the stars
		cmd.DrawMeshInstancedIndirect(mesh,0,mat,1,cbDrawArgs,0);
		Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque,cmd);
	}

	private void ReleaseResources ()
	{
		if (cbDrawArgs != null) cbDrawArgs.Release (); cbDrawArgs = null;
		if (cbPoints != null) cbPoints.Release(); cbPoints = null;
	}
	
	void OnDisable ()
	{
		ReleaseResources ();
	}
}