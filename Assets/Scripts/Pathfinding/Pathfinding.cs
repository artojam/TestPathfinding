using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Pathfinding : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;

    // временно публичные надо зделать конструктор
    public byte[] grid;

    public int gridWidth;
    public int gridHeight;

    public Vector2Int gridOffset;

    private int[,] directions = {
        { 0, 1 },
        { 1, 0 },
        { 0, -1 },
        { -1, 0 },
    };

    // генерация сетки по Tilemap
    public static void GenerateGridFromTilemap(
        Tilemap tilemap,
        Vector2Int posRoom,
        Vector2Int _sizeRoom,
        TileBase Floor,
        ref byte[] gridBuffer
        )
    {
        gridBuffer = new byte[_sizeRoom.x * _sizeRoom.y];
        BoundsInt bounds = tilemap.cellBounds;
        Vector2Int offsetRoom = posRoom - (_sizeRoom / 2) + (posRoom / -4);

        for (int x = 0; x < _sizeRoom.x; x++)
        {
            for (int y = 0; y < _sizeRoom.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(offsetRoom.x + x, offsetRoom.y + y, 0);
                TileBase tile = tilemap.GetTile(tilePosition);
                gridBuffer[x * _sizeRoom.x + y] = tile == Floor ? (byte)0 : (byte)255;
            }
        }
    }

    public void InitGrid(byte[] _grid, int _gridWidth, int _gridHeight, Vector2Int _gridOffset)
    {
        grid = _grid;

        gridWidth = _gridWidth;
        gridHeight = _gridHeight;

        gridOffset = _gridOffset;
    }

    // поиск пути немного улучшеный A*
    public int FindPath(Vector3 posStart, Vector3 posEnd, ref Vector3[] pathPointsBuffer)
    {
        if (HasObstacleBetweenWorld(posStart, posEnd))
        {
            pathPointsBuffer[0] = posEnd;
            return 1;
        }

        Vector2Int gridPosStart = GetGridPosition(posStart);
        Vector2Int gridPosEnd = GetGridPosition(posEnd);

        PriorityQueue<Node> WaitingNodes = new PriorityQueue<Node>(); // хранит узлы для проверки
        HashSet<Vector2Int> CheckedNodes = new HashSet<Vector2Int>(); // хранит провереные позиции

        Node[] parentNodes = new Node[grid.Length / 2];

        Node startNode = new Node(0, gridPosStart, gridPosEnd, -1);
        CheckedNodes.Add(startNode.pos);

        GetNeighbourNodes(startNode, gridPosEnd, ref WaitingNodes, CheckedNodes, ref parentNodes, 0);

        for (int length = 1; length < grid.Length; length++)
        {
            Node currentNode = WaitingNodes.Dequeue();

            if (currentNode.pos == gridPosEnd)
            {
                int lengthBuffer = CalculatePathFromNode(currentNode, posStart, ref pathPointsBuffer, parentNodes);
                Array.Reverse(pathPointsBuffer);
                return lengthBuffer;
            }

            GetNeighbourNodes(currentNode, gridPosEnd, ref WaitingNodes, CheckedNodes, ref parentNodes, length);
        }
        return 0;
    }

    // находит соседей узла
    private void GetNeighbourNodes(
        Node node,
        Vector2Int posTarget,
        ref PriorityQueue<Node> WaitingNodesBuffer,
        HashSet<Vector2Int> CheckedNodes,
        ref Node[] parentNodesBuffer, int len
        )
    {
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            Vector2Int pos = new Vector2Int(directions[i, 0] + node.pos.x, directions[i, 1] + node.pos.y);

            if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight && grid[pos.x * gridWidth + pos.y] < 255)
            {
                Node newNode = new Node(node.G + 1, pos, posTarget, len);
                if (!CheckedNodes.Contains(newNode.pos))
                {
                    WaitingNodesBuffer.Enqueue(newNode);
                    CheckedNodes.Add(newNode.pos);

                }

            }
        }

        parentNodesBuffer[len] = node;

    }


    // строит и зглаживает путь [Временное решение]
    private int CalculatePathFromNode(Node node, Vector3 posStart, ref Vector3[] pathBuffer, Node[] parentNodesBuffer)
    {
        //if (node == null) return 0;

        int indexNode = 0;

        Node currentNode = node;
        Node oldNode = new Node();

        Vector2Int gridPosStart = GetGridPosition(posStart);
        Vector2Int endPos = node.pos;

        bool isPathToEnd = true;



        pathBuffer[indexNode].x = GetWorldAxis(currentNode.pos.x);
        pathBuffer[indexNode].y = GetWorldAxis(currentNode.pos.y);

        //DrawPoint(pathBuffer[indexNode], Color.green);

        indexNode++;

        oldNode = currentNode;
        currentNode = parentNodesBuffer[currentNode.indexParent];

        while (isPathToEnd)
        {
            if (HasObstacleBetweenNodeToWorld(currentNode, posStart))
            {
                pathBuffer[indexNode].x = GetWorldAxis(oldNode.pos.x);// + 0.25f * -dir.x;
                pathBuffer[indexNode].y = GetWorldAxis(oldNode.pos.y);// + 0.25f * -dir.y;

                //DrawPoint(pathBuffer[indexNode], Color.green);

                indexNode++;

                pathBuffer[indexNode].x = GetWorldAxis(currentNode.pos.x);// + 0.25f * -dir.x;
                pathBuffer[indexNode].y = GetWorldAxis(currentNode.pos.y);// + 0.25f * -dir.y;

                //DrawPoint(pathBuffer[indexNode], Color.white);

                indexNode++;

                isPathToEnd = false;
                return indexNode;
            }

            if (PseudoLinecast(currentNode.pos, endPos))
            {
                oldNode = currentNode;
                currentNode = parentNodesBuffer[currentNode.indexParent];
                //DrawPoint(currentNode.pos, Color.red);
            }
            else
            {

                pathBuffer[indexNode].x = GetWorldAxis(oldNode.pos.x);
                pathBuffer[indexNode].y = GetWorldAxis(oldNode.pos.y);


                //DrawPoint(pathBuffer[indexNode], Color.green);

                indexNode++;

                endPos = oldNode.pos;
                oldNode = currentNode;

                currentNode = parentNodesBuffer[currentNode.indexParent];
            }
        }

        return indexNode;
    }

    public bool PseudoLinecast(Vector2Int start, Vector2Int end)
    {
        int width = gridWidth;
        int height = gridHeight;

        int dx = Mathf.Abs(end.x - start.x), dy = Mathf.Abs(end.y - start.y);
        int sx = (start.x < end.x) ? 1 : -1, sy = (start.y < end.y) ? 1 : -1;
        int err = dx - dy;

        Vector2Int pos = start;

        while (true)
        {

            if (grid[pos.x * gridWidth + pos.y] == 255)
                return false;

            if (pos == end)
                return true;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; pos.x += sx; }
            if (e2 < dx) { err += dx; pos.y += sy; }
        }
    }


    public void DrawPoint(Vector2Int pos, Color color)
    {
        Debug.DrawLine(
                GetWorldPosition(pos) - new Vector3(0, 0.25f),
                GetWorldPosition(pos) + new Vector3(0, 0.25f),
                color,
                25f);

        Debug.DrawLine(
            GetWorldPosition(pos) - new Vector3(0.25f, 0),
            GetWorldPosition(pos) + new Vector3(0.25f, 0),
            color,
            25f);
    }

    public void DrawPoint(Vector3 pos, Color color)
    {
        Debug.DrawLine(
                pos - new Vector3(0, 0.25f),
                pos + new Vector3(0, 0.25f),
                color,
                25f);

        Debug.DrawLine(
            pos - new Vector3(0.25f, 0),
            pos + new Vector3(0.25f, 0),
            color,
            25f);
    }



    public bool HasObstacleBetweenGrid(Vector2Int posStart, Vector2Int posEnd)
    {
        Vector3 posWorldStart = GetWorldPosition(posStart);
        Vector3 posWorldEnd = GetWorldPosition(posEnd);

        RaycastHit2D target = Physics2D.Linecast(posWorldStart, posWorldEnd, layer);

        return target.collider == null;
    }

    public bool HasObstacleBetweenNodeToWorld(Node node, Vector3 posEnd)
    {
        Vector2 posWorldStart = new Vector2(node.pos.x + gridOffset.x, node.pos.y + gridOffset.y);

        RaycastHit2D target = Physics2D.Linecast(posWorldStart, posEnd, layer);

        return target.collider == null;
    }

    public bool HasObstacleBetweenWorld(Vector3 posStart, Vector3 posEnd) =>
        Physics2D.Linecast(posStart, posEnd, layer).collider == null;

    public Vector3 CheckDirWallsAround(Vector3 posAgent)
    {
        Collider2D hit = Physics2D.OverlapCircle(posAgent, 0.45f, layer);
        if (hit != null)
        {
            Vector3 closestPoint = hit.ClosestPoint(posAgent);

            // Вычисляем направление от центра круга к точке столкновения
            Vector3 dir = (closestPoint - posAgent).normalized;

            return dir;
        }

        return Vector3.zero;
    }


    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2Int gridPos = Vector2Int.RoundToInt(worldPosition);
        int x = Mathf.Clamp(gridPos.x - gridOffset.x, 0, gridWidth - 1);
        int y = Mathf.Clamp(gridPos.y - gridOffset.y, 0, gridHeight - 1);
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition) =>
        new Vector3(gridPosition.x + gridOffset.x, gridPosition.y + gridOffset.y);

    public float GetWorldAxis(int axis) => axis + gridOffset.x;

}

[Serializable]
public struct Node : IComparable<Node>
{
    public int G; // Длина пути от старта
    public int H; // Оценка расстояния до цели
    public int F => G + H; // Полная стоимость
    public Vector2Int pos; // Позиция в сетке
    public int indexParent; // индекс придыдущего узла
    

    public Node(int g, Vector2Int position, Vector2Int target, int index)
    {
        G = g;
        pos = position;
        indexParent = index;

        H = Mathf.Abs(position.x - target.x) + Mathf.Abs(position.y - target.y); // Манхэттенское расстояние
    }

    // Добавляем сравнение узлов по F (чем меньше, тем лучше)
    public int CompareTo(Node other)
    {
        int fComparison = F.CompareTo(other.F);
        if (fComparison == 0)
            return G.CompareTo(other.G); // При равном F выбираем узел с меньшим G
        return fComparison;
    }
}

