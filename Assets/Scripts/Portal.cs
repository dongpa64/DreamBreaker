using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform destinationPortal;

    private void OnDrawGizmos()
    {
        // Z+ 방향(파란 화살표) = 입구 방향
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destinationPortal == null)
            return;

        // --- 플레이어 포탈 통과 ---
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if (cc != null && playerMove != null)
            {
                cc.enabled = false;

                // 1. 위치 이동 (출구 뒤쪽, -Z 방향)
                Vector3 exitPosition = destinationPortal.position - destinationPortal.forward * 1.5f;
                other.transform.position = exitPosition;

                // 2. 회전 (출구 뒤(-Z) 방향을 바라보게)
                Vector3 forward = -destinationPortal.forward;
                forward.y = 0f;
                if (forward != Vector3.zero)
                    other.transform.rotation = Quaternion.LookRotation(forward);

                // 3. 속도 변환(포탈2 스타일: 입구→출구 회전 반영)
                Quaternion rotationDelta = Quaternion.FromToRotation(transform.forward, -destinationPortal.forward);
                Vector3 oldVelocity = playerMove.Velocity;
                Vector3 newVelocity = rotationDelta * oldVelocity;

                // 4. 포탈 출구 감속 (미끄러짐 완화)
                playerMove.Velocity = newVelocity * 0.93f; // 0.9~0.98로 미끄러짐 정도 조절

                cc.enabled = true;
            }
        }
        // --- 큐브(오브젝트) 포탈 통과 ---
        else if (other.CompareTag("Grabbable") && other.attachedRigidbody != null)
        {
            Rigidbody rb = other.attachedRigidbody;

            // 1. 위치 이동 (출구 뒤쪽, -Z 방향)
            Vector3 exitPos = destinationPortal.position - destinationPortal.forward * 1.5f;
            rb.position = exitPos;

            // 2. 회전 (출구 뒤(-Z) 방향을 바라보게)
            Quaternion targetRot = Quaternion.LookRotation(-destinationPortal.forward, Vector3.up);
            rb.rotation = targetRot;

            // 3. 속도/각속도 변환
            Quaternion rotationDelta = Quaternion.FromToRotation(transform.forward, -destinationPortal.forward);
            rb.velocity = rotationDelta * rb.velocity * 0.93f;         // 감속 포함
            rb.angularVelocity = rotationDelta * rb.angularVelocity * 0.93f;
        }
    }
}
