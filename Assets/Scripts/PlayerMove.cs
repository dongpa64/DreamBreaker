using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // <-- 추가
    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
    // -->

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        cc.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. 중력 적용
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded)
        {
            velocity.x *= 0.93f; // 0.9~0.98 사이에서 조절
            velocity.z *= 0.93f;
        }


        cc.Move(velocity * Time.deltaTime);
    }
}
