using System.Collections.Generic;
using UnityEngine;

public class Duplicator : MonoBehaviour
{
    [Header("Replication Settings")]
    [SerializeField] private GameObject applePrefab; // ������ ��� ������ (�ڱ� �ڽ� �Ǵ� �ٸ� ��)
    [SerializeField] private Transform vrCamera; // VR ī�޶� Ʈ������
    [SerializeField] private LayerMask replicableLayer; // ���� ������ ���̾� (��� ������Ʈ�� ���� ���̾�)
    [SerializeField] private float maxReplicationDistance = 10f; // �ִ� ���� ���� �Ÿ�

    [Header("Scaling Settings")]
    // �Ÿ��� ���� ������ ���� ����. �� ���� 0�� �������� �ָ��� ������ �� �� �۰� �����մϴ�.
    [SerializeField] private float distanceScaleFactor = 0.5f;
    // ��: 1�̸� �Ÿ��� ��� (���� �Ѻ��� ũ��), 0.5�� �Ÿ��� ��������� �������� �۾����� ����
    [SerializeField] private float minSpawnScale = 0.1f; // ������ �� �ִ� �ּ� ���� ������
    [SerializeField] private float maxSpawnScale = 2.0f; // ������ �� �ִ� �ִ� ���� ������

    void Update()
    {
        // OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) // Ʈ���� ��ư�� ����
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) // ������ ��Ʈ�ѷ��� A ��ư
        {
            TryReplicate();
        }
    }

    void TryReplicate()
    {
        Ray ray = new Ray(vrCamera.position, vrCamera.forward);
        RaycastHit hit;

        // ī�޶� �������� Raycast �߻��Ͽ� ���� ������ ������Ʈ ����
        if (Physics.Raycast(ray, out hit, maxReplicationDistance, replicableLayer))
        {
            // Raycast�� �� ��ũ��Ʈ�� �پ��ִ� ������Ʈ(���)�� ������� Ȯ��
            if (hit.collider.gameObject == gameObject)
            {
                ReplicateApple(hit.point);
            }
        }
    }

    void ReplicateApple(Vector3 hitPoint)
    {
        if (applePrefab == null)
        {
            Debug.LogError("Apple Prefab is not assigned to AppleReplicator.", this);
            return;
        }

        // 1. ī�޶�� ��� ������ ���� �Ÿ��� �����մϴ�.
        float distanceToCamera = Vector3.Distance(transform.position, vrCamera.position);

        // 2. �Ÿ��� ���� ���ο� ����� �������� ����մϴ�.
        // Superliminal������ �Ÿ��� �ּ��� �������� �۰� �����˴ϴ�.
        // ���⼭�� '�Ÿ�'�� 'scaleFactor'�� ���Ͽ� ������ ������ �����մϴ�.
        // ���� ���, distanceScaleFactor�� 0.5�� �Ÿ��� 2�� �־��� ������ �������� ������ �˴ϴ�.
        // initialScale (���� ������) * (distanceScaleFactor / distanceToCamera)

        // ���� ����� ���� �������� �������� ����մϴ�.
        Vector3 originalScale = transform.localScale;

        // �Ÿ��� ���� �������� "�پ���" ������ �ַ���, �Ÿ��� �־������� ������ ���Ͱ� �۾����� �մϴ�.
        // ��: �Ÿ��� 1m�� �� 1��, 2m�� �� 0.5��, 4m�� �� 0.25��
        // ����: scaleFactor = 1.0f / (distanceToCamera * InverseDistanceScaleFactor)
        // �Ǵ� �����ϰ�: scaleFactor = (���ϴ� �ּ� �Ÿ�) / distanceToCamera

        // ���⼭�� �Ÿ��� �ݺ���Ͽ� �������� �۾������� �ܼ�ȭ�մϴ�.
        // ��: 1���� �Ÿ����� 1��, 2���� �Ÿ����� 0.5��, 5���� �Ÿ����� 0.2��
        // distanceScaleFactor�� '���� �Ÿ�'�� ��Ÿ���� ��ó�� ����մϴ�.
        float calculatedScaleFactor = distanceScaleFactor / distanceToCamera;

        // ������ ���� ����
        calculatedScaleFactor = Mathf.Clamp(calculatedScaleFactor, minSpawnScale, maxSpawnScale);

        Vector3 newScale = originalScale * calculatedScaleFactor;

        // 3. ���ο� ����� �����մϴ�.
        // ���� ��ġ�� Raycast�� ���� ���� �Ǵ� ���� ����� ��ġ
        GameObject newApple = Instantiate(applePrefab, transform.position, transform.rotation);
        newApple.transform.localScale = newScale;

        // 4. ������ ����� ���� Ȱ��ȭ (�������� ȿ��)
        Rigidbody newRb = newApple.GetComponent<Rigidbody>();
        if (newRb == null)
        {
            newRb = newApple.AddComponent<Rigidbody>(); // Rigidbody�� ������ �߰�
        }
        newRb.isKinematic = false; // ���� Ȱ��ȭ
        newRb.useGravity = true; // �߷� ����

        // ������ ����� ���� �� ������ GrabbableObject ������Ʈ ���� (Ȥ�� �����տ� �پ��ִٸ�)
        GrabbableObject grabbable = newApple.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            Destroy(grabbable); // ���� �� ���� ��
        }
        // PerspectiveScaler�� ���� (������ ����� �Ѻ��� ũ�� ������ �ʿ� ����)
        PerspectiveScaler scaler = newApple.GetComponent<PerspectiveScaler>();
        if (scaler != null)
        {
            Destroy(scaler);
        }

        Debug.Log($"Replicated apple at distance: {distanceToCamera:F2}m, new scale: {newScale.x:F2}");
    }

}
