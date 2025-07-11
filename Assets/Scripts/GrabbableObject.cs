using UnityEngine;
using Photon.Pun;

public class GrabbableObject : MonoBehaviourPun, IPerspectiveScalable
{
    // IPerspectiveScalable 인터페이스 구현
    public Transform ScalableTransform => transform;
    public Vector3 InitialGrabScale { get; set; }
    public float InitialGrabDistanceToCamera { get; set; }
    public bool IsScalingEnabled { get; set; } = false;

    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnGrab(Vector3 initialScale, float initialDistanceToCamera)
    {
        if (rb != null) rb.isKinematic = true;

        InitialGrabScale = initialScale;
        InitialGrabDistanceToCamera = initialDistanceToCamera;
        IsScalingEnabled = true;
    }

    public void OnRelease(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = releaseVelocity;
            rb.angularVelocity = releaseAngularVelocity;
        }
        // 놓인 후에도 스케일링이 계속되도록 IsScalingEnabled를 true로 유지 (충돌 시 중단)
        // 잡는 상태(isGrabbed)는 PlayerGrabber에서 false로 만들 것입니다.
        // OnCollisionEnter에서 isScalingEnabled를 false로 변경합니다.
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsScalingEnabled)
        {
            IsScalingEnabled = false;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}