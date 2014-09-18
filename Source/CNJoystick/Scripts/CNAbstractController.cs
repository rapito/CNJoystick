using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[Serializable]
public abstract class CNAbstractController : MonoBehaviour, ICNController
{
    // Constants for optimization. We don't need separate strings for every control object
    private const string AxisNameHorizontal = "Horizontal";
    private const string AxisNameVertical = "Vertical";

    // Some neat bitwise enums
    [Flags]
    protected enum AnchorsBase
    {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }

    // Combined enums
    public enum Anchors
    {
        LeftTop = AnchorsBase.Left | AnchorsBase.Top,
        LeftBottom = AnchorsBase.Left | AnchorsBase.Bottom,
        RightTop = AnchorsBase.Right | AnchorsBase.Top,
        RightBottom = AnchorsBase.Right | AnchorsBase.Bottom
    }

    // --------------------------------
    // Editor visible public properties
    // --------------------------------

    // Anchor is a place where the controls snap
    public Anchors Anchor { get { return _anchor; } set { _anchor = value; } }
    // Axis names are used with the GetAxis(..) method. It's just like Input.GetAxis(..)
    public string AxisNameX { get { return _axisNameX; } set { _axisNameX = value; } }
    public string AxisNameY { get { return _axisNameY; } set { _axisNameY = value; } }
    //
    public Vector2 Margin { get { return _margin; } set { _margin = value; } }
    // Touch zone size indicates how big is the sensitive area of the control
    // TODO: check whether different touch zones intersect
    public Vector2 TouchZoneSize { get { return _touchZoneSize; } set { _touchZoneSize = value; } }

    // -------------------
    // Event based control
    // -------------------

    // Fires when the user tweaks the control
    public event Action<Vector3, ICNController> ControllerMovedEvent;
    // Fires when the user has just touched the control (the control became active)
    public event Action<ICNController> FingerTouchedEvent;
    // Fires when the user has just abandoned the control (the control became inactive)
    public event Action<ICNController> FingerLiftedEvent;

    // Simple Transform property, used in runtime, it's more fast than getting the .transform property
    protected Transform TransformCache { get; set; }
    // Parent camera is an Orthographical camera where all CNControls are stored 
    protected Camera ParentCamera { get; set; }
    // Runtime calculated Rect, used for touch position checks
    protected Rect CalculatedTouchZone { get; set; }
    // Pretty self-explanatory
    protected Vector2 CurrentAxisValues { get; set; }
    // Current captured finger ID
    protected int CurrentFingerId { get; set; }
    // Nullable Vector3 for optimization. We can check if we've already found it
    protected Vector3? CalculatedPosition { get; set; }
    // Whether the control is being currently tweaked
    protected bool IsCurrentlyTweaking { get; set; }

    // Private fields
    [SerializeField]
    [HideInInspector]
    private Anchors _anchor = Anchors.LeftBottom;
    [SerializeField]
    [HideInInspector]
    private string _axisNameX = AxisNameHorizontal;
    [SerializeField]
    [HideInInspector]
    private string _axisNameY = AxisNameVertical;
    [SerializeField]
    [HideInInspector]
    private Vector2 _touchZoneSize = new Vector2(6f, 6f);
    [SerializeField]
    [HideInInspector]
    private Vector2 _margin = new Vector2(3f, 3f);

    public virtual float GetAxis(string axisName)
    {
        if (AxisNameX == null || AxisNameY == null || AxisNameX == String.Empty || AxisNameY == String.Empty)
        {
            throw new UnityException("Input Axis " + axisName + " is not setup");
        }

        if (axisName == AxisNameX)
            return CurrentAxisValues.x;

        if (axisName == AxisNameY)
            return CurrentAxisValues.y;

        throw new UnityException("Input Axis " + axisName + " is not setup");
    }

    /// <summary>
    /// Call this method to temporarily disable the control
    /// It will hide and won't respond to any of the Unity callbacks
    /// </summary>
    public virtual void Disable()
    {
        CurrentAxisValues = Vector2.zero;
        // Unity defined MonoBehaviour property
        enabled = false;
    }

    /// <summary>
    /// Call this method to enable the control back
    /// </summary>
    public virtual void Enable()
    {
        // Unity defined MonoBehaviour property
        enabled = true;
    }

    /// <summary>
    /// Neat initialization method
    /// </summary>
    public virtual void OnEnable()
    {
        TransformCache = GetComponent<Transform>();

#if UNITY_EDITOR
        // If we've instantiated the prefab but haven't parented it to a camera
        // Editor only issue
        if (TransformCache.parent == null) return;
#endif

        ParentCamera = TransformCache.parent.GetComponent<Camera>();

        TransformCache.localPosition = InitializePosition();
    }

