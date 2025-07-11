#define PC
//#define Oculus

using UnityEngine;

public class PGrabber : MonoBehaviour
{
    public ARAVRInput.Controller controller = ARAVRInput.Controller.RTouch;

    [SerializeField]
    private GameObject grabbedObject = null;
    private Rigidbody grabbedRigidbody = null;

    private Vector3 lastPosition;
    private Vector3 controllerVelocity;

    public float grabRadius = 0.2f;

    // ������ ���� ������
    public float throwVelocityStrength = 5f;   // ���� �ӵ� (velocity)
    public float throwForceStrength = 100f;    // ���� �� (AddForce)

    void Update()
    {
#if PC
        if (grabbedObject == null)
            UpdateGrabberPositionByMouse();

        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("Grab �õ���!");
            TryGrab();
        }

        if (Input.GetMouseButtonUp(2))
        {
            Release();
        }

        if (grabbedObject != null && Input.GetKeyDown(KeyCode.R))
        {
            Vector3 throwDir = Camera.main.transform.forward;
            ThrowObject(throwDir);
        }

#else
        Vector3 currentPosition = (controller == ARAVRInput.Controller.RTouch)
            ? ARAVRInput.RHandPosition
            : ARAVRInput.LHandPosition;

        controllerVelocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        transform.position = currentPosition;

        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, controller))
        {
            TryGrab();
        }

        if (ARAVRInput.GetUp(ARAVRInput.Button.IndexTrigger, controller))
        {
            ThrowObject(controllerVelocity.normalized);
        }
#endif
    }

#if PC
    private void UpdateGrabberPositionByMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            Vector3 targetPosition = hit.point;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }
    }
#endif

    private void TryGrab()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, grabRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Grabbable"))
            {
                Debug.Log("����: " + col.gameObject.name); // �� �߰�!
                grabbedObject = col.gameObject;
                grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();

                if (grabbedRigidbody != null)
                {
                    grabbedRigidbody.isKinematic = true;
                    grabbedObject.transform.SetParent(this.transform);
                    Debug.Log("Grabbed: " + grabbedObject.name);
                }
                else
                {
                    Debug.LogWarning("Grabbable object missing Rigidbody!");
                }

                break;
            }
        }
    }

    private void Release()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);

            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = false;

#if PC
                grabbedRigidbody.velocity = Vector3.zero; // R Ű �ܿ��� ������ ����
#else
                grabbedRigidbody.velocity = controllerVelocity.normalized * throwVelocityStrength;
                grabbedRigidbody.AddForce(controllerVelocity.normalized * throwForceStrength, ForceMode.Impulse);
#endif
            }

            grabbedObject = null;
            grabbedRigidbody = null;
        }
    }

    private void ThrowObject(Vector3 throwDirection)
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);

            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = false;

                // �ӵ��� �� ��� ����
                Vector3 velocity = throwDirection.normalized * throwVelocityStrength;
                Vector3 force = throwDirection.normalized * throwForceStrength;

                grabbedRigidbody.velocity = velocity;
                grabbedRigidbody.AddForce(force, ForceMode.Impulse);

                Debug.Log("Thrown with velocity: " + velocity + ", force: " + force);
            }

            grabbedObject = null;
            grabbedRigidbody = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}
