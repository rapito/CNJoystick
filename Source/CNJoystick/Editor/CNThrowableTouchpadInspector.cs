using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CNThrowableTouchpad))]
public class CNThrowableTouchpadInspector : CNTouchpadInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var cnThrowTouchpad = (CNThrowableTouchpad)target;
        cnThrowTouchpad.SpeedDecay = EditorGUILayout.Slider("Speed decay:", cnThrowTouchpad.SpeedDecay, 0.01f, 0.99f);
    }
}
