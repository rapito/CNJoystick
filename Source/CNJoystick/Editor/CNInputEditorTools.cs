using UnityEditor;
using UnityEngine;

public class CNInputEditorTools : EditorWindow
{
    public static GameObject GetControlCamera()
    {
        GameObject cameraGo = GameObject.Find("CNControlCamera");

        if (cameraGo == null)
        {
            cameraGo = AssetDatabase.LoadAssetAtPath("Assets/CNJoystick/Prefabs/CNControlCamera.prefab", typeof(GameObject)) as GameObject;
            if (cameraGo == null)
            {
                throw new UnityException("Can't find CNControlCamera prefab. " +
                               "Asset Database may be corrupted, or you could've renamed or moved the folder and/or the prefab. " +
                               "Please reimport the package or change everything back");
            }

            cameraGo = GameObject.Instantiate(cameraGo,
                new Vector3(-50f, 0f, 0f),
                Quaternion.identity) as GameObject;
            cameraGo.name = "CNControlCamera";
        }

        return cameraGo;
    }

    public static void CreateControlFromPrefabsByName(string controlName)
    {
        GameObject cameraGo = CNInputEditorTools.GetControlCamera();

        var controlObject = AssetDatabase.LoadAssetAtPath(
            "Assets/CNJoystick/Prefabs/" + controlName + ".prefab", 
            typeof(GameObject)) as GameObject;

        if (controlObject == null)
        {
            throw new UnityException("Can't find " + controlName + " prefab. " +
                           "Asset Database may be corrupted, or you could've renamed or moved the folder and/or the prefab. " +
                           "Please reimport the package or change everything back");
        }

        // TODO Check for any CNControls on the scene and change the Anchor property of the new one accordingly

        GameObject instantiatedControl = GameObject.Instantiate(controlObject, Vector3.zero, Quaternion.identity) as GameObject;
        instantiatedControl.transform.parent = cameraGo.GetComponent<Transform>();
        instantiatedControl.name = controlName;
        instantiatedControl.GetComponent<CNAbstractController>().OnEnable();
    }
}
