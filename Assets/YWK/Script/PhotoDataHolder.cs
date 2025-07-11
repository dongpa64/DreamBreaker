using System.Collections.Generic;
using UnityEngine;

public class PhotoDataHolder : MonoBehaviour
{
    [Header("이 사진이 담고 있는 오브젝트들")]
    public List<GameObject> sourceObjects; // 사진에 담긴 원본 오브젝트들

    [Header("사진을 찍은 카메라 (오브젝트 기준점)")]
    public Transform cameraOrigin; // 사진을 찍었던 카메라의 위치 및 회전 (원본 기준 좌표계)

    [Header("Y축 보정값 (위로 띄우기)")]
    public float yOffset = 2f; // 지면에 박히지 않도록 살짝 위로 올릴 거리


    /// 오브젝트들을 복제해서 놓되, 추적은 하지 않음 (1회성 배치용)

    public void ProjectTo(Vector3 targetPosition, Quaternion targetRotation)
    {
        foreach (var obj in sourceObjects)
        {
            if (obj == null) continue;

            // 1. 위치 보정: 카메라 기준으로 로컬 위치 계산 후 플레이어 기준으로 재배치
            Vector3 localPos = cameraOrigin.InverseTransformPoint(obj.transform.position);
            Vector3 worldPos = targetPosition + targetRotation * localPos;
            worldPos.y += yOffset;

            // 2. 회전 보정: 카메라 기준 상대 회전 → 현재 기준 회전으로 변환
            Quaternion localRot = Quaternion.Inverse(cameraOrigin.rotation) * obj.transform.rotation;
            Quaternion worldRot = targetRotation * localRot;

            // 3. 복제
            GameObject clone = Instantiate(obj, worldPos, worldRot);
            clone.transform.localScale = obj.transform.localScale;
        }
    }

    /// 오브젝트들을 복제하고, 생성된 오브젝트 리스트를 반환 (되감기 등 추적용)

    public List<GameObject> ProjectToAndReturn(Vector3 targetPosition, Quaternion targetRotation)
    {
        List<GameObject> spawned = new List<GameObject>();

        foreach (var obj in sourceObjects)
        {
            if (obj == null) continue;

            // 1. 위치 보정
            Vector3 localPos = cameraOrigin.InverseTransformPoint(obj.transform.position);
            Vector3 worldPos = targetPosition + targetRotation * localPos;
            worldPos.y += yOffset;

            // 2. 회전 보정
            Quaternion localRot = Quaternion.Inverse(cameraOrigin.rotation) * obj.transform.rotation;
            Quaternion worldRot = targetRotation * localRot;

            // 3. 복제 및 추적 리스트에 추가
            GameObject clone = Instantiate(obj, worldPos, worldRot);
            clone.transform.localScale = obj.transform.localScale;

            spawned.Add(clone);
        }

        return spawned;
    }
}
