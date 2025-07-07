using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerspectiveScalable
{
    Transform ScalableTransform { get; }
    Vector3 InitialGrabScale { get; set; }
    float InitialGrabDistanceToCamera { get; set; }
    bool IsScalingEnabled { get; set; } // �����ϸ� Ȱ��/��Ȱ�� �÷���
}
