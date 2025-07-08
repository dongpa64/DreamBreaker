using UnityEngine;

using UnityEngine.EventSystems;

public class SimpleCursor : OVRCursor

{

    public override void SetCursorRay(Transform ray)

    {

        if (ray != null)

            transform.position = ray.position + ray.forward * 0.2f; // Ŀ���� ray ���� ��ġ

    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)

    {

        transform.position = dest;

    }

}