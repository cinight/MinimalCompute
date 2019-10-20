using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexObstacle : MonoBehaviour 
{
	public ComputeShader shader;
	public Texture2D initTex;
	public Material _mat;
	public Collider mc;

	private int size;
	private int _kernel;

	//Initial texture
	// R = 1 is particle
	// G = speed
	// B = 1 is obstacle

    //Mouse input
    private Camera cam;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Vector2 defaultposition = new Vector2(-9, -9); //make it far away
	
	void Start () 
	{
		//For mouse input
        cam = Camera.main;

		size = initTex.width;
		_kernel = shader.FindKernel ("CSMain");

		RenderTexture tex = new RenderTexture (size, size, 0);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.filterMode = FilterMode.Point;
		tex.enableRandomWrite = true;
		tex.Create ();
		
		Graphics.Blit(initTex,tex);

		_mat.SetTexture ("_MainTex", tex);
		shader.SetTexture (_kernel, "Result", tex);
		shader.SetInt("_Size",size);
	}

	void Update()
	{
        //Getting mouse position. MeshCollider is needed for getting hit.textureCoord
        if (
            Input.GetMouseButton(0) &&
            Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) &&
            hit.collider == mc
        )
        {
            if (mousePos != hit.textureCoord) mousePos = hit.textureCoord;
        }
        else
        {
            if (mousePos != defaultposition) mousePos = defaultposition;
        }

        //Run compute shader
        shader.SetVector("_MousePos", mousePos);		
		shader.SetFloat("_Time",Time.time);
		shader.Dispatch (_kernel, Mathf.CeilToInt(size / 1f), Mathf.CeilToInt(size / 1f), 1);
	}
}
