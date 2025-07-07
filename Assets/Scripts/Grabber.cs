using UnityEngine;

public class Grabber : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private OVRInput.Button grabButton = OVRInput.Button.PrimaryIndexTrigger;
    [SerializeField] private Transform rightHandController; // ��Ʈ�ѷ� Ʈ������ (�ӵ� ����)
    [SerializeField] private Transform vrCamera; // VR ī�޶� Ʈ������ (�þ� ������)
    [SerializeField] private LayerMask grabbableLayer; // ���� �� �ִ� ������Ʈ ���̾�
    [SerializeField] private float remoteGrabDistance = 20f; // ���Ÿ� ��� �Ÿ� (�ִ� ����� �Ÿ�)

    [Header("Superliminal Movement Settings")]
    [SerializeField] private float minDistanceFromCollision = 0.1f; // �浹 �������κ��� �ּ��� ������ ���� �Ÿ�
    [SerializeField] private LayerMask environmentLayer; // ȯ�� ���̾� (�� ��)
    [SerializeField] private float verticalLiftDistanceMultiplier = 1.0f; // ���� ���ø� �� �Ÿ� ���� ����

    private IPerspectiveScalable currentGrabbedScalable;
    private GrabbableObject currentGrabbedObject;

    private float initialGrabDistanceToCamera; // ��ü�� ��� ������ ī�޶�κ����� �ʱ� �Ÿ� (�ּҰ����� ���)
    private float currentDistanceToCamera; // ī�޶�κ��� ��ü�� �����Ϸ��� ��ǥ �Ÿ� (�������� ��ȭ)
    private Quaternion initialRotation; // ��ü ���� ���� ī�޶� ��� ȸ�� ������
    private Vector3 initialPosition; // ��ü ���� �� ��Ʈ�ѷ��� ���� ��ġ

    void Update()
    {
        if (currentGrabbedObject == null)
        {
            TryGrab();
        }
        else
        {
            UpdateGrabbedObjectPosition();
            TryUnGrab();
        }
    }

    void TryGrab()
    {
        if (OVRInput.GetDown(grabButton, OVRInput.Controller.RTouch))
        {
            Ray ray = new Ray(vrCamera.position, vrCamera.forward);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, remoteGrabDistance, grabbableLayer))
            {
                GrabbableObject grabbable = hitInfo.collider.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    currentGrabbedObject = grabbable;
                    currentGrabbedScalable = grabbable;

                    currentGrabbedObject.OnGrab(grabbable.transform.localScale, Vector3.Distance(grabbable.transform.position, vrCamera.position));

                    initialGrabDistanceToCamera = Vector3.Distance(grabbable.transform.position, vrCamera.position);
                    currentDistanceToCamera = initialGrabDistanceToCamera;

                    initialRotation = Quaternion.Inverse(vrCamera.rotation) * grabbable.transform.rotation;
                    initialPosition = rightHandController.localPosition;
                }
            }
        }
    }

    void UpdateGrabbedObjectPosition()
    {
        if (currentGrabbedObject == null || currentGrabbedScalable == null) return;

        currentGrabbedObject.transform.rotation = vrCamera.rotation * initialRotation;

        float objectColliderRadius = 0.1f;
        Collider objectCollider = currentGrabbedObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            if (objectCollider is SphereCollider sphere)
            {
                objectColliderRadius = sphere.radius * currentGrabbedObject.transform.localScale.x;
            }
            else if (objectCollider is BoxCollider box)
            {
                Vector3 scaledExtents = Vector3.Scale(box.size / 2f, currentGrabbedObject.transform.localScale);
                objectColliderRadius = Mathf.Max(scaledExtents.x, scaledExtents.y, scaledExtents.z);
            }
        }

        // 1. ���� �þ� �������� ��ü�� ����� �� �ִ� �ִ� �Ÿ� (�������� �Ÿ�) ���
        float maxAllowedDistanceByView = remoteGrabDistance;
        RaycastHit wallHit;
        if (Physics.SphereCast(vrCamera.position, objectColliderRadius, vrCamera.forward, out wallHit, remoteGrabDistance, environmentLayer))
        {
            maxAllowedDistanceByView = wallHit.distance - objectColliderRadius - minDistanceFromCollision;
            maxAllowedDistanceByView = Mathf.Max(0.1f, maxAllowedDistanceByView);
        }

        // 2. �÷��̾��� ��Ʈ�ѷ� Y�� �������� ���� ��ü ��ǥ �Ÿ� ����
        float controllerVerticalMovement = rightHandController.localPosition.y - initialPosition.y;
        float verticalLiftContribution = controllerVerticalMovement * verticalLiftDistanceMultiplier;

        // 3. ���� ��ǥ �Ÿ� ����
        //    ��ü�� ���ο� ��ǥ �Ÿ��� ���� �� ���� �� �� ���� ū ������ �����մϴ�.
        //    a. ��� ������ �ʱ� �Ÿ� (initialGrabDistanceToCameraAtGrabMoment)
        //    b. ���� ��Ʈ�ѷ��� ���� �̵��� ���� ������ �Ÿ� (initialGrabDistanceToCameraAtGrabMoment + verticalLiftContribution)
        //    c. ���� �������� currentObjectTargetDistanceToCamera (�������� �þ ����)

        float potentialTargetDistance = Mathf.Max(initialGrabDistanceToCamera, initialGrabDistanceToCamera + verticalLiftContribution);
        potentialTargetDistance = Mathf.Max(potentialTargetDistance, currentDistanceToCamera); // ���� ��ǥ���� �۾����� �ʰ�

        //    ����, �� '������' ��ǥ �Ÿ��� �׻� '�������� �ִ� �Ÿ�' (maxAllowedDistanceByView)�� ���� �� �����ϴ�.
        currentDistanceToCamera = Mathf.Min(potentialTargetDistance, maxAllowedDistanceByView);

        // ���������� �ּ� �Ÿ� ���� (0.1m)�� �� �� �� �����Ͽ� ���� ����
        currentDistanceToCamera = Mathf.Max(currentDistanceToCamera, 0.1f);

        // ��ü�� ���� ��ġ ����
        currentGrabbedObject.transform.position = vrCamera.position + vrCamera.forward * currentDistanceToCamera;
    }

    void TryUnGrab()
    {
        if (OVRInput.GetUp(grabButton, OVRInput.Controller.RTouch))
        {
            if (currentGrabbedObject != null)
            {
                Vector3 releaseVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
                Vector3 releaseAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);

                currentGrabbedObject.OnRelease(releaseVelocity, releaseAngularVelocity);

                currentGrabbedObject = null;
                currentGrabbedScalable = null;
            }
        }
    }
}