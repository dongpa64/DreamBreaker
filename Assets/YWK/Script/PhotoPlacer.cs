using System.Collections.Generic;
using UnityEngine;

public class PhotoPlacer : MonoBehaviour
{
    [Header("���� ������")]
    public PhotoDataHolder photoData;

    [Header("���� �г�")]
    public Transform photoPanelTransform;

    [Header("������ ������Ʈ�� ����")]
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Update()
    {
        // X ��ư ������ ����
        if (OVRInput.GetDown(OVRInput.Button.Three)) // Oculus X ��ư (���� ��Ʈ�ѷ�)
        {
            ResetSpawnedObjects();
        }
    }

    public void ProjectPhoto()
    {
        if (photoData == null || photoPanelTransform == null)
        {
            Debug.LogWarning("PhotoDataHolder �Ǵ� ���� ���� Transform�� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 placePos = photoPanelTransform.position;
        Quaternion placeRot = photoPanelTransform.rotation;

        //  ���� �����Ϳ��� ������Ʈ�� ����
        var newObjects = photoData.ProjectToAndReturn(placePos, placeRot);
        spawnedObjects.AddRange(newObjects);

        //  ���� ��Ȱ��ȭ
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

        //  ���� �ٽ� ���̰� �ϱ�
        photoPanelTransform.gameObject.SetActive(true);
    }
}
