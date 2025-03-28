using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform Pleyer;
    
    [SerializeField]
    private float SmoothTime = 5f;

    private Transform tr;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        tr = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        Vector3 desiredPosition = new Vector3(Pleyer.position.x, Pleyer.position.y, tr.position.z);
        transform.position = Vector3.SmoothDamp(tr.position, desiredPosition, ref velocity, SmoothTime * Time.fixedDeltaTime);
    }
}
