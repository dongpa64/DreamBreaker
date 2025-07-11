using UnityEngine;

public class PerspectiveScaler : MonoBehaviour
{
    [SerializeField] private float minApparentScale = 0.1f;
    [SerializeField] private float maxApparentScale = 20.0f;
    [SerializeField] private Transform vrCam;
    private IPerspectiveScalable scalableObject; // IPerspectiveScalable 인터페이스 참조

    void Awake()
    {
        scalableObject = GetComponent<IPerspectiveScalable>();

    }

    void Update()
    {
        // IPerspectiveScalable의 IsScalingEnabled 플래그에 따라 스케일 조절
        if (scalableObject.IsScalingEnabled)
        {

            float currentDistanceToCamera = Vector3.Distance(scalableObject.ScalableTransform.position, vrCam.position);

            // new_scale = initial_scale * (current_distance / initial_distance)
            float scaleFactor = currentDistanceToCamera / scalableObject.InitialGrabDistanceToCamera;

            Vector3 newScale = scalableObject.InitialGrabScale * scaleFactor;

            // Clamp를 initialScale 기준으로 적용
            newScale.x = Mathf.Clamp(newScale.x, scalableObject.InitialGrabScale.x * minApparentScale, scalableObject.InitialGrabScale.x * maxApparentScale);
            newScale.y = Mathf.Clamp(newScale.y, scalableObject.InitialGrabScale.y * minApparentScale, scalableObject.InitialGrabScale.y * maxApparentScale);
            newScale.z = Mathf.Clamp(newScale.z, scalableObject.InitialGrabScale.z * minApparentScale, scalableObject.InitialGrabScale.z * maxApparentScale);

            scalableObject.ScalableTransform.localScale = newScale;
        }
    }

    // VR 카메라를 외부에서 설정할 수 있는 메서드 (GameManager에서 호출)
    public void SetVrCamera(Transform cameraTransform)
    {
        vrCam = cameraTransform;
    }
}