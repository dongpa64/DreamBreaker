using UnityEngine;

public class FanForce : MonoBehaviour
{
    public float pushForce = 10f; // 바람의 세기
    public Vector3 pushDirection = Vector3.forward; // 바람의 방향 (선풍기 로컬 기준)

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 선풍기의 로컬 방향을 월드 방향으로 변환하여 힘 적용
            rb.AddForce(transform.TransformDirection(pushDirection) * pushForce, ForceMode.Acceleration);
        }
    }
}