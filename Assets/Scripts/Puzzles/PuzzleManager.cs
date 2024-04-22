using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PuzzleManager : MonoBehaviour
{
    /*The puzzle manager will create a grid and attach the various tiles to the center
     * of those grid points. When a tile is clicked, a random piece of the available pieces 
     * will be selected to display. Below a certain number of pieces being available, the 
     * puzzle manager will be less random and provide a puzzle piece based on the grid point
     * the tile is being requested at. One of however many tiles matching that grid point
     * will be selected.*/

    #region Grid Related Variables
    [SerializeField] private GameObject gridStart;
    [SerializeField] private GameObject prfbGridPoint;
    [SerializeField] private GameObject[] tileSets;
    [Tooltip("Puzzle Manager only supports SQUARE grids, so make sure your tilesets are squared!")]
    [SerializeField] private int columnCount;
    
    [Tooltip("Standard unit in Unity is 1 = 1 meter. Therefore value must be between 0.1-0.5")]
    [SerializeField] private float columnSpacingY;
    [SerializeField] private float columnSpacingX;
    [SerializeField] private GridPoint[,] gridPoints;
    #endregion

    #region Input Manipulation Variables
    //the currently hovered/hit piece
    [SerializeField] public TilePiece hitPiece;
    //the tile being manipulated by the mouse
    [SerializeField] public TilePiece heldPiece;
    //to return to original
    [SerializeField] public GameObject originGO;

    //material for held piece
    Material hpMaterial;
   //for the above material
   [SerializeField] float semiTransparent = .5f;

    UnityEngine.Color color;

    #endregion

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
    void Start()
    {
        gridPoints = GenerateGrid(gridStart.transform, columnCount);
        GenerateTileset(tileSets, gridPoints);
    }

    // Update is called once per frame
    void Update()
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
        //using the transform given and the grid size, create a multi-dim array of grid points
        GridPoint[,] outGrid = new GridPoint[gridSize, gridSize];
        GameObject GO;
        Vector3 pos = startPoint.position;
        float spX = pos.x;
        float spY = pos.y;
        float spZ = pos.z;
        int gpCount = 1;

        //we are going to create the grid with a nested for loop and place one point at each spot
        //columns
        for (int i = 0; i < gridSize; i++)
        {
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

    private void GenerateTileset(GameObject[] collection, GridPoint[,] targetGrid)
    {
        //for now we will just generate the full image to check spacing and functionality
        Tileset TS = collection[0].GetComponent<Tileset>();

        //iteration counter
        int counter = 0;

   /*     if (TS != null)
        {
            Debug.Log("Tileset at index 0 found is: " + TS.setName);
        }*/

        TilePiece[] tilePieces = TS.tilePieces;

        //add randomness later

        //first, let's setup a double for loop, find the square root size

        int gridSize = (int)Mathf.Sqrt(targetGrid.Length);
        for (int i = 0; i < gridSize; i++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                //instantiate the desired tile piece, parent to grid point
                Vector3 pos = targetGrid[i, y].transform.position;

               TilePiece cPiece = Instantiate(
                   tilePieces[counter], 
                   pos, 
                   Quaternion.identity, 
                   targetGrid[i, y].transform);
                counter++;
            }
        }
    }

    private void ValidateTilesets()
    {
        if (columnCount == 0 || columnCount == 1 || tileSets == null) return;
        //for tracking our iteration, might change this
        int i = 0;
        //the goal here is to reject any tileset that is not a matching square for the column count
        foreach (var tileSet in tileSets)
        {
            Tileset CS = tileSet.GetComponent<Tileset>();

            if (CS != null)
            {
                bool isSquared = columnCount == Mathf.Sqrt(CS.tilePieces.Length);

                if (!isSquared)
                {
                    //remove the offending item
                    tileSets.SetValue(null, i);

                    //log the error
                    string msg = $"The tileset input has a length that cannot be squared to using the columnCount. {tileSet.name}";
                    Debug.LogError(msg);
                }

            }
            i++;
        }
    }

    public void AssignPiece()
    {
        heldPiece = hitPiece;

        //re-parent to having no parent, store original        
        originGO = heldPiece.transform.parent.gameObject;
        heldPiece.transform.SetParent(transform.parent, false);

        //set to mosPOS
        heldPiece.transform.position = GetComponent<InputManager>().mosPOS;

        #region Visual Changes
        //set held piece scale and opacity to semi-transparent
        //opacity:
        hpMaterial = heldPiece.transform.Find("Quad").GetComponent<MeshRenderer>().material;
        MaterialExtensions.ToTransparentMode(hpMaterial);

        //transparency:
        color = hpMaterial.color;
        hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, semiTransparent);

        //scale:
        heldPiece.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        #endregion
    }

    public void SwapPiece()
    {
        TilePiece tempPiece;
        //put the currently heldPiece on the spot of the hitPiece
        tempPiece = heldPiece;
        tempPiece.transform.SetParent(hitPiece.transform.parent.gameObject.transform, false);
        tempPiece.transform.position = hitPiece.transform.parent.gameObject.transform.position;

        #region Swapped Piece Visual Reset
        //opacity:
        MaterialExtensions.ToOpaqueMode(hpMaterial);

        //transparency:
        color = hpMaterial.color;
        hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, 1);
        hpMaterial = null;

        //scale:
        tempPiece.transform.localScale = new Vector3(1f, 1f, 1f);
        #endregion

        //replace with new
        heldPiece = hitPiece;
        heldPiece.transform.SetParent(transform.parent, false);

        //set to mosPOS
        heldPiece.transform.position = GetComponent<InputManager>().mosPOS;

        #region New Held Piece Visual Changes
        //set held piece scale and opacity to semi-transparent
        //opacity:
        hpMaterial = heldPiece.transform.Find("Quad").GetComponent<MeshRenderer>().material;
        MaterialExtensions.ToTransparentMode(hpMaterial);

        //transparency:
        color = hpMaterial.color;
        hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, semiTransparent);

        //scale:
        heldPiece.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        #endregion
    }

    public void ResetHeldPiece()
    {
        #region Visual Changes
        //opacity:
        MaterialExtensions.ToOpaqueMode(hpMaterial);

        //transparency:
        color = hpMaterial.color;
        hpMaterial.color = new UnityEngine.Color(color.r, color.g, color.b, 1);
        hpMaterial = null;

        //scale:
        heldPiece.transform.localScale = new Vector3(1f, 1f, 1f);
        #endregion

        //reparent to original GP/GO
        heldPiece.transform.SetParent(originGO.transform, false);
        heldPiece.transform.position = originGO.transform.position;

        //reset variables
        originGO = null;
        heldPiece = null;
    }

    private void FindTilePiece()
    {
        RaycastHit outHit;
        Ray dir;

        //perform a raycast for the backgroundPiece and return early if a piece is already held

        if (heldPiece)
        {
            // forward of the held piece for direction
            Physics.Raycast(
                heldPiece.transform.position,
                heldPiece.transform.forward,
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
}
