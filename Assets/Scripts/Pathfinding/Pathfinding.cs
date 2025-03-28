using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class Pathfinding : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;

    // временно публичные надо зделать конструктор
    public byte[] grid;
    
    public int gridWidth;
    public int gridHeight;
    
    public Vector2Int gridOffset;



    private int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 0, -1 }, { -1, 0 }, { -1, -1 }, { -1, 1 }, { 1, -1 } };

    private PriorityQueue<Node> WaitingNodes = new PriorityQueue<Node>(); // хранит узлы для проверки
    private HashSet<Vector2Int> CheckedNodes = new HashSet<Vector2Int>(); // хранит провереные позиции

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
        Vector2Int offsetRoom = (posRoom - _sizeRoom / 2);

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

    // поиск пути немного улучшеный A*
    public int FindPath(Vector3 start, Vector3 target, ref Vector3[] pathBuffer)
    {
        
        Vector2Int posStart = GetGridPosition(start);
        Vector2Int posTarget = GetGridPosition(target);

        if (GetToLine(posStart, posTarget))
        {
            pathBuffer[0] = target;
            return 1;
        }

        WaitingNodes.Clear();
        CheckedNodes.Clear();
        Node[] parentNodes = new Node[grid.Length / 2];

        Node startNode = new Node(0, posStart, posTarget, -1);
        CheckedNodes.Add(startNode.pos);

        GetNeighbourNodes(startNode, posTarget, ref WaitingNodes, CheckedNodes, ref parentNodes, 0);

        for (int length = 1; length < grid.Length; length++)
        {
            Node currentNode = WaitingNodes.Dequeue();

            if (currentNode.pos == posTarget)
            {
                int lengthBuffer = CalculatePathFromNode(currentNode, start, ref pathBuffer, parentNodes);
                Array.Reverse(pathBuffer);
                return lengthBuffer;
            }

            GetNeighbourNodes(currentNode, posTarget, ref WaitingNodes, CheckedNodes, ref parentNodes, length);
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

    private Vector2Int GetNeighbour(Vector2Int pos)
    {
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            Vector2Int dir = new Vector2Int(directions[i, 0], directions[i, 1]);

            if (grid[(pos.x + dir.x) * gridWidth + (pos.y + dir.y)] == 255)
                return dir;
        }
        return new Vector2Int(0, 0);
    }

    // строит и зглаживает путь [Временное решение]
    private int CalculatePathFromNode(Node node, Vector3 posStart, ref Vector3[] pathBuffer, Node[] parentNodesBuffer)
    {
        //if (node == null) return 0;
        
        int indexNode = 0;

        Node currentNode = node;
        Node oldNode = new Node();

        Vector2Int endPos = node.pos;

        bool isPathToEnd = true;

        pathBuffer[indexNode].x = GetWorldAxis(currentNode.pos.x);
        pathBuffer[indexNode].y = GetWorldAxis(currentNode.pos.y);

        DrawPoint(pathBuffer[indexNode], Color.green);

        indexNode++;

        oldNode = currentNode;
        currentNode = parentNodesBuffer[currentNode.indexParent];

        while (isPathToEnd)
        {
            if (GetToLineWorld(currentNode, posStart))
            {
                Vector2Int dir = GetNeighbour(oldNode.pos);
                pathBuffer[indexNode].x = GetWorldAxis(oldNode.pos.x) + 0.5f * -dir.x;
                pathBuffer[indexNode].y = GetWorldAxis(oldNode.pos.y) + 0.5f * -dir.y;
                
                //DrawPoint(pathBuffer[indexNode], Color.green);
                
                indexNode++;
                if (Vector3.Distance(posStart, GetWorldPosition(currentNode.pos)) > 0.5f)
                {
                    dir = GetNeighbour(currentNode.pos);
                    pathBuffer[indexNode].x = GetWorldAxis(currentNode.pos.x) + 0.5f * -dir.x;
                    pathBuffer[indexNode].y = GetWorldAxis(currentNode.pos.y) + 0.5f * -dir.y;

                    //DrawPoint(pathBuffer[indexNode], Color.white);

                    indexNode++;
                }

                isPathToEnd = false;
                return indexNode;
            }

            if (GetToLine(currentNode.pos, endPos))
            {
                oldNode = currentNode;
                currentNode = parentNodesBuffer[currentNode.indexParent];
                //DrawPoint(currentNode.pos, Color.red);
            }
            else
            {

                Vector2Int dir = GetNeighbour(oldNode.pos);
                pathBuffer[indexNode].x = GetWorldAxis(oldNode.pos.x) + 0.5f * -dir.x;
                pathBuffer[indexNode].y = GetWorldAxis(oldNode.pos.y) + 0.5f * -dir.y;


                //DrawPoint(pathBuffer[indexNode], Color.green);

                indexNode++;
                
                endPos = oldNode.pos;
                oldNode = currentNode;

                currentNode = parentNodesBuffer[currentNode.indexParent];
            }
        }

        return indexNode;
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
                5f);

        Debug.DrawLine(
            pos - new Vector3(0.25f, 0),
            pos + new Vector3(0.25f, 0),
            color,
            5f);
    }

    public bool GetToLine(Vector2Int posStart, Vector2Int posEnd)
    {
        Vector3 posWorldStart = GetWorldPosition(posStart);
        Vector3 posWorldEnd = GetWorldPosition(posEnd);

        RaycastHit2D target = Physics2D.Linecast(posWorldStart, posWorldEnd, layer);

        return target.collider == null;
    }

    public bool GetToLineWorld(Node node, Vector3 posEnd)
    {
        Vector2 posWorldStart = new Vector2(node.pos.x + gridOffset.x, node.pos.y + gridOffset.y);

        RaycastHit2D target = Physics2D.Linecast(posWorldStart, posEnd, layer);

        return target.collider == null;
    }

    public bool GetToLineWorld(Vector3 posStart, Vector3 posEnd) =>
        Physics2D.Linecast(posStart, posEnd, layer).collider == null;

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

    // временно находится здесь надо зделать отдельный класс агента
    public IEnumerator AgentMove(Rigidbody2D agent, Transform target, float speed, string name)
    {
        Vector3 lastTargetPos = target.position;
        Vector3[] path = new Vector3[(gridHeight)];
        
        
        int length = FindPath(agent.position, lastTargetPos, ref path);
        int index = gridHeight - length;

        Vector3 nextPoint = path[index];
        Vector2 dir;

        while (true)
        {
            // Если агент дошёл до последней точки и target не двигается, он должен стоять
            if (index >= length + gridHeight - length && Vector3.Distance(agent.position, target.position) <= 0.5f)
            {
                agent.velocity = Vector2.zero;
                yield return null; // Ждём, пока target не двинется
                continue;
            }
            
            // Если target сменил позицию, пересчитываем путь
            if (index >= length + gridHeight - length - 1 || Vector3.Distance(lastTargetPos, target.position) > 0.05f)
            {
                if (GetToLineWorld(agent.position, target.position))
                {
                    lastTargetPos = target.position;
                    length = 1;
                    path[gridHeight - length] = target.position;
                    
                    index = gridHeight - length;
                }
                else
                {
                    lastTargetPos = target.position;
                    length = FindPath(agent.position, lastTargetPos, ref path);
                    index = gridHeight - length;
                }
            }            
            
            // Двигаемся к следующей точке пути
            nextPoint = path[index];
            dir = ((Vector2)nextPoint - agent.position).normalized;
            agent.MovePosition(agent.position + (dir * speed * Time.fixedDeltaTime));

            if (grid[GetGridPosition(agent.position).x * gridWidth + GetGridPosition(agent.position).y] == 255)
            {
                Debug.Log("Wall");
            }


            Debug.DrawLine(agent.position, nextPoint, Color.green, 0.05f);

            // Если достигли текущей точки пути, переходим к следующей
            if (Vector3.Distance(agent.position, nextPoint) <= 0.05f && index < length + gridHeight - length - 1)
            {
                index++;
            }

            yield return new WaitForFixedUpdate();
        }
    }
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

