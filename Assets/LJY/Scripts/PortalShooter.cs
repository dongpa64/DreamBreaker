using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PortalShooter : MonoBehaviour
{
    public GameObject bluePortalprefab;
    public GameObject orangePortalprefab;

    public Transform cameraAimPointer; // �ν����Ϳ��� �Ҵ�(���� ��, Crosshair ��)

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
    }

    void Update()
    {
        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        // ===== ���� ������(HMD �þ� ���߾� ����) �ǽð� ��ġ �̵� =====
        if (cameraAimPointer != null && Camera.main != null)
        {
            float rayLength = 5.0f;
            Vector3 camPos = Camera.main.transform.position;
            Vector3 camDir = Camera.main.transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(camPos, camDir, out hit, rayLength, portalSurfaceMask))
            {
                cameraAimPointer.position = hit.point;
            }
            else
            {
                cameraAimPointer.position = camPos + camDir * rayLength;
            }
            cameraAimPointer.forward = camDir;
            Debug.DrawRay(camPos, camDir * rayLength, Color.cyan, 0f, false);
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

                useBlueNext = !useBlueNext;
            }
        }
    }

    // ==== ��Ż ��ġ: PC/VR ��� ī�޶� ���� Ray ====
    void TryPlacePortal(ref GameObject portalInstance, GameObject portalPrefab, PlatformType platform)
    {
        Ray ray;

        if (platform == PlatformType.PC)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else // VR: HMD(ī�޶�) ���߾� �������� Ray
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