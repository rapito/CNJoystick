using UnityEditor;
using UnityEngine;

public class CNTouchpadCreatorMenu : EditorWindow
{
    [MenuItem("GameObject/Create Other/CNControl/CNTouchpad")]
    private static void CreateCNTouchpad()
    {
        CNInputEditorTools.CreateControlFromPrefabsByName("CNTouchpad");
    }
}
