using System.Collections;
using UnityEngine;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(apiServerDemo))]
public class apiServerDemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        apiServerDemo demo = (apiServerDemo)target;
        if (GUILayout.Button("getSig"))
        {
            demo.getSig(HttpManager.token);
        }
        if (GUILayout.Button("toFriend"))
        {
            int rid = 1;
            demo.toFriend(HttpManager.token, rid);
        }
        if (GUILayout.Button("myinfo"))
        {
            demo.myinfo(HttpManager.token);
        }
        if (GUILayout.Button("info"))
        {
            int rid = 1;
            demo.info(HttpManager.token, rid);
        }
        if (GUILayout.Button("relationUser"))
        {
            int rid = 1;
            demo.relationUser(HttpManager.token, rid);
        }
    }
}
#endif
public class apiServerDemo : MonoBehaviour
{
    public void getSig(string token)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        string url = $"{ConstValue.api_route}/api/user/sig";
        HttpManager.HttpPost(url, form, onGetSig);
    }
    public void onGetSig(string json)
    {
        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            Debug.Log($"userSig={obj.data}");
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
        }
    }

    //TODO: 测试通过调用RESTAPI，SDK回调事件。
    // 加好友
    public void toFriend(string token, int rid)
    {
        //"http://localhost:8082/api/relationship/toFriend?token=51b19cf624e5162abcd8b8f441d9b065df3283742ae6f412&rid=1"
        string url = $"{ConstValue.api_route}/api/relationship/toFriend?token={token}&rid={rid}";
        string result = RESTAPI.HttpGet(url);
        Debug.Log(result);
    }

    public void myinfo(string token)
    {
        string url = $"{ConstValue.api_route}/api/user/myinfo?token={token}";
        HttpManager.HttpGet(url, onMyInfo);
        //"_id":2,"
        //updateTime":1585299769011,
        //"coin":0,"diamond":0,"exp":0,"flower":0,"flowerBalance":0,"role":0,"visitor":0,"nickname":"用户4","avatar":"http://avatar.zd1312.com/def/women_320_320.png","sex":0,"birthday":0,"signalType":0,"signal":null,"poseId":0,"phone":0,"vip":0,"wardrobeCnt":0,"imInit":1,"province":null,"city":null,"cpId":null}
    }
    public void onMyInfo(string json)
    {
        Debug.Log(json);
        var obj = JsonMapper.ToObject<HttpCallback<MyInfoCallback>>(json);
        if (obj.code == 0)
        {
            Debug.Log($"myinfo={obj.data}");
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
        }
    }

    public void info(string token, int uid)
    {
        string url = $"{ConstValue.api_route}/api/user/myinfo?token={token}";
        string result = RESTAPI.HttpGet(url);
        Debug.Log(result);
    }

    public void relationUser(string token, int rid)
    {
        ////http://localhost:8082/api/user/relationUser?token=51b19cf624e5162abcd8b8f441d9b065df3283742ae6f412&rid=1
        //string url = $"{ConstValue.api_route}/api/user/relationUser?token={token}&rid={rid}";
        //string result = RESTAPI.HttpGet(url);
        //Debug.Log(result);
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("rid", rid);
        string url = $"{ConstValue.api_route}/api/user/relationUser";
        WWW www = new WWW(url, form);
        while (!www.isDone) { }
        //yield return www;
        if(!string.IsNullOrEmpty(www.error)) 
        {
            Debug.LogError(www.error);
            //yield break;
        }
        string result = www.text;
        Debug.Log(result);
    }
}
