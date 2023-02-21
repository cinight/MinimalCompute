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
	public float sampleDistance = 0.01f;
	private Vector2 drawPosition;
	public List<Vector2> drawingPositions;
	
	void Start () 
	{
		customButton = new GUIStyle("button");
		customButton.fontSize = 28;

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

					//Add position to list
					float dist = 1f;
					if(drawingPositions.Count > 0)
					{
						//if distance is same as last recorded position, then we skip it
						dist = Vector2.Distance(drawPosition,drawingPositions[drawingPositions.Count-1]);
					}
					if(dist > sampleDistance)
					{
						drawingPositions.Add(drawPosition);
					}
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

        //Run compute shader
		shader.SetVector("_MousePos", mousePos);
		shader.Dispatch (_kernel,dispatchCount.x , dispatchCount.y, 1);
	}

	//Signal should be distributed according to time, i.e. evenly distributed
	public void MakeSampledPositionEvenlySpaced()
	{	
		List<Vector2> tempList = new List<Vector2>(drawingPositions);

		//Get the length of the path
		float totalLengthOfPath = 0f;
		for(int i=0; i<drawingPositions.Count-1; i++)
		{
			totalLengthOfPath += Vector2.Distance(drawingPositions[i],drawingPositions[i+1]);
		}

		//Adjust the positions according to spacing they should have
		float spacing = totalLengthOfPath / (drawingPositions.Count-1);
		int currentSegmentStart = 0;
		for(int i=1; i<drawingPositions.Count-1; i++) //won't move the first and last one
		{
			Vector2 from = tempList[i-1];
			Vector2 to = drawingPositions[currentSegmentStart+1];
			float currentSegmentDistance = Vector2.Distance(from,to);
			float moveDistance = spacing;
			
			//Find which segment the position should ly on
			while(moveDistance > currentSegmentDistance)
			{
				moveDistance -= currentSegmentDistance;
				currentSegmentStart ++;
				from = drawingPositions[currentSegmentStart];
				to = drawingPositions[currentSegmentStart+1];
				currentSegmentDistance = Vector2.Distance(from,to);
			}

			//Move the position to the evened space
			float blend = moveDistance/currentSegmentDistance;
			tempList[i] = Vector2.Lerp(from, to, blend);
		}

		drawingPositions = tempList;
	}

	private GUIStyle customButton;
	void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Redraw", customButton))
        {
            Restart ();
        }
    }
	
	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Vector3 offset = Vector3.zero;
		offset.x = transform.localPosition.x / transform.localScale.x;
		offset.y = transform.localPosition.y / transform.localScale.y;
		offset.z = transform.localPosition.z / transform.localScale.z;

		for(int i=0; i<drawingPositions.Count;i++)
		{
			Vector3 pos = drawingPositions[i];
			pos += offset;
			pos = transform.TransformPoint(pos) * 0.5f;
			pos.y = 0f;
			Gizmos.DrawSphere(pos, 0.1f);
		}
		
	}
}
