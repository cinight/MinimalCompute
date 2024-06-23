using System;
using UnityEngine;

public class ColorPalette : MonoBehaviour 
{
	public ComputeShader shader;
	public Material matPalette;
	public Material matResult;
	public Texture2D originalTexture;
	public Renderer[] palettes;
	public Renderer[] palettesReplace;
	public Color[] colorNew;

	//Settings
	[Range(3, 10)] public int colorLevel = 3;
	[Range(0.1f, 3f)] public float mergeThreshold = 0.1f;
	
	private Vector2 _size;
	private int _kernelPixelate;
	private int _kernelExtractColor;
	private int _kernelFrequency;
	private int _kernelReplace;
	private Vector2Int _dispatchSize;
	
	struct ColorData
	{
		public Vector4 color;
		public Vector3 hsv;
		public uint frequency;
	};
	private const int _maxColorCount = 100; //max colorLevel * colorLevel
	private ColorData[] _colorList = new ColorData[_maxColorCount];
	private ComputeBuffer cBuffer;
	private Vector4[] _tempNewColor;
	private bool firstTime = true;
	

	void Start () 
	{
		colorNew = new Color[palettesReplace.Length];
		_tempNewColor = new Vector4[palettesReplace.Length];
		
		_kernelPixelate = shader.FindKernel ("CSMainPixelate");
		_kernelExtractColor = shader.FindKernel ("CSMainExtractColor");
		_kernelFrequency = shader.FindKernel ("CSMainFrequency");
		_kernelReplace = shader.FindKernel ("CSMainReplace");

		_size.x = originalTexture.width;
		_size.y = originalTexture.height;
		_dispatchSize.x = Mathf.CeilToInt(_size.x / 8f);
		_dispatchSize.y = Mathf.CeilToInt(_size.y / 8f);
		
		RenderTexture texPalette = new RenderTexture ((int)_size.x, (int)_size.y, 0);
		texPalette.enableRandomWrite = true;
		texPalette.Create ();
		
		RenderTexture texResult = new RenderTexture ((int)_size.x, (int)_size.y, 0);
		texResult.enableRandomWrite = true;
		texResult.Create ();
		
		//Set general properties
		matPalette.SetTexture ("_MainTex", texPalette);
		matResult.SetTexture ("_MainTex", texResult);
		shader.SetVector("_Size",_size);
		
		//For kernel 0
		shader.SetTexture (_kernelPixelate, "Palette", texPalette);
		shader.SetTexture (_kernelPixelate, "Original", originalTexture);
		
		//For kernel 1
		cBuffer = new ComputeBuffer(_maxColorCount, 32); // 4*4bytes + 3*4bytes+ 4bytes
		cBuffer.SetData(_colorList);
		shader.SetBuffer(_kernelExtractColor, "ColorList", cBuffer);
		shader.SetTexture (_kernelExtractColor, "Pixelated", texPalette);
		
		//For kernel 2
		shader.SetTexture (_kernelFrequency, "Palette", texPalette);
		shader.SetTexture (_kernelFrequency, "Original", originalTexture);
		shader.SetBuffer(_kernelFrequency, "ColorList", cBuffer);
		
		//For kernel 3
		shader.SetTexture (_kernelReplace, "Result", texResult);
		shader.SetTexture (_kernelReplace, "Original", originalTexture);
		shader.SetTexture (_kernelReplace, "Palette", texPalette);
		shader.SetBuffer(_kernelReplace, "ColorList", cBuffer);
	}

