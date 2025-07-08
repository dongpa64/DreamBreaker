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
            if (cc != null)
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
