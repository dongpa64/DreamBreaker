using Photon.Pun;
using UnityEngine;

public class BlockWall : MonoBehaviourPun
{
    public AudioClip breakSound;                // 벽 부서질 때 사운드
    public GameObject breakEffect;              // 벽 부서질 때 파티클 프리팹
    public float effectDuration = 2f;           // 이펙트 자동 삭제 시간

    [PunRPC]
    public void RemoveWall()
    {
        // 1. 파티클 이펙트 재생 (있으면)
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);    // effectDuration(예: 2초) 후 자동 삭제
        }

        // 2. 사운드 재생 (있으면)
        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);

        // 3. 벽 오브젝트 비활성화(사라짐)
        gameObject.SetActive(false);
    }
}
