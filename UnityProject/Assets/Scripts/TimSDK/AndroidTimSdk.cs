using System;
using UnityEngine;

public class AndroidTimSdk : ITimSdk, IDisposable
{
    private const string className = "com.zdkj.plugin.TimFragment";
    private AndroidJavaClass jc = null;
    private AndroidJavaObject jo = null;
    public AndroidTimSdk(GameObject go)
    {
        try
        {
            jc = new AndroidJavaClass(className);
            //jo = jc.CallStatic<AndroidJavaObject>("GetInstance", go.name);
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.CallStatic("GetInstance", go.name);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Dispose()
    {
        jc.Dispose();
        jc = null;
        jo.Dispose();
        jo = null;
    }

    #region 初始化

    public void Init(int sdkAppId)
    {
        jo.Call("init", sdkAppId);
    }

    #endregion

    #region 登录

    public void Login(string identifier, string userSig)
    {
        jo.Call("login", identifier, userSig);
    }

    public void Logout()
    {
        jo.Call("logout"); //当前用户登出
    }
    
    public string GetLoginUser()
    {
        var userId = jo.Call<string>("getLoginUser");
        return userId;
    }

    #endregion

    #region 消息收发

    // 文本消息发送
    public void SendTextElement(string content, string identifier, int type)
    {
        jo.Call("sendTextElement", content, identifier, type);
    }

    // 图片消息发送
    public void SendImageElement(string fullPath, string identifier, int type)
    {
        jo.Call("sendImageElement", fullPath, identifier, type);
    }

    // 语音消息发送
    public void SendSoundElement(string json, string identifier, int type)
    {
        jo.Call("sendSoundElement", json, identifier, type);
    }

    // 地理位置消息发送
    public void SendLocationElement(string json, string identifier, int type)
    {
        jo.Call("sendLocationElement", json, identifier, type);
    }

    // 获取所有会话
    public void GetConversationList()
    {
        jo.Call("getConversationList");
    }

    // 获取会话本地消息
    public void GetLocalMessageFirst(string peer, int count)
    {
        jo.Call("getLocalMessageFirst", peer, count);
    }
    public void GetLocalMessageNext(string peer, int count)
    {
        jo.Call("getLocalMessageNext", peer, count);
    }

    // 获取会话漫游消息
    public void GetMessageFirst(string peer, int count)
    {
        jo.Call("getMessageFirst", peer, count);
    }
    public void GetMessageNext(string peer, int count)
    {
        jo.Call("getMessageNext", peer, count);
    }

    // 删除会话
    public bool DeleteConversation(string peer, bool deleteLocal)
    {
        return jo.Call<bool>("deleteConversation", peer, deleteLocal);
    }

    // 同步获取会话最后的消息
    public void GetLastMsg(string peer)
    {
        jo.Call("getLastMsg", peer);
    }

    // 删除会话本地消息
    public void DeleteLocalMessage(string peer)
    {
        jo.Call("deleteLocalMessage", peer);
    }

    // 删除消息
    public bool RemoveMessage(string json)
    {
        return jo.Call<bool>("removeMessage", json);
    }

    // 撤回消息
    public void RevokeMessage(string peer, string json)
    {
        jo.Call("revokeMessage", peer, json);
    }

    #endregion

    #region 未读计数

    // 获取当前未读消息数量
    public long GetUnreadMessageNum(string identifier, int type)
    {
        long num = jo.Call<long>("getUnreadMessageNum", identifier, type);
        //Debug.Log($"我与{identifier}的未读消息数量：{num}");
        return num;
    }

    // 已读上报（单聊）
    public void SetReadMessage(string peer)
    {
#if UNITY_EDITOR
        return;
#endif
        jo.Call("setReadMessage", peer);
    }

    #endregion

    #region 群组相关

    #endregion

    #region 用户资料与关系链

    // ************ 用户资料 ************ //

    // 获取自己的资料
    public void GetSelfProfile(int type)
    {
        jo.Call("getSelfProfile", type); //0=云端，1=本地
    }
    // 获取指定用户的资料（列表）
    public void GetUsersProfile(int type, string users)
    {
        jo.Call("getUsersProfile", type, users); //0=云端，1=本地
    }

    // 修改自己的资料（默认字段）
    public void ModifySelfNick(string value)
    {
        jo.Call("modifySelfNick", value);
    }
    public void ModifySelfGender(int value)
    {
        jo.Call("modifySelfGender", value);
    }
    public void ModifySelfBirthday(long value)
    {
        jo.Call("modifySelfBirthday", value);
    }
    public void ModifySelfAllowType(int value)
    {
#if UNITY_EDITOR
        return;
#endif
        jo.Call("modifySelfAllowType", value);
    }
    public void ModifySelfProfile()
    {
        jo.Call("modifySelfProfile");
    }
    // 修改自己的资料（自定义字段）
    public void ModifySelfProfileCustom()
    {
        jo.Call("modifySelfProfileCustom");
    }

    // ************ 好友关系 ************ //

    // 获取所有好友
    public void GetFriendList()
    {
        jo.Call("getFriendList");
    }

    // 修改好友
    public void ModifyFriend() { }

    // 添加好友
    public void AddFriend(string identifier, string hello)
    {
        jo.Call("addFriend", identifier, hello); 
    }

    // 删除好友
    public void DeleteFriends(int type, string usersjson)
    {
        jo.Call("deleteFriends", type, usersjson);
    }

    // 同意/拒绝好友申请
    public void DoFriendResponse(string identifier, int type)
    {
        jo.Call("doFriendResponse", identifier, type);
    }

    // 校验好友关系
    public void CheckFriends(int type, string usersjson)
    {
        jo.Call("checkFriends", type, usersjson);
    }

    // ************ 好友未决 ************ //

    // 获取未决列表
    public void GetPendencyList()
    {
        jo.Call("getPendencyList");
    }

    // ************ 黑名单 ************ //

    // ************ 好友分组 ************ //

    // ************ 关系链变更系统通知 ************ //

    // ************ 用户资料变更系统通知 ************ //

    #endregion

    #region 离线推送

    #endregion

    //tmp

    #region 相册方法

    public void OpenGallery()
    {
        jo.Call("openGallery");
    }

    public void OpenCamera()
    {
        Utils.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        jo.Call("openCamera");
    }

    public void OpenVideo()
    {
        jo.Call("openVideo");
    }

    #endregion

    #region 定位方法

    public bool CheckGPSIsOpen()
    {
        return jo.Call<bool>("checkGPSIsOpen");
    }

    public void OpenGPSSetting()
    {
        jo.Call("openGPSSetting");
    }

    #endregion
}
