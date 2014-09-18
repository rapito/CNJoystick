using System.Reflection;
using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        object touch = new Touch();

        // FieldInfo field = typeof (Touch).GetField("phase");
        // Debug.Log(field);

        FieldInfo field = typeof(Touch).GetField("m_Phase", BindingFlags.NonPublic | BindingFlags.Instance);

        field.SetValue( touch, TouchPhase.Moved);

        Debug.Log(((Touch)touch).phase);
        //FieldInfo[] members = typeof(Touch).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      //  foreach (FieldInfo m in members)
        //    Debug.Log(m.Name);
    }
}