    /// <summary>
    /// Utility method, finds the touch by it's fingerID, which is often different from it's index in .touches
    /// </summary>
    /// <param name="fingerId">The fingerId to find touch for</param>
    /// <returns>null if no touch found, returns a Touch if it's found</returns>
    protected virtual Touch? GetTouchByFingerId(int fingerId)
    {
        int touchCount = Input.touchCount;

        for (int i = 0; i < touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.fingerId == fingerId) return touch;
        }

#if UNITY_EDITOR
        // If we're in the editor, we also take our mouse as input
        // Let's say it's fingerId is 255;
        if (fingerId == 255)
        {
            return ConstructTouchFromMouseInput();
        }
#endif

        // If there's no Touch with the specified fingerId, return null
        return null;
    }

    protected virtual void OnControllerMoved(Vector2 input)
    {
        if (ControllerMovedEvent != null)
            ControllerMovedEvent(input, this);
    }

    protected virtual void OnFingerTouched()
    {
        if (FingerTouchedEvent != null)
            FingerTouchedEvent(this);
    }

    protected virtual void OnFingerLifted()
    {
        if (FingerLiftedEvent != null)
            FingerLiftedEvent(this);
    }

    /// <summary>
    /// Calculates local position based on margins
    /// </summary>
    /// <returns>Calculated position</returns>
    protected Vector3 InitializePosition()
    {
#if !UNITY_EDITOR
        if (CalculatedPosition != null)
            return CalculatedPosition.Value;
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

        // Bitwise checks
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

    protected virtual void ResetControlState()
    {
        // It's no longer tweaking
        IsCurrentlyTweaking = false;
        // Setting our inner axis values back to zero
        CurrentAxisValues = Vector2.zero;
        // Fire our FingerLiftedEvents
        OnFingerLifted();
    }

    protected virtual bool TweakIfNeeded()
    {
        // Check for touches
        if (IsCurrentlyTweaking)
        {
            Touch? touch = GetTouchByFingerId(CurrentFingerId);
            if (touch == null || touch.Value.phase == TouchPhase.Ended)
            {
                ResetControlState();
                return false;
            }
            TweakControl(touch.Value.position);
            return true;
        }
        return false;
    }

    protected virtual bool IsTouchCaptured(out Touch capturedTouch)
    {
        // Some optimization things
        int touchCount = Input.touchCount;

#if UNITY_EDITOR
        int actualTouchCount = touchCount;
        touchCount++;
#endif

        // For every touch out there
        for (int i = 0; i < touchCount; i++)
        {
#if UNITY_EDITOR
             Touch currentTouch = i >= actualTouchCount ? ConstructTouchFromMouseInput() : Input.GetTouch(i);
#else
            // God bless local variables of value types
            Touch currentTouch = Input.GetTouch(i);
#endif
            // Check if we're interested in this touch
            if (currentTouch.phase == TouchPhase.Began && IsTouchInZone(currentTouch.position))
            {
                // If we are, capture the touch and make it ours
                IsCurrentlyTweaking = true;
                // Store it's finger ID so we can find it later
                CurrentFingerId = currentTouch.fingerId;
                // Fire our FingerTouchedEvent
                OnFingerTouched();

                capturedTouch = currentTouch;
                // We don't need to check other touches
                return true;
            }
        }

        // To satisfy the compiler. It won't be used
        capturedTouch = new Touch();
        return false;
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

    protected abstract void TweakControl(Vector2 touchPosition);

    // Some editor-only stuff. It won't compile to any of the builds
#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        TransformCache = GetComponent<Transform>();
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

    private Touch ConstructTouchFromMouseInput()
    {
        // Boxing
        object mouseAsTouch = new Touch();

        // Some nasty Reflection stuff
        FieldInfo phaseFieldInfo = mouseAsTouch.GetType().
            GetField("m_Phase", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo positionFieldInfo = mouseAsTouch.GetType().
            GetField("m_Position", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo fingerIdFieldInfo = mouseAsTouch.GetType().
            GetField("m_FingerId", BindingFlags.NonPublic | BindingFlags.Instance);

        if (Input.GetMouseButtonDown(0))
            phaseFieldInfo.SetValue(mouseAsTouch, TouchPhase.Began);
        else if (Input.GetMouseButtonUp(0))
            phaseFieldInfo.SetValue(mouseAsTouch, TouchPhase.Ended);
        else
            // We don't check if it's actually moved for simplicity, we don't use Moved / Stationary anyway
            phaseFieldInfo.SetValue(mouseAsTouch, TouchPhase.Moved);

        positionFieldInfo.SetValue(mouseAsTouch, new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        fingerIdFieldInfo.SetValue(mouseAsTouch, 255);

        return (Touch)mouseAsTouch;
    }
#endif
}
