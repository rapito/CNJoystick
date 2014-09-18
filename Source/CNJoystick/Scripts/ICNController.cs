using System;
using UnityEngine;

public interface ICNController
{
    event Action<Vector3, ICNController> ControllerMovedEvent;
    event Action<ICNController> FingerTouchedEvent;
    event Action<ICNController> FingerLiftedEvent;
    float GetAxis(string axisName);
}
