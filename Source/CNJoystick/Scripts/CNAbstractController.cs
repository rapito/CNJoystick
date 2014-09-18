using System;
using UnityEngine;

[Serializable]
public abstract class CNAbstractController : MonoBehaviour, ICNController
{
    // Constants for optimization. We don't need separate strings for every joystick object
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
    public Anchors Anchor { get {return _anchor;} set { _anchor = value; } }
    // Axis names are used with the GetAxis(..) method. It's just like Input.GetAxis(..)
    public string AxisNameX { get { return _axisNameX; } set { _axisNameX = value; } }
    public string AxisNameY { get { return _axisNameY; } set { _axisNameY = value; } }
    // Touch zone size indicates how big is the sensitive area of the control
    // TODO: check whether different touch zones intersect
    public Vector2 TouchZoneSize { get { return _touchZoneSize; } set { _touchZoneSize = value; } }

    // -------------------
    // Event based control
    // -------------------
    
    // Fires when user tweaks the control
    public event Action<Vector3, ICNController> ControllerMovedEvent;
    // Fires when user has just touched the control (the control became active)
    public event Action<ICNController> FingerTouchedEvent;
    // Fires when user has just abandoned the control (the control became inactive)
    public event Action<ICNController> FingerLiftedEvent;

    // Simple Transform property, used in runtime, it's more fast than getting the .transform property
    protected Transform TransformCache { get; set; }
    // Parent camera is an Orthographical camera where all CNControls are stored 
    protected Camera ParentCamera { get; set; }
    // Runtime calculated Rect, used for touch position checks
    protected Rect CalculatedTouchZone { get; set; }
    // Pretty self-explanatory
    protected Vector3 CurrentAxisValues { get; set; }
    // Current captured finger ID
    protected int CurrentFingerId { get; set; }

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
    private Vector2 _touchZoneSize = new Vector2(2f, 2f);

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
    /// Utility method, finds the touch by it's fingerID, which is often different from it's index in .touches
    /// </summary>
    /// <param name="fingerId">The fingerId to find touch for</param>
    /// <returns>null if no touch found, returns a Touch if it's found</returns>
    protected virtual Touch? GetTouchByFingerID(int fingerId)
    {
        int touchCount = Input.touchCount;

        for (int i = 0; i < touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.fingerId == fingerId) return touch;
        }

        // If there's no Touch with the specified fingerId, return null
        return null;
    }

    protected virtual void OnControllerMoved(Vector3 input)
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
}
