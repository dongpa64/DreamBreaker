using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    void Start()
    {
        gameObject.tag = "Grabbable"; // �ڵ� �±� ����
    }
}
