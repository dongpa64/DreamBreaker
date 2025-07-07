using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlatformType { PC, Oculus, Vive }
    public PlatformType currentPlatform = PlatformType.PC;

    public float moveSpeed = 2f;
    public float jumpPower = 5f;
    public float gravity = -9.81f;

    public float turnSpeed = 30f; // 부드러운 회전 속도

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = 0f, z = 0f;
        float turnInput = 0f;
        bool jumpPressed = false;

        switch (currentPlatform)
        {
            case PlatformType.PC:
                x = Input.GetAxis("Horizontal");
                z = Input.GetAxis("Vertical");
                jumpPressed = Input.GetButtonDown("Jump");
                break;

            case PlatformType.Oculus:
            case PlatformType.Vive:
                // 왼쪽 썸스틱: 이동
                x = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.LTouch);
                z = ARAVRInput.GetAxis("Vertical", ARAVRInput.Controller.LTouch);

                // 점프 버튼
                jumpPressed = ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.RTouch);

                // 오른쪽 썸스틱: 회전 입력
                turnInput = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.RTouch);
                break;
        }
        // 카메라 기준 방향 구하기
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // 수평 방향만 고려
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 이동 처리
        Vector3 move = camRight * x + camForward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 점프 처리
        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
            Debug.Log("Jump!");
        }

        // 중력 처리
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 회전 처리 (오른쪽 썸스틱)
        if (Mathf.Abs(turnInput) > 0.1f)  // Dead zone 처리
        {
            transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);
        }
    }
}
