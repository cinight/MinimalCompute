using UnityEngine;

public class IndirectCompute : MonoBehaviour 
{
    public ComputeShader shader;
    public TextMesh tx;
    public int _amount = 3; //max no. of data that you want to handle
    public float _filter = 0f; //just to make the filter to give different result in each frame
    //public bool staticTest = false;

    private ComputeBuffer cbDrawArgs;
    private int[] args;
    private ComputeBuffer cbPoints;
    private int _kernelIndirect;
    private int _kernelDirect;
    private uint _threadsizeX = 0;
    private uint _threadsizeY = 0;
    private uint _threadsizeZ = 0;

    void Start () 
	{
        release(); //just to make sure the buffer are clean

        _kernelDirect = shader.FindKernel("CSMainDirect");
        _kernelIndirect = shader.FindKernel("CSMainIndirect");

        shader.GetKernelThreadGroupSizes(_kernelIndirect, out _threadsizeX, out _threadsizeY, out _threadsizeZ);

        if (cbDrawArgs == null)
        {
            cbDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
            args = new int[4];
            args[0] = (int)_threadsizeX;
            args[1] = (int)_threadsizeY;
            args[2] = (int)_threadsizeZ;
            args[3] = 0;
            cbDrawArgs.SetData(args);
        }

        if (cbPoints == null)
        {
            cbPoints = new ComputeBuffer(_amount, 4, ComputeBufferType.Append); //because 1*uint

            //Set buffer to kernels
            shader.SetBuffer(_kernelDirect, "pointBufferOutput", cbPoints);
            shader.SetBuffer(_kernelDirect, "pointBuffer", cbPoints);
            shader.SetBuffer(_kernelIndirect, "pointBuffer", cbPoints);
        }
    }

    private void Update()
    {
        //Make filter change, so that we see different result
        //if (!staticTest) { if (_filter >= _amount) { _filter = 0; } else { _filter++; } }

        //Reset count
        cbPoints.SetCounterValue(0);

        //Direct dispatch to do filter
        shader.SetFloat("_Time", Time.time);
        shader.SetFloat("_Filter", _filter);
        shader.Dispatch(_kernelDirect, _amount * (int)_threadsizeX, (int)_threadsizeY, (int)_threadsizeZ);

        //Copy Count
        ComputeBuffer.CopyCount(cbPoints, cbDrawArgs, 0);

        //Indirect dispatch to only execute kernel on filtered data
        shader.DispatchIndirect(_kernelIndirect, cbDrawArgs, 0);

        //Read data from GPU
        int[] ff = new int[cbPoints.count];
        cbPoints.GetData(ff);
        int[] aa = new int[args.Length];
        cbDrawArgs.GetData(aa);

        //Output
        tx.text = "Indirect compute \n cbDrawArgs \n [0]:" + aa[0] + "\n [1]:" + aa[1] + "\n [2]:" + aa[2] + "\n [3]:" + aa[3] + " \n cbPoints (total data count) = " + cbPoints.count;
        for (int i = 0; i < ff.Length; i++)
        {
            tx.text += "\n" + "ff[" + i + "] = " + ff[i];
        }
    }

    //------------------------------------------------------------------------------

    private void release()
    {
        if (cbDrawArgs != null)
        {
            cbDrawArgs.Dispose();
            cbDrawArgs.Release();
            cbDrawArgs = null;
            //Debug.Log("OnDestroy - Release cbDrawArgs compute buffer");
        }
        if (cbPoints != null)
        {
            cbPoints.Dispose();
            cbPoints.Release();
            cbPoints = null;
            //Debug.Log("OnDestroy - Release cbPoints compute buffer");
        }
    }
	void OnDestroy()
	{
        release();
    }

	void OnApplicationQuit()
	{
        release();
    }

}
