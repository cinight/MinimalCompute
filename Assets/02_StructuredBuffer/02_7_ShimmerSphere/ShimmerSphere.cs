using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShimmerSphere : MonoBehaviour
{
    public Transform sphereParent;
    public Transform quad;
    public Material mat;
    
    //The same struct in Shader
    struct myObjectStruct
    {
        public Vector3 objPosition;
        public float objSize;
    };

    private ComputeBuffer _computeBuffer;
    private myObjectStruct[] _mos;
    private static int _noOfObj;
    private Transform[] _myObjects;
    
    void Start ()
    {
        _myObjects = sphereParent.GetComponentsInChildren<Transform>();
        _noOfObj = _myObjects.Length;
        _mos = new myObjectStruct[_noOfObj];
        _computeBuffer = new ComputeBuffer(_noOfObj,16); //(4)*4bytes in myObjectStruct
        mat.SetBuffer("myObjectBuffer", _computeBuffer);
        
        Debug.Log("Number of objects: " + _noOfObj);
    }
    
	void Update ()
    {
        mat.SetVector("objectCenter", quad.position);
        for (int i = 0; i < _noOfObj; i++)
        {
            _mos[i].objPosition = _myObjects[i].position;
            _mos[i].objSize = _myObjects[i].lossyScale.x;
        }
        _computeBuffer.SetData(_mos);
    }

    void OnDestroy()
    {
        _computeBuffer.Release();
    }
}


