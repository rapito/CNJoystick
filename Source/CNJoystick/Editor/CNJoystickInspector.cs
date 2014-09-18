using UnityEditor;

[CustomEditor(typeof(CNJoystick))]
public class CNJoystickInspector : CNAbstractControllerInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var cnJoystick = (CNJoystick)target;

        EditorGUI.BeginChangeCheck();

        cnJoystick.Margin = EditorGUILayout.Vector2Field("Margins:", cnJoystick.Margin);
        cnJoystick.DragRadius = EditorGUILayout.FloatField("Drag radius:", cnJoystick.DragRadius);
        cnJoystick.IsSnappedToFinger = EditorGUILayout.Toggle("Snaps to finger:", cnJoystick.IsSnappedToFinger);

        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
    }
}
