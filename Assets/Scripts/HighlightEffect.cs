using UnityEngine;
using System.Collections.Generic;

public class HighlightEffect : MonoBehaviour
{
    [SerializeField] private Material highlightMaterialPrefab; 

    private Renderer objectRenderer;
    private Material[] originalMaterials; // �������� ��Ƽ���� �迭�� ����
    private bool isHighlighted = false; // ���� ���̶���Ʈ ���� ����

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    public void ApplyHighlight()
    {
        if (isHighlighted || objectRenderer == null || highlightMaterialPrefab == null) return;

        // ���� ��Ƽ���� �迭�� �����մϴ�.
        // objectRenderer.materials�� �׻� ���纻�� ��ȯ�մϴ�.
        originalMaterials = objectRenderer.materials;

        // ���̶���Ʈ ��Ƽ������ ������ �� �迭�� ����ϴ�.
        // ���� ��Ƽ���� ����ŭ ���̶���Ʈ ��Ƽ������ �����Ͽ� �Ҵ��մϴ�.
        Material[] newMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = Instantiate(highlightMaterialPrefab);

            // �ƿ����� ���̴��� ���� �ؽ�ó�� ������ ���� ��Ƽ���󿡼� ������ ���� �ֽ��ϴ�.
            // newMaterials[i].SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
            // newMaterials[i].SetColor("_Color", originalMaterials[i].GetColor("_Color"));
        }
        objectRenderer.materials = newMaterials;
        isHighlighted = true;
    }

    public void RemoveHighlight()
    {
        if (!isHighlighted || objectRenderer == null || originalMaterials == null) return;

        // ���� ��Ƽ����� �����մϴ�.
        objectRenderer.materials = originalMaterials;

        // ������ �����ߴ� ���̶���Ʈ ��Ƽ���� �ν��Ͻ����� �ı��մϴ�.
        // �̷��� ���� ������ �޸� ������ �߻��� �� �ֽ��ϴ�.
        foreach (Material mat in objectRenderer.materials) // ���� �������� �Ҵ�� ��Ƽ������ ������
        {
            if (mat != null && mat.shader == highlightMaterialPrefab.shader) // highlightMaterialPrefab�� ���̴��� ���� ���̴��� ����Ѵٸ�
            {
                Destroy(mat); // ��Ÿ�ӿ� ������ ��Ƽ���� �ν��Ͻ� �ı�
            }
        }
        originalMaterials = null; // ���� ����
        isHighlighted = false;
    }

    // ������Ʈ ��Ȱ��ȭ �Ǵ� �ı� �� Ŭ����
    void OnDisable()
    {
        if (isHighlighted) // ���̶���Ʈ�� ����� ���¶��
        {
            RemoveHighlight(); // ���̶���Ʈ ���� �� ���� ����
        }
    }

    // ������Ʈ�� �ı��� �� ȣ��˴ϴ�.
    void OnDestroy()
    {
        // OnDisable�� ȣ����� �ʴ� ��Ȳ (��: �����Ϳ��� ���� ����) ���
        if (isHighlighted)
        {
            RemoveHighlight();
        }
    }
}