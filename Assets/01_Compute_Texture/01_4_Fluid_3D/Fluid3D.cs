/* Reference
My code was originally based on: https://github.com/Scrawk/GPU-GEMS-2D-Fluid-Simulation
Nice tutorial understanding basic fluid concept: https://www.youtube.com/watch?v=iKAVRgIrUOU
Very nice tutorial for artists to understand the maths: https://shahriyarshahrabi.medium.com/gentle-introduction-to-fluid-simulation-for-programmers-and-technical-artists-7c0045c40bac
*/

using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Fluid3D : MonoBehaviour 
{
	public ComputeShader shader;
	public Material matResult;
	public int size = 256;
	public Transform sphere; //represents mouse
	public int solverIterations = 50;

	[Header("Force Settings")]
	public float forceIntensity = 200f;
	public float forceRange = 0.01f;
	private Vector3 sphere_prevPos = Vector3.zero;
	//public Color dyeColor = Color.white;

	private RenderTexture velocityTex;
	private RenderTexture densityTex;
	private RenderTexture pressureTex;
	private RenderTexture divergenceTex;

	private int dispatchSize = 0;
	private int kernel_Init = 0;
	private int kernel_Diffusion = 0;
	private int kernel_UserInput = 0;
	private int kernel_Jacobi = 0;
	private int kernel_Advection = 0;
	private int kernel_Divergence = 0;
	private int kernel_SubtractGradient = 0;

	private RenderTexture CreateTexture(GraphicsFormat format)
	{
		RenderTexture dataTex = new RenderTexture (size, size, format, 0);
		dataTex.volumeDepth = size;
		dataTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
		dataTex.filterMode = FilterMode.Bilinear;
		dataTex.wrapMode = TextureWrapMode.Clamp;
		dataTex.enableRandomWrite = true;
		dataTex.Create ();

		return dataTex;
	}

	private void DispatchCompute(int kernel)
	{
		shader.Dispatch (kernel, dispatchSize, dispatchSize, dispatchSize);
	}

	void Start () 
	{
		//Create textures
		velocityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 velocity , float unused
		densityTex = CreateTexture(GraphicsFormat.R16G16B16A16_SFloat); //float3 color , float density
		pressureTex = CreateTexture(GraphicsFormat.R16_SFloat); //float pressure
		divergenceTex = CreateTexture(GraphicsFormat.R16_SFloat); //float divergence

		//Output
		matResult.SetTexture ("_MainTex", densityTex);

		//Set shared variables for compute shader
		shader.SetInt("size",size);
		shader.SetFloat("forceIntensity",forceIntensity);
		shader.SetFloat("forceRange",forceRange);

		//Set texture for compute shader
		/* 
		This example is not optimized, some textures are readonly, 
		but I keep it like this for the sake of convenience
		*/
		kernel_Init = shader.FindKernel ("Kernel_Init");
		shader.SetTexture (kernel_Init, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Init, "DensityTex", densityTex);
		shader.SetTexture (kernel_Init, "PressureTex", pressureTex);
		shader.SetTexture (kernel_Init, "DivergenceTex", divergenceTex);

		kernel_Diffusion = shader.FindKernel ("Kernel_Diffusion");
		shader.SetTexture (kernel_Diffusion, "DensityTex", densityTex);

		kernel_Advection = shader.FindKernel ("Kernel_Advection");
		shader.SetTexture (kernel_Advection, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Advection, "DensityTex", densityTex);

		kernel_UserInput = shader.FindKernel ("Kernel_UserInput");
		shader.SetTexture (kernel_UserInput, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_UserInput, "DensityTex", densityTex);

		kernel_Divergence = shader.FindKernel ("Kernel_Divergence");
		shader.SetTexture (kernel_Divergence, "VelocityTex", velocityTex);
		shader.SetTexture (kernel_Divergence, "DivergenceTex", divergenceTex);

		kernel_Jacobi = shader.FindKernel ("Kernel_Jacobi");
		shader.SetTexture (kernel_Jacobi, "DivergenceTex", divergenceTex);
		shader.SetTexture (kernel_Jacobi, "PressureTex", pressureTex);

		kernel_SubtractGradient = shader.FindKernel ("Kernel_SubtractGradient");
		shader.SetTexture (kernel_SubtractGradient, "PressureTex", pressureTex);
		shader.SetTexture (kernel_SubtractGradient, "VelocityTex", velocityTex);

		//Init data texture value
		dispatchSize = Mathf.CeilToInt(size / 8);
		DispatchCompute (kernel_Init);
	}

	void FixedUpdate()
	{
		//Send sphere (mouse) position
		Vector3 npos = new Vector3( sphere.position.x / transform.lossyScale.x, sphere.position.y / transform.lossyScale.y, sphere.position.z / transform.lossyScale.z );
		shader.SetVector("spherePos",npos);

		//Send sphere (mouse) velocity
		Vector3 velocity = npos - sphere_prevPos;
		shader.SetVector("sphereVelocity",velocity);
		shader.SetFloat("_deltaTime", Time.fixedDeltaTime);
		shader.SetVector("dyeColor",SetSphereColor.color);

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
