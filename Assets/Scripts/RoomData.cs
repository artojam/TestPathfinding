using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomData : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase floorTile;
    public Vector2Int size;
    public Vector2Int pos;
    public Vector2Int offset;

    public Pathfinding path;

    public byte[] grid;

    public List<Vector3> _path;

    private void Awake()
    {
        offset = pos - (size / 2);

        Pathfinding.GenerateGridFromTilemap(tilemap, pos, size, floorTile, ref grid);

        //Debug.Log(grid.Length);

        path.grid = grid;
        path.gridWidth = size.x;
        path.gridHeight = size.y;
        path.gridOffset = offset;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector2 _offset = offset; //+ new Vector2(0.5f, 0.5f);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3 pos = new Vector3(_offset.x + x, _offset.y + y, 0);
                    Gizmos.color = Color.blue;
                    if (grid[x * size.x + y] < 255)
                        Gizmos.DrawSphere(pos, 0.2f);
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(pos, 0.2f);
                    }

                }
            } 
        }
        return;
    }
}
