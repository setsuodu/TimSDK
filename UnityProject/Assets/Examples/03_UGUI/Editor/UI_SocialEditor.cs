using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Client;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_Social))]
public class UI_SocialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_Social demo = (UI_Social)target;
        if (GUILayout.Button("LoadReset"))
        {
            SocialData.Get.LoadReset();
        }
        if (GUILayout.Button("RequestData"))
        {
            SocialData.Get.RequestData(0);
        }
        if (GUILayout.Button("Refresh"))
        {
            SocialData.Get.mLoopListView.RefreshAllShownItem();
        }
    }
}
#endif
