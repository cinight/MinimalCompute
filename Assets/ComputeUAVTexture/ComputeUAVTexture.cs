using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexture : MonoBehaviour 
{

	public ComputeShader shader;

	private int size = 128;
	private int _kernel;
	public Material _mat;

	void Start () 
	{
		_kernel = shader.FindKernel ("CSMain");

		RenderTexture tex = new RenderTexture (size, size, 0);
		tex.enableRandomWrite = true;
		tex.Create ();
		
		_mat.SetTexture ("_MainTex", tex);
		_mat = GetComponent<Renderer> ().material;
		
		shader.SetTexture (_kernel, "Result", tex);
	}

	void Update()
	{
		shader.Dispatch (_kernel, Mathf.CeilToInt(size / 8f), Mathf.CeilToInt(size / 8f), 1);
	}
}
