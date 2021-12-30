using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadDraw : MonoBehaviour 
{
	public ComputeShader shader;
	public Texture2D initTex;
	public Material _mat;
	public Collider mc;

	private int size;
	private int _kernel;
	private Vector2Int dispatchCount;
	private RenderTexture tex;

    //Mouse input
    private Camera cam;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Vector2 defaultposition = new Vector2(-90, -90); //make it far away

	//Position list for FT
	public float sampleInterval = 0.05f;
	public float sampleDistance = 0.01f;
	private float sampleCounter = 0f;
	private Vector2 drawPosition;
	public List<Vector2> drawingPositions;
	
	void Start () 
	{
		//For mouse input
        cam = Camera.main;

		size = initTex.width;
		_kernel = shader.FindKernel ("CSMain");

		tex = new RenderTexture (size, size, 0);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.filterMode = FilterMode.Point;
		tex.enableRandomWrite = true;
		tex.Create ();

		_mat.SetTexture ("_MainTex", tex);
		shader.SetTexture (_kernel, "Result", tex);
		shader.SetInt("_Size",size);

        uint threadX = 0;
        uint threadY = 0;
        uint threadZ = 0;
        shader.GetKernelThreadGroupSizes(_kernel, out threadX, out threadY, out threadZ);
		dispatchCount.x = Mathf.CeilToInt(size / threadX);
		dispatchCount.y = Mathf.CeilToInt(size / threadY);

		drawingPositions = new List<Vector2>();

		Restart();
	}

	void Restart ()
	{
		drawingPositions.Clear();
		Graphics.Blit(initTex,tex);
	}

	void Update()
	{
        //Getting mouse position. MeshCollider is needed for getting hit.textureCoord
        if ( Input.GetMouseButton(0) || Input.GetMouseButton(1) )
		{
			if( Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider == mc )
			{
				if (mousePos != hit.textureCoord)
				{
					mousePos = hit.textureCoord;
					drawPosition = (mousePos - new Vector2(0.5f,0.5f))*2.0f; //range -1 to 1
				}
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

		//Add position to list
		float dist = 1f;
		if(drawingPositions.Count > 0)
		{
			//if distance is same as last recorded position, then we skip it
			dist = Vector2.Distance(drawPosition,drawingPositions[drawingPositions.Count-1]);
		}
		if(sampleCounter >= sampleInterval && dist > sampleDistance)
		{
			drawingPositions.Add(drawPosition);
			sampleCounter = 0f;
		}
		sampleCounter += Time.deltaTime;

        //Run compute shader
		shader.SetVector("_MousePos", mousePos);
		shader.Dispatch (_kernel,dispatchCount.x , dispatchCount.y, 1);
	}

	void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Redraw"))
        {
            Restart ();
        }
    }
}
