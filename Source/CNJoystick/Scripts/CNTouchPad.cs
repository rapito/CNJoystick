using System;
using UnityEngine;
using System.Collections;

public class CNTouchPad : ICNController
{
    public event Action<Vector3, ICNController> ControllerMovedEvent;
    public event Action<ICNController> FingerTouchedEvent;
    public event Action<ICNController> FingerLiftedEvent;

    public float GetAxis(string axisName)
    {
        throw new NotImplementedException();
    }
}
