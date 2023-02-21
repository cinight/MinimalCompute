using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeUAVTexFlow : MonoBehaviour 
{
	public ComputeShader shader;
	public Texture2D initTex;
	public Material _mat;
	public Collider mc;

	private int size;
	private int _kernel;
	private Vector2Int dispatchCount;

	//Initial texture
	// R = 1 is particle
	// G = no meaning now
	// B = 1 is obstacle

    //Mouse input
    private Camera cam;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Vector2 defaultposition = new Vector2(-9, -9); //make it far away
	private int mouseMode = 0;
	
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

        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        shader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
		dispatchCount.x = Mathf.CeilToInt(size / threadX);
		dispatchCount.y = Mathf.CeilToInt(size / threadY);
	}

	void Update()
	{
        //Getting mouse position. MeshCollider is needed for getting hit.textureCoord
        if ( Input.GetMouseButton(0) || Input.GetMouseButton(1) )
		{
			if( Input.GetMouseButton(0) && Input.GetMouseButton(1) ) mouseMode = 1; //1=drawobstacle
			else if( Input.GetMouseButton(0) && !Input.GetMouseButton(1) ) mouseMode = 0; //0=drawpixel
			else if( !Input.GetMouseButton(0) && Input.GetMouseButton(1) ) mouseMode = 2; //2=removeobstacle

			if( Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider == mc )
			{
				if (mousePos != hit.textureCoord) mousePos = hit.textureCoord;
			}
			else
			{
				if (mousePos != defaultposition) mousePos = defaultposition;
			}
		}
        else
        {
            if (mousePos != defaultposition) mousePos = defaultposition;
        }

        //Run compute shader
		shader.SetInt("_MouseMode", mouseMode);	
        shader.SetVector("_MousePos", mousePos);		
		shader.SetFloat("_Time",Time.time);
		shader.Dispatch (_kernel,dispatchCount.x , dispatchCount.y, 1);
	}
}
