using System;
using UnityEngine;

public class PixelPercentage : MonoBehaviour 
{
	public ComputeShader shader;
	public Material mat;
	public Collider mc;
	public int size = 128;
	public TextMesh textPercentage;
	
	private int _kernelDraw = 0;
	private int _kernelPercentage = 1;
	private Vector2Int dispatchCount;
	
	//The same struct in Shader
	struct PixelPercentageData
	{
		public uint filledCount;
		public uint processedCount;
	};
	private PixelPercentageData[] _data;
	private ComputeBuffer _computeBuffer;

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
        
        //Create texture
		RenderTexture tex = new RenderTexture (size, size, 0);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.filterMode = FilterMode.Point;
		tex.enableRandomWrite = true;
		tex.Create ();
		
		//Set texture to shader
		mat.SetTexture ("_MainTex", tex);
		shader.SetTexture (_kernelDraw, "Result", tex);
		shader.SetTexture (_kernelPercentage, "ResultForPercentage", tex);
		
		//Get dispatch size
        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        shader.GetKernelThreadGroupSizes(_kernelDraw, out threadX, out threadY, out threadZ);
		dispatchCount.x = Mathf.CeilToInt(size / threadX);
		dispatchCount.y = Mathf.CeilToInt(size / threadY);
		
		//Compute buffer for percentage
		_data = new PixelPercentageData[1];
		_data[0].filledCount = 0;
		_data[0].processedCount = 0;
		_computeBuffer = new ComputeBuffer(_data.Length,8); //(2)*4bytes in PixelPercentageData
		_computeBuffer.SetData(_data);
		shader.SetBuffer(_kernelPercentage, "data", _computeBuffer);
	}

	void Update()
	{
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			//Mouse input for draw area
			if( Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider == mc )
			{
				mousePos = hit.textureCoord;
			}
			
			//Draw
			if (Input.GetMouseButton(0))
			{
				mouseMode = 0;
			}
			//Erase
			else if (Input.GetMouseButton(1))
			{
				mouseMode = 1;
			}
		}
		else
		{
			mouseMode = -1;//Nothing
			mousePos = defaultposition;
		}

        //Run compute shader
		shader.SetInt("_MouseMode", mouseMode);	
        shader.SetVector("_MousePos", mousePos * size);
		shader.Dispatch (_kernelDraw,dispatchCount.x , dispatchCount.y, 1);
		shader.Dispatch (_kernelPercentage,dispatchCount.x , dispatchCount.y, 1);
		
		//Get data back from GPU to CPU
		_computeBuffer.GetData(_data);
		
		//Check if done processing
		if(_data[0].processedCount >= size*size)
		{
			//Update percentage
			float percentage = ((float)_data[0].filledCount / (size*size))*100f;
			textPercentage.text = String.Concat("Filled: ", string.Format("{0:0.00}",percentage), "%");
			
			//Reset
			_data[0].filledCount = 0;
			_data[0].processedCount = 0;
			_computeBuffer.SetData(_data);
		}
	}
	
	void OnDestroy()
	{
		_computeBuffer.Release();
	}
}
