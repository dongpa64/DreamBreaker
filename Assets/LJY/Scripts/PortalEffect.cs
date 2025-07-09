using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalEffect : MonoBehaviour
{
    public Material portalMat;   // Inspector에서 머티리얼 할당
    public float scrollSpeed = 0.1f; // 흐르는 속도(원하면 조정)

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        portalMat.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}
