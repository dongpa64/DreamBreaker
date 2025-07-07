using UnityEngine;

public class RightHand : MonoBehaviour
{
    void Update()
    {
#if !PC
        transform.position = ARAVRInput.RHandPosition;
#endif
    }
}
