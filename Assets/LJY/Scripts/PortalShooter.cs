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

    [Header("포탈 쿼드 크기(단위: 미터)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f); // (폭, 높이) 쿼드의 실제 크기

    // ** 번갈아 설치용 변수 **
    private bool useBlueNext = true;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        // 플랫폼 현재 상태 가져오기
        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        if (platform == PlatformType.PC)
        {
            // 우클릭(마우스 오른쪽 버튼)만 사용
            if (Input.GetMouseButtonDown(1))
            {
                if (useBlueNext)
                {
                    TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);
                }
                else
                {
                    TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);
                }
                useBlueNext = !useBlueNext; // 설치 후 토글
            }
        }
        else // Oculus 또는 Vive일 때 VR 컨트롤러 사용 (기존 방식 유지)
        {
            bool bluePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);   // X 버튼
            bool orangePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch); // Y 버튼

            if (bluePortalPressed)
                TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);
            if (orangePortalPressed)
                TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);
        }
    }

    void TryPlacePortal(ref GameObject portalInstance, GameObject portalPrefab, PlatformType platform)
    {
        Ray ray;

        // PC는 마우스 위치에서 레이, VR은 오른손 컨트롤러에서 레이
        if (platform == PlatformType.PC)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else // VR 환경
        {
            Transform rightHand = FindFirstObjectByType<RightHand>()?.transform;
            if (rightHand != null)
                ray = new Ray(rightHand.position, rightHand.forward);
            else
                ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        }

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, portalSurfaceMask))
        {
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

            // 네 점 모두에서 짧은 Raycast (0.1m) → 벽일 때만 설치
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
