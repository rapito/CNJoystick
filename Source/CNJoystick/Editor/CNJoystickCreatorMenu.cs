using UnityEngine;
using UnityEditor;

public class CNJoystickCreatorMenu : EditorWindow
{
    [MenuItem("GameObject/Create Other/CNControl/CNJoystick")]
    private static void CreateCNJoystick()
    {
        CNInputEditorTools.CreateControlFromPrefabsByName("CNJoystick");
    }

    private CNJoystick.Anchors _currentlySelectedAnchor = CNJoystick.Anchors.LeftBottom;

    void OnGUI()
    {
        title = "CNJoystick";

        _currentlySelectedAnchor = (CNJoystick.Anchors)EditorGUILayout.EnumPopup("Anchor", _currentlySelectedAnchor);
    }
}
