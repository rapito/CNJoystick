using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CNJoystick : CNAbstractController
{
    // ---------------------------------
    // Editor visible public properties
    // ---------------------------------
    public float DragRadius { get { return _dragRadius; } set { _dragRadius = value; } }
    public bool IsSnappedToFinger { get { return _isSnappedToFinger; } set { _isSnappedToFinger = value; } }
    public bool IsHiddenIfNotTweaking { get { return _isHiddenIfNotTweaking; } set { _isHiddenIfNotTweaking = value; } }

    // Serializable fields (user preferences)
    [SerializeField]
    [HideInInspector]
    private float _dragRadius = 1.5f;
    [SerializeField]
    [HideInInspector]
    private bool _isSnappedToFinger = true;
    [SerializeField]
    [HideInInspector]
    private bool _isHiddenIfNotTweaking;

    // Runtime used fields
    private Transform _stickTransform;
    private Transform _baseTransform;
    private GameObject _stickGameObject;
    private GameObject _baseGameObject;

    public override void OnEnable()
    {
        base.OnEnable();

        _stickTransform = TransformCache.FindChild("Stick").GetComponent<Transform>();
        _baseTransform = TransformCache.FindChild("Base").GetComponent<Transform>();

        _stickGameObject = _stickTransform.gameObject;
        _baseGameObject = _baseTransform.gameObject;

        if (IsHiddenIfNotTweaking)
        {
            _baseGameObject.gameObject.SetActive(false);
            _stickGameObject.gameObject.SetActive(false);
        }
        else
        {
            _baseGameObject.gameObject.SetActive(true);
            _stickGameObject.gameObject.SetActive(true);
        }
        
    }

    public override void Disable()
    {
        base.Disable();
        gameObject.SetActive(false);
       // _baseTransform.gameObject.SetActive(false);
       // _stickTransform.gameObject.SetActive(false);
    }

    public override void Enable()
    {
        base.Enable();
        gameObject.SetActive(true);
       // _baseTransform.gameObject.SetActive(true);
       // _stickTransform.gameObject.SetActive(true);
    }

    protected override void ResetControlState()
    {
        base.ResetControlState();
        // Setting the stick and base local positions back to local zero
        _stickTransform.localPosition = _baseTransform.localPosition = Vector3.zero;
    }

    protected override void OnFingerLifted()
    {
        base.OnFingerLifted();
        if (!IsHiddenIfNotTweaking) return;

        _baseGameObject.gameObject.SetActive(false);
        _stickGameObject.gameObject.SetActive(false);
    }

    protected override void OnFingerTouched()
    {
        base.OnFingerTouched();
        if (!IsHiddenIfNotTweaking) return;

        _baseGameObject.gameObject.SetActive(true);
        _stickGameObject.gameObject.SetActive(true);
    }

    /// <summary>
    /// Your favorite Update method where all the magic happens
    /// </summary>
    protected virtual void Update()
    {
        // Check for touches
        if (TweakIfNeeded())
                return;

        Touch currentTouch;
        if (IsTouchCaptured(out currentTouch))
            // Place joystick under the finger 
            // The "no jumping" logic is also in this method
            PlaceJoystickBaseUnderTheFinger(currentTouch);
    }

    /// <summary>
    /// Function for joystick tweaking (moving under the finger)
    /// The values of the Axis are also calculated here
    /// </summary>
    /// <param name="touchPosition">Current touch position in screen cooridnates (pixels)</param>
    protected override void TweakControl(Vector2 touchPosition)
    {
        // First, let's find our current touch position in world space
        Vector3 worldTouchPosition = ParentCamera.ScreenToWorldPoint(touchPosition);

        // Now we need to find a directional vector from the center of the joystick
        // to the touch position
        Vector3 differenceVector = (worldTouchPosition - _baseTransform.position);

        // If we're out of the drag range
        if (differenceVector.sqrMagnitude >
            DragRadius * DragRadius)
        {
            // Normalize this directional vector
            differenceVector.Normalize();

            //  And place the stick to it's extremum position
            _stickTransform.position = _baseTransform.position +
                differenceVector * DragRadius;
        }
        else
        {
            // If we're inside the drag range, just place it under the finger
            _stickTransform.position = worldTouchPosition;
        }

        // Store calculated axis values to our private variable
        CurrentAxisValues = differenceVector;

        // We also fire our event if there are subscribers
        OnControllerMoved(differenceVector);
    }

    /// <summary>
    /// Snap the joystick under the finger if it's expected
    /// </summary>
    /// <param name="touch">Current touch position in screen pixels</param>
    protected virtual void PlaceJoystickBaseUnderTheFinger(Touch touch)
    {
        if (!_isSnappedToFinger) return;

        _baseTransform.position = ParentCamera.ScreenToWorldPoint(touch.position);
    }

}
