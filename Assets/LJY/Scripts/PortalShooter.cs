using System.Collections;
using UnityEngine;
using Photon.Pun;
using static PlayerMovement;

public class PortalShooter : MonoBehaviourPun
{
    public enum PortalType { Blue, Orange }
    [Header("포탈 타입 (자동 할당, 인스펙터 기본값 무시)")]
    public PortalType myPortalType;

    [Header("포탈 프리팹")]
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    [Header("카메라 에임 포인터 프리팹")]
    public GameObject cameraAimPointerPrefab;
    private Transform cameraAimPointerInstance;

    [Header("총 모델 오브젝트 (PortalGun)")]
    public GameObject portalGunModel;

    [Header("포탈 총구 VFX (SetActive 방식)")]
    public GameObject muzzleVFX;
    public float vfxDuration = 0.5f;

    [Header("포탈 발사 사운드")]
    public AudioSource shootSfx;

    private GameObject myPortalInstance; // 내 포탈만 관리
    public LayerMask portalSurfaceMask;

    private PlayerMovement playerMovement;

    [Header("포탈 쿼드 크기(단위: 미터)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f);

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        // [중요] 내 플레이어라면 타입을 직접 할당!
        if (photonView.IsMine)
        {
            myPortalType = (PhotonNetwork.LocalPlayer.ActorNumber == 1)
                ? PortalType.Blue
                : PortalType.Orange;
            Debug.Log($"[PortalShooter] 내 포탈타입 자동 할당: {myPortalType}, ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber}");
        }

        if (portalGunModel != null) portalGunModel.SetActive(false);
        if (muzzleVFX != null) muzzleVFX.SetActive(false);

        enabled = false; // 아이템 먹을 때만 활성화
    }

    // --- 아이템 먹을 때 호출 ---
    public void ActivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(true);

        if (cameraAimPointerInstance == null && cameraAimPointerPrefab != null)
        {
            cameraAimPointerInstance = Instantiate(cameraAimPointerPrefab, transform).transform;
            cameraAimPointerInstance.gameObject.SetActive(true);
        }

        enabled = true;
    }

    public void DeactivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(false);

        if (cameraAimPointerInstance != null)
        {
            Destroy(cameraAimPointerInstance.gameObject);
            cameraAimPointerInstance = null;
        }
        enabled = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return; // 내 플레이어만 조작

        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        // ===== 에임 포인터 위치 실시간 이동 =====
        if (cameraAimPointerInstance != null && Camera.main != null)
        {
            float rayLength = 5.0f;
            Vector3 camPos = Camera.main.transform.position;
            Vector3 camDir = Camera.main.transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(camPos, camDir, out hit, rayLength, portalSurfaceMask))
                cameraAimPointerInstance.position = hit.point;
            else
                cameraAimPointerInstance.position = camPos + camDir * rayLength;

            cameraAimPointerInstance.forward = camDir;
            cameraAimPointerInstance.localScale = Vector3.one * 0.05f;
        }

        // ===== 입력 처리(PC/VR) =====
        bool shootInput = false;
        if (platform == PlatformType.PC)
            shootInput = Input.GetMouseButtonDown(1);
        else
            shootInput = ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch);

        if (shootInput)
        {
            TryPlaceMyPortal(platform);
            PlayShootEffects();
        }
    }

    void PlayShootEffects()
    {
        if (shootSfx != null) shootSfx.Play();
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

    // ==== 내 포탈만 설치: PC/VR 모두 카메라 정면 Ray ====
    void TryPlaceMyPortal(PlatformType platform)
    {
        Ray ray;
        if (platform == PlatformType.PC)
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("Camera.main을 찾을 수 없습니다!");
                return;
            }
            Vector3 camPos = Camera.main.transform.position;
            Vector3 camDir = Camera.main.transform.forward;
            ray = new Ray(camPos, camDir);
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

            // 기존 내 포탈 네트워크상에서 제거
            if (myPortalInstance != null)
                PhotonNetwork.Destroy(myPortalInstance);

            // 내 타입에 맞는 프리팹
            GameObject prefab = (myPortalType == PortalType.Blue) ? bluePortalprefab : orangePortalprefab;
            Vector3 position = center;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal);

            // 네트워크상에서 포탈 생성
            myPortalInstance = PhotonNetwork.Instantiate(prefab.name, position, rotation);

            // 생성 후 owner/type 동기화, 쌍 연결 시도
            photonView.RPC("SetMyPortalType", RpcTarget.AllBuffered,
                myPortalInstance.GetComponent<PhotonView>().ViewID, (int)myPortalType, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    void SetMyPortalType(int portalViewID, int portalTypeInt, int actorNum)
    {
        GameObject portalObj = PhotonView.Find(portalViewID).gameObject;
        var p = portalObj.GetComponent<Portal>();
        if (p != null)
        {
            p.SetOwnerAndType(actorNum, (Portal.PortalType)portalTypeInt);
            p.TryAutoConnect(); // 쌍 연결 시도
        }
    }
}
