using UnityEngine;

public class MergeImage : MonoBehaviour
{ 
  
    [SerializeField]GameObject _2DImage; // 2D �̹����� ǥ���� Quad
    [SerializeField]GameObject _3DObject;     // 2D �̹����� ��ȯ�� 3D ������Ʈ

    [SerializeField]Transform _targetCameraPosition; // ���� ī�޶� ��ġ (�� GameObject�� ����)
    [SerializeField]float _positionTolerance = 0.5f; // ��ġ ��� ���� (����)
    [SerializeField]float _rotationTolerance = 15.0f; // ȸ�� ��� ���� (��)

    [SerializeField] float _raycastDist = 20;
    [SerializeField] LayerMask _2DLayer;
    [SerializeField] LayerMask _3DLayer;
  
    Camera _mainCamera;
    Renderer _2DRenderer;
    bool _is3DActive = false; // ���� 3D ������Ʈ�� Ȱ��ȭ�� ��������

    void Awake()
    {
        _mainCamera = Camera.main;
        _2DRenderer = _2DImage.GetComponent<Renderer>();
        // �ʱ� ���� ����
        _2DImage.SetActive(true);
        _3DObject.SetActive(false); // 3D ������Ʈ�� ó���� ��Ȱ��ȭ
        _is3DActive = false;
    }

    void Update()
    {
        CheckCameraPosition();
    }

    private void CheckCameraPosition()
    {
        // ī�޶� ��ġ �� ȸ�� ���� ���
        float dist = Vector3.Distance(_mainCamera.transform.position, _targetCameraPosition.position);
        float angle = Quaternion.Angle(_mainCamera.transform.rotation, _targetCameraPosition.rotation);
        bool isInRange = (dist <= _positionTolerance && angle <= _rotationTolerance);
        // ray�� ������ 2D �̹����� 3D ������Ʈ�� ��ġ�� ������ ����
        RaycastHit hit2D;
        RaycastHit hit3D;
        bool hit2DObject = Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit2D, _raycastDist, _2DLayer);
        bool hit3DObject = Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit3D, _raycastDist, _3DLayer);

        bool raycastOverlap = false;
        if (hit2DObject && hit3DObject)
        {
            if(hit2D.collider.gameObject == _2DImage && hit3D.collider.gameObject == _3DObject)
            {
                raycastOverlap = true;
            }
        }
        bool is3D = isInRange && raycastOverlap;
        if (is3D) 
        {
            SetTransition(true);
        }
        else if(!is3D)
        {
            SetTransition(false);
        }
    }

    private void SetTransition(bool to3D)
    {
        _is3DActive = to3D;      // ��ǥ ���� ����
        // ��ȯ ���� �� 3D ������Ʈ�� Ȱ��ȭ�ǰų� 2D ���尡 Ȱ��ȭ�Ǿ� �־�� ��
        if (to3D)
        {
            _2DImage.SetActive(false);
            _3DObject.SetActive(true);
        }
    }

}