using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CNAbstractController))]
public class CNAbstractControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
      //  base.OnInspectorGUI();

        
        var cnAbstractController = (CNAbstractController)target;

        EditorGUI.BeginChangeCheck();
        
        cnAbstractController.Anchor = (CNAbstractController.Anchors) 
            EditorGUILayout.EnumPopup("Anchors:", cnAbstractController.Anchor);
        cnAbstractController.AxisNameX = EditorGUILayout.TextField("X Axis:", cnAbstractController.AxisNameX);
        cnAbstractController.AxisNameY = EditorGUILayout.TextField("Y Axis:", cnAbstractController.AxisNameY);
        cnAbstractController.TouchZoneSize = EditorGUILayout.Vector2Field("Touch Zone Size:",
            cnAbstractController.TouchZoneSize);

        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
        
    }
}


