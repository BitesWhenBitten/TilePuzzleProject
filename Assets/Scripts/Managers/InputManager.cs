using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    //START DEBUG PROPERTIES

    //END DEBUG PROPERTIES

    //the currently hovered/hit piece
    TilePiece hitPiece;

    //the tile being manipulated by the mouse
    TilePiece heldPiece;
  
    //for returning to spot
   Vector3 originPOS;

    Vector3 mosInput;

    Vector3 mosPOS;

    //Mesh Renderer for rMaterial
    MeshRenderer mRenderer;
    //The previous Mesh Renderer
    MeshRenderer lkRenderer;

    float alpha = 0;
    //to track whether we should be counting from 0-1 or 1-0
    bool reverseAlpha;
 
   [SerializeField] float fadeSpeed = 0.0075f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPOS = Mouse.current.position.ReadValue();

        mosPOS = Camera.main.ScreenToWorldPoint(screenPOS);
        Debug.Log(mosPOS);
        HighlightActivePiece();

       
    }

    /// <summary>
    /// Assigns or clears the the tile piece clicked on as the heldPiece. Original position is notated,
    /// z plan adjustment made to avoid clipping/fighting.
    /// </summary>
    /// <param name="ctx"></param>
    public void OnClick(InputAction.CallbackContext ctx)
    {
        //Check Input Actions Asset if strange toggling behavior, when configured as 'value'
        //the held piece works as expected.
        //while held, the hit Piece should follow the mouse
        //switched to a toggle so it is not necessary to hold mouse click
       
        if (hitPiece != null && heldPiece == null)
        {         
                heldPiece = hitPiece;
                originPOS = heldPiece.transform.position;

            //buffer the held piece towards the screen a bit, only a small change is necessary
            Vector3 zAdjustedPOS = originPOS;
            zAdjustedPOS.z -= .01f;
            heldPiece.transform.SetPositionAndRotation(zAdjustedPOS, Quaternion.identity);
        }
        //encapsulating logic to swap with another piece, perform a whole raycast...
        else if (heldPiece != null)
        {   
                //return to original spot when released
                heldPiece.transform.SetPositionAndRotation(originPOS, Quaternion.identity);
                heldPiece = null;
        }
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();
        if (value != null)
        {
            
            mosInput.x = Mathf.Clamp(value.x, -1f, 1f);
            mosInput.y = Mathf.Clamp(value.y, -1f, 1f);
            

        }
        if (heldPiece)
        {

            //create a new vector reflecting the mouse movement, apply to piece
            Vector3 newPOS = heldPiece.transform.position;
            newPOS.x = mosPOS.x ;
            newPOS.y =  mosPOS.y ;

            heldPiece.transform.SetPositionAndRotation(newPOS, Quaternion.identity);

            Debug.Log("applied new vector");
        }

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
            hitPiece = outHit.collider.gameObject.transform.parent.GetComponent<TilePiece>();
            //get the frame grid mesh renderer & material
            GameObject FrameGrid = hitPiece.transform.Find("FrameGrid").gameObject;
            mRenderer = FrameGrid.GetComponent<MeshRenderer>();

        }
        catch (System.Exception)
        {
            //call is annoying, disabling
            //Debug.LogWarning("No tilepiece found");
            //if the hit piece could not be found under current hit, set to null
            hitPiece = null;
        }
        #endregion

        #region Process Tiles Hit
        if (hitPiece)
        {
                #region Perform the highlighting and switching
            
            //first pass catch
            if (mRenderer == null || lkRenderer == null)
            {
                //assign to current
                lkRenderer = mRenderer;
                //turn on rendering for the current Mesh renderer
                mRenderer.enabled = true;
            }
            else if (mRenderer != lkRenderer)
            {
                //disable render on last known
                lkRenderer.enabled = false;
                //re-assign to current
                lkRenderer = mRenderer;
                //finally turn on rendering for the current Mesh renderer
                mRenderer.enabled = true;
            } 
            //avoids a no-highlight bug where variables have not changed
            else if (!mRenderer.enabled)
            {
                mRenderer.enabled = true;
            }
            #endregion  
        }
        //turn off all renderers if nothing caught
        else if (hitPiece == null && lkRenderer != null || mRenderer != null)
        {
            lkRenderer.enabled = false;
            mRenderer.enabled = false;
        }

        #endregion

        #region Loop 0-1 Transparency for Flash
        if(mRenderer && mRenderer.enabled) StartCoroutine(TransparencyFade(mRenderer));
      #endregion
    }

    /// <summary>
    /// Be sure that the passed renderer has a material using Transparent for rendering mode.
    /// MT_Highlight is setup for this purpose.
    /// </summary>
    /// <param name="renderer"></param>
    /// <returns></returns>
    IEnumerator TransparencyFade(MeshRenderer renderer)
    {
        while (!reverseAlpha)
        {
            alpha -= fadeSpeed * Time.deltaTime;

            //new color cannot be applied directly, make new and assign
            UnityEngine.Color color = renderer.material.color;
            renderer.material.color = new UnityEngine.Color(color.r, color.g, color.b, alpha);
            //begins oscillation from 0-1
            if(alpha <=0) reverseAlpha = true;

                    yield return null;
        }
        while (reverseAlpha)
            {
                alpha += fadeSpeed * Time.deltaTime;

            //new color cannot be applied directly, make new and assign
                UnityEngine.Color color = renderer.material.color;
                renderer.material.color = new UnityEngine.Color(color.r, color.g, color.b, alpha);
                //begins oscillation from 1-0
                if (alpha >= 1) reverseAlpha = false;

                yield return null;
            }
    }

}