	void Update()
	{
		//General
		shader.SetInt("_ColorLevel",colorLevel);
		
		//Run Kernel 0
		shader.Dispatch (_kernelPixelate, _dispatchSize.x, _dispatchSize.y, 1);
		
		//Run Kernel 1
		shader.Dispatch (_kernelExtractColor, _dispatchSize.x, _dispatchSize.y, 1);
		
		//Run Kernel 2
		shader.Dispatch (_kernelFrequency, _dispatchSize.x, _dispatchSize.y, 1);
		
		//Get data back
		cBuffer.GetData(_colorList);
		
		//Deduplicate the same colors
		for(int i=0; i<_colorList.Length-1; i++)
		{
			var c = _colorList[i];
			if(c.frequency <= 0) continue;
			
			var cNext = _colorList[i+1];
			float dist = GetColorDistance(c, cNext);
			if(dist < mergeThreshold)
			{
				Vector3 hsv;
				hsv.x = cNext.hsv.x;
				hsv.y = Mathf.Max(c.hsv.y, + cNext.hsv.y);
				hsv.z = (c.hsv.z + cNext.hsv.z) / 2f;
				_colorList[i + 1].color = Color.HSVToRGB(hsv.x,hsv.y, hsv.z);
				_colorList[i+1].frequency += c.frequency;
				_colorList[i].frequency = 0;
			}
		}
		
		//Sort _colorList by frequency
		Array.Sort(_colorList, (x, y) => y.frequency.CompareTo(x.frequency));
		cBuffer.SetData(_colorList);
		
		//Fill the palettes
		for(int i=0; i<palettes.Length; i++)
		{
			MaterialPropertyBlock block = new MaterialPropertyBlock();
			block.SetColor("_Color", _colorList[i].color);
			palettes[i].SetPropertyBlock(block);
		}
		
		//Run Kernel 3
		if (firstTime)
		{
			RandomPaletteReplace();
			firstTime = false;
		}
		UpdateNewColorGrids();
		for(int i=0; i<palettesReplace.Length; i++)
		{
			_tempNewColor[i] = colorNew[i];
		}
		shader.SetVectorArray("NewColorList", _tempNewColor);
		shader.Dispatch (_kernelReplace, _dispatchSize.x, _dispatchSize.y, 1);
	}
	
	float GetColorDistance(ColorData c1, ColorData c2)
	{
		Color col1 = c1.color;
		Color col2 = c2.color;
		float dist = Mathf.Abs(col1.r - col2.r) + Mathf.Abs(col1.g - col2.g) + Mathf.Abs(col1.b - col2.b);
		dist *= 0.7f;
		
		Vector3 col1HSV = c1.hsv;
		Vector3 col2HSV = c2.hsv;
		float hueDiff = Mathf.Abs(col1HSV.x - col2HSV.x);
		hueDiff = Mathf.Min(hueDiff, 1f-hueDiff); //because hue value is circular
		dist += hueDiff + Mathf.Abs(col1HSV.y - col2HSV.y) + Mathf.Abs(col1HSV.z - col2HSV.z);
		
		return dist;
	}
	
	[ContextMenu("ResetPaletteReplace")]
	public void ResetPaletteReplace()
	{
		for(int i=0; i<palettesReplace.Length; i++)
		{
			colorNew[i] = _colorList[i].color;
		}
	}
	
	[ContextMenu("RandomPaletteReplace")]
	public void RandomPaletteReplace()
	{
		//reset color
		ResetPaletteReplace();
			
		//random Hue
		for(int i=0; i<palettesReplace.Length; i++)
		{
			Color.RGBToHSV(colorNew[i], out float h, out float s, out float v);
			colorNew[i] = UnityEngine.Random.ColorHSV(0f,1f,s,s,v,v);
		}
	}

	private void UpdateNewColorGrids()
	{
		for(int i=0; i<palettesReplace.Length; i++)
		{
			MaterialPropertyBlock block = new MaterialPropertyBlock();
			block.SetColor("_Color", colorNew[i]);
			palettesReplace[i].SetPropertyBlock(block);
		}
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 400, Screen.height - 150, 250, 100), "Random Palette Colors"))
		{
			RandomPaletteReplace();
		}
	}
	
	void OnDestroy()
	{
		cBuffer.Release();
	}
}
