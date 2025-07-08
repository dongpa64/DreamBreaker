using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // UI 요소를 제어하기 위함

public class VRUIRaycaster : MonoBehaviour
{
    public float maxDistance = 100f; // 레이캐스트 최대 거리
    public LayerMask uiLayer; // UI가 있는 레이어 (Inspector에서 설정)
    public Transform rightHand;
    private LineRenderer lineRenderer;
    private PointerEventData pointerEventData; // 현재는 사용하지 않지만, 향후 UI 이벤트 시스템 확장 시 유용


    // 현재 바라보고 있는 UI 요소 저장
    private GameObject currentLookAtUI = null;

    void Start()
    {
        // LineRenderer 설정 (옵션: 시각적인 레이캐스트 표현)
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 적절한 Material 사용
            lineRenderer.startColor = Color.cyan;
            lineRenderer.endColor = Color.blue;
        }
    }

    void Update()
    {
        RaycastHit hit;
        // OVRCameraRig의 눈 위치에서 레이 발사 (예: CenterEyeAnchor)
        // 실제 사용 시 OVRCameraRig의 LeftEyeAnchor 또는 RightEyeAnchor를 사용하는 것이 좋습니다.
        Ray ray = new Ray(rightHand.position, rightHand.forward);

        // LineRenderer 업데이트 (시각적인 표현)
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * maxDistance);
        }

        GameObject hitObject = null; // 이번 프레임에 레이캐스트된 UI 오브젝트

        // UI 레이어에 대한 레이캐스트
        if (Physics.Raycast(ray, out hit, maxDistance, uiLayer))
        {
            hitObject = hit.collider.gameObject;

            // 새로 바라보는 UI 오브젝트라면 처리 (하이라이트 등)
            if (hitObject != currentLookAtUI)
            {
                currentLookAtUI = hitObject;
            }

            // 상호작용 (예: Oculus Controller의 PrimaryIndexTrigger 버튼)
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                // 레이캐스트된 오브젝트에서 Button 컴포넌트 찾기
                Button hitButton = hitObject.GetComponent<Button>();
                if (hitButton != null)
                {
                    // 버튼이 클릭 가능한 상태라면 클릭 이벤트 호출
                    if (hitButton.interactable)
                    {
                        hitButton.onClick.Invoke(); // 버튼의 클릭 이벤트 직접 호출

                    }
                }
            }
        }
    }
}