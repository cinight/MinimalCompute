using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ComputePaintTexture : MonoBehaviour 
{
	public ComputeShader shader;
	public Material _mat;
	public int size = 128;
	public Transform[] spheres;
	public int drawType = 0;

    struct Particle
    {
		public Vector3 prevposition;
        public Vector3 position;
    };
	private Particle[] particleArray;
	private int _kernel;
	private ComputeBuffer cBuffer;
	private bool firstTime = true;

	void Start () 
	{
		_kernel = shader.FindKernel ("CSMain");

		//Quad texture
		RenderTexture tex = new RenderTexture (size, size, 0, GraphicsFormat.R32G32B32A32_SFloat);
		tex.enableRandomWrite = true;
		tex.Create ();
		_mat.SetTexture ("_MainTex", tex);
		shader.SetTexture (_kernel, "Result", tex);
		shader.SetInt("size",size);

		//Spheres
        particleArray = new Particle[spheres.Length];
        cBuffer = new ComputeBuffer(particleArray.Length, 12 + 12 );
        cBuffer.SetData(particleArray);
		shader.SetBuffer(_kernel,"particleBuffer",cBuffer);
		shader.SetInt("particleCount",particleArray.Length);

		//draw type
		switch(drawType)
		{
			case 1: 
				shader.EnableKeyword("CURVEDSTAIGHTLINE");
				shader.DisableKeyword("DRAWLINE");
				shader.DisableKeyword("CIRCLES");
				break;

			case 2: 
				shader.EnableKeyword("CIRCLES");
				shader.DisableKeyword("DRAWLINE");
				shader.DisableKeyword("CURVEDSTAIGHTLINE");
				break;

			default:
				shader.EnableKeyword("DRAWLINE");
				shader.DisableKeyword("CURVEDSTAIGHTLINE");
				shader.DisableKeyword("CIRCLES");
				break;
		}
	}

	void Update()
	{
        for (int i = 0; i < spheres.Length; ++i)
        {
			Vector3 npos = spheres[i].position;
			npos.y = 0f;

			if(firstTime)
			{
				particleArray[i].prevposition = npos;
				firstTime = false;
			}
			else
			{
				particleArray[i].prevposition = particleArray[i].position;
			}
            particleArray[i].position = npos;
        }
		cBuffer.SetData(particleArray);
		shader.Dispatch (_kernel, Mathf.CeilToInt(size / 8f), Mathf.CeilToInt(size / 8f), 1);
	}

    void OnDestroy()
    {
        cBuffer.Release();
    }
}
