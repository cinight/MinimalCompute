//AsyncGPUReadback is not supported on OpenGL

using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class AsyncGPUReadbackTex : MonoBehaviour
{
    public ComputeShader comshader;
    public Material mat;

    //This format works for DX11/DX12/Vulkan(but weird color)/Metal
    private GraphicsFormat format = GraphicsFormat.R32G32B32A32_SFloat;
    
    private RenderTexture tex;
    private int size = 128;
    private NativeArray<Color> texcolors_float4;
    private int _kernel = 0;
    private AsyncGPUReadbackRequest request;
    private Texture2D resultTex;

    void Start()
    {
		//The texture2D for showing result
        resultTex = new Texture2D(size,size);
        mat.SetTexture("_MainTex",resultTex);

		//Create RT texture, for compute and readback
		tex = new RenderTexture( size , size , 0, format);
		tex.antiAliasing = 1;
		tex.volumeDepth = 1;
		tex.enableRandomWrite = true;
		tex.Create();
		comshader.SetTexture(_kernel, "Result", tex);

		//color array for storing readback data
		texcolors_float4 = new NativeArray<Color>(size*size, Allocator.Temp);
    
		//Request AsyncReadback
		request = AsyncGPUReadback.Request(tex);
    }

    void Update()
    {
	    //Run compute shader
        comshader.Dispatch(_kernel, Mathf.CeilToInt(size / 8f), Mathf.CeilToInt(size / 8f), 1);
        
        if(request.done && !request.hasError)
        {
	        //Readback And show result on texture
	        texcolors_float4 = request.GetData<Color>();
			resultTex.SetPixels(texcolors_float4.ToArray());
			resultTex.Apply();

			//Request AsyncReadback again
			request = AsyncGPUReadback.Request(tex);
        }
    }
}
