using UnityEngine;
public class TilePiece : MonoBehaviour
{
    [SerializeField] private int tileNumber = 1;
    public int GetTileNumber()=>tileNumber;

}
