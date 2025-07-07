using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerspectiveScalable
{
    Transform ScalableTransform { get; }
    Vector3 InitialGrabScale { get; set; }
    float InitialGrabDistanceToCamera { get; set; }
    bool IsScalingEnabled { get; set; } // 스케일링 활성/비활성 플래그
}
