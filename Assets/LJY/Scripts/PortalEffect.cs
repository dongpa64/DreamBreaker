using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalEffect : MonoBehaviour
{
    public Material portalMat;   // Inspector���� ��Ƽ���� �Ҵ�
    public float scrollSpeed = 0.1f; // �帣�� �ӵ�(���ϸ� ����)

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        portalMat.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}
