using System.Collections.Generic;
using UnityEngine;

public class Duplicator : MonoBehaviour
{
    [Header("Replication Settings")]
    [SerializeField] private GameObject applePrefab; // 복제할 사과 프리팹 (자기 자신 또는 다른 모델)
    [SerializeField] private Transform vrCamera; // VR 카메라 트랜스폼
    [SerializeField] private LayerMask replicableLayer; // 복제 가능한 레이어 (사과 오브젝트가 속한 레이어)
    [SerializeField] private float maxReplicationDistance = 10f; // 최대 복제 가능 거리

    [Header("Scaling Settings")]
    // 거리에 따른 스케일 감소 배율. 이 값이 0에 가까울수록 멀리서 복제될 때 더 작게 시작합니다.
    [SerializeField] private float distanceScaleFactor = 0.5f;
    // 예: 1이면 거리에 비례 (동일 겉보기 크기), 0.5면 거리에 비례하지만 절반으로 작아지는 느낌
    [SerializeField] private float minSpawnScale = 0.1f; // 스폰될 수 있는 최소 실제 스케일
    [SerializeField] private float maxSpawnScale = 2.0f; // 스폰될 수 있는 최대 실제 스케일

    void Update()
    {
        // OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) // 트리거 버튼도 가능
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) // 오른쪽 컨트롤러의 A 버튼
        {
            TryReplicate();
        }
    }

    void TryReplicate()
    {
        Ray ray = new Ray(vrCamera.position, vrCamera.forward);
        RaycastHit hit;

        // 카메라 시점에서 Raycast 발사하여 복제 가능한 오브젝트 감지
        if (Physics.Raycast(ray, out hit, maxReplicationDistance, replicableLayer))
        {
            // Raycast가 이 스크립트가 붙어있는 오브젝트(사과)를 맞췄는지 확인
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

        // 1. 카메라와 사과 사이의 현재 거리를 측정합니다.
        float distanceToCamera = Vector3.Distance(transform.position, vrCamera.position);

        // 2. 거리에 따라 새로운 사과의 스케일을 계산합니다.
        // Superliminal에서는 거리가 멀수록 원본보다 작게 복제됩니다.
        // 여기서는 '거리'에 'scaleFactor'를 곱하여 스케일 비율을 조절합니다.
        // 예를 들어, distanceScaleFactor가 0.5면 거리가 2배 멀어질 때마다 스케일이 절반이 됩니다.
        // initialScale (원본 스케일) * (distanceScaleFactor / distanceToCamera)

        // 원본 사과의 실제 스케일을 기준으로 계산합니다.
        Vector3 originalScale = transform.localScale;

        // 거리에 따라 스케일이 "줄어드는" 느낌을 주려면, 거리가 멀어질수록 스케일 팩터가 작아져야 합니다.
        // 예: 거리가 1m일 때 1배, 2m일 때 0.5배, 4m일 때 0.25배
        // 공식: scaleFactor = 1.0f / (distanceToCamera * InverseDistanceScaleFactor)
        // 또는 간단하게: scaleFactor = (원하는 최소 거리) / distanceToCamera

        // 여기서는 거리에 반비례하여 스케일이 작아지도록 단순화합니다.
        // 예: 1미터 거리에서 1배, 2미터 거리에서 0.5배, 5미터 거리에서 0.2배
        // distanceScaleFactor는 '기준 거리'를 나타내는 것처럼 사용합니다.
        float calculatedScaleFactor = distanceScaleFactor / distanceToCamera;

        // 스케일 제한 적용
        calculatedScaleFactor = Mathf.Clamp(calculatedScaleFactor, minSpawnScale, maxSpawnScale);

        Vector3 newScale = originalScale * calculatedScaleFactor;

        // 3. 새로운 사과를 복제합니다.
        // 복제 위치는 Raycast가 맞은 지점 또는 현재 사과의 위치
        GameObject newApple = Instantiate(applePrefab, transform.position, transform.rotation);
        newApple.transform.localScale = newScale;

        // 4. 복제된 사과에 물리 활성화 (떨어지는 효과)
        Rigidbody newRb = newApple.GetComponent<Rigidbody>();
        if (newRb == null)
        {
            newRb = newApple.AddComponent<Rigidbody>(); // Rigidbody가 없으면 추가
        }
        newRb.isKinematic = false; // 물리 활성화
        newRb.useGravity = true; // 중력 적용

        // 복제된 사과는 잡을 수 없도록 GrabbableObject 컴포넌트 제거 (혹시 프리팹에 붙어있다면)
        GrabbableObject grabbable = newApple.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            Destroy(grabbable); // 잡을 수 없게 함
        }
        // PerspectiveScaler도 제거 (복제된 사과는 겉보기 크기 조절이 필요 없음)
        PerspectiveScaler scaler = newApple.GetComponent<PerspectiveScaler>();
        if (scaler != null)
        {
            Destroy(scaler);
        }

        Debug.Log($"Replicated apple at distance: {distanceToCamera:F2}m, new scale: {newScale.x:F2}");
    }

}
