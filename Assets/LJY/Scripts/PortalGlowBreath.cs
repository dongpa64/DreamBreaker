using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGlowBreath : MonoBehaviour
{
    // Inspector에서 포탈 머티리얼을 할당하세요.
    public Material portalMat;

    // Glow 숨쉬기 세기/속도 커스터마이즈
    public Color glowColor = new Color(0f, 0.8f, 1f); // 파란색 (Color.cyan)
    public float baseGlow = 2.8f; // 평균 밝기
    public float pulseAmount = 1.2f; // 진폭(숨쉬기 강도)
    public float pulseSpeed = 2f; // 숨쉬기 속도

    void Update()
    {
        // Emission Glow 값 숨쉬기처럼 변화
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + baseGlow;
        portalMat.SetColor("_EmissionColor", glowColor * pulse);
    }
}
