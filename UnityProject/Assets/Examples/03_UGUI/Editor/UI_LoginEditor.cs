#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UI_Login))]
public class UI_LoginEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_Login demo = (UI_Login)target;
        if (GUILayout.Button("Reset"))
        {
            demo.UserNameInput.text = "18069828910";
            demo.PasswordInput.text = "999000";
        }
    }
}
#endif
