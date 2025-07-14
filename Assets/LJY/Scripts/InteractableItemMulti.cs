using Photon.Pun;
using UnityEngine;

public class InteractableItemMulti : MonoBehaviourPun
{
    public BlockWall wallToRemove;
    public int allowedActorNumber;
    public AudioClip getItemSound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null) return;
        if (pv.Owner.ActorNumber != allowedActorNumber) return;

        // 벽 동작 전체 동기화!
        if (wallToRemove != null)
            wallToRemove.photonView.RPC("RemoveWall", RpcTarget.AllBuffered);

        if (getItemSound != null)
            AudioSource.PlayClipAtPoint(getItemSound, transform.position);

        // ▶ 포탈건 비활성화 (자기 자신만)
        PortalShooter shooter = other.GetComponent<PortalShooter>();
        if (shooter != null && pv.IsMine)   // 내 플레이어만 비활성화
            shooter.DeactivatePortalShooter();

        // 아이템 오브젝트도 동기화 파괴
        photonView.RPC("DestroyMe", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
