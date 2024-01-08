using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PuzzleManager PM;
    public GridPoint FP;
    public Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
       /* //Move the x axis of the first grid point with A & D keys
        if (FP == null)
        {
            FP = PM.FirstPoint;
            Debug.Log("First point set");
        }


        if(Input.GetKey(KeyCode.A)) 
        {
            Debug.Log("A pressed");
            pos = FP.transform.position;
            pos.x += (4 * Time.deltaTime);
            //FP.transform.localPosition.Set(pos.x,pos.y,pos.z);
            FP.transform.SetPositionAndRotation(pos, FP.transform.rotation);
        }

        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("D pressed");
            pos = FP.transform.position;
            pos.x -= (4 * Time.deltaTime);
            //FP.transform.position.Set(pos.x, pos.y, pos.z);
            FP.transform.SetPositionAndRotation(pos, FP.transform.rotation);

        }
*/
    }
}
