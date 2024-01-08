using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint : MonoBehaviour
{
    //the purpose of this class is to compare the tiles to their position on the board
    //should always be at least one, no tiles will be labeled zero
    [SerializeField] private int gridPosition = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGridPosition(int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public int GetGridPosition()
    {
        return gridPosition;
    }
}
