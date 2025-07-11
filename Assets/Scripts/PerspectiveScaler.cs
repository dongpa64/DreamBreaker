using UnityEngine;

public class PerspectiveScaler : MonoBehaviour
{
    [SerializeField] private float minApparentScale = 0.1f;
    [SerializeField] private float maxApparentScale = 20.0f;
    [SerializeField] private Transform vrCam;
    private IPerspectiveScalable scalableObject; // IPerspectiveScalable �������̽� ����

    void Awake()
    {
        scalableObject = GetComponent<IPerspectiveScalable>();

    }

    void Update()
    {
        // IPerspectiveScalable�� IsScalingEnabled �÷��׿� ���� ������ ����
        if (scalableObject.IsScalingEnabled)
        {

            float currentDistanceToCamera = Vector3.Distance(scalableObject.ScalableTransform.position, vrCam.position);

            // new_scale = initial_scale * (current_distance / initial_distance)
            float scaleFactor = currentDistanceToCamera / scalableObject.InitialGrabDistanceToCamera;

            Vector3 newScale = scalableObject.InitialGrabScale * scaleFactor;

            // Clamp�� initialScale �������� ����
            newScale.x = Mathf.Clamp(newScale.x, scalableObject.InitialGrabScale.x * minApparentScale, scalableObject.InitialGrabScale.x * maxApparentScale);
            newScale.y = Mathf.Clamp(newScale.y, scalableObject.InitialGrabScale.y * minApparentScale, scalableObject.InitialGrabScale.y * maxApparentScale);
            newScale.z = Mathf.Clamp(newScale.z, scalableObject.InitialGrabScale.z * minApparentScale, scalableObject.InitialGrabScale.z * maxApparentScale);

            scalableObject.ScalableTransform.localScale = newScale;
        }
    }

    // VR ī�޶� �ܺο��� ������ �� �ִ� �޼��� (GameManager���� ȣ��)
    public void SetVrCamera(Transform cameraTransform)
    {
        vrCam = cameraTransform;
    }
}