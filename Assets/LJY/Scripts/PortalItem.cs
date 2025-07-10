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
            // PortalShooter Ȱ��ȭ
            PortalShooter shooter = other.GetComponent<PortalShooter>();
            if (shooter != null)
                shooter.ActivatePortalShooter();

            // ���� ���
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();

            // ���尡 ������ ������Ʈ �ı� (���� ����)
            float delay = audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
            Destroy(gameObject, delay > 0.05f ? delay : 0f);
        }
    }
}
