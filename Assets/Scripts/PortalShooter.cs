using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PortalShooter : MonoBehaviour
{
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    private GameObject bluePortalInstance;
    private GameObject orangePortalInstance;

    public LayerMask portalSurfaceMask;

    private PlayerMovement playerMovement;

    // === 쿼드(PortalVisual) 실제 크기를 반드시 맞게 세팅! ===
    [Header("포탈 쿼드 크기(단위: 미터)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f); // (폭, 높이) 쿼드의 실제 크기(필요시 조정)

    void Start()
    {
        // PlayerMovement에서 플랫폼 정보 받아오기 (플랫폼 자동인식)
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        // 플랫폼 현재 상태 가져오기
        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        bool bluePortalPressed = false;
        bool orangePortalPressed = false;

        // 입력 처리: PC일 때 마우스 클릭, VR일 때 컨트롤러 버튼
        if (platform == PlatformType.PC)
        {
            bluePortalPressed = Input.GetMouseButtonDown(0);
            orangePortalPressed = Input.GetMouseButtonDown(1);
        }
        else // Oculus 또는 Vive일 때 VR 컨트롤러 사용
        {
            bluePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);   // X 버튼
            orangePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch);  // Y 버튼
        }

        if (bluePortalPressed)
            TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);

        if (orangePortalPressed)
            TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);
    }

    void TryPlacePortal(ref GameObject portalInstance, GameObject portalPrefab, PlatformType platform)
    {
        Ray ray;

        // 조준 처리: PC는 마우스 위치에서 레이, VR은 오른손 컨트롤러에서 레이
        if (platform == PlatformType.PC)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else // VR 환경에서 조준은 오른손 컨트롤러 방향
        {
            Transform rightHand = FindFirstObjectByType<RightHand>()?.transform;
            if (rightHand != null)
                ray = new Ray(rightHand.position, rightHand.forward);
            else // RightHand 오브젝트가 없을 때는 ARAVRInput에서 직접 얻기
                ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        }

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, portalSurfaceMask))
        {
            // 1. 포탈 쿼드의 사이즈(portalSize)는 인스펙터에서 직접 설정!
            // 2. 포탈의 네 귀퉁이(로컬) → 월드 좌표로 변환
            Vector3 up = (hit.normal == Vector3.up || hit.normal == Vector3.down) ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(up, hit.normal).normalized;
            up = Vector3.Cross(hit.normal, right).normalized;

            Vector3 center = hit.point + hit.normal * 0.05f;
            float halfW = portalSize.x * 0.5f;
            float halfH = portalSize.y * 0.5f;

            Vector3[] corners = {
                center + right * halfW + up * halfH,
                center - right * halfW + up * halfH,
                center + right * halfW - up * halfH,
                center - right * halfW - up * halfH,
            };

            // 3. 네 점 모두에서 짧은 Raycast (0.1m) → 벽일 때만 설치
            bool canInstall = true;
            foreach (var c in corners)
            {
                Ray cornerRay = new Ray(c, -hit.normal);
                if (!Physics.Raycast(cornerRay, 0.1f, portalSurfaceMask))
                {
                    canInstall = false;
                    break;
                }
            }

            if (!canInstall)
            {
                Debug.Log("포탈 크기보다 벽이 좁아서 설치 불가!");
                return;
            }

            // === 포탈 설치 ===
            Vector3 position = center;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal);

            if (portalInstance != null)
                Destroy(portalInstance);

            portalInstance = Instantiate(portalPrefab, position, rotation);

            UpdatePortalConnections();
        }
    }

    void UpdatePortalConnections()
    {
        if (bluePortalInstance != null && orangePortalInstance != null)
        {
            var bluePortal = bluePortalInstance.GetComponent<Portal>();
            var orangePortal = orangePortalInstance.GetComponent<Portal>();

            bluePortal.destinationPortal = orangePortalInstance.transform;
            orangePortal.destinationPortal = bluePortalInstance.transform;
        }
    }
}
