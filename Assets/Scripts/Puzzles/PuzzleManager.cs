using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    /*The puzzle manager will create a grid and attach the various tiles to the center
     * of those grid points. When a tile is clicked, a random piece of the available pieces 
     * will be selected to display. Below a certain number of pieces being available, the 
     * puzzle manager will be less random and provide a puzzle piece based on the grid point
     * the tile is being requested at. One of however many tiles matching that grid point
     * will be selected.*/

    [SerializeField] private GameObject gridStart;
    [SerializeField] private GameObject prfbGridPoint;
    [SerializeField] private GameObject[] tileSets;
    [Tooltip("Puzzle Manager only supports SQUARE grids, so make sure your tilesets are squared!")]
    [SerializeField] private int columnCount;
    
    [Tooltip("Standard unit in Unity is 1 = 1 meter. Therefore value must be between 0.1-0.5")]
    [SerializeField] private float columnSpacingY;
    [SerializeField] private float columnSpacingX;
    [SerializeField] private GridPoint[,] gridPoints;

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
}
