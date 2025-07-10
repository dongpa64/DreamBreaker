using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    public float floatSpeed = 2f;       // µÕ½Ç ¼Óµµ
    public float floatAmount = 0.2f;    // µÕ½Ç ¹üÀ§
    public float rotationSpeed = 60f;   // È¸Àü ¼Óµµ

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // À§¾Æ·¡·Î µÕ½ÇµÕ½Ç
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = startPos + Vector3.up * newY;
        // YÃà È¸Àü(ºù±Ûºù±Û)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
