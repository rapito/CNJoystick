using UnityEditor;
using UnityEngine;

public class CNThrowableTouchpadCreatorMenu : EditorWindow
{
    [MenuItem("GameObject/Create Other/CNControl/CN Throwable Touchpad")]
    private static void CreateCNTouchpad()
    {
        CNInputEditorTools.CreateControlFromPrefabsByName("CNThrowableTouchpad");
    }
}
