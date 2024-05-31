using UnityEngine;
public class GridPoint : MonoBehaviour
{
    [SerializeField] private int gridPosition = 1;
    public void SetGridPosition(int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
    public int GetGridPosition()
    {
        return gridPosition;
    }
}
