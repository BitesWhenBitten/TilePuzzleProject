using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    private float mPosX;
    private float mPosY;

    [SerializeField] private float raycastDistance;

    //the currently hovered/hit piece
    TilePiece hPiece;

    //Mesh Renderer for rMaterial
    MeshRenderer mRenderer;
    //The previous Mesh Renderer
    MeshRenderer lkRenderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HighlightActivePiece();

    }

    void HighlightActivePiece()
    {
        #region Set-up Mouse to Screen RayCast
        Ray dir = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit outHit;

        Physics.Raycast(
            Camera.main.transform.position,
            dir.direction,
             out outHit);
        #endregion

        #region Assign hit tilepiece, Mesh Renderer
        //this is scoped with a try/catch to remove null reference errors 
        try
        {
            //Frame is located below the Tile Piece component in hierarchy, move to parent transform
            hPiece = outHit.collider.gameObject.transform.parent.GetComponent<TilePiece>();
            //get the frame grid mesh renderer & material
            GameObject FrameGrid = hPiece.transform.Find("FrameGrid").gameObject;
            mRenderer = FrameGrid.GetComponent<MeshRenderer>();

        }
        catch (System.Exception)
        {
            //call is annoying, disabling
            //Debug.LogWarning("No tilepiece found");
            //if the hit piece could not be found under current hit, set to null
            hPiece = null;
        }
        #endregion

        #region Process Tiles Hit
        if (hPiece)
        {
                #region Perform the highlighting and switching
            if (mRenderer && lkRenderer && mRenderer != lkRenderer)
            {
                //process turning off previous tile first

                //disable render on last known
                lkRenderer.enabled = false;
                //re-assign to current
                lkRenderer = mRenderer;
                //finally turn on rendering for the current Mesh renderer
                mRenderer.enabled = true;
            }
            else if (lkRenderer && mRenderer != lkRenderer)
            {
                //turn of Mesh renderer for last known
                lkRenderer.enabled = false;
            }
            //first pass catch
            else if (mRenderer && lkRenderer == null)
            {
                //assign to current
                lkRenderer = mRenderer;
                //finally turn on rendering for the current Mesh renderer
                mRenderer.enabled = true;
            }
            #endregion  
        }
        //turn off all renderers if nothing caught
        else if (lkRenderer || mRenderer)
        {
            lkRenderer.enabled = false;
            mRenderer.enabled = false;
        }
        #endregion


    }

    /*private float GetMouseX()
    {
        return Input.GetAxis("Mouse X");
    }

    private float GetMouseY()
    {
        return Input.GetAxis("Mouse Y");
    }*/
}
