using System;
using UnityEngine;
using System.Collections;

public class CNTouchpad : CNAbstractController
{
    // -------------------------
    // Editor visible properties
    // -------------------------
    // Set to true if you wan't to fully control the speed of the drag
    public bool IsAlwaysNormalized { get { return _isAlwaysNormalized; } set { _isAlwaysNormalized = value; } }

    // Serialized fields
    [SerializeField]
    [HideInInspector]
    private bool _isAlwaysNormalized = true;

    // To find touch movement delta we need to store previous touch position
    // It's stored in world coordinates to provide resolution invariance
    // since different mobile devices have different DPI
    public Vector3 PreviousPosition { get; set; }

    // We check if it's the first frame after we touch. To get any input we need at least two frames
    public bool IsFirstFrameAfterTouched { get; set; }

    protected virtual void Update()
    {
        // If we tweaked, we return and don't check other touches
        if (TweakIfNeeded())
            return;

        // If we didn't tweak, we try to capture any touch
        Touch currentTouch;
        if (IsTouchCaptured(out currentTouch))
        {
            IsFirstFrameAfterTouched = true;
            PreviousPosition = currentTouch.position;
        }
    }

    /// <summary>
    /// Automatically called by TweakIfNeeded
    /// </summary>
    /// <param name="touchPosition">Touch position in screen pixels</param>
    protected override void TweakControl(Vector2 touchPosition)
    {
        if (IsFirstFrameAfterTouched)
        {
            PreviousPosition = ParentCamera.ScreenToWorldPoint(touchPosition);
            IsFirstFrameAfterTouched = false;
        }
        else
        {
            Vector3 worldPosition = ParentCamera.ScreenToWorldPoint(touchPosition);

            Vector3 difference = worldPosition - PreviousPosition;

            if(IsAlwaysNormalized)
                difference.Normalize();

            CurrentAxisValues = difference;

            OnControllerMoved(difference);

            PreviousPosition = worldPosition;
        }
    }
}
