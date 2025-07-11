using UnityEngine;

public class PenguinItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[PenguinItem] 충돌 발생: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[PenguinItem] 플레이어와 충돌!");

            PlayerFormManager formManager = other.GetComponent<PlayerFormManager>();
            if (formManager != null)
            {
                Debug.Log("[PenguinItem] PlayerFormManager 찾음, 변신 시도");
                formManager.ChangeForm(PlayerForm.Penguin);

                Destroy(gameObject);
                Debug.Log("[PenguinItem] 아이템 삭제됨");
            }
            else
            {
                Debug.LogWarning("[PenguinItem] ? PlayerFormManager를 찾을 수 없음!");
            }
        }
    }
}
