using UnityEngine;
using Photon.Pun;

public class GrabbableObject : MonoBehaviourPun, IPerspectiveScalable
{
    // IPerspectiveScalable �������̽� ����
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
        // ���� �Ŀ��� �����ϸ��� ��ӵǵ��� IsScalingEnabled�� true�� ���� (�浹 �� �ߴ�)
        // ��� ����(isGrabbed)�� PlayerGrabber���� false�� ���� ���Դϴ�.
        // OnCollisionEnter���� isScalingEnabled�� false�� �����մϴ�.
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