using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_FriendList))]
public class UI_FriendListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_FriendList demo = (UI_FriendList)target;
        if (GUILayout.Button("RefreshData"))
        {
            //demo.RefreshData();
        }
    }
}
#endif
