using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    void Start()
    {
        gameObject.tag = "Grabbable"; // 자동 태그 설정
    }
}
