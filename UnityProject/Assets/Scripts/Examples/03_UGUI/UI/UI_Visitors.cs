using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_Visitors))]
public class UI_VisitorsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_Visitors demo = (UI_Visitors)target;
        if (GUILayout.Button("Refresh"))
        {
            demo.Refresh();
        }
    }
}
#endif

/// <summary>
/// 访客列表
/// </summary>
public class UI_Visitors : UIWidget
{
    private GameObject prefab;
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Transform m_ItemGroup;
    [SerializeField] private List<TIMUserProfile> visitorsList; //Data
    [SerializeField] private List<Item_Friend> visitorItemList; //View

    void Awake()
    {
        prefab = Resources.Load<GameObject>("prefabs/item/item_friend");
        visitorsList = new List<TIMUserProfile>();
        visitorItemList = new List<Item_Friend>();

        m_BackBtn.onClick.AddListener(() => base.Close(true));
    }

    public void Refresh()
    {

    }
}
