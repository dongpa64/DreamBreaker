using UnityEngine;

public class PortalView : MonoBehaviour
{
    public PortalView linkedPortal;        // ��� ��Ż�� PortalView ����
    public MeshRenderer screen;            // PortalVisual(����) MeshRenderer
    public Camera portalCamera;            // PortalCamera
    public Transform playerCamera;         // MainCamera(�÷��̾�)

    void LateUpdate()
    {
        if (!linkedPortal || !playerCamera) return;

        // 1. �÷��̾� ��ġ/�ü� �� ��� ��Ż ���� ��ǥ�� ��ȯ
        Vector3 localPos = linkedPortal.transform.InverseTransformPoint(playerCamera.position);
        Vector3 localDir = linkedPortal.transform.InverseTransformDirection(playerCamera.forward);

        // 2. �Ա����ⱸ ȸ������ ����(�߿�)
        Quaternion rotDelta = transform.rotation * Quaternion.Inverse(linkedPortal.transform.rotation);

        // 3. �� ��Ż �������� ���� (��ġ/ȸ��)
        portalCamera.transform.position = transform.TransformPoint(localPos);
        portalCamera.transform.rotation = rotDelta * Quaternion.LookRotation(localDir, Vector3.up);

        // 4. nearClipPlane ����(�ʹ� ������ �پ��� �� ���)
        float dist = Vector3.Distance(portalCamera.transform.position, transform.position);
        portalCamera.nearClipPlane = Mathf.Max(0.01f, dist - 0.2f);

        // 5. ��Ƽ���� RenderTexture(_MainTex) ����
        if (screen && portalCamera.targetTexture)
            screen.material.SetTexture("_MainTex", portalCamera.targetTexture);
    }
}
