using UnityEngine;
using UnityEditor;

public class CNJoystickCreatorMenu : EditorWindow
{
    [MenuItem("GameObject/Create Other/CNJoystick")]
    static void ShowWindow()
    {
        //EditorWindow.GetWindow(typeof (CNJoystickCreatorMenu));
        GameObject cameraGo = GameObject.Find("CNControlCamera");

        if (cameraGo == null)
        {
            Debug.Log("Not found");
        }
        else
        {
           // var cameraTransform = cameraGo.GetComponent<Transform>();
            var joysticObject = AssetDatabase.LoadAssetAtPath("Assets/CNJoystick/Prefabs/Joystick.prefab", typeof (GameObject)) as GameObject;
            if (joysticObject == null)
            {
                Debug.LogError("Can't find Joystick prefab. " +
                               "Asset Database may be corrupted, or you could've renamed or moved the folder and/or the prefab. " +
                               "Please reimport the package or change everything back");
                return;
            }

            GameObject instantiatedJoystick = GameObject.Instantiate(joysticObject, Vector3.zero, Quaternion.identity) as GameObject;
            instantiatedJoystick.transform.parent = cameraGo.transform;
            instantiatedJoystick.name = "Joystick";
            instantiatedJoystick.GetComponent<CNJoystick>().OnEnable();

        }
    }

    private CNJoystick.Anchors _currentlySelectedAnchor = CNJoystick.Anchors.LeftBottom;

    void OnGUI()
    {
        title = "CNJoystick";

        _currentlySelectedAnchor = (CNJoystick.Anchors) EditorGUILayout.EnumPopup("Anchor", _currentlySelectedAnchor);
    }
}
