using UnityEngine;

public class MergeImage : MonoBehaviour
{ 
  
    [SerializeField]GameObject _2DImage; // 2D 이미지를 표시할 Quad
    [SerializeField]GameObject _3DObject;     // 2D 이미지로 전환될 3D 오브젝트

    [SerializeField]Transform _targetCameraPosition; // 정답 카메라 위치 (빈 GameObject로 설정)
    [SerializeField]float _positionTolerance = 0.5f; // 위치 허용 오차 (미터)
    [SerializeField]float _rotationTolerance = 15.0f; // 회전 허용 오차 (도)

    [SerializeField] float _raycastDist = 20;
    [SerializeField] LayerMask _2DLayer;
    [SerializeField] LayerMask _3DLayer;
  
    Camera _mainCamera;
    Renderer _2DRenderer;
    bool _is3DActive = false; // 현재 3D 오브젝트가 활성화된 상태인지

    void Awake()
    {
        _mainCamera = Camera.main;
        _2DRenderer = _2DImage.GetComponent<Renderer>();
        // 초기 상태 설정
        _2DImage.SetActive(true);
        _3DObject.SetActive(false); // 3D 오브젝트는 처음엔 비활성화
        _is3DActive = false;
    }

    void Update()
    {
        CheckCameraPosition();
    }

    private void CheckCameraPosition()
    {
        // 카메라 위치 및 회전 오차 계산
        float dist = Vector3.Distance(_mainCamera.transform.position, _targetCameraPosition.position);
        float angle = Quaternion.Angle(_mainCamera.transform.rotation, _targetCameraPosition.rotation);
        bool isInRange = (dist <= _positionTolerance && angle <= _rotationTolerance);
        // ray를 쐈을때 2D 이미지와 3D 오브젝트가 겹치는 순간을 포착
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
        _is3DActive = to3D;      // 목표 상태 설정
        // 전환 시작 시 3D 오브젝트가 활성화되거나 2D 쿼드가 활성화되어 있어야 함
        if (to3D)
        {
            _2DImage.SetActive(false);
            _3DObject.SetActive(true);
        }
    }

}