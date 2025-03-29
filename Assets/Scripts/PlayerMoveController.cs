using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float Speed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Transform tr;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = transform;
    }

    private void Update()
    {
        MoveInputKeyboard();
    }

    private void FixedUpdate()
    {

        Move();
    }

    private void MoveInputKeyboard()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(moveX, moveY);
    }
    private void Move()
    {
        //rb.MovePosition(rb.position + movement * Speed * Time.fixedDeltaTime);
        rb.velocity = movement * Speed * 10 * Time.fixedDeltaTime;
    }
}
