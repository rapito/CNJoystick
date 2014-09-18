using UnityEngine;
using System.Collections;

public class CNThrowableTouchpad : CNTouchpad
{
    // -------------------------
    // Editor visible properties
    // -------------------------
    public float SpeedDecay { get { return _speedDecay; } set { _speedDecay = value; } }

    [SerializeField]
    [HideInInspector]
    private float _speedDecay = 0.9f;

    protected override void ResetControlState()
    {
        IsCurrentlyTweaking = false;
        OnFingerLifted();
    }

    protected override void Update()
    {
        // If we tweaked, we return and don't check other touches
        if (TweakIfNeeded())
            return;

        // If we didn't tweak, we try to capture any touch
        Touch currentTouch;
        if (IsTouchCaptured(out currentTouch))
        {
            IsFirstFrameAfterTouched = true;
            return;
        }

        if (CurrentAxisValues.sqrMagnitude <= 0.001f)
        {
            CurrentAxisValues = Vector2.zero;
            return;
        }
        
        // We reached the "Throw" code
        CurrentAxisValues *= SpeedDecay;
        OnControllerMoved(CurrentAxisValues);
    }
}
