using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PenguinAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerFormManager formManager;
    private Rigidbody rb;

    public float speedMultiplier = 0.3f;
    public float smoothTime = 0.1f;
    private float currentSpeed = 0f;

    private bool onIce = false;  // 추가: 빙판 여부

    void Start()
    {
        animator = GetComponent<Animator>();
        formManager = FindObjectOfType<PlayerFormManager>();
        rb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        float targetSpeed = 0f;

        if (formManager.currentForm == PlayerForm.Penguin && rb != null)
        {
            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0f;
            targetSpeed = horizontalVelocity.magnitude * speedMultiplier;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("onIce", onIce);  // 추가: onIce 값 애니메이터에 넘기기
    }

    // 빙판 진입 감지
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IceSurface"))
        {
            onIce = true;
        }
    }

    // 빙판 이탈 감지
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IceSurface"))
        {
            onIce = false;
        }
    }
}
