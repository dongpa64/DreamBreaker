using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGlowBreath : MonoBehaviour
{
    // Inspector���� ��Ż ��Ƽ������ �Ҵ��ϼ���.
    public Material portalMat;

    // Glow ������ ����/�ӵ� Ŀ���͸�����
    public Color glowColor = new Color(0f, 0.8f, 1f); // �Ķ��� (Color.cyan)
    public float baseGlow = 2.8f; // ��� ���
    public float pulseAmount = 1.2f; // ����(������ ����)
    public float pulseSpeed = 2f; // ������ �ӵ�

    void Update()
    {
        // Emission Glow �� ������ó�� ��ȭ
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + baseGlow;
        portalMat.SetColor("_EmissionColor", glowColor * pulse);
    }
}
