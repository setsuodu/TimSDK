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

    #region ��ʼ��

    public void Init(int sdkAppId)
    {
        jo.Call("init", sdkAppId);
    }

    #endregion

    #region ��¼

    public void Login(string identifier, string userSig)
    {
        jo.Call("login", identifier, userSig);
    }

    public void Logout()
    {
        jo.Call("logout"); //��ǰ�û��ǳ�
    }
    
    public string GetLoginUser()
    {
        var userId = jo.Call<string>("getLoginUser");
        return userId;
    }

    #endregion

    #region ��Ϣ�շ�

    // �ı���Ϣ����
    public void SendTextElement(string content, string identifier, int type)
    {
        jo.Call("sendTextElement", content, identifier, type);
    }

    // ͼƬ��Ϣ����
    public void SendImageElement(string fullPath, string identifier, int type)
    {
        jo.Call("sendImageElement", fullPath, identifier, type);
    }

    // ������Ϣ����
    public void SendSoundElement(string json, string identifier, int type)
    {
        jo.Call("sendSoundElement", json, identifier, type);
    }

    // ����λ����Ϣ����
    public void SendLocationElement(string json, string identifier, int type)
    {
        jo.Call("sendLocationElement", json, identifier, type);
    }

    // ��ȡ���лỰ
    public void GetConversationList()
    {
        jo.Call("getConversationList");
    }

    // ��ȡ�Ự������Ϣ
    public void GetLocalMessageFirst(string peer, int count)
    {
        jo.Call("getLocalMessageFirst", peer, count);
    }
    public void GetLocalMessageNext(string peer, int count)
    {
        jo.Call("getLocalMessageNext", peer, count);
    }

    // ��ȡ�Ự������Ϣ
    public void GetMessageFirst(string peer, int count)
    {
        jo.Call("getMessageFirst", peer, count);
    }
    public void GetMessageNext(string peer, int count)
    {
        jo.Call("getMessageNext", peer, count);
    }

    // ɾ���Ự
    public bool DeleteConversation(string peer, bool deleteLocal)
    {
        return jo.Call<bool>("deleteConversation", peer, deleteLocal);
    }

    // ͬ����ȡ�Ự������Ϣ
    public void GetLastMsg(string peer)
    {
        jo.Call("getLastMsg", peer);
    }

    // ɾ���Ự������Ϣ
    public void DeleteLocalMessage(string peer)
    {
        jo.Call("deleteLocalMessage", peer);
    }

    // ɾ����Ϣ
    public bool RemoveMessage(string json)
    {
        return jo.Call<bool>("removeMessage", json);
    }

    // ������Ϣ
    public void RevokeMessage(string peer, string json)
    {
        jo.Call("revokeMessage", peer, json);
    }

    #endregion

    #region δ������

    // ��ȡ��ǰδ����Ϣ����
    public long GetUnreadMessageNum(string identifier, int type)
    {
        long num = jo.Call<long>("getUnreadMessageNum", identifier, type);
        //Debug.Log($"����{identifier}��δ����Ϣ������{num}");
        return num;
    }

    // �Ѷ��ϱ������ģ�
    public void SetReadMessage(string peer)
    {
#if UNITY_EDITOR
        return;
#endif
        jo.Call("setReadMessage", peer);
    }

    #endregion

    #region Ⱥ�����

    #endregion

    #region �û��������ϵ��

    // ************ �û����� ************ //

    // ��ȡ�Լ�������
    public void GetSelfProfile(int type)
    {
        jo.Call("getSelfProfile", type); //0=�ƶˣ�1=����
    }
    // ��ȡָ���û������ϣ��б�
    public void GetUsersProfile(int type, string users)
    {
        jo.Call("getUsersProfile", type, users); //0=�ƶˣ�1=����
    }

    // �޸��Լ������ϣ�Ĭ���ֶΣ�
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
    // �޸��Լ������ϣ��Զ����ֶΣ�
    public void ModifySelfProfileCustom()
    {
        jo.Call("modifySelfProfileCustom");
    }

    // ************ ���ѹ�ϵ ************ //

    // ��ȡ���к���
    public void GetFriendList()
    {
        jo.Call("getFriendList");
    }

    // �޸ĺ���
    public void ModifyFriend() { }

    // ��Ӻ���
    public void AddFriend(string identifier, string hello)
    {
        jo.Call("addFriend", identifier, hello); 
    }

    // ɾ������
    public void DeleteFriends(int type, string usersjson)
    {
        jo.Call("deleteFriends", type, usersjson);
    }

    // ͬ��/�ܾ���������
    public void DoFriendResponse(string identifier, int type)
    {
        jo.Call("doFriendResponse", identifier, type);
    }

    // У����ѹ�ϵ
    public void CheckFriends(int type, string usersjson)
    {
        jo.Call("checkFriends", type, usersjson);
    }

    // ************ ����δ�� ************ //

    // ��ȡδ���б�
    public void GetPendencyList()
    {
        jo.Call("getPendencyList");
    }

    // ************ ������ ************ //

    // ************ ���ѷ��� ************ //

    // ************ ��ϵ�����ϵͳ֪ͨ ************ //

    // ************ �û����ϱ��ϵͳ֪ͨ ************ //

    #endregion

    #region ��������

    #endregion

    //tmp

    #region ��᷽��

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

    #region ��λ����

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
