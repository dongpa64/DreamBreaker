using UnityEngine;

public class PenguinItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[PenguinItem] �浹 �߻�: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[PenguinItem] �÷��̾�� �浹!");

            PlayerFormManager formManager = other.GetComponent<PlayerFormManager>();
            if (formManager != null)
            {
                Debug.Log("[PenguinItem] PlayerFormManager ã��, ���� �õ�");
                formManager.ChangeForm(PlayerForm.Penguin);

                Destroy(gameObject);
                Debug.Log("[PenguinItem] ������ ������");
            }
            else
            {
                Debug.LogWarning("[PenguinItem] ? PlayerFormManager�� ã�� �� ����!");
            }
        }
    }
}
