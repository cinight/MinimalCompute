using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuredBufferNoCompute : MonoBehaviour
{
    //The same struct in Shader
    struct myObjectStruct
    {
        public Vector3 objPosition;
    };

    public Material Mat;
    private ComputeBuffer _computeBuffer;
    private myObjectStruct[] _mos;
    private static int _noOfObj;
    public Transform[] _myObjects;

    // Use this for initialization
    void Start ()
    {
        _noOfObj = _myObjects.Length;

        _mos = new myObjectStruct[_noOfObj];

        //Initiate buffer
        _computeBuffer = new ComputeBuffer(_noOfObj,12); //(3)*4bytes in myObjectStruct
    }
	
	// Update is called once per frame
	void Update ()
    {
        RunShader();
    }

    public void RunShader()
    {
        for (int i = 0; i < _noOfObj; i++)
        {
            _mos[i].objPosition = _myObjects[i].position;
           // Debug.Log(i+" "+_myObjects[i].position);
        }

        //Set buffer
        _computeBuffer.SetData(_mos);

        //Assign buffer to unlit shader
        Mat.SetBuffer("myObjectBuffer", _computeBuffer);
    }

    void OnDestroy()
    {
        //Clean Buffer
        _computeBuffer.Release();
    }
}


