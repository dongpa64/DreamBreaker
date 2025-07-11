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

    private bool onIce = false;  // �߰�: ���� ����

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
        animator.SetBool("onIce", onIce);  // �߰�: onIce �� �ִϸ����Ϳ� �ѱ��
    }

    // ���� ���� ����
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IceSurface"))
        {
            onIce = true;
        }
    }

    // ���� ��Ż ����
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IceSurface"))
        {
            onIce = false;
        }
    }
}
