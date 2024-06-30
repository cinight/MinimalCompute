using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ShaderBakeToTexture : MonoBehaviour 
{
	public Material sphereMat;
	public Material quadMat;
	public Material resultSphereMat;
	public int size = 256;
	
	public static RenderTexture tex;
	private int targetID = 6; //match with shader "register(u6)"
	private Texture2D tex2D;
	
	void Start () 
	{
		//Create the UAV render texture
		tex = new RenderTexture(size, size, 0);
		tex.hideFlags = HideFlags.HideAndDontSave;
		tex.enableRandomWrite = true;
		tex.Create ();
		
		//Assign the UAV render texture to materials
		sphereMat.SetInt("_Size", tex.width);
		sphereMat.SetTexture("Result", tex);
		quadMat.SetInt("_Size", tex.width);
		quadMat.SetTexture("Result", tex);
		
		//Create and assign the Texture2D to result material
		tex2D = new Texture2D(size, size, tex.graphicsFormat, TextureCreationFlags.None);
		resultSphereMat.mainTexture = tex2D;
	}
	
	// This does not work in RenderGraph path, only works in Compatible Mode
	private void OnRenderObject()
	{
		Graphics.ClearRandomWriteTargets();
		Graphics.SetRandomWriteTarget(targetID, tex);
	}

	void OnDisable()
	{
		Graphics.ClearRandomWriteTargets();
		tex.Release();
	}

	private void BakeToTexture2D()
	{
		Graphics.CopyTexture(tex, tex2D);
		
		// If you want to save tex2D to disk, use EncodeToPNG etc.
		// https://docs.unity3d.com/ScriptReference/ImageConversion.html
	}
	
	void OnGUI()
	{
		float w = Screen.width/6f;
		Rect rect = new Rect(Screen.width/2f - w/2f, Screen.height/2f + 200, w, 100);
		if(GUI.Button(rect, "Bake To Texture2D")) BakeToTexture2D();
	}
}
