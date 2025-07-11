using UnityEngine;

public class PortalItem : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // PortalShooter 활성화
            PortalShooter shooter = other.GetComponent<PortalShooter>();
            if (shooter != null)
                shooter.ActivatePortalShooter();

            // 사운드 재생
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();

            // 사운드가 끝나고 오브젝트 파괴 (지연 삭제)
            float delay = audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
            Destroy(gameObject, delay > 0.05f ? delay : 0f);
        }
    }
}
