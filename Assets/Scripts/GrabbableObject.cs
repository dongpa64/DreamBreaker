using UnityEngine;

public class GrabbableObject : MonoBehaviour, IPerspectiveScalable
{
    // IPerspectiveScalable 인터페이스 구현
    public Transform ScalableTransform => transform;
    public Vector3 InitialGrabScale { get; set; }
    public float InitialGrabDistanceToCamera { get; set; }
    public bool IsScalingEnabled { get; set; } = false;

    private Rigidbody rb;
    private PerspectiveScaler perspectiveScaler;
    private HighlightEffect highlightEffect; // HighlightEffect 참조 추가

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        perspectiveScaler = GetComponent<PerspectiveScaler>();
        highlightEffect = GetComponent<HighlightEffect>(); // HighlightEffect 참조 가져오기

    }

    public void OnGrab(Vector3 initialScale, float initialDistanceToCamera)
    {
        if (rb != null) rb.isKinematic = true;

        InitialGrabScale = initialScale;
        InitialGrabDistanceToCamera = initialDistanceToCamera;
        IsScalingEnabled = true;

        if (highlightEffect != null)
        {
            highlightEffect.ApplyHighlight(); // 잡았을 때 하이라이트 적용
        }
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

        if (highlightEffect != null)
        {
            highlightEffect.RemoveHighlight(); // 놓았을 때 하이라이트 제거
        }
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
            if (highlightEffect != null)
            {
                highlightEffect.RemoveHighlight(); // 충돌하여 스케일링이 멈출 때도 하이라이트 제거
            }
        }
    }
}