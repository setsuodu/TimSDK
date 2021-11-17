using System.Runtime.InteropServices;

public class iOSHook
{
    [DllImport("__Internal")]
    public static extern void GetInstance(string obj);

    // 初始化
    [DllImport("__Internal")]
    public static extern void Init(int sdkAppId);

    // 登录
    [DllImport("__Internal")]
    public static extern void Login(string identifier, string userSig);
    [DllImport("__Internal")]
    public static extern void Logout();
    [DllImport("__Internal")]
    public static extern string GetLoginUser();

    // 消息收发
    [DllImport("__Internal")]
    public static extern void SendTextElement(string content, string identifier, int type);
    [DllImport("__Internal")]
    public static extern void SendImageElement(string path, string identifier, int type);
    [DllImport("__Internal")]
    public static extern void SendSoundElement(string json, string identifier, int type);
    [DllImport("__Internal")]
    public static extern void SendLocationElement(string json, string identifier, int type);
    [DllImport("__Internal")]
    public static extern void GetConversationList();
    [DllImport("__Internal")]
    public static extern void GetLocalMessageFirst(string peer, int count);
    [DllImport("__Internal")]
    public static extern void GetLocalMessageNext(string peer, int count);
    [DllImport("__Internal")]
    public static extern void GetMessageFirst(string peer, int count);
    [DllImport("__Internal")]
    public static extern void GetMessageNext(string peer, int count);
    [DllImport("__Internal")]
    public static extern void GetLastMsg(string peer);
    [DllImport("__Internal")]
    public static extern bool DeleteConversation(string peer, bool deleteLocal);
    [DllImport("__Internal")]
    public static extern bool RemoveMessage(string json);
    [DllImport("__Internal")]
    public static extern void RevokeMessage(string peer, string json);
    [DllImport("__Internal")]
    public static extern void debugMessages();
    [DllImport("__Internal")]
    public static extern void printMessage(string json);

    // 未读计数
    [DllImport("__Internal")]
    public static extern int GetUnreadMessageNum(string peer, int type); //iOS是int，android是long
    [DllImport("__Internal")]
    public static extern void SetReadMessage(string peer);

    // 群组相关

    // 用户资料与关系链
    [DllImport("__Internal")]
    public static extern void GetSelfProfile(int type);
    [DllImport("__Internal")]
    public static extern void GetUsersProfile(int type, string usersjson);
    [DllImport("__Internal")]
    public static extern void GetFriendList();
    [DllImport("__Internal")]
    public static extern void AddFriend(string identifier, string word);
    [DllImport("__Internal")]
    public static extern void DeleteFriends(int type, string usersjson);
    [DllImport("__Internal")]
    public static extern void DoFriendResponse();
    [DllImport("__Internal")]
    public static extern void CheckFriends(int type, string usersjson);
    [DllImport("__Internal")]
    public static extern void GetPendencyList();

    // 离线推送

    //tmp

    // 相册方法
    [DllImport("__Internal")]
    public static extern void OpenGallery();
    [DllImport("__Internal")]
    public static extern void OpenCamera();
    [DllImport("__Internal")]
    public static extern void OpenVideo();

    // 定位方法
    [DllImport("__Internal")]
    public static extern bool CheckGPSIsOpen();
    [DllImport("__Internal")]
    public static extern void OpenGPSSetting();
}
