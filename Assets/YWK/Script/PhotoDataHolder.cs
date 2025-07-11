using System.Collections.Generic;
using UnityEngine;

public class PhotoDataHolder : MonoBehaviour
{
    [Header("�� ������ ��� �ִ� ������Ʈ��")]
    public List<GameObject> sourceObjects; // ������ ��� ���� ������Ʈ��

    [Header("������ ���� ī�޶� (������Ʈ ������)")]
    public Transform cameraOrigin; // ������ ����� ī�޶��� ��ġ �� ȸ�� (���� ���� ��ǥ��)

    [Header("Y�� ������ (���� ����)")]
    public float yOffset = 2f; // ���鿡 ������ �ʵ��� ��¦ ���� �ø� �Ÿ�


    /// ������Ʈ���� �����ؼ� ����, ������ ���� ���� (1ȸ�� ��ġ��)

    public void ProjectTo(Vector3 targetPosition, Quaternion targetRotation)
    {
        foreach (var obj in sourceObjects)
        {
            if (obj == null) continue;

            // 1. ��ġ ����: ī�޶� �������� ���� ��ġ ��� �� �÷��̾� �������� ���ġ
            Vector3 localPos = cameraOrigin.InverseTransformPoint(obj.transform.position);
            Vector3 worldPos = targetPosition + targetRotation * localPos;
            worldPos.y += yOffset;

            // 2. ȸ�� ����: ī�޶� ���� ��� ȸ�� �� ���� ���� ȸ������ ��ȯ
            Quaternion localRot = Quaternion.Inverse(cameraOrigin.rotation) * obj.transform.rotation;
            Quaternion worldRot = targetRotation * localRot;

            // 3. ����
            GameObject clone = Instantiate(obj, worldPos, worldRot);
            clone.transform.localScale = obj.transform.localScale;
        }
    }

    /// ������Ʈ���� �����ϰ�, ������ ������Ʈ ����Ʈ�� ��ȯ (�ǰ��� �� ������)

    public List<GameObject> ProjectToAndReturn(Vector3 targetPosition, Quaternion targetRotation)
    {
        List<GameObject> spawned = new List<GameObject>();

        foreach (var obj in sourceObjects)
        {
            if (obj == null) continue;

            // 1. ��ġ ����
            Vector3 localPos = cameraOrigin.InverseTransformPoint(obj.transform.position);
            Vector3 worldPos = targetPosition + targetRotation * localPos;
            worldPos.y += yOffset;

            // 2. ȸ�� ����
            Quaternion localRot = Quaternion.Inverse(cameraOrigin.rotation) * obj.transform.rotation;
            Quaternion worldRot = targetRotation * localRot;

            // 3. ���� �� ���� ����Ʈ�� �߰�
            GameObject clone = Instantiate(obj, worldPos, worldRot);
            clone.transform.localScale = obj.transform.localScale;

            spawned.Add(clone);
        }

        return spawned;
    }
}
