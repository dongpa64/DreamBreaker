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

    [Header("��Ż ���� ũ��(����: ����)")]
    public Vector2 portalSize = new Vector2(1.0f, 2.0f); // (��, ����) ������ ���� ũ��

    // ** ������ ��ġ�� ���� **
    private bool useBlueNext = true;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        // �÷��� ���� ���� ��������
        var platform = playerMovement ? playerMovement.currentPlatform : PlatformType.PC;

        if (platform == PlatformType.PC)
        {
            // ��Ŭ��(���콺 ������ ��ư)�� ���
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
                useBlueNext = !useBlueNext; // ��ġ �� ���
            }
        }
        else // Oculus �Ǵ� Vive�� �� VR ��Ʈ�ѷ� ��� (���� ��� ����)
        {
            bool bluePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch);   // X ��ư
            bool orangePortalPressed = ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch); // Y ��ư

            if (bluePortalPressed)
                TryPlacePortal(ref bluePortalInstance, bluePortalprefab, platform);
            if (orangePortalPressed)
                TryPlacePortal(ref orangePortalInstance, orangePortalprefab, platform);
        }
    }

    void TryPlacePortal(ref GameObject portalInstance, GameObject portalPrefab, PlatformType platform)
    {
        Ray ray;

        // PC�� ���콺 ��ġ���� ����, VR�� ������ ��Ʈ�ѷ����� ����
        if (platform == PlatformType.PC)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else // VR ȯ��
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

            // �� �� ��ο��� ª�� Raycast (0.1m) �� ���� ���� ��ġ
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

            // === ��Ż ��ġ ===
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
