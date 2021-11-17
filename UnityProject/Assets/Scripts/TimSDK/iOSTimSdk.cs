using System;
using UnityEngine;

public class iOSTimSdk : ITimSdk, IDisposable
{
    public iOSTimSdk(GameObject go)
    {
        iOSHook.GetInstance(go.name);
    }

    public void Dispose()
    {

    }

    #region 初始化

    public void Init(int sdkAppId)
    {
        iOSHook.Init(sdkAppId);
    }

    #endregion

    #region 登录

    public void Login(string identifier, string userSig)
    {
        iOSHook.Login(identifier, userSig);
    }

    public void Logout()
    {
        iOSHook.Logout();
    }

    public string GetLoginUser()
    {
        return iOSHook.GetLoginUser();
    }

    #endregion

    #region 消息收发

    // 文本消息发送
    public void SendTextElement(string content, string identifier, int type)
    {
        iOSHook.SendTextElement(content, identifier, type);
    }
    // 图片消息发送
    public void SendImageElement(string fullPath, string identifier, int type)
    {
        iOSHook.SendImageElement(fullPath, identifier, type);
    }
    // 语音消息发送
    public void SendSoundElement(string json, string identifier, int type)
    {
        iOSHook.SendSoundElement(json, identifier, type);
    }
    // 地理位置消息发送
    public void SendLocationElement(string json, string identifier, int type)
    {
        iOSHook.SendLocationElement(json, identifier, type);
    }

    public void GetConversationList()
    {
        iOSHook.GetConversationList();
    }

    public void GetLocalMessageFirst(string peer, int count)
    {
        iOSHook.GetLocalMessageFirst(peer, count);
    }
    public void GetLocalMessageNext(string peer, int count)
    {
        iOSHook.GetLocalMessageNext(peer, count);
    }

    public void GetMessageFirst(string peer, int count)
    {
        iOSHook.GetMessageFirst(peer, count);
    }
    public void GetMessageNext(string peer, int count)
    {
        iOSHook.GetMessageNext(peer, count);
    }

    // 删除会话本地消息
    public bool DeleteConversation(string peer, bool deleteLocal)
    {
        return iOSHook.DeleteConversation(peer, deleteLocal);
    }

    // 同步获取会话最后的消息
    public void GetLastMsg(string peer)
    {
        iOSHook.GetLastMsg(peer);
    }

    // 删除会话本地消息
    public void DeleteLocalMessage(string peer)
    {
        iOSHook.RemoveMessage(peer);
    }

    // 撤回消息
    public void RevokeMessage(string peer, string json)
    {
        iOSHook.RevokeMessage(peer, json);
    }

    // 删除消息
    public bool RemoveMessage(string json)
    {
        return iOSHook.RemoveMessage(json);
    }

    #endregion

    #region 未读计数

    // 获取当前未读消息数量
    public long GetUnreadMessageNum(string identifier, int type)
    {
        return iOSHook.GetUnreadMessageNum(identifier, type);
    }

    // 已读上报
    public void SetReadMessage(string peer)
    {
        iOSHook.SetReadMessage(peer);
    }

    #endregion

    #region 群组相关
    #endregion

    #region 用户资料与关系链

    public void GetSelfProfile(int type)
    {
        iOSHook.GetSelfProfile(type);
    }

    public void GetUsersProfile(int type, string usersjson)
    {
        iOSHook.GetUsersProfile(type, usersjson);
    }

    public void AddFriend(string identifier, string word)
    {
        iOSHook.AddFriend(identifier, word);
    }

    public void DeleteFriends(int type, string usersjson)
    {
        iOSHook.DeleteFriends(type, usersjson);
    }

    public void CheckFriends(int type, string usersjson)
    {
        iOSHook.CheckFriends(type, usersjson);
    }

    public void DoFriendResponse(string identifier, int type)
    {
        iOSHook.DoFriendResponse();
    }

    public void GetFriendList()
    {
        iOSHook.GetFriendList();
    }

    public void GetPendencyList()
    {
        iOSHook.GetPendencyList();
    }

    public void ModifyFriend()
    {
        //iOSHook.ModifyFriend();
    }

    public void ModifySelfAllowType(int value)
    {
        //iOSHook.ModifySelfAllowType(value);
    }

    public void ModifySelfBirthday(long value)
    {
        //iOSHook.ModifySelfBirthday(value);
    }

    public void ModifySelfGender(int value)
    {
        //iOSHook.ModifySelfGender(value);
    }

    public void ModifySelfNick(string value)
    {
        //iOSHook.ModifySelfNick(value);
    }

    public void ModifySelfProfile()
    {
        //iOSHook.ModifySelfProfile();
    }

    public void ModifySelfProfileCustom()
    {
        //iOSHook.ModifySelfProfileCustom();
    }

    #endregion

    #region 离线推送
    #endregion

    //tmp

    #region 相册方法

    public void OpenGallery()
    {
        iOSHook.OpenGallery();
    }

    public void OpenCamera()
    {
        iOSHook.OpenCamera();
    }

    public void OpenVideo()
    {
        iOSHook.OpenVideo();
    }

    #endregion

    #region 定位方法

    public bool CheckGPSIsOpen()
    {
        return iOSHook.CheckGPSIsOpen();
    }

    public void OpenGPSSetting()
    {
        iOSHook.OpenGPSSetting();
    }

    #endregion
}
