using System.Collections;
using UnityEngine;
using Photon.Pun;
using static PlayerMovement;

public class PortalShooter : MonoBehaviourPun
{
    public enum PortalType { Blue, Orange }
    [Header("��Ż Ÿ�� (�ڵ� �Ҵ�, �ν����� �⺻�� ����)")]
    public PortalType myPortalType;

    [Header("��Ż ������")]
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    [Header("ī�޶� ���� ������ ������")]
    public GameObject cameraAimPointerPrefab;
    private Transform cameraAimPointerInstance;

    [Header("�� �� ������Ʈ (PortalGun)")]
    public GameObject portalGunModel;

    [Header("��Ż �ѱ� VFX (SetActive ���)")]
    public GameObject muzzleVFX;
    public float vfxDuration = 0.5f;

    [Header("��Ż �߻� ����")]
    public AudioSource shootSfx;

    private GameObject myPortalInstance; // �� ��Ż�� ����
    public LayerMask portalSurfaceMask;

    private PlayerMovement playerMovement;

    [Header("��Ż ���� ũ��(����: ����)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f);

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        // [�߿�] �� �÷��̾��� Ÿ���� ���� �Ҵ�!
        if (photonView.IsMine)
        {
            myPortalType = (PhotonNetwork.LocalPlayer.ActorNumber == 1)
                ? PortalType.Blue
                : PortalType.Orange;
            Debug.Log($"[PortalShooter] �� ��ŻŸ�� �ڵ� �Ҵ�: {myPortalType}, ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber}");
        }

        if (portalGunModel != null) portalGunModel.SetActive(false);
        if (muzzleVFX != null) muzzleVFX.SetActive(false);

        enabled = false; // ������ ���� ���� Ȱ��ȭ
    }

    // --- ������ ���� �� ȣ�� ---
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
        if (!photonView.IsMine) return; // �� �÷��̾ ����

        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        // ===== ���� ������ ��ġ �ǽð� �̵� =====
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

        // ===== �Է� ó��(PC/VR) =====
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

    // ==== �� ��Ż�� ��ġ: PC/VR ��� ī�޶� ���� Ray ====
    void TryPlaceMyPortal(PlatformType platform)
    {
        Ray ray;
        if (platform == PlatformType.PC)
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("Camera.main�� ã�� �� �����ϴ�!");
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
                Debug.Log("��Ż ũ�⺸�� ���� ���Ƽ� ��ġ �Ұ�!");
                return;
            }

            // ���� �� ��Ż ��Ʈ��ũ�󿡼� ����
            if (myPortalInstance != null)
                PhotonNetwork.Destroy(myPortalInstance);

            // �� Ÿ�Կ� �´� ������
            GameObject prefab = (myPortalType == PortalType.Blue) ? bluePortalprefab : orangePortalprefab;
            Vector3 position = center;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal);

            // ��Ʈ��ũ�󿡼� ��Ż ����
            myPortalInstance = PhotonNetwork.Instantiate(prefab.name, position, rotation);

            // ���� �� owner/type ����ȭ, �� ���� �õ�
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
            p.TryAutoConnect(); // �� ���� �õ�
        }
    }
}
