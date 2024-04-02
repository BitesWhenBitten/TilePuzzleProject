using Assets.Scripts.Extensions;
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

    //current frame grid to highlight
    GameObject FrameGrid;

    //to return to original
   [SerializeField] GameObject originGO;
 
    //before cast
    Vector3 screenMousePOS;

    //must be inputted BEFORE ScreenToWorldPoint cast
   [SerializeField] float zOffset = 3.9f;

    //post ScreenToWorldPoint
    Vector3 mosPOS;

    //Mesh Renderer for rMaterial
    MeshRenderer FrameMRenderer;
    //The previous Mesh Renderer
    MeshRenderer FrameOGMeshRenderer;

    //material for held piece
    Material hpMaterial;

    //for the above material
   
    [SerializeField]float semiTransparent = .5f;

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
        UpdateMousePOS();

       if(heldPiece == null) FindTilePiece();
        HighlightPieces();
    
    }

    private void UpdateMousePOS()
    {
        screenMousePOS = Input.mousePosition;
        screenMousePOS.z += zOffset;
        mosPOS = Camera.main.ScreenToWorldPoint(screenMousePOS);
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

            //re-parent to having no parent, store original        
            originGO = heldPiece.transform.parent.gameObject;
            heldPiece.transform.SetParent(transform.parent, false);

            //set to mosPOS
            heldPiece.transform.position = mosPOS;

            //set held piece scale and opacity to semi-transparent
            //opacity:
            hpMaterial = heldPiece.transform.Find("Quad").GetComponent<MeshRenderer>().material;       
            MaterialExtensions.ToTransparentMode(hpMaterial);

            //transparency:
            UnityEngine.Color color = hpMaterial.color;
            hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, semiTransparent);

            //scale:
            heldPiece.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
        }
        //encapsulating logic to swap with another piece, perform a whole raycast...
        else if (heldPiece != null)
        {
            //reset held piece values and material
            //opacity:
            MaterialExtensions.ToOpaqueMode(hpMaterial);

            //transparency:
            UnityEngine.Color color = hpMaterial.color;
            hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, 1);
            hpMaterial = null;

            //scale:
            heldPiece.transform.localScale = new Vector3(1f, 1f, 1f);

            //reparent to original GP/GO
            heldPiece.transform.SetParent(originGO.transform, false);
            heldPiece.transform.position = originGO.transform.position;

            //reset variables
            originGO = null;
            heldPiece = null;
        }
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        if (heldPiece) heldPiece.transform.position = mosPOS;
    }

    private void FindTilePiece()
    {
        #region Mouse to Screen RayCast
        Ray dir = Camera.main.ScreenPointToRay(screenMousePOS);
        RaycastHit outHit;

        Physics.Raycast(
            Camera.main.transform.position,
            dir.direction,
            out outHit);

        #endregion

        //Assign piece if hit

        try
        {
            hitPiece = outHit.collider.gameObject.transform.parent.GetComponent<TilePiece>();

        }
        catch (System.Exception)
        {
            hitPiece = null;
            //call is annoying, disabling
            //Debug.LogWarning("No tilepiece found");
            //throw;
        }
    }

    void HighlightPieces()
    {
        #region Find Frame Grid Mesh Renderer
        if (hitPiece)
        {
            //Frame is located below the Tile Piece component in hierarchy, move to parent transform
            //get the frame grid mesh renderer
            FrameGrid = hitPiece.transform.Find("FrameGrid").gameObject;
            FrameMRenderer = FrameGrid.GetComponent<MeshRenderer>();

        }
        else
        {
            FrameGrid = null;
        }
        #endregion

        #region Process Tiles Hit
        if (FrameGrid)
        {
            #region Perform the highlighting and switching

            //first pass catch
            //FrameMRenderer == null || -> deleted from below if
            if ( FrameOGMeshRenderer == null)
            {
                //assign to current
                FrameOGMeshRenderer = FrameMRenderer;
                //turn on rendering for the current Mesh renderer
                FrameMRenderer.enabled = true;
            }
            else if (FrameMRenderer != FrameOGMeshRenderer)
            {
                //disable render on last known
                FrameOGMeshRenderer.enabled = false;
                //re-assign to current
                FrameOGMeshRenderer = FrameMRenderer;
                //finally turn on rendering for the current Mesh renderer
                FrameMRenderer.enabled = true;
            }
            //avoids a no-highlight bug where variables have not changed
            else if (!FrameMRenderer.enabled)
            {
                FrameMRenderer.enabled = true;
            }
           
            #endregion  
        }
        //turn off all renderers if nothing caught
        else if (hitPiece == null && FrameOGMeshRenderer != null || FrameMRenderer != null)
        {
            FrameOGMeshRenderer.enabled = false;
            FrameMRenderer.enabled = false;
        }

        #endregion

        #region Loop 0-1 Transparency for Flash
        if(FrameMRenderer && FrameMRenderer.enabled) StartCoroutine(TransparencyFade(FrameMRenderer));
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
