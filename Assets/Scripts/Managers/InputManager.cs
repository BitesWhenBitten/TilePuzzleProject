using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
   private PuzzleManager puzzleManager;

    #region Mouse Input Variables
    public Vector3 ZAdjustedMousePOS;

    //must be inputted BEFORE ScreenToWorldPoint cast
    [SerializeField] private float zOffset = 3.9f;
    [System.NonSerialized] public Vector3 ConvertedMousePOS;
    #endregion

    #region Highlight Variables
    private GameObject frameGrid = null;
    private MeshRenderer frameMRenderer;
    private MeshRenderer priorFrameMRenderer;
    private float alpha = 0;
    private bool isReverseAlpha;
    [SerializeField] private float fadeSpeed = 0.0075f;
    private UnityEngine.Color color;
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
        ZAdjustedMousePOS = Input.mousePosition;
        ZAdjustedMousePOS.z += zOffset;
        ConvertedMousePOS = Camera.main.ScreenToWorldPoint(ZAdjustedMousePOS);
    }

    /// <summary>
    /// Assigns, swaps, or clears the the tile piece clicked on as the HeldPiece
    /// </summary>
    /// <param name="ctx"></param>
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if(ctx.performed) 
        {
            //perform another raycast for the buttons
            if (puzzleManager.HitPiece == null)
            {
                #region Mouse to Screen RayCast
               Ray dir = Camera.main.ScreenPointToRay(ZAdjustedMousePOS);
               RaycastHit outHit;

                Physics.Raycast(
                    Camera.main.transform.position,
                    dir.direction,
                    out outHit);
                #endregion
                string tag;
                
                try
                {
                    tag = outHit.collider.tag;
                }
                catch (System.Exception)
                {
                    tag = null;
                   // throw;
                }

                switch (tag)
                    {
                        case "Submit":
                            puzzleManager.CheckSubmission();
                            break;

                        case "Restart":
                            puzzleManager.RestartGame();
                            break;

                        case "Quit":
                            puzzleManager.QuitGame();
                            break;
                    }
            }

            if (CanAssignPiece())
            {
                puzzleManager.AssignPiece();
            }
            else if (CanSwapPiece())
            {
                puzzleManager.SwapPiece();
            }
            else if (CanResetPiece())
            {
                puzzleManager.ResetHeldPiece();
            }
        }    
    }
    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        if (puzzleManager.HeldPiece) { puzzleManager.SetHeldPiecePOS(ConvertedMousePOS); }
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
            //I cannot find a better way around this, not a fan of this solution
            else if (!frameMRenderer.enabled)
            {
                frameMRenderer.enabled = true;
            }

            #endregion
        }
        else if (!frameGrid)
        {
            if (priorFrameMRenderer != null)
            {
                priorFrameMRenderer.enabled = false;
            }
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
        //new color cannot be applied directly, make new and assign
        color = renderer.material.color;
        color.a = alpha;
        renderer.material.color = color;

            while (!isReverseAlpha)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                if (alpha <= 0) isReverseAlpha = true;
                yield return null;
            }
            while (isReverseAlpha)
            {
                alpha += fadeSpeed * Time.deltaTime;
                if (alpha >= 1) isReverseAlpha = false;
                yield return null;
            }
    }
    private bool CanAssignPiece()
    {
        if (puzzleManager.HitPiece == null) { return false; }
        if (puzzleManager.HeldPiece != null) { return false; }
        else return true;
    }
    private bool CanSwapPiece()
    {
        if(puzzleManager.HitPiece == null) { return false; }
        if (puzzleManager.HitPiece == puzzleManager.HeldPiece) { return false; }
        return true;
    }
    private bool CanResetPiece()
    {
        if (puzzleManager.HeldPiece != null) { return true; }
        return false;
    }
}
