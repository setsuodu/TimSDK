#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Client;

[CustomEditor(typeof(UI_Message))]
public class UI_MessageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_Message demo = (UI_Message)target;
        if (GUILayout.Button("LoadMore"))
        {
            ConversationData.Get.LoadMore();
        }
        if (GUILayout.Button("Loading"))
        {
            demo.StartCoroutine(demo.OnRefresh());
        }
        if (GUILayout.Button("FadeIn"))
        {
            demo.FadeIn();
        }
        if (GUILayout.Button("FadeOut"))
        {
            demo.FadeOut();
        }
    }
}
#endif
