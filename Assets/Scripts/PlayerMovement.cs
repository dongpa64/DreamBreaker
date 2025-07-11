using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
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

    // 네트워크 동기화를 위한 변수들
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float lastNetworkUpdateTime;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if(!photonView.IsMine) 
            enabled = false;    
    }

    void Update()
    {
        // 자신의 플레이어가 아닐 경우, 네트워크로부터 받은 데이터를 사용하여 움직임을 보간
        if (!photonView.IsMine)
        {
            // 부드러운 보간을 위해 Lerp 사용
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f); // 10f는 보간 속도
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f); // 10f는 보간 속도
            return;
        }
        if (photonView.IsMine)
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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 이 클라이언트가 데이터를 전송합니다. 자신의 위치와 회전을 보냅니다.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(velocity); // velocity도 동기화하면 점프 등이 더 자연스러울 수 있습니다.
        }
        else
        {
            // 다른 클라이언트로부터 데이터를 수신합니다. 수신된 위치와 회전을 저장합니다.
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
        }
    }
}
