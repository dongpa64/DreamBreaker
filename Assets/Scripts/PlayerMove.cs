using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 90.0f; // degrees per second

    public Transform playerRig;

    void Update()
    {
        // ¿ÞÂÊ ½æ½ºÆ½À¸·Î ÀÌµ¿ (OVRInput »ç¿ë)
        Vector2 leftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 moveDirection = playerRig.right * leftThumbstick.x + playerRig.forward * leftThumbstick.y;
        playerRig.position += moveDirection * moveSpeed * Time.deltaTime;

        // ¿À¸¥ÂÊ ½æ½ºÆ½À¸·Î ¸ö È¸Àü (OVRInput »ç¿ë)
        Vector2 rightThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        float yawRotation = rightThumbstick.x * rotationSpeed * Time.deltaTime;
        playerRig.Rotate(Vector3.up, yawRotation);
    }
}