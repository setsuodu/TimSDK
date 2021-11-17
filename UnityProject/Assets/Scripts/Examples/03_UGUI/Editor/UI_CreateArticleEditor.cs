using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_CreateArticle))]
public class UI_CreateArticleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_CreateArticle demo = (UI_CreateArticle)target;
        if (GUILayout.Button("ToString"))
        {
            demo.Print();
        }
        if (GUILayout.Button("Add"))
        {
            demo.Add();
        }
    }
}
#endif
