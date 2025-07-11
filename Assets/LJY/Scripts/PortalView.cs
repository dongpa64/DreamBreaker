using UnityEngine;

public class PortalView : MonoBehaviour
{
    public PortalView linkedPortal;        // 상대 포탈의 PortalView 연결
    public MeshRenderer screen;            // PortalVisual(쿼드) MeshRenderer
    public Camera portalCamera;            // PortalCamera
    public Transform playerCamera;         // MainCamera(플레이어)

    void LateUpdate()
    {
        if (!linkedPortal || !playerCamera) return;

        // 1. 플레이어 위치/시선 → 상대 포탈 기준 좌표로 변환
        Vector3 localPos = linkedPortal.transform.InverseTransformPoint(playerCamera.position);
        Vector3 localDir = linkedPortal.transform.InverseTransformDirection(playerCamera.forward);

        // 2. 입구→출구 회전차이 보정(중요)
        Quaternion rotDelta = transform.rotation * Quaternion.Inverse(linkedPortal.transform.rotation);

        // 3. 내 포탈 기준으로 복원 (위치/회전)
        portalCamera.transform.position = transform.TransformPoint(localPos);
        portalCamera.transform.rotation = rotDelta * Quaternion.LookRotation(localDir, Vector3.up);

        // 4. nearClipPlane 보정(너무 가까이 붙었을 때 대비)
        float dist = Vector3.Distance(portalCamera.transform.position, transform.position);
        portalCamera.nearClipPlane = Mathf.Max(0.01f, dist - 0.2f);

        // 5. 머티리얼에 RenderTexture(_MainTex) 연결
        if (screen && portalCamera.targetTexture)
            screen.material.SetTexture("_MainTex", portalCamera.targetTexture);
    }
}
