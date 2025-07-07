using UnityEngine;

public class FanForce : MonoBehaviour
{
    public float pushForce = 10f; // �ٶ��� ����
    public Vector3 pushDirection = Vector3.forward; // �ٶ��� ���� (��ǳ�� ���� ����)

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // ��ǳ���� ���� ������ ���� �������� ��ȯ�Ͽ� �� ����
            rb.AddForce(transform.TransformDirection(pushDirection) * pushForce, ForceMode.Acceleration);
        }
    }
}