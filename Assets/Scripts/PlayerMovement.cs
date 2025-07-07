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

    public float turnSpeed = 30f; // �ε巯�� ȸ�� �ӵ�

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
                // ���� �潺ƽ: �̵�
                x = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.LTouch);
                z = ARAVRInput.GetAxis("Vertical", ARAVRInput.Controller.LTouch);

                // ���� ��ư
                jumpPressed = ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.RTouch);

                // ������ �潺ƽ: ȸ�� �Է�
                turnInput = ARAVRInput.GetAxis("Horizontal", ARAVRInput.Controller.RTouch);
                break;
        }
        // ī�޶� ���� ���� ���ϱ�
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // ���� ���⸸ ���
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // �̵� ó��
        Vector3 move = camRight * x + camForward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ���� ó��
        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
            Debug.Log("Jump!");
        }

        // �߷� ó��
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ȸ�� ó�� (������ �潺ƽ)
        if (Mathf.Abs(turnInput) > 0.1f)  // Dead zone ó��
        {
            transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);
        }
    }
}
