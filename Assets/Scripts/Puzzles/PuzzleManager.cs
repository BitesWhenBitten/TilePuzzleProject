using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PuzzleManager : MonoBehaviour
{
    
    /*The puzzle manager will create a grid and attach the various tiles to the center
     * of those grid points. When a tile is clicked it will be held for swapping with other
     *pieces. Order pieces correctly and submit to win*/

    #region Grid Related Variables
    [SerializeField] private GameObject gridStart;
    [SerializeField] private GameObject prfbGridPoint;
    [SerializeField] private GameObject[] tileSets;

    [Tooltip("Puzzle Manager only supports SQUARE grids, so make sure your tilesets are squared!")]
    [SerializeField] private int columnCount;
    
    [Tooltip("Standard unit in Unity is 1 = 1 meter. Suggested spacing is between 0.1-0.5")]
    [SerializeField] private float columnSpacingY;
    [SerializeField] private float columnSpacingX;
    [SerializeField] private GridPoint[,] gridPoints;
    #endregion

    #region Input Manipulation Variables

    [System.NonSerialized] public TilePiece HitPiece;
    [System.NonSerialized] public TilePiece HeldPiece;

    //DESIGN VIOLATION: used only in this class, why public?
    [System.NonSerialized] public GameObject OriginGO;

    private Material heldPieceMaterial;

    //intended for use w/ above Material
    [SerializeField] private float semiTransparent = .5f;

    private UnityEngine.Color color;

    #endregion

    //DESIGN VIOLATION: used only in this class, why public?
    public TextMeshProUGUI textElement;

    /// <summary>
    /// Ensure tilesets submitted are square as only square tilesets are supported.
    /// </summary>
    public void OnValidate()
    {
        ValidateTilesets();

        //column spacing clamp
        if(columnSpacingY > 1f || columnSpacingY < 0.1f)
        {
            columnSpacingY = Mathf.Clamp(columnSpacingY, 0.1f, 1f);
        }

        if (columnSpacingX > 1f || columnSpacingX < 0.1f)
        {
            columnSpacingX = Mathf.Clamp(columnSpacingX, 0.1f, 1f);
        }

    }

    // Start is called before the first frame update
    private void Start()
    {
        gridPoints = GenerateGrid(gridStart.transform, columnCount);
        GenerateTileset(tileSets, gridPoints);
    }

    // Update is called once per frame
    private void Update()
    {
        FindTilePiece();
    }

    /// <summary>
    /// Generates a grid from a start point to a given grid size. Evenly spaces the grid.
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="gridSize"></param>
    /// <returns>An array of GridPoints</returns>
    private GridPoint[,] GenerateGrid(Transform startPoint, int gridSize)
    {
        //multi-dim array is efficient for making a grid, pre-req positional setup
        GridPoint[,] outGrid = new GridPoint[gridSize, gridSize];
        GameObject GO;
        Vector3 pos = startPoint.position;
        float spX = pos.x;
        float spY = pos.y;

        int gpCount = 1;

        //demonstrate use of nested for loop, efficient for grid creation
        //columns
        for (int i = 0; i < gridSize; i++)
        {
            //rows
           for (int j  = 0; j < gridSize; j++)
            {
                #region Create New pos and Instantiate
                pos = startPoint.position + new Vector3((spX + i) * columnSpacingX, (spY - j) * columnSpacingY, 0);

                GO = Instantiate(prfbGridPoint, startPoint);
                GO.transform.SetPositionAndRotation(pos, Quaternion.identity);
                #endregion

                #region Set Grid Point Number, name, and position
                GridPoint GP = GO.GetComponent<GridPoint>();
                GP.SetGridPosition(gpCount);
                GO.name = "GP: " + gpCount;
                outGrid[i, j] = GP;
                gpCount++;
                #endregion
            }
        }

        return outGrid;

    }
   
    /// <summary>
    /// Generates the tileset on top of the generated grid.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="targetGrid"></param>
    private void GenerateTileset(GameObject[] collection, GridPoint[,] targetGrid)
    {
        Tileset TileSet = collection[0].GetComponent<Tileset>();

        List<TilePiece> tilePieces = TileSet.tilePieces.ToList();

        //need the square root for nested for loop generation
        int gridSize = (int)Mathf.Sqrt(targetGrid.Length);

        for (int i = 0; i < gridSize; i++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 pos = targetGrid[i, y].transform.position;

                // creating somerandomness
                int length = tilePieces.Count;
                int random = Random.Range(0, length);

                   Instantiate(
                    tilePieces[random],
                    pos,
                    Quaternion.identity,
                    targetGrid[i, y].transform);

                //delete @ random to correct the array size
                tilePieces.RemoveAt(random);
            }
        }
    }

    /// <summary>
    /// Triggers GameWon() if all tiles are correct.
    /// </summary>
    public void CheckSubmission()
    {
        foreach (GridPoint cPoint in gridPoints)
        {        
            TilePiece cPiece = cPoint.GetComponentInChildren<TilePiece>();
            if (cPoint.GetGridPosition() != cPiece.tileNumber)
            {
               StartCoroutine(ShowTemporaryMessage("Puzzle is incomplete", 3));
                return;
            }
            GameWon();
        }      
    }
    private void GameWon()
    {
        textElement.text = "You have defeated the puzzle, good job!";
        textElement.enabled = true;
    }
    public void RestartGame()
    {
       SceneManager.LoadScene("Multi-Image Tile Puzzle");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Ensures tilesets are square, removes tilesets if not squared.
    /// </summary>
    private void ValidateTilesets()
    {
        if (columnCount == 0 || columnCount == 1 || tileSets == null) { return; }
        int i = 0;

        foreach (GameObject tileSet in tileSets)
        {
            Tileset currentSet = tileSet.GetComponent<Tileset>();

            if (currentSet != null)
            {
                bool isSquared = columnCount == Mathf.Sqrt(currentSet.tilePieces.Length);

                if (!isSquared)
                {
                    //remove the offending item
                    tileSets.SetValue(null, i);

                    string msg = $"The tileset input has a length that cannot be " +
                    $"squared to using the columnCount. {tileSet.name}";
                    Debug.LogError(msg);
                }
            }
            i++;
        }
    }

    /// <summary>
    /// Makes the hit piece held by the mouse and free-floating.
    /// </summary>
    public void AssignPiece()
    {
        HeldPiece = HitPiece;

        //make free-floating       
        OriginGO = HeldPiece.transform.parent.gameObject;
        HeldPiece.transform.SetParent(transform.parent, false);

        HeldPiece.transform.position = GetComponent<InputManager>().mosPOS;

        #region Visual Changes
        //set held piece scale and opacity to semi-transparent
        heldPieceMaterial = HeldPiece.transform.Find("Quad").GetComponent<MeshRenderer>().material;
        MaterialExtensions.ToTransparentMode(heldPieceMaterial);

        //transparency:
        color = heldPieceMaterial.color;
        heldPieceMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, semiTransparent);

        //scale:
        HeldPiece.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        #endregion
    }

    /// <summary>
    /// Swaps the mouse held piece with the hit piece behind it.
    /// </summary>
    public void SwapPiece()
    {
        TilePiece tempPiece;
        //put the currently HeldPiece on the spot of the HitPiece
        tempPiece = HeldPiece;
        tempPiece.transform.SetParent(HitPiece.transform.parent.gameObject.transform, false);
        tempPiece.transform.position = HitPiece.transform.parent.gameObject.transform.position;

        #region Swapped Piece Visual Reset
        MaterialExtensions.ToOpaqueMode(heldPieceMaterial);

        //transparency:
        color = heldPieceMaterial.color;
        heldPieceMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, 1);
        heldPieceMaterial = null;

        //scale:
        tempPiece.transform.localScale = new Vector3(1f, 1f, 1f);
        #endregion

        //replace with new
        HeldPiece = HitPiece;
        HeldPiece.transform.SetParent(transform.parent, false);

        HeldPiece.transform.position = GetComponent<InputManager>().mosPOS;

        #region New Held Piece Visual Changes
        //set held piece scale and opacity to semi-transparent
        heldPieceMaterial = HeldPiece.transform.Find("Quad").GetComponent<MeshRenderer>().material;
        MaterialExtensions.ToTransparentMode(heldPieceMaterial);

        //transparency:
        color = heldPieceMaterial.color;
        heldPieceMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, semiTransparent);

        //scale:
        HeldPiece.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        #endregion
    }

    /// <summary>
    /// Clears any held piece.
    /// </summary>
    public void ResetHeldPiece()
    {
        #region Visual Changes
        MaterialExtensions.ToOpaqueMode(heldPieceMaterial);

        //transparency:
        color = heldPieceMaterial.color;
        heldPieceMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, 1);
        heldPieceMaterial = null;

        //scale:
        HeldPiece.transform.localScale = new Vector3(1f, 1f, 1f);
        #endregion

        //reparent to original GP/GO
        HeldPiece.transform.SetParent(OriginGO.transform, false);
        HeldPiece.transform.position = OriginGO.transform.position;

        //reset variables
        OriginGO = null;
        HeldPiece = null;
    }

    /// <summary>
    /// Performs a raycast for either any piece or for a piece behind the currently
    /// held piece.
    /// </summary>
    private void FindTilePiece()
    {
        RaycastHit outHit;
        Ray dir;

        //perform a raycast for the backgroundPiece and return early if a piece is already held
        if (HeldPiece)
        {
            Physics.Raycast(
                HeldPiece.transform.position,
                HeldPiece.transform.forward,
                out outHit
                );

        }
        else
        {
            #region Mouse to Screen RayCast
            dir = Camera.main.ScreenPointToRay(GetComponent<InputManager>().screenMousePOS);

            Physics.Raycast(
                Camera.main.transform.position,
                dir.direction,
                out outHit);

            #endregion
        }

        //Assign piece if hit

        try
        {
            HitPiece = outHit.collider.gameObject.transform.parent.GetComponent<TilePiece>();

        }
        catch (System.Exception)
        {
            HitPiece = null;
            //throw;
        }
    }
    IEnumerator ShowTemporaryMessage(string message, float delay)
    {

        textElement.text = message;
        textElement.enabled = true;
        yield return new WaitForSeconds(delay);
        textElement.enabled = false;
        textElement.text = "";
    }

}
