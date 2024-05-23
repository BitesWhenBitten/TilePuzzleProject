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

    PuzzleManager puzzleManager;

    #region Mouse Input Variables
    //before cast
    [System.NonSerialized] public Vector3 screenMousePOS;

    //must be inputted BEFORE ScreenToWorldPoint cast
   [SerializeField] float zOffset = 3.9f;

    //post ScreenToWorldPoint
    [System.NonSerialized] public Vector3 mosPOS;
    #endregion

    #region Highlight Variables
    //current frame grid to highlight
    GameObject FrameGrid;

    //Mesh Renderer for rMaterial
    MeshRenderer FrameMRenderer;
    //The previous Mesh Renderer
    MeshRenderer FrameOGMeshRenderer;

    float alpha = 0;
    //to track whether we should be counting from 0-1 or 1-0
    bool reverseAlpha;
 
   [SerializeField] float fadeSpeed = 0.0075f;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        puzzleManager = GetComponent<PuzzleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMousePOS();
        HighlightPieces();    
    }
    private void UpdateMousePOS()
    {
        screenMousePOS = Input.mousePosition;
        screenMousePOS.z += zOffset;
        mosPOS = Camera.main.ScreenToWorldPoint(screenMousePOS);
    }

    /// <summary>
    /// Assigns, swaps, or clears the the tile piece clicked on as the HeldPiece
    /// </summary>
    /// <param name="ctx"></param>
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if(ctx.performed) {
            //Check Input Actions Asset if strange toggling behavior, when configured as 'value'
            //the held piece works as expected.

            //future click submit button
            if (puzzleManager.HitPiece == null)
            {
                //perform another raycast for the submit button

                #region Mouse to Screen RayCast
               Ray dir = Camera.main.ScreenPointToRay(screenMousePOS);
               RaycastHit outHit;

                Physics.Raycast(
                    Camera.main.transform.position,
                    dir.direction,
                    out outHit);
                #endregion
                try
                {
                    if (outHit.collider.CompareTag("Submit")) puzzleManager.CheckSubmission();
                    if (outHit.collider.CompareTag("Restart")) puzzleManager.RestartGame();
                    if (outHit.collider.CompareTag("Quit")) puzzleManager.QuitGame();
                }
                catch (System.Exception)
                {

                    //throw;
                }
            }


            //expected when no held piece
            if (puzzleManager.HitPiece != null && puzzleManager.HeldPiece == null)
            {
                puzzleManager.AssignPiece();
            }
            //swap w/ HitPiece 
            else if (puzzleManager.HitPiece != null && puzzleManager.HitPiece != puzzleManager.HeldPiece)
            {
                puzzleManager.SwapPiece();
            }
            //return to original spot
            else if (puzzleManager.HeldPiece != null)
            {
                puzzleManager.ResetHeldPiece();
            }
        }    
    }
    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        if (puzzleManager.HeldPiece) puzzleManager.HeldPiece.transform.position = mosPOS;
    }
    void HighlightPieces()
    {
        #region Find Frame Grid Mesh Renderer
        if (puzzleManager.HitPiece)
        {
            //Frame is located below the Tile Piece component in hierarchy, look in parent transform
            //get the frame grid mesh renderer
            FrameGrid = puzzleManager.HitPiece.transform.Find("FrameGrid").gameObject;
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
            if ( FrameOGMeshRenderer == null)
            {
                //assign to current
                FrameOGMeshRenderer = FrameMRenderer;
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
        else if (puzzleManager.HitPiece == null && FrameOGMeshRenderer != null || FrameMRenderer != null)
        {
            FrameOGMeshRenderer.enabled = false;
            FrameMRenderer.enabled = false;
        }

        #endregion

        #region Loop 0-1 Transparency for Flash
        if(FrameMRenderer && FrameMRenderer.enabled) StartCoroutine(TransparencyFade(FrameMRenderer));
      #endregion
    }
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
