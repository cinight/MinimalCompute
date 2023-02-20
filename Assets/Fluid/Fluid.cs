/* Reference
My code was originally based on: https://github.com/Scrawk/GPU-GEMS-2D-Fluid-Simulation
Nice tutorial understanding basic fluid concept: https://www.youtube.com/watch?v=iKAVRgIrUOU
Very nice tutorial for artists to understand the maths: https://shahriyarshahrabi.medium.com/gentle-introduction-to-fluid-simulation-for-programmers-and-technical-artists-7c0045c40bac
*/

using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Fluid : MonoBehaviour 
{
	public ComputeShader shader;
	public Material matResult;
	public int size = 1024;
	public Transform sphere; //represents mouse
	public int solverIterations = 50;
	public Texture2D obstacleTex;

	[Header("Force Settings")]
	public float forceIntensity = 200f;
	public float forceRange = 0.01f;
	private Vector2 sphere_prevPos = Vector3.zero;
	public Color dyeColor = Color.white;

	private RenderTexture velocityTex;
	private RenderTexture densityTex;
	private RenderTexture pressureTex;
	private RenderTexture divergenceTex;

	private int dispatchSize = 0;
	private int kernelCount = 0;
	private int kernel_Init = 0;
	private int kernel_Diffusion = 0;
	private int kernel_UserInput = 0;
	private int kernel_Jacobi = 0;
	private int kernel_Advection = 0;
	private int kernel_Divergence = 0;
	private int kernel_SubtractGradient = 0;

	private RenderTexture CreateTexture()
	{
		/* 
		This example is not optimized, some texture channels are unused.
		You can probably either pack the data and utilize the channels thus use fewer textures,
		or specific a more suitable GraphicsFormat for each texture according to what value they holds
		*/
		RenderTexture dataTex = new RenderTexture (size, size, 0, GraphicsFormat.R32G32B32A32_SFloat);
		dataTex.filterMode = FilterMode.Bilinear;
		dataTex.wrapMode = TextureWrapMode.Clamp;
		dataTex.enableRandomWrite = true;
		dataTex.Create ();

		return dataTex;
	}

	private void DispatchCompute(int kernel)
	{
		shader.Dispatch (kernel, dispatchSize, dispatchSize, 1);
	}

	void Start () 
	{
		//Create textures
		velocityTex = CreateTexture();
		densityTex = CreateTexture();
		pressureTex = CreateTexture();
		divergenceTex = CreateTexture();

		//Output
		matResult.SetTexture ("_MainTex", densityTex);

		//Set shared variables for compute shader
		shader.SetInt("size",size);
		shader.SetFloat("forceIntensity",forceIntensity);
		shader.SetFloat("forceRange",forceRange);

		//Set texture for compute shader
		kernel_Init = shader.FindKernel ("Kernel_Init"); kernelCount++;
		kernel_Diffusion = shader.FindKernel ("Kernel_Diffusion"); kernelCount++;
		kernel_UserInput = shader.FindKernel ("Kernel_UserInput"); kernelCount++;
		kernel_Divergence = shader.FindKernel ("Kernel_Divergence"); kernelCount++;
		kernel_Jacobi = shader.FindKernel ("Kernel_Jacobi"); kernelCount++;
		kernel_Advection = shader.FindKernel ("Kernel_Advection"); kernelCount++;
		kernel_SubtractGradient = shader.FindKernel ("Kernel_SubtractGradient"); kernelCount++;
		for(int kernel=0; kernel<kernelCount; kernel++)
		{
			/* 
			This example is not optimized, not all kernels read/write into all textures,
			but I keep it like this for the sake of convenience
			*/
			shader.SetTexture (kernel, "VelocityTex", velocityTex);
			shader.SetTexture (kernel, "DensityTex", densityTex);
			shader.SetTexture (kernel, "PressureTex", pressureTex);
			shader.SetTexture (kernel, "DivergenceTex", divergenceTex);
			shader.SetTexture (kernel, "ObstacleTex", obstacleTex);
		}

		//Init data texture value
		dispatchSize = Mathf.CeilToInt(size / 16);
		DispatchCompute (kernel_Init);
	}

	void FixedUpdate()
	{
		//Send sphere (mouse) position
		Vector2 npos = new Vector2( sphere.position.x / transform.localScale.x, sphere.position.z / transform.localScale.z );
		shader.SetVector("spherePos",npos);

		//Send sphere (mouse) velocity
		Vector2 velocity = npos - sphere_prevPos;
		shader.SetVector("sphereVelocity",velocity);
		shader.SetFloat("_deltaTime", Time.fixedDeltaTime);
		shader.SetVector("dyeColor",dyeColor);

		//Run compute shader
		DispatchCompute (kernel_Diffusion);
		DispatchCompute (kernel_Advection);
		DispatchCompute (kernel_UserInput);
		DispatchCompute (kernel_Divergence);
		for(int i=0; i<solverIterations; i++)
		{
			DispatchCompute (kernel_Jacobi);
		}
		DispatchCompute (kernel_SubtractGradient);
		
		//Save the previous position for velocity
		sphere_prevPos = npos;
	}
}
