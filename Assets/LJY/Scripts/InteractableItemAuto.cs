using UnityEngine;

public class InteractableItemAuto : MonoBehaviour
{
    public BlockWall wallToRemove;
    public AudioClip getItemSound; // 아이템 획득 사운드(옵션)

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 벽 제거
        if (wallToRemove != null)
            wallToRemove.RemoveWall();

        // 아이템 획득 사운드
        if (getItemSound != null)
            AudioSource.PlayClipAtPoint(getItemSound, transform.position);

        // ▶ 포탈건 비활성화
        PortalShooter shooter = other.GetComponent<PortalShooter>();
        if (shooter != null)
            shooter.DeactivatePortalShooter();

        // 아이템 오브젝트 파괴
        Destroy(gameObject);
    }
}
