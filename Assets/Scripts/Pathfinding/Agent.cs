using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


// реализация класса временная 
public class Agent : MonoBehaviour
{
    public float speed;
    public Transform target;

    [SerializeField]
    private Pathfinding path;
    
    private Rigidbody2D start;

    //private List<Vector3> _path = new List<Vector3>();

    private void Start()
    {
        start = GetComponent<Rigidbody2D>();
        StartCoroutine(path.AgentMove(start, target, 2.5f, name));
    }

    /*private void OnDrawGizmos()
    {
        if (Application.isPlaying && _path.Count >= 1)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.DrawLine(_path[i], _path[i+1]);
            }
        }
    }*/
}
