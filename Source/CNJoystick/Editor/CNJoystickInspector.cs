using UnityEditor;

[CustomEditor(typeof(CNJoystick))]
public class CNJoystickInspector : CNAbstractControllerInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var cnJoystick = (CNJoystick)target;

        EditorGUI.BeginChangeCheck();

        cnJoystick.DragRadius = EditorGUILayout.FloatField("Drag radius:", cnJoystick.DragRadius);
        cnJoystick.IsSnappedToFinger = EditorGUILayout.Toggle("Snaps to finger:", cnJoystick.IsSnappedToFinger);
        cnJoystick.IsHiddenIfNotTweaking = EditorGUILayout.Toggle("Hide on release:", cnJoystick.IsHiddenIfNotTweaking);

        if (EditorGUI.EndChangeCheck())
            cnJoystick.OnEnable();
    }
}
