using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using LitJson;
using MaterialUI;

public class TimSdkManager : MonoBehaviour
{
    public static TimSdkManager Instance;

    [SerializeField] private int sdkAppId = 1400286941; //1400326624;

    private ITimSdk timSdk;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        //LitJson默认将long解析成int
        JsonMapper.RegisterImporter<int, long>((int value) =>
        {
            return (long)value;
        });

#if UNITY_EDITOR
        timSdk = null;
#elif UNITY_ANDROID
        timSdk = new AndroidTimSdk(gameObject);
        //Screen.fullScreen = true;
        //ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible; //ugui会被顶上去
        //ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;
#elif UNITY_IOS
        timSdk = new iOSTimSdk(gameObject);
#endif
    }

    void Start()
    {
        Init();
        Utils.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        Utils.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);

        //TODO: 测试图片tmp
        {
            Texture2D t2d = Resources.Load<Texture2D>($"Sprites/Splash");
            string filePath = Application.persistentDataPath + "/Splash.png";
            Utils.SaveTextureToFile(t2d, filePath);
        }
        //TODO: 测试音频
        {
            var clip = Resources.Load<AudioClip>($"Sounds/Clip192");
            string filePath = Application.persistentDataPath + "/Clip192.mp3";
            byte[] bytes = WavUtility.FromAudioClip(clip, filePath, true);
        }
    }

    #region 回调消息处理

    // 字符串消息返回
    public void NativeCallback(string log)
    {
        Debug.Log($"[NativeCallback] {log}");
    }
    // json消息返回
    public void JsonCallback(string json)
    {
        var obj = JsonMapper.ToObject<TimCallback>(json);
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.OnForceOffline: //顶号
                {
                    PanelManager.Instance.CloseAll();
                    PanelManager.Instance.CreatePanel<UI_Login>();
                    ToastManager.Show("账号在别处登录", 0.5f, MaterialUIManager.UIRoot);
                }
                break;
            case TimSdkMessage.Logout: //登出
                if (obj.code == 0)
                {
                    Debug.Log($"<color=green>登出.成功：{obj.data}</color>");

                    PanelManager.Instance.CloseAll();
                    PanelManager.Instance.CreatePanel<UI_Login>();
                }
                else
                {
                    Debug.LogError($"登出.失败：code={obj.code}, data={obj.data}");
                }
                break;
            default:
                Debug.Log($"[Notify] msg=[{obj.msg}]{(TimSdkMessage)obj.msg}, code={obj.code}, data={obj.data}");
                TimEventManager.Notify((TimSdkMessage)obj.msg, obj); //对UIWidget推送消息
                break;
        }
    }

    #endregion


    #region 初始化

    public void Init()
    {
        Debug.Log($"init: {sdkAppId}");
#if UNITY_EDITOR
        return;
#endif
        timSdk.Init(sdkAppId);
    }

    public bool Inited() 
    {
        return true;
    }

    #endregion

    #region 登录

#if UNITY_EDITOR
    private TimUser[] testUsers;
    private void LoadTestUsers()
    {
        var json = Resources.Load<TextAsset>("TestData/testUsers").text;
        testUsers = JsonMapper.ToObject<TimUser[]>(json);
    }
