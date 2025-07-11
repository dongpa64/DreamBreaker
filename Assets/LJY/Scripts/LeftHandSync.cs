using UnityEngine;

public class LeftHandSync : MonoBehaviour
{
    public Transform leftHandAnchor; // 인스펙터에서 OVRCameraRig/TrackingSpace/LeftHandAnchor를 드래그!
    public Vector3 handOffset = new Vector3(-0.3f, 1.2f, 0.2f); // 예시(플레이어 기준 위치, 직접 조정)

    void LateUpdate()
    {
        if (leftHandAnchor != null)
        {
            // 플레이어 위치 + 오프셋(상대 위치)로 강제 동기화
            leftHandAnchor.position = transform.position + transform.rotation * handOffset;
            // 플레이어의 회전과 동일하게
            leftHandAnchor.rotation = transform.rotation;
        }
    }
}
