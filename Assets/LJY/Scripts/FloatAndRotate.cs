using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    public float floatSpeed = 2f;       // �ս� �ӵ�
    public float floatAmount = 0.2f;    // �ս� ����
    public float rotationSpeed = 60f;   // ȸ�� �ӵ�

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // ���Ʒ��� �սǵս�
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = startPos + Vector3.up * newY;
        // Y�� ȸ��(���ۺ���)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
