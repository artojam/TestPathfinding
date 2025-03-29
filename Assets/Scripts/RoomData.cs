using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomData : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;
    
    [SerializeField]
    private TileBase floorTile;

    [SerializeField] 
    private Vector2Int size;
    
    [SerializeField] 
    private Vector2Int pos;

    [SerializeField] 
    private Pathfinding path;

    private byte[] grid;
    private Vector2Int offset;

    private void Awake()
    {
        offset = pos - (size / 2);

        Pathfinding.GenerateGridFromTilemap(tilemap, pos, size, floorTile, ref grid);

        //Debug.Log(grid.Length);
        path.InitGrid(grid, size.x, size.y, offset);
    }
}
