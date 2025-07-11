using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Portal : MonoBehaviourPun
{
    public enum PortalType { Blue, Orange }
    public PortalType portalType;
    public int ownerActorNumber; // 유지, 단 연결엔 사용 안함
    public Transform destinationPortal;

    public void SetOwnerAndType(int actorNum, PortalType type)
    {
        ownerActorNumber = actorNum;
        portalType = type;
    }

    public void TryAutoConnect()
    {
        StartCoroutine(CoFindAndConnectOtherPortal());
    }

    IEnumerator CoFindAndConnectOtherPortal()
    {
        yield return new WaitForSeconds(0.2f); // 네트워크 동기화 대기

        Portal pair = null;
        foreach (Portal p in FindObjectsOfType<Portal>())
        {
            if (p == this) continue;
            // 타입만 다르면(== 블루는 오렌지, 오렌지는 블루) 무조건 연결!
            if (p.portalType != portalType)
            {
                pair = p;
                break;
            }
        }
        if (pair != null)
        {
            // 양쪽 모두 서로 연결!
            this.destinationPortal = pair.transform;
            pair.destinationPortal = this.transform;
            Debug.Log($"[Portal] 연결 완료: {portalType} <-> {pair.portalType}");
        }
        else
        {
            Debug.LogWarning($"[Portal] 쌍 포탈을 못 찾음! (type={portalType}, pos={transform.position})");
        }
        DebugAllPortals();
    }

    void DebugAllPortals()
    {
        foreach (Portal p in FindObjectsOfType<Portal>())
        {
            Debug.Log($"[Portal-Status] type={p.portalType}, owner={p.ownerActorNumber}, pos={p.transform.position}, dest={(p.destinationPortal ? p.destinationPortal.name : "null")}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destinationPortal == null)
        {
            Debug.LogWarning($"[Portal] destinationPortal이 연결되어 있지 않음! (type={portalType})");
            return;
        }

        // === 플레이어 포탈 진입 ===
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                Vector3 exitPosition = destinationPortal.position - destinationPortal.forward * 1.5f;
                other.transform.position = exitPosition;

                Vector3 forward = -destinationPortal.forward; forward.y = 0f;
                if (forward != Vector3.zero)
                    other.transform.rotation = Quaternion.LookRotation(forward);

                cc.enabled = true;
                Debug.Log($"[Portal] 플레이어 이동 완료! {portalType} -> {destinationPortal.GetComponent<Portal>().portalType}");
            }
        }
        // === 오브젝트 포탈 진입 ===
        else if (other.CompareTag("Grabbable") && other.attachedRigidbody != null)
        {
            Rigidbody rb = other.attachedRigidbody;
            Vector3 exitPos = destinationPortal.position - destinationPortal.forward * 1.5f;
            rb.position = exitPos;

            Quaternion targetRot = Quaternion.LookRotation(-destinationPortal.forward, Vector3.up);
            rb.rotation = targetRot;

            Quaternion rotationDelta = Quaternion.FromToRotation(transform.forward, -destinationPortal.forward);
            rb.velocity = rotationDelta * rb.velocity * 0.93f;
            rb.angularVelocity = rotationDelta * rb.angularVelocity * 0.93f;
            Debug.Log($"[Portal] 오브젝트 이동 완료! {portalType} -> {destinationPortal.GetComponent<Portal>().portalType}");
        }
    }
}
