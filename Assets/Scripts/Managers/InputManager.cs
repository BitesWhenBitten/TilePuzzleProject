using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{ 
   private PuzzleManager puzzleManager;

    #region Mouse Input Variables

    [System.NonSerialized] public Vector3 RawMousePOS;

    //must be inputted BEFORE ScreenToWorldPoint cast
   [SerializeField] private float zOffset = 3.9f;

    [System.NonSerialized] public Vector3 ConvertedMousePOS;
    #endregion

    #region Highlight Variables

    private GameObject frameGrid;
    private MeshRenderer frameMRenderer;
    private MeshRenderer priorFrameMRenderer;
    private float alpha = 0;
    private bool isReverseAlpha;
    [SerializeField] private float fadeSpeed = 0.0075f;
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
        RawMousePOS = Input.mousePosition;
        RawMousePOS.z += zOffset;
        ConvertedMousePOS = Camera.main.ScreenToWorldPoint(RawMousePOS);
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
            if (puzzleManager.HitPiece == null)
            {
                //perform another raycast for the submit button

                #region Mouse to Screen RayCast
               Ray dir = Camera.main.ScreenPointToRay(RawMousePOS);
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
        if (puzzleManager.HeldPiece) { puzzleManager.HeldPiece.transform.position = ConvertedMousePOS; }
    }
    void HighlightPieces()
    {
        #region Find Frame Grid Mesh Renderer
        if (puzzleManager.HitPiece)
        {
            //Frame is located below the Tile Piece component in hierarchy, look in parent transform
            frameGrid = puzzleManager.HitPiece.transform.Find("FrameGrid").gameObject;
            frameMRenderer = frameGrid.GetComponent<MeshRenderer>();

        }
        else
        {
            frameGrid = null;
        }
        #endregion

        #region Process Tiles Hit
        if (frameGrid)
        {
            #region Perform the highlighting and switching

            //first pass catch
            if ( priorFrameMRenderer == null)
            {
                //assign to current
                priorFrameMRenderer = frameMRenderer;
                frameMRenderer.enabled = true;
            }
            else if (frameMRenderer != priorFrameMRenderer)
            {
                //disable render on last known
                priorFrameMRenderer.enabled = false;
                //re-assign to current
                priorFrameMRenderer = frameMRenderer;
                //finally turn on rendering for the current Mesh renderer
                frameMRenderer.enabled = true;
            }
            //avoids a no-highlight bug where variables have not changed
            else if (!frameMRenderer.enabled)
            {
                frameMRenderer.enabled = true;
            }
           
            #endregion  
        }
        //turn off all renderers if nothing caught
        else if (puzzleManager.HitPiece == null && priorFrameMRenderer != null || frameMRenderer != null)
        {
            priorFrameMRenderer.enabled = false;
            frameMRenderer.enabled = false;
        }

        #endregion

        #region Loop 0-1 Transparency for Flash
        if (frameMRenderer && frameMRenderer.enabled) { StartCoroutine(TransparencyFade(frameMRenderer)); }
        #endregion
    }

    /// <summary>
    /// Scrolls through the alpha from 0-1 & 1-0 per fadeSpeed.
    /// </summary>
    /// <param name="renderer"></param>
    /// <returns></returns>
    IEnumerator TransparencyFade(MeshRenderer renderer)
    {
        while (!isReverseAlpha)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            //new color cannot be applied directly, make new and assign
            UnityEngine.Color color = renderer.material.color;
            renderer.material.color = new UnityEngine.Color(color.r, color.g, color.b, alpha);
         
            if(alpha <=0) isReverseAlpha = true;

                    yield return null;
        }
       while (isReverseAlpha)
            {
                alpha += fadeSpeed * Time.deltaTime;

            //new color cannot be applied directly, make new and assign
                UnityEngine.Color color = renderer.material.color;
                renderer.material.color = new UnityEngine.Color(color.r, color.g, color.b, alpha);
                if (alpha >= 1) isReverseAlpha = false;

                yield return null;
            }
    }
}
