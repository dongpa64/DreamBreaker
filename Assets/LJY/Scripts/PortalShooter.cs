using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PortalShooter : MonoBehaviour
{
    [Header("��Ż ������")]
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    [Header("ī�޶� ���� ������ ������")]
    public GameObject cameraAimPointerPrefab; // �ݵ�� �����ո� ���(���� ���� �ʱ�)
    private Transform cameraAimPointerInstance; // �ν��Ͻ� �����

    [Header("�� �� ������Ʈ (PortalGun)")]
    public GameObject portalGunModel;

    [Header("��Ż �ѱ� VFX (SetActive ���)")]
    public GameObject muzzleVFX;
    public float vfxDuration = 0.5f;

    [Header("��Ż �߻� ����")]
    public AudioSource shootSfx;

    private GameObject bluePortalInstance;
    private GameObject orangePortalInstance;

    public LayerMask portalSurfaceMask;

    private PlayerMovement playerMovement;

    [Header("��Ż ���� ũ��(����: ����)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f);

    private bool useBlueNext = true;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (portalGunModel != null)
            portalGunModel.SetActive(false);

        if (muzzleVFX != null)
            muzzleVFX.SetActive(false);

        // �����ʹ� �ʿ��� �� ����!
        enabled = false;
    }

    // --- ������ ���� �� ȣ�� ---
    public void ActivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(true);

        // ���� �����Ͱ� ���ٸ� �����տ��� �ν��Ͻ� ����
        if (cameraAimPointerInstance == null && cameraAimPointerPrefab != null)
        {
            cameraAimPointerInstance = Instantiate(cameraAimPointerPrefab, transform).transform;
            cameraAimPointerInstance.gameObject.SetActive(true);
        }

        enabled = true;
    }

    // --- ��Ż�� ��Ȱ��ȭ �� ȣ�� ---
    public void DeactivatePortalShooter()
    {
        if (portalGunModel != null)
            portalGunModel.SetActive(false);

        // ���� ������ ������Ʈ ����
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

        // ===== ���� ������ ��ġ �ǽð� �̵� =====
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

            // (����) ũ�� ����
            cameraAimPointerInstance.localScale = Vector3.one * 0.05f;
        }

        // ===== �Է� ó��(PC/VR) =====
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
        else // VR (��ŧ���� ��)
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

    // ==== ��Ż ��ġ: PC/VR ��� ī�޶� ���� Ray ====
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
                Debug.LogWarning("Camera.main�� ã�� �� �����ϴ�!");
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
                Debug.Log("��Ż ũ�⺸�� ���� ���Ƽ� ��ġ �Ұ�!");
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
