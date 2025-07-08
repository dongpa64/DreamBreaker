using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ��Ż ���� Ȱ��ȭ
            PortalShooter shooter = other.GetComponent<PortalShooter>();
            if (shooter != null)
            {
                shooter.enabled = true;
            }
            // ������ �����
            Destroy(gameObject);
        }
    }
}
