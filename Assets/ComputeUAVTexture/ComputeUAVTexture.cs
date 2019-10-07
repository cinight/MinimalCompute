using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexture : MonoBehaviour 
{

	public ComputeShader shader;

	private int _kernel;
	private Material _mat;

	void Start () 
	{
		_kernel = shader.FindKernel ("CSMain");
		_mat = GetComponent<Renderer> ().material;
		RunShader ();
	}
		
	public void RunShader()
	{
		RenderTexture tex = new RenderTexture (512, 512, 24);
		tex.enableRandomWrite = true;
		tex.Create ();

		shader.SetTexture (_kernel, "Result", tex);
		shader.Dispatch (_kernel, 512 / 8, 512 / 8, 1);

		_mat.SetTexture ("_MainTex", tex);

	}
}
