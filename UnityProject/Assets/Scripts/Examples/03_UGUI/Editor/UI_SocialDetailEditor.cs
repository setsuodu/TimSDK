using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Client;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_SocialDetail))]
public class UI_SocialDetailEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_SocialDetail demo = (UI_SocialDetail)target;
        if (GUILayout.Button("LoadMore"))
        {
            SocialDetailData.Get.DoLoadMoreDataSource();
            SocialDetailData.Get.mLoopListView.SetListItemCount(SocialDetailData.Get.TotalItemCount + 1, false);
            Debug.Log($"Load {SocialDetailData.Get.TotalItemCount + 1}");
        }
        if (GUILayout.Button("Refresh"))
        {
            SocialDetailData.Get.mLoopListView.RefreshAllShownItem();
        }
    }
}
#endif