#endif

    public void Login(string identifier, string userSig)
    {
        //Debug.Log($"identifier={identifier} userSig={userSig}");

#if UNITY_EDITOR
        var obj = new TimCallback(TimSdkMessage.Login, 0, "login succ"); //成功
        //var obj = new TimCallback(TimSdkMessage.Login, 6017, "userID or userSig is empty"); //失败
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif

        //通过http登录请求，获取UserSig
        timSdk.Login(identifier, userSig);
    }

    public void Logout()
    {
#if UNITY_EDITOR
        var obj = new TimCallback(TimSdkMessage.Logout, 0, "logout succ");
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.Logout(); //当前用户登出
    }

    public string GetLoginUser()
    {
        string userId = timSdk.GetLoginUser();
        Debug.Log($"当前用户是：{userId}");
        return userId;
    }

    #endregion

    #region 消息收发

    /// <summary>
    /// 文本消息发送
    /// </summary>
    /// <param name="content">文本内容</param>
    /// <param name="identifier">对方账号</param>
    /// <param name="type">0=无效的 | 1=单聊 | 2=群聊 | 3=系统</param>
    public void SendTextElement(string content, string identifier, int type)
    {
        timSdk.SendTextElement(content, identifier, type);
    }

    /// <summary>
    /// 图片消息发送
    /// </summary>
    /// <param name="fullPath">本地图片路径</param>
    /// <param name="identifier">对方账号</param>
    /// <param name="type">0: 原图发送 | 1: 高压缩率图发送(图片较小，默认值) | 2:高清图发送(图片较大)</param>
    public void SendImageElement(string fullPath, string identifier, int type)
    {
        timSdk.SendImageElement(fullPath, identifier, type);
    }

    // 语音消息发送
    public void SendSoundElement(string json, string identifier, int type)
    {
        timSdk.SendSoundElement(json, identifier, type);
    }

    // 获取所有会话
    public void GetConversationList()
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetConversationList();
    }

    // 获取会话本地消息
    public void GetLocalMessageFirst(string peer, int count)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetLocalMessageFirst(peer, count);
    }

    // 获取会话本地消息（下一页）
    public void GetLocalMessageNext(string peer, int count)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetLocalMessageNext(peer, count);
    }

    // 获取会话漫游消息（首页）
    public void GetMessageFirst(string peer, int count)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetMessageFirst(peer, count);
    }

    // 获取会话漫游消息（下一页）
    public void GetMessageNext(string peer, int count)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetMessageNext(peer, count);
    }

    // 删除会话
    public bool DeleteConversation(string peer, bool deleteLocal)
    {
#if UNITY_EDITOR
        return false;
#endif
        return timSdk.DeleteConversation(peer, deleteLocal);
    }

    // 获取会话中获取最后一条消息
    public void GetLastMsg(string peer)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetLastMsg(peer);
    }

    // 删除消息
    public bool RemoveMessage(string json)
    {
#if UNITY_EDITOR
        Debug.Log("删除消息");
        return true;
#endif
        return timSdk.RemoveMessage(json);
    }

    // 撤回消息
    public void RevokeMessage(string peer, string json)
    {
#if UNITY_EDITOR
        Debug.Log("撤回消息");
        return;
#endif
        timSdk.RevokeMessage(peer, json);
    }

    #endregion

    #region 未读计数

    // 获取当前未读消息数量
    public long GetUnreadMessageNum(string identifier, int type)
    {
        return timSdk.GetUnreadMessageNum(identifier, type);
    }

    // 已读上报（单聊）
    public void SetReadMessage(string identifier)
    {
        timSdk.SetReadMessage(identifier);
    }

    #endregion

    #region 群组相关

    #endregion

    #region 用户资料与关系链

    // ************ 用户资料 ************ //

    /// <summary>
    /// 获取自己的资料
    /// </summary>
    /// <param name="type">0=云端，1=本地</param>
    public void GetSelfProfile(int type)
    {
        timSdk.GetSelfProfile(type);
    }
    // 获取指定用户的资料（列表）
    public void GetUsersProfile(int type, string usersjson)
    {
#if UNITY_EDITOR
        var user = new TIMUserProfile()
        {
            identifier = "test01",
            nickName = $"test01_undefined",
            gender = Random.Range(0, 2),
            birthday = long.Parse($"{Random.Range(1970, 2000)}{Random.Range(1, 13).ToString().PadLeft(2, '0')}{Random.Range(1, 28).ToString().PadLeft(2, '0')}"),
        };

        var array = testUsers.Where(x => x.Identifier == "test01").ToArray();
        if(array == null || array.Length <= 0)
        {
            Debug.LogError("用户不存在");
            ToastManager.Show("用户不存在", 0.5f, MaterialUIManager.UIRoot);
            return;
        }

        string userData = JsonMapper.ToJson(user);
        var obj = new TimCallback(TimSdkMessage.GetUsersProfile, 0, userData);
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif

        timSdk.GetUsersProfile(type, usersjson); //0=云端，1=本地
    }

    // 修改自己的资料（默认字段）
    public void ModifySelfNick(string value)
    {
#if UNITY_EDITOR
        var obj = new TimCallback(TimSdkMessage.ModifySelfProfile, 0, "modifySelfProfile success");
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.ModifySelfNick(value);
    }
    public void ModifySelfGender(int value)
    {
        timSdk.ModifySelfGender(value);
    }
    public void ModifySelfBirthday(long value)
    {
        timSdk.ModifySelfBirthday(value);
    }
    public void ModifySelfAllowType(int value)
    {
        timSdk.ModifySelfAllowType(value);
    }
    public void ModifySelfProfile()
    {
#if UNITY_EDITOR
        var obj = new TimCallback(TimSdkMessage.ModifySelfProfile, 0, "modifySelfProfile success");
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.ModifySelfProfile();
    }
    // 修改自己的资料（自定义字段）
    public void ModifySelfProfileCustom()
    {
        timSdk.ModifySelfProfileCustom();
    }

    // ************ 好友关系 ************ //

    // 获取所有好友
    public void GetFriendList()
    {
#if UNITY_EDITOR
        List<TIMUserProfile> friendList = new List<TIMUserProfile>();
        friendList.Add(new TIMUserProfile() { identifier = "test01", nickName = "test01_undefined" });
        friendList.Add(new TIMUserProfile() { identifier = "test04", nickName = "test04_undefined" });
        var str = JsonMapper.ToJson(friendList);
        var obj = new TimCallback(TimSdkMessage.GetFriendList, 0, str);
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.GetFriendList();
    }

    // 修改好友
    public void ModifyFriend() { }

    // 添加好友
    public void AddFriend(string identifier, string word)
    {
#if UNITY_EDITOR
        Debug.Log("已申请，请等待");
        return;
#endif
        timSdk.AddFriend(identifier, word);
    }

    // 删除好友
    public void DeleteFriends(int type, string usersjson)
    {
        timSdk.DeleteFriends(type, usersjson);
    }

    // 同意/拒绝好友申请
    public void DoFriendResponse(string identifier, int type)
    {
        timSdk.DoFriendResponse(identifier, type);
    }

    // 校验好友关系 //1=单向好友，2=互为好友
    public void CheckFriends(int type, string usersjson)
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.CheckFriends(type, usersjson);
    }

    // ************ 好友未决 ************ //

    // 获取未决列表
    public void GetPendencyList()
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.GetPendencyList();
    }

    // ************ 黑名单 ************ //

    // ************ 好友分组 ************ //

    // ************ 关系链变更系统通知 ************ //

    // ************ 用户资料变更系统通知 ************ //

    #endregion

    #region 离线推送

    #endregion

    //

    #region 相册方法

    public void OpenGallery()
    {
#if UNITY_EDITOR
        string srcPath = "C:/Users/Administrator/Pictures";
        string dstPath = EditorUtility.OpenFilePanel("Select png", srcPath, "*.png;*.jpg;*.mp3;*.mp4");
        string ext = System.IO.Path.GetExtension(dstPath);
        TimCallback obj = null;
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
                obj = new TimCallback(TimSdkMessage.Image, 0, dstPath);
                break;
            case ".mp3":
            case ".wav":
                obj = new TimCallback(TimSdkMessage.Sound, 0, dstPath);
                break;
            case ".mp4":
                obj = new TimCallback(TimSdkMessage.Video, 0, dstPath);
                break;
        }
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.OpenGallery();
    }

    public void OpenCamera()
    {
        timSdk.OpenCamera();
    }

    public void OpenVideo()
    {
#if UNITY_EDITOR
        string srcPath = "C:/Users/Administrator/Pictures";
        string dstPath = EditorUtility.OpenFilePanel("Select png", srcPath, "*.png;*.jpg;*.mp3;*.mp4");
        string ext = System.IO.Path.GetExtension(dstPath);
        TimCallback obj = null;
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
                obj = new TimCallback(TimSdkMessage.Image, 0, dstPath);
                break;
            case ".mp3":
            case ".wav":
                obj = new TimCallback(TimSdkMessage.Sound, 0, dstPath);
                break;
            case ".mp4":
                obj = new TimCallback(TimSdkMessage.Video, 0, dstPath);
                break;
        }
        var json = JsonMapper.ToJson(obj);
        JsonCallback(json);
        return;
#endif
        timSdk.OpenVideo();
    }

    #endregion

    #region 定位方法

    public bool CheckGPSIsOpen()
    {
#if UNITY_EDITOR
        return true;
#endif
        return timSdk.CheckGPSIsOpen();
    }

    public void OpenGPSSetting()
    {
#if UNITY_EDITOR
        return;
#endif
        timSdk.OpenGPSSetting();
    }

    #endregion
}
