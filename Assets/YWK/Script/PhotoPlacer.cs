using System.Collections.Generic;
using UnityEngine;

public class PhotoPlacer : MonoBehaviour
{
    [Header("사진 데이터")]
    public PhotoDataHolder photoData;

    [Header("사진 패널")]
    public Transform photoPanelTransform;

    [Header("생성된 오브젝트들 추적")]
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Update()
    {
        // X 버튼 누르면 리셋
        if (OVRInput.GetDown(OVRInput.Button.Three)) // Oculus X 버튼 (왼쪽 컨트롤러)
        {
            ResetSpawnedObjects();
        }
    }

    public void ProjectPhoto()
    {
        if (photoData == null || photoPanelTransform == null)
        {
            Debug.LogWarning("PhotoDataHolder 또는 투사 기준 Transform이 설정되지 않았습니다.");
            return;
        }

        Vector3 placePos = photoPanelTransform.position;
        Quaternion placeRot = photoPanelTransform.rotation;

        //  사진 데이터에서 오브젝트들 복제
        var newObjects = photoData.ProjectToAndReturn(placePos, placeRot);
        spawnedObjects.AddRange(newObjects);

        //  사진 비활성화
        photoPanelTransform.gameObject.SetActive(false);
    }

    public void ResetSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();

        //  사진 다시 보이게 하기
        photoPanelTransform.gameObject.SetActive(true);
    }
}
