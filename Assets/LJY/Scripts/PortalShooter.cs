using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PortalShooter : MonoBehaviour
{
    [Header("포탈 프리팹")]
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    [Header("카메라 에임 포인터 프리팹")]
    public GameObject cameraAimPointerPrefab; // 반드시 프리팹만 등록(씬에 두지 않기)
    private Transform cameraAimPointerInstance; // 인스턴스 저장용

    [Header("총 모델 오브젝트 (PortalGun)")]
    public GameObject portalGunModel;

    [Header("포탈 총구 VFX (SetActive 방식)")]
    public GameObject muzzleVFX;
    public float vfxDuration = 0.5f;

    [Header("포탈 발사 사운드")]
    public AudioSource shootSfx;

    private GameObject bluePortalInstance;
    private GameObject orangePortalInstance;

    public LayerMask portalSurfaceMask;

    private PlayerMovement playerMovement;

    [Header("포탈 쿼드 크기(단위: 미터)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f);

    private bool useBlueNext = true;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (portalGunModel != null)
            portalGunModel.SetActive(false);

        if (muzzleVFX != null)
            muzzleVFX.SetActive(false);

        // 포인터는 필요할 때 생성!
        enabled = false;
    }

    // --- 아이템 먹을 때 호출 ---
    public void ActivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(true);

        // 에임 포인터가 없다면 프리팹에서 인스턴스 생성
        if (cameraAimPointerInstance == null && cameraAimPointerPrefab != null)
        {
            cameraAimPointerInstance = Instantiate(cameraAimPointerPrefab, transform).transform;
            cameraAimPointerInstance.gameObject.SetActive(true);
        }

        enabled = true;
    }

    // --- 포탈건 비활성화 시 호출 ---
    public void DeactivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(false);

        // 에임 포인터 오브젝트 삭제
        if (cameraAimPointerInstance != null)
        {
            Destroy(cameraAimPointerInstance.gameObject);
            cameraAimPointerInstance = null;
        }

        enabled = false;
    }

    void Update()
    {
        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        // ===== 에임 포인터 위치 실시간 이동 =====
        if (cameraAimPointerInstance != null && Camera.main != null)
        {
            float rayLength = 5.0f;
            Vector3 camPos = Camera.main.transform.position;
            Vector3 camDir = Camera.main.transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(camPos, camDir, out hit, rayLength, portalSurfaceMask))
            {
                cameraAimPointerInstance.position = hit.point;
            }
            else
            {
                cameraAimPointerInstance.position = camPos + camDir * rayLength;
            }
            cameraAimPointerInstance.forward = camDir;

            // (선택) 크기 고정
            cameraAimPointerInstance.localScale = Vector3.one * 0.05f;
        }

        // ===== 입력 처리(PC/VR) =====
        if (platform == PlatformType.PC)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (useBlueNext)
                    TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);
                else
                    TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);

                PlayShootEffects();
                useBlueNext = !useBlueNext;
            }
        }
        else // VR (오큘러스 등)
        {
            bool triggerPressed = ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch);

            if (triggerPressed)
            {
                if (useBlueNext)
                    TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);
                else
                    TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);

                PlayShootEffects();
                useBlueNext = !useBlueNext;
            }
        }
    }

    void PlayShootEffects()
    {
        if (shootSfx != null)
            shootSfx.Play();
        if (muzzleVFX != null)
        {
            muzzleVFX.SetActive(false);
            muzzleVFX.SetActive(true);
            StartCoroutine(DisableVFXAfterDelay());
        }
    }

    IEnumerator DisableVFXAfterDelay()
    {
        yield return new WaitForSeconds(vfxDuration);
        if (muzzleVFX != null)
            muzzleVFX.SetActive(false);
    }

    // ==== 포탈 설치: PC/VR 모두 카메라 정면 Ray ====
    void TryPlacePortal(ref GameObject portalInstance, GameObject portalPrefab, PlatformType platform)
    {
        Ray ray;

        if (platform == PlatformType.PC)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            if (Camera.main != null)
            {
                Vector3 camPos = Camera.main.transform.position;
                Vector3 camDir = Camera.main.transform.forward;
                ray = new Ray(camPos, camDir);
            }
            else
            {
                Debug.LogWarning("Camera.main을 찾을 수 없습니다!");
                return;
            }
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
