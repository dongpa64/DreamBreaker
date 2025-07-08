using UnityEngine;

public class PortalView : MonoBehaviour
{
    public PortalView linkedPortal;          // 상대 포탈 연결 (코드에서 할당)
    public MeshRenderer screen;              // PortalScreen(쿼드) MeshRenderer
    public Camera portalCamera;              // PortalCamera
    public Transform playerCamera;           // MainCamera

    void LateUpdate()
    {
        if (!linkedPortal || !playerCamera) return;

        // (1) 플레이어 위치/방향을 상대 포탈 기준으로 변환
        Vector3 localPos = linkedPortal.transform.InverseTransformPoint(playerCamera.position);
        Vector3 localDir = linkedPortal.transform.InverseTransformDirection(playerCamera.forward);

        // (2) 포탈의 입구→출구 회전차 보정
        Quaternion rotDelta = transform.rotation * Quaternion.Inverse(linkedPortal.transform.rotation);

        // (3) 내 포탈 기준으로 다시 위치/회전 복원
        portalCamera.transform.position = transform.TransformPoint(localPos);
        portalCamera.transform.rotation = rotDelta * Quaternion.LookRotation(localDir, Vector3.up);

        // (4) nearClipPlane 보정 (필요시)
        float dist = Vector3.Distance(portalCamera.transform.position, transform.position);
        portalCamera.nearClipPlane = Mathf.Max(0.01f, dist - 0.2f);

        // (5) RT 연결
        if (screen && portalCamera.targetTexture)
            screen.material.SetTexture("_MainTex", portalCamera.targetTexture);
    }

}
