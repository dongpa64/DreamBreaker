using UnityEngine;
using Photon.Pun;
public class BGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private OVRInput.Button grabButton = OVRInput.Button.PrimaryIndexTrigger;
    [SerializeField] private Transform rightHandController; // 컨트롤러 트랜스폼 (속도 계산용)
    [SerializeField] private Transform vrCamera; // VR 카메라 트랜스폼 (시야 고정용)
    [SerializeField] private LayerMask grabbableLayer; // 잡을 수 있는 오브젝트 레이어
    [SerializeField] private float remoteGrabDistance = 20f; // 원거리 잡기 거리 (최대 뻗어나갈 거리)

    [Header("Superliminal Movement Settings")]
    [SerializeField] private float minDistanceFromCollision = 0.1f; // 충돌 지점으로부터 최소한 떨어져 있을 거리
    [SerializeField] private LayerMask environmentLayer; // 환경 레이어 (벽 등)
    [SerializeField] private float verticalLiftDistanceMultiplier = 1.0f; // 위로 들어올릴 때 거리 증가 배율

    private IPerspectiveScalable currentGrabbedScalable;
    private GrabbableObject currentGrabbedObject;
    
    private float initialGrabDistanceToCamera; // 물체를 잡는 순간의 카메라로부터의 초기 거리 (최소값으로 사용)
    private float currentDistanceToCamera; // 카메라로부터 물체가 유지하려는 목표 거리 (동적으로 변화)
    private Quaternion initialRotation; // 물체 잡을 때의 카메라 상대 회전 오프셋
    private Vector3 initialPosition; // 물체 잡을 때 컨트롤러의 로컬 위치

    void Update()
    {
        if (currentGrabbedObject == null)
        {
            TryGrab();
        }
        else
        {
            UpdateGrabbedObjectPosition();
            TryUnGrab();
        }
    }

    void TryGrab()
    {
        if (OVRInput.GetDown(grabButton, OVRInput.Controller.RTouch))
        {
            Ray ray = new Ray(vrCamera.position, vrCamera.forward);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, remoteGrabDistance, grabbableLayer))
            {
                GrabbableObject grabbable = hitInfo.collider.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    grabbable.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    currentGrabbedObject = grabbable;
                    currentGrabbedScalable = grabbable;

                    currentGrabbedObject.OnGrab(grabbable.transform.localScale, Vector3.Distance(grabbable.transform.position, vrCamera.position));

                    initialGrabDistanceToCamera = Vector3.Distance(grabbable.transform.position, vrCamera.position);
                    currentDistanceToCamera = initialGrabDistanceToCamera;

                    initialRotation = Quaternion.Inverse(vrCamera.rotation) * grabbable.transform.rotation;
                    initialPosition = rightHandController.localPosition;
                }
            }
        }
    }

    void UpdateGrabbedObjectPosition()
    {
        if (currentGrabbedObject == null || currentGrabbedScalable == null) return;

        currentGrabbedObject.transform.rotation = vrCamera.rotation * initialRotation;

        float objectColliderRadius = 0.1f;
        Collider objectCollider = currentGrabbedObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            if (objectCollider is SphereCollider sphere)
            {
                objectColliderRadius = sphere.radius * currentGrabbedObject.transform.localScale.x;
            }
            else if (objectCollider is BoxCollider box)
            {
                Vector3 scaledExtents = Vector3.Scale(box.size / 2f, currentGrabbedObject.transform.localScale);
                objectColliderRadius = Mathf.Max(scaledExtents.x, scaledExtents.y, scaledExtents.z);
            }
        }

        // 1. 현재 시야 방향으로 물체가 뻗어나갈 수 있는 최대 거리 (벽까지의 거리) 계산
        float maxAllowedDistanceByView = remoteGrabDistance;
        RaycastHit wallHit;
        if (Physics.SphereCast(vrCamera.position, objectColliderRadius, vrCamera.forward, out wallHit, remoteGrabDistance, environmentLayer))
        {
            maxAllowedDistanceByView = wallHit.distance - objectColliderRadius - minDistanceFromCollision;
            maxAllowedDistanceByView = Mathf.Max(0.1f, maxAllowedDistanceByView);
        }

        // 2. 플레이어의 컨트롤러 Y축 움직임을 통해 물체 목표 거리 증가
        float controllerVerticalMovement = rightHandController.localPosition.y - initialPosition.y;
        float verticalLiftContribution = controllerVerticalMovement * verticalLiftDistanceMultiplier;

        // 3. 최종 목표 거리 결정
        //    물체의 새로운 목표 거리는 다음 세 가지 값 중 가장 큰 값으로 시작합니다.
        //    a. 잡는 순간의 초기 거리 (initialGrabDistanceToCameraAtGrabMoment)
        //    b. 현재 컨트롤러의 수직 이동에 의해 유도된 거리 (initialGrabDistanceToCameraAtGrabMoment + verticalLiftContribution)
        //    c. 이전 프레임의 currentObjectTargetDistanceToCamera (점진적인 늘어남 유지)

        float potentialTargetDistance = Mathf.Max(initialGrabDistanceToCamera, initialGrabDistanceToCamera + verticalLiftContribution);
        potentialTargetDistance = Mathf.Max(potentialTargetDistance, currentDistanceToCamera); // 이전 목표보다 작아지지 않게

        //    이후, 이 '잠재적' 목표 거리는 항상 '벽까지의 최대 거리' (maxAllowedDistanceByView)를 넘을 수 없습니다.
        currentDistanceToCamera = Mathf.Min(potentialTargetDistance, maxAllowedDistanceByView);

        // 최종적으로 최소 거리 제한 (0.1m)을 한 번 더 적용하여 오류 방지
        currentDistanceToCamera = Mathf.Max(currentDistanceToCamera, 0.1f);

        // 물체의 최종 위치 설정
        currentGrabbedObject.transform.position = vrCamera.position + vrCamera.forward * currentDistanceToCamera;
    }

    void TryUnGrab()
    {
        if (OVRInput.GetUp(grabButton, OVRInput.Controller.RTouch))
        {
            if (currentGrabbedObject != null)
            {
                Vector3 releaseVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
                Vector3 releaseAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);

                currentGrabbedObject.OnRelease(releaseVelocity, releaseAngularVelocity);

                currentGrabbedObject = null;
                currentGrabbedScalable = null;
            }
        }
    }
}