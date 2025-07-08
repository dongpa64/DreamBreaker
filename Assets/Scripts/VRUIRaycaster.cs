using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // UI ��Ҹ� �����ϱ� ����

public class VRUIRaycaster : MonoBehaviour
{
    public float maxDistance = 100f; // ����ĳ��Ʈ �ִ� �Ÿ�
    public LayerMask uiLayer; // UI�� �ִ� ���̾� (Inspector���� ����)
    public Transform rightHand;
    private LineRenderer lineRenderer;
    private PointerEventData pointerEventData; // ����� ������� ������, ���� UI �̺�Ʈ �ý��� Ȯ�� �� ����

    // LobbyManager ���� �߰�
    public LobbyManager lobbyManager; // �ν����Ϳ��� �Ҵ�

    // ���� �ٶ󺸰� �ִ� UI ��� ����
    private GameObject currentLookAtUI = null;

    void Start()
    {
        // LineRenderer ���� (�ɼ�: �ð����� ����ĳ��Ʈ ǥ��)
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // ������ Material ���
            lineRenderer.startColor = Color.cyan;
            lineRenderer.endColor = Color.blue;
        }
    }

    void Update()
    {
        RaycastHit hit;
        // OVRCameraRig�� �� ��ġ���� ���� �߻� (��: CenterEyeAnchor)
        // ���� ��� �� OVRCameraRig�� LeftEyeAnchor �Ǵ� RightEyeAnchor�� ����ϴ� ���� �����ϴ�.
        Ray ray = new Ray(rightHand.position, rightHand.forward);

        // LineRenderer ������Ʈ (�ð����� ǥ��)
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * maxDistance);
        }

        GameObject hitObject = null; // �̹� �����ӿ� ����ĳ��Ʈ�� UI ������Ʈ

        // UI ���̾ ���� ����ĳ��Ʈ
        if (Physics.Raycast(ray, out hit, maxDistance, uiLayer))
        {
            hitObject = hit.collider.gameObject;

            // ���� �ٶ󺸴� UI ������Ʈ��� ó�� (���̶���Ʈ ��)
            if (hitObject != currentLookAtUI)
            {
                currentLookAtUI = hitObject;
            }

            // ��ȣ�ۿ� (��: Oculus Controller�� PrimaryIndexTrigger ��ư)
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                // ����ĳ��Ʈ�� ������Ʈ���� Button ������Ʈ ã��
                Button hitButton = hitObject.GetComponent<Button>();
                if (hitButton != null)
                {
                    // ��ư�� Ŭ�� ������ ���¶�� Ŭ�� �̺�Ʈ ȣ��
                    if (hitButton.interactable)
                    {
                        hitButton.onClick.Invoke(); // ��ư�� Ŭ�� �̺�Ʈ ���� ȣ��
                        Debug.Log($"��ư Ŭ��: {hitButton.name}");

                        // ���� LobbyManager�� OnClickJoin ��ư�̶�� Ư���� ó��
                        if (hitButton == lobbyManager.joinButton)
                        {
                            
                        }
                    }
                }
            }
        }
    }
}