using UnityEngine;
using System.Collections.Generic;

public class HighlightEffect : MonoBehaviour
{
    [SerializeField] private Material highlightMaterialPrefab; 

    private Renderer objectRenderer;
    private Material[] originalMaterials; // 오리지널 머티리얼 배열을 저장
    private bool isHighlighted = false; // 현재 하이라이트 상태 추적

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    public void ApplyHighlight()
    {
        if (isHighlighted || objectRenderer == null || highlightMaterialPrefab == null) return;

        // 원본 머티리얼 배열을 저장합니다.
        // objectRenderer.materials는 항상 복사본을 반환합니다.
        originalMaterials = objectRenderer.materials;

        // 하이라이트 머티리얼을 적용할 새 배열을 만듭니다.
        // 원본 머티리얼 수만큼 하이라이트 머티리얼을 생성하여 할당합니다.
        Material[] newMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = Instantiate(highlightMaterialPrefab);

            // 아웃라인 셰이더의 메인 텍스처와 색상을 원본 머티리얼에서 가져올 수도 있습니다.
            // newMaterials[i].SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
            // newMaterials[i].SetColor("_Color", originalMaterials[i].GetColor("_Color"));
        }
        objectRenderer.materials = newMaterials;
        isHighlighted = true;
    }

    public void RemoveHighlight()
    {
        if (!isHighlighted || objectRenderer == null || originalMaterials == null) return;

        // 원본 머티리얼로 복원합니다.
        objectRenderer.materials = originalMaterials;

        // 이전에 생성했던 하이라이트 머티리얼 인스턴스들을 파괴합니다.
        // 이렇게 하지 않으면 메모리 누수가 발생할 수 있습니다.
        foreach (Material mat in objectRenderer.materials) // 현재 렌더러에 할당된 머티리얼을 가져옴
        {
            if (mat != null && mat.shader == highlightMaterialPrefab.shader) // highlightMaterialPrefab의 셰이더와 같은 셰이더를 사용한다면
            {
                Destroy(mat); // 런타임에 생성된 머티리얼 인스턴스 파괴
            }
        }
        originalMaterials = null; // 참조 해제
        isHighlighted = false;
    }

    // 오브젝트 비활성화 또는 파괴 시 클린업
    void OnDisable()
    {
        if (isHighlighted) // 하이라이트가 적용된 상태라면
        {
            RemoveHighlight(); // 하이라이트 제거 및 원본 복원
        }
    }

    // 오브젝트가 파괴될 때 호출됩니다.
    void OnDestroy()
    {
        // OnDisable이 호출되지 않는 상황 (예: 에디터에서 강제 종료) 대비
        if (isHighlighted)
        {
            RemoveHighlight();
        }
    }
}