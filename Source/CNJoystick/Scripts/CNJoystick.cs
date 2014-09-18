using System;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CNJoystick : CNAbstractController
{
    // Editor visible public properties
    public Vector2 Margin { get { return _margin; } set { _margin = value; } }
    public float DragRadius { get { return _dragRadius; } set { _dragRadius = value; } }
    public bool IsSnappedToFinger { get { return _isSnappedToFinger; } set { _isSnappedToFinger = value; } }

    // Serializable fields (user preferences)
    [SerializeField]
    private Vector2 _margin = new Vector2(5f, 5f);
    [SerializeField]
    private float _dragRadius = 1.5f;
    [SerializeField]
    private bool _isSnappedToFinger = true;

    // Runtime used fields
    private Transform _stickTransform;
    private Transform _baseTransform;
    private bool _isCurrentlyTweaking;
    private int _currentFingerId;
    private SpriteRenderer[] _joystickRenderers;

    /// <summary>
    /// Neat initialization method
    /// </summary>
    public void OnEnable()
    {
        TransformCache = GetComponent<Transform>();

#if UNITY_EDITOR
        // If we've instantiated the prefab but haven't parented it to a camera
        // Editor only issue
        if (TransformCache.parent == null) return;
#endif

        ParentCamera = TransformCache.parent.GetComponent<Camera>();
        _stickTransform = TransformCache.FindChild("Stick").GetComponent<Transform>();
        _baseTransform = TransformCache.FindChild("Base").GetComponent<Transform>();

        TransformCache.localPosition = InitializePosition();
    }

    /// <summary>
    /// Your favorite Update method where all the magic happens
    /// </summary>
    private void Update()
    {
        // Check for touches
        if (_isCurrentlyTweaking)
        {
            // Find our touch by fingerID
            // It's a nullable Touch, so we can test whether it's equal to null
            Touch? touch = GetTouchByFingerID(_currentFingerId);

            // If there's no touch, we missed it's Ended phase OR
            // If there's one and it's phase is Ended
            // we just reset the joystick to it's default state
            if (touch == null || touch.Value.phase == TouchPhase.Ended)
            {
                // It's no longer tweaking
                _isCurrentlyTweaking = false;
                // Setting the stick local position back to local zero
                _stickTransform.localPosition = Vector3.zero;
                // It's optimized for one-time calculation inside the game
                // In the editor, however, we may wan't to recalculate it
                _baseTransform.localPosition = Vector3.zero;
                // Setting our inner axis values back to zero
                CurrentAxisValues = Vector2.zero;

                // Fire our FingerLiftedEvents
                OnFingerLifted();
            }
            // If we have our touch, we just continue to tweak the joystick with it
            else
            {
                Touch currentTouch = touch.Value;
                TweakJoystick(currentTouch.position);
                // Since we already got our Touch, there's no need to check other touches
                // We return early
                return;
            }
        }

        // Some optimization things
        int touchCount = Input.touchCount;

        // For every touch out there
        for (int i = 0; i < touchCount; i++)
        {
            // God bless local variables of value types
            Touch currentTouch = Input.GetTouch(i);

            // Check if we're interested in this touch
            if (currentTouch.phase == TouchPhase.Began && IsTouchInZone(currentTouch.position))
            {
                // If we are, capture the touch and make it ours

                _isCurrentlyTweaking = true;
                // Store it's finger ID so we can find it later
                _currentFingerId = currentTouch.fingerId;

                // Place joystick under the finger 
                // The "no jumping" logic is also in this method
                PlaceJoystickBaseUnderTheFinger(currentTouch);

                // Fire our FingerTouchedEvent
                OnFingerTouched();

                // We don't need to check other touches
                break;
            }
        }
    }

    /// <summary>
    /// Function for joystick tweaking (moving under the finger)
    /// The values of the Axis are also calculated here
    /// </summary>
    /// <param name="touchPosition">Current touch position in screen cooridnates (pixels)</param>
    private void TweakJoystick(Vector2 touchPosition)
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
    private void PlaceJoystickBaseUnderTheFinger(Touch touch)
    {
        if (!_isSnappedToFinger) return;

        _baseTransform.position = ParentCamera.ScreenToWorldPoint(touch.position);
    }

    /// <summary>
    /// Utility method, chechks whether the touch is inside the touch zone (green rect)
    /// </summary>
    /// <param name="touchPosition">Current touch position in screen pixels</param>
    /// <returns>Whether it's inside of the touch zone</returns>
    private bool IsTouchInZone(Vector2 touchPosition)
    {
        return CalculatedTouchZone.Contains(ParentCamera.ScreenToWorldPoint(touchPosition), false);
    }

    /// <summary>
    /// Calculates local position based on margins
    /// </summary>
    /// <returns>Calculated position</returns>
    private Vector3 InitializePosition()
    {
#if !UNITY_EDITOR
        if (_calculatedPosition != null)
            return _calculatedPosition.Value;
#endif

#if UNITY_EDITOR
        // Editor error "handling"
        // Happens when you duplicate the joystick in the editor window
        // causes a bit of recursion, but it's ok, it will just try to calculate joystick position twice
        if (ParentCamera == null)
            OnEnable();
#endif

        // Camera based calculations (different aspect ratios)
        float halfHeight = ParentCamera.orthographicSize;
        float halfWidth = halfHeight * ParentCamera.aspect;

        var newPosition = new Vector3(0f, 0f, 0f);

        if (((int)Anchor & (int)AnchorsBase.Left) != 0)
            newPosition.x = -halfWidth + Margin.x;
        else
            newPosition.x = halfWidth - Margin.x;

        if (((int)Anchor & (int)AnchorsBase.Top) != 0)
            newPosition.y = halfHeight - Margin.y;
        else
            newPosition.y = -halfHeight + Margin.y;

        CalculatedTouchZone = new Rect(
            TransformCache.position.x - TouchZoneSize.x / 2f,
            TransformCache.position.y - TouchZoneSize.y / 2f,
            TouchZoneSize.x,
            TouchZoneSize.y);

        return newPosition;
    }

    // Some editor-only stuff. It won't compile to any of the builds
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // We have no need to recalculate the base position
        // Tweaking these things in Playmode won't save anyway
        if (!EditorApplication.isPlaying)
            TransformCache.localPosition = InitializePosition();            

        // Store the Gizmos color to restore everything back once we finish
        Color color = Gizmos.color;
        Gizmos.color = Color.green;

        // It's a local variable for more readability
        Vector3 localRectCenter = new Vector3(
                CalculatedTouchZone.x + CalculatedTouchZone.width / 2f,
                CalculatedTouchZone.y + CalculatedTouchZone.height / 2f,
                TransformCache.position.z);

        Gizmos.DrawWireCube(
            localRectCenter,
            new Vector3(TouchZoneSize.x, TouchZoneSize.y, 0f));
        
        // Perfect time to restore the original color back
        // It's rarely an issue though
        Gizmos.color = color;
    }
#endif


}
