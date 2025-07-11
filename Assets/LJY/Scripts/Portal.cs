using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform destinationPortal; // 연결된 다른 포탈의 Transform
    private AudioSource audioSource;    // 포탈 사운드용

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destinationPortal == null)
            return;

        // --- 플레이어가 포탈 통과 ---
        if (other.CompareTag("Player"))
        {
            // 포탈 사운드 재생
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();

            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;

                // 출구 위치 및 회전 조정
                Vector3 exitPosition = destinationPortal.position - destinationPortal.forward * 1.5f;
                other.transform.position = exitPosition;

                Vector3 forward = -destinationPortal.forward;
                forward.y = 0f;
                if (forward != Vector3.zero)
                    other.transform.rotation = Quaternion.LookRotation(forward);

                cc.enabled = true;
            }
        }
        // --- 오브젝트(큐브 등) 포탈 통과 ---
        else if (other.CompareTag("Grabbable") && other.attachedRigidbody != null)
        {
            // 오브젝트 포탈 사운드 (원하면)
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();

            Rigidbody rb = other.attachedRigidbody;

            Vector3 exitPos = destinationPortal.position - destinationPortal.forward * 1.5f;
            rb.position = exitPos;

            Quaternion targetRot = Quaternion.LookRotation(-destinationPortal.forward, Vector3.up);
            rb.rotation = targetRot;

            // 속도/회전 자연스럽게 보정
            Quaternion rotationDelta = Quaternion.FromToRotation(transform.forward, -destinationPortal.forward);
            rb.velocity = rotationDelta * rb.velocity * 0.93f;
            rb.angularVelocity = rotationDelta * rb.angularVelocity * 0.93f;
        }
    }
}
