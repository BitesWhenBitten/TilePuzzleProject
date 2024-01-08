using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public PrimitiveType _shapeType;
    private GameObject _shape;


    // Start is called before the first frame update
    void Start()
    {
      _shape =  GameObject.CreatePrimitive(_shapeType);
        _shape.transform.position = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
