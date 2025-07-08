using UnityEngine;

using UnityEngine.EventSystems;

public class SimpleCursor : OVRCursor

{

    public override void SetCursorRay(Transform ray)

    {

        if (ray != null)

            transform.position = ray.position + ray.forward * 0.2f; // 커서를 ray 끝에 위치

    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)

    {

        transform.position = dest;

    }

}