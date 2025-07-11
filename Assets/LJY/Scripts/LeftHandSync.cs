using UnityEngine;

public class LeftHandSync : MonoBehaviour
{
    public Transform leftHandAnchor; // �ν����Ϳ��� OVRCameraRig/TrackingSpace/LeftHandAnchor�� �巡��!
    public Vector3 handOffset = new Vector3(-0.3f, 1.2f, 0.2f); // ����(�÷��̾� ���� ��ġ, ���� ����)

    void LateUpdate()
    {
        if (leftHandAnchor != null)
        {
            // �÷��̾� ��ġ + ������(��� ��ġ)�� ���� ����ȭ
            leftHandAnchor.position = transform.position + transform.rotation * handOffset;
            // �÷��̾��� ȸ���� �����ϰ�
            leftHandAnchor.rotation = transform.rotation;
        }
    }
}
