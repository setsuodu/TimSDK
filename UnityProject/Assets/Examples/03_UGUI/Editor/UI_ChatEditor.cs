using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UI_Chat))]
public class UI_ChatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UI_Chat demo = (UI_Chat)target;
        if (GUILayout.Button("Send Text"))
        {
            demo.TextInput.text = "test";
            demo.SendText();
        }
        if (GUILayout.Button("Send Image"))
        {
            demo.SendImage();
        }
        if (GUILayout.Button("Recive Text"))
        {
            TIMMessageResp resp = new TIMMessageResp();
            resp.setMessage(0, 0, Utils.ToTimestamp(System.DateTime.Now), false, false, false)
                    .setType((int)TIMElemType.Text, -1)
                    .setText("对方的发言");
            List<TIMMessageResp> lst = new List<TIMMessageResp>();
            lst.Add(resp);
            string json = JsonMapper.ToJson(lst);

            var recvData = new TimCallback(TimSdkMessage.RecvNewMessages, 0, json);
            var recvJson = JsonMapper.ToJson(recvData);
            TimSdkManager.Instance.JsonCallback(recvJson);
        }
        if (GUILayout.Button("Recive Image"))
        {
            string path = System.IO.Path.Combine(Application.dataPath, "Resources/Sprites/Splash.png");
            TIMMessageResp resp = new TIMMessageResp();
            resp.setMessage(0, 0, Utils.ToTimestamp(System.DateTime.Now), false, false, false)
                    .setType((int)TIMElemType.Image, -1)
                    .setText(path);
            List<TIMMessageResp> lst = new List<TIMMessageResp>();
            lst.Add(resp);
            string json = JsonMapper.ToJson(lst);

            var recvData = new TimCallback(TimSdkMessage.RecvNewMessages, 0, json);
            var recvJson = JsonMapper.ToJson(recvData);
            TimSdkManager.Instance.JsonCallback(recvJson);
        }
        if (GUILayout.Button("Refresh"))
        {
            demo.mContentFiller.listScrollRect.RefreshContent();
        }
        if (GUILayout.Button("Goto"))
        {
            demo.mContentFiller.listScrollRect.GoToListItem(demo.idx);
        }
        if (GUILayout.Button("SetRead"))
        {
            demo.mContentFiller.SetRead();
        }
        if (GUILayout.Button("Report"))
        {
            demo.Report();
        }
    }
}
#endif
