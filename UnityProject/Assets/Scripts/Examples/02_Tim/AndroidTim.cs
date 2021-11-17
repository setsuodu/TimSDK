using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using Client;

public class AndroidTim : MonoBehaviour
{
    private const string className = "com.zdkj.plugin.TimFragment";
    private AndroidJavaClass jc = null;
    private AndroidJavaObject jo = null;

    [SerializeField] private InputField m_UserIdInput;
    [SerializeField] private InputField m_OtherUserIdInput;
    [SerializeField] private InputField m_SendTextInput;
    [SerializeField] private InputField m_NickNameInput;
    [SerializeField] private AudioSource m_AudioSource;
    [Header("TimSdk")]
    [SerializeField] private int sdkAppId = 1400326624;
    [Header("消息收发")]
    [SerializeField] private Button m_DeleteConversationBtn;
    [SerializeField] private Button m_RemoveMessageBtn;
    [SerializeField] private Button m_RevokeMessageBtn;
    [SerializeField] private RawImage m_ShowImage;
    [Header("用户资料与关系链")]
    [SerializeField] private Dropdown m_GenderDrop;
    [SerializeField] private Dropdown mAllowTypeDrop;
    [SerializeField] private Button m_GetSelfProfileBtn;
    [SerializeField] private Button m_GetUsersProfileBtn;
    [SerializeField] private Button m_ModifySelfNickNameBtn;
    [SerializeField] private Button m_ModifySelfGenderBtn;
    [SerializeField] private Button m_ModifySelfAllowTypeBtn;
    [SerializeField] private Button m_GetFriendListBtn;
    [SerializeField] private Button m_AddFriendBtn;
    [SerializeField] private Button m_DeleteFriendsBtn;
    [SerializeField] private Button m_CheckriendsBtn;
    [SerializeField] private Button m_GetPendencyListBtn;

    void Awake()
    {
        Screen.fullScreen = false;
        LoadTestUsers();
        JsonMapper.RegisterImporter<int, long>((int value) =>
        {
            return (long)value;
        });

#if UNITY_ANDROID
        jc = new AndroidJavaClass(className);
        //jo = jc.CallStatic<AndroidJavaObject>("GetInstance", gameObject.name);
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.CallStatic("GetInstance", gameObject.name);
#elif UNITY_IOS
        iOSHook.GetInstance(gameObject.name);
#endif

        // ************ TimSdk ************ //
        m_DeleteConversationBtn.onClick.AddListener(DeleteConversation);
        m_RemoveMessageBtn.onClick.AddListener(RemoveMessage);
        m_RevokeMessageBtn.onClick.AddListener(RevokeMessage);

        m_GetSelfProfileBtn.onClick.AddListener(GetSelfProfile);
        m_GetUsersProfileBtn.onClick.AddListener(GetUsersProfile);
        m_ModifySelfNickNameBtn.onClick.AddListener(ModifySelfNick);
        m_ModifySelfGenderBtn.onClick.AddListener(ModifySelfGender);
        m_ModifySelfAllowTypeBtn.onClick.AddListener(ModifySelfAllowType);
        m_GetFriendListBtn.onClick.AddListener(GetFriendList);
        m_AddFriendBtn.onClick.AddListener(AddFriend);
        m_DeleteFriendsBtn.onClick.AddListener(DeleteFriends);
        m_CheckriendsBtn.onClick.AddListener(CheckFriends);
        m_GetPendencyListBtn.onClick.AddListener(GetPendencyList);
    }

    void Start()
    {
        Utils.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        Utils.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);

        //TODO: 测试图片
        {
            Texture2D t2d = Resources.Load<Texture2D>($"Sprites/Splash");
            string filePath = Application.persistentDataPath + "/Splash.png";
            Utils.SaveTextureToFile(t2d, filePath);
        }

        //TODO: 测试音频
        {
            var clip = Resources.Load<AudioClip>($"Sounds/Clip192");
            //Debug.Log($"samples={clip.samples} channels={clip.channels} frequency={clip.frequency} length={clip.length}");
            string filePath = Application.persistentDataPath + "/Clip192.mp3";
            byte[] bytes = WavUtility.FromAudioClip(clip, filePath, true);
        }
    }

    #region 本地

    public void SystemPrint()
    {
        jo.Call("SystemPrint");
    }

    public void HelloWorld()
    {
        jo.Call("HelloWorld");
    }

    //检查安卓手机厂商
    public void CheckOEM()
    {
#if UNITY_ANDROID
        string classname = "com.zdkj.plugin.NativeFragment";
        var nativeClass = new AndroidJavaClass(classname);
        var nativeObject = nativeClass.CallStatic<AndroidJavaObject>("GetInstance", gameObject.name);
        //nativeObject.Call("HelloWorld");
        var oem = nativeObject.Call<string>("CheckOEM");
        Debug.Log(oem);
#elif UNITY_IOS
#endif
    }

    public void Test1()
    {
#if UNITY_ANDROID
        var activityClass = new AndroidJavaClass("com.zdkj.plugin.NativeActivity");
        var activityObject = activityClass.GetStatic<AndroidJavaObject>("mActivity");
        activityObject.Call("Test1");
#elif UNITY_IOS
#endif
    }

    public void OpenGallery()
    {
#if UNITY_ANDROID
        jo.Call("openGallery");
#elif UNITY_IOS
#endif
    }

    public void OpenCamera()
    {
#if UNITY_ANDROID
        jo.Call("openCamera");
#elif UNITY_IOS
#endif
    }

    public void OpenVideo()
    {
#if UNITY_ANDROID
        jo.Call("openVideo");
#elif UNITY_IOS
#endif
    }

    #endregion

    //

    #region 初始化

    public void Init()
    {
#if UNITY_ANDROID
        jo.Call("init", sdkAppId);
#elif UNITY_IOS
        iOSHook.Init(sdkAppId);
#endif
    }

    #endregion

    #region 登录

    private TimUser[] testUsers;
    private void LoadTestUsers()
    {
        var json = Resources.Load<TextAsset>("testUsers").text;
        testUsers = JsonMapper.ToObject<TimUser[]>(json);
    }

    public void Login()
    {
        string userid = m_UserIdInput.text;
        string userSig = string.Empty;
        var array = testUsers.Where(x => x.Identifier == userid).ToArray();
        if (array != null && array.Length > 0)
        {
            userSig = array.FirstOrDefault().UserSig;
        }

#if UNITY_ANDROID
        jo.Call("login", userid, userSig);
#elif UNITY_IOS
        iOSHook.Login(userid, userSig);
#endif
    }

    public void Logout()
    {
#if UNITY_ANDROID
        jo.Call("logout"); //当前用户登出
#elif UNITY_IOS
        iOSHook.Logout();
#endif
    }

    public void GetLoginUser()
    {
#if UNITY_ANDROID
        var userId = jo.Call<string>("getLoginUser");
        Debug.Log($"当前用户是：{userId}");
#elif UNITY_IOS
        var userId = iOSHook.GetLoginUser();
        Debug.Log($"当前用户是：{userId}");
#endif
    }

    #endregion

    #region 消息收发

    // 文本消息发送
    public void SendTextElement()
    {
        string userid = m_OtherUserIdInput.text;
        string content = m_SendTextInput.text;

#if UNITY_ANDROID
        jo.Call("sendTextElement", content, userid, 1); //0=无效的，1=单聊，2=群聊，3=系统
#elif UNITY_IOS
        iOSHook.SendTextElement(content, userid, 1);
#endif
        m_SendTextInput.text = string.Empty;
    }

    // 图片消息发送
    public void SendImageElement()
    {
        string userid = m_OtherUserIdInput.text;
        string path = Path.Combine(Application.persistentDataPath, "Splash.png");
        Debug.Log($"path={path} exist={File.Exists(path)}");

#if UNITY_ANDROID
        jo.Call("sendImageElement", path, userid, (int)TIMConversationType.C2C);
#elif UNITY_IOS
        iOSHook.SendImageElement(path, userid, (int)TIMConversationType.C2C);
#endif
    }

    // 语音消息发送
    public void SendSoundElement()
    {
        string userid = m_OtherUserIdInput.text;
        string path = Path.Combine(Application.persistentDataPath, "Clip192.mp3");
        Debug.Log($"path={path} exist={File.Exists(path)}");

        TIMSoundElemResp cmd = new TIMSoundElemResp();
        cmd.path = path;
        cmd.duration = 3;
        string json = JsonMapper.ToJson(cmd);

#if UNITY_ANDROID
        jo.Call("sendSoundElement", json, userid, (int)TIMConversationType.C2C);
#elif UNITY_IOS
        iOSHook.SendSoundElement(json, userid, (int)TIMConversationType.C2C);
#endif
    }

    // 地理位置消息发送
    public void SendLocationElement()
    {
        TIMLocationElemResp cmd = new TIMLocationElemResp();
        cmd.desc = "腾讯大厦";
        cmd.latitude = 113.93;
        cmd.longitude = 22.54;
        string json = JsonMapper.ToJson(cmd);
        string userid = m_OtherUserIdInput.text;

#if UNITY_ANDROID
        jo.Call("sendLocationElement", json, userid, (int)TIMConversationType.C2C);
#elif UNITY_IOS
        iOSHook.SendLocationElement(json, userid, (int)TIMConversationType.C2C);
#endif
    }

    // 获取所有会话
    public void GetConversationList()
    {
#if UNITY_ANDROID
        jo.Call("getConversationList");
#elif UNITY_IOS
        iOSHook.GetConversationList();
#endif
    }

    // 获取会话中最后一条消息
    public void GetLastMsg()
    {
        string userid = m_OtherUserIdInput.text;

#if UNITY_ANDROID
        jo.Call("getLastMsg", userid);
#elif UNITY_IOS
        iOSHook.GetLastMsg(userid);
#endif
    }

    private List<TIMMessageResp> messages = new List<TIMMessageResp>();

    // 本地消息（首页）
    public void GetLocalMessageFirst()
    {
        string userid = m_OtherUserIdInput.text;
        messages = new List<TIMMessageResp>();

#if UNITY_ANDROID
        jo.Call("getLocalMessageFirst", userid, 5);
#elif UNITY_IOS
        iOSHook.GetLocalMessageFirst(userid, 5);
#endif
    }

    // 本地消息（下一页）
    public void GetLocalMessageNext()
    {
        string userid = m_OtherUserIdInput.text;

#if UNITY_ANDROID
        jo.Call("getLocalMessageNext", userid, 5);
#elif UNITY_IOS
        iOSHook.GetLocalMessageNext(userid, 5);
#endif
    }

    // 漫游消息（首页）
    public void GetMessageFirst()
    {
        string userid = m_OtherUserIdInput.text;
        messages = new List<TIMMessageResp>();

#if UNITY_ANDROID
        jo.Call("getMessageFirst", userid, 5);
#elif UNITY_IOS
        iOSHook.GetMessageFirst(userid, 5);
#endif
    }

    // 漫游消息（下一页）
    public void GetMessageNext()
    {
        string userid = m_OtherUserIdInput.text;

#if UNITY_ANDROID
        jo.Call("getMessageNext", userid, 5);
#elif UNITY_IOS
        iOSHook.GetMessageNext(userid, 5);
#endif
    }

    // 删除会话
    public void DeleteConversation()
    {
        string peer = m_OtherUserIdInput.text;
        bool deleteLocal = true;

        bool result = false;
#if UNITY_ANDROID
        result = jo.Call<bool>("deleteConversation", peer, deleteLocal);
#elif UNITY_IOS
        result = iOSHook.DeleteConversation(peer, deleteLocal);
#endif
        Debug.Log($"result={result}");
    }

    // 删除消息
    public void RemoveMessage()
    {
        int index = int.Parse(m_Index.text);
        var msg = messages?[index];
        TIMMessageLocatorResp resp = new TIMMessageLocatorResp();
        resp.seq = msg.seq;
        resp.rand = msg.rand;
        resp.timestamp = msg.timestamp;
        resp.isSelf = msg.isSelf;
        string json = JsonMapper.ToJson(resp);

        bool result = false;
#if UNITY_ANDROID
        result = jo.Call<bool>("removeMessage", json);
#elif UNITY_IOS
        result = iOSHook.RemoveMessage(json);
#endif
        Debug.Log($"返回 result={result}");
        if (result)
            messages.Remove(msg);
    }

    // 撤回消息
    public void RevokeMessage()
    {
        int index = int.Parse(m_Index.text);
        var msg = messages?[index];
        TIMMessageLocatorResp resp = new TIMMessageLocatorResp();
        resp.seq = msg.seq;
        resp.rand = msg.rand;
        resp.timestamp = msg.timestamp;
        resp.isSelf = msg.isSelf;
        string json = JsonMapper.ToJson(resp);

#if UNITY_ANDROID
        jo.Call("revokeMessage", m_OtherUserIdInput.text, json);
#elif UNITY_IOS
        iOSHook.RevokeMessage(m_OtherUserIdInput.text, json);
#endif
    }

    public void ShowMessages()
    {
#if UNITY_ANDROID
        jo.Call("debugMessages");
#elif UNITY_IOS
        iOSHook.debugMessages();
#endif
    }

    // 最后一条消息作为起始，idnex=0
    public InputField m_Index;
    public void PrintMessage()
    {
        int index = int.Parse(m_Index.text);
        var msg = messages[index];

        TIMMessageLocatorResp resp = new TIMMessageLocatorResp();
        resp.seq = msg.seq;
        resp.rand = msg.rand;
        resp.timestamp = msg.timestamp;
        resp.isSelf = msg.isSelf;
        string json = JsonMapper.ToJson(resp);
#if UNITY_ANDROID
        jo.Call("printMessage", json);
#elif UNITY_IOS
        iOSHook.printMessage(json);
#endif
    }

    #endregion

    #region 未读计数

    // 获取当前未读消息数量
    public void GetUnreadMessageNum()
    {
        string userid = m_OtherUserIdInput.text;
        var user = testUsers.Where(x => x.Identifier == userid).ToList()[0];

#if UNITY_ANDROID
        var num = jo.Call<long>("getUnreadMessageNum", user.Identifier, 1);
        Debug.Log($"我与{user.Identifier}的未读消息数量：{num}");
#elif UNITY_IOS
        var num = iOSHook.GetUnreadMessageNum(userid, 1);
        Debug.Log($"我与{userid}的未读消息数量：{num}");
#endif
    }

    // 已读上报（单聊）
    public void SetReadMessage()
    {
        string userid = m_OtherUserIdInput.text;

#if UNITY_ANDROID
        jo.Call("setReadMessage", userid);
#elif UNITY_IOS
        iOSHook.SetReadMessage(userid);
#endif
    }

    #endregion

    #region 群组相关

    #endregion

    #region 用户资料与关系链

    // ************ 用户资料 ************ //

    // 获取自己的资料
    public void GetSelfProfile()
    {
#if UNITY_ANDROID
        jo.Call("getSelfProfile", 0); //0=云端，1=本地
#elif UNITY_IOS
        iOSHook.GetSelfProfile(0);
#endif
    }
    // 获取指定用户的资料（列表）
    public void GetUsersProfile()
    {
        List<string> users = new List<string>();
        users.Add("test01");
        users.Add("test02");
        string json = JsonMapper.ToJson(users);
        Debug.Log(json);

#if UNITY_ANDROID
        jo.Call("getUsersProfile", 0, users.ToArray()); //0=云端，1=本地 //TODO: 统一数据格式
#elif UNITY_IOS
        iOSHook.GetUsersProfile(0, json);
#endif
    }

    // 修改自己的资料（默认字段）
    public void ModifySelfProfile(string key, object value)
    {
#if UNITY_ANDROID
        jo.Call("modifySelfProfile", key, value);
#elif UNITY_IOS
        //iOSHook.ModifySelfProfile();
#endif
    }
    // 修改自己的昵称
    public void ModifySelfNick()
    {
        jo.Call("modifySelfNick", m_NickNameInput.text);
    }
    // 修改自己的性别
    public void ModifySelfGender()
    {
        jo.Call("modifySelfGender", m_GenderDrop.value + 1);
    }
    // 修改自己的隐私
    public void ModifySelfAllowType()
    {
        int allowType = 0;
        switch (mAllowTypeDrop.value)
        {
            case 0:
                allowType = (int)TIMFriendAllowType.TIM_FRIEND_ALLOW_ANY;
                break;
            case 1:
                allowType = (int)TIMFriendAllowType.TIM_FRIEND_NEED_CONFIRM;
                break;
            case 2:
                allowType = (int)TIMFriendAllowType.TIM_FRIEND_DENY_ANY;
                break;
        }
        jo.Call("modifySelfAllowType", allowType);
    }
    // 修改自己的资料（自定义字段）
    public void ModifySelfProfileCustom()
    {
#if UNITY_ANDROID
        jo.Call("modifySelfProfileCustom");
#elif UNITY_IOS
        //iOSHook.ModifySelfProfileCustom();
#endif
    }

    // ************ 好友关系 ************ //

    // 获取所有好友
    public void GetFriendList()
    {
#if UNITY_ANDROID
        jo.Call("getFriendList");
#elif UNITY_IOS
        iOSHook.GetFriendList();
#endif
    }

    // 修改好友
    public void ModifyFriend() { }

    // 添加好友
    public void AddFriend()
    {
        string userid = m_OtherUserIdInput.text;
#if UNITY_ANDROID
        jo.Call("addFriend", userid, "hello");
#elif UNITY_IOS
        iOSHook.AddFriend(userid, "hello");
#endif
    }

    // 删除好友 //1=单向，2=双向（会把自己从对方好友列表中删除）
    public void DeleteFriends()
    {
        List<string> users = new List<string>();
        users.Add(m_OtherUserIdInput.text);
        string json = JsonMapper.ToJson(users);
        //Debug.Log(json);

#if UNITY_ANDROID
        jo.Call("deleteFriends", 2, users.ToArray()); //TODO: 统一数据格式
#elif UNITY_IOS
        iOSHook.DeleteFriends(2, json);
#endif
    }

    // 同意/拒绝好友申请
    public void DoFriendResponse()
    {
#if UNITY_ANDROID
        jo.Call("doFriendResponse");
#elif UNITY_IOS
        iOSHook.DoFriendResponse();
#endif
    }

    // 校验好友关系 //1=单向好友，2=互为好友
    public void CheckFriends()
    {
        List<string> users = new List<string>();
        users.Add(m_OtherUserIdInput.text);
        string json = JsonMapper.ToJson(users);
        Debug.Log(json);

#if UNITY_ANDROID
        jo.Call("checkFriends", 2, users.ToArray()); //TODO: 统一数据格式
#elif UNITY_IOS
        iOSHook.CheckFriends(2, json);
#endif
    }

    // ************ 好友未决 ************ //

    // 获取未决列表
    public void GetPendencyList()
    {
#if UNITY_ANDROID
        jo.Call("getPendencyList");
#elif UNITY_IOS
        iOSHook.GetPendencyList();
#endif
    }

    // ************ 黑名单 ************ //

    // ************ 好友分组 ************ //

    // ************ 关系链变更系统通知 ************ //

    // ************ 用户资料变更系统通知 ************ //

    #endregion

    #region 离线推送

    #endregion

    //

    #region 工具类

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
            case TimSdkMessage.GetSelfProfile:
                {
                    var user = JsonMapper.ToObject<TIMUserProfile>(obj.data);
                    Debug.Log($"nickName={user.nickName}, gender={user.gender}, birthday={user.birthday}, allowType={user.allowType}");
                }
                break;
            case TimSdkMessage.RecvNewMessages: //收到新消息 OnNewMessages
            case TimSdkMessage.GetLocalMessage: //本地消息
            case TimSdkMessage.GetMessage: //漫游消息
            case TimSdkMessage.GetLastMsg: //最后一条消息
                {
                    var arr = JsonMapper.ToObject<TIMMessageResp[]>(obj.data);
                    Debug.Log($"收到新消息={arr.Length}条");
                    Debug.Log(obj.data);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        messages.Add(arr[i]);

                        TIMChatMsg msg = new TIMChatMsg();
                        msg.seq = arr[i].seq;
                        msg.rand = arr[i].rand;
                        msg.timestamp = arr[i].timestamp;
                        msg.isSelf = arr[i].isSelf;
                        msg.isRead = arr[i].isRead;
                        msg.isPeerReaded = arr[i].isPeerReaded;
                        msg.elemType = arr[i].elemType;
                        msg.subType = arr[i].subType;
                        msg.text = arr[i].text;
                        msg.param = arr[i].param;

                        switch ((TIMElemType)msg.elemType)
                        {
                            case TIMElemType.Text:
                                {
                                    Debug.Log($"[{i}]Text msg={msg.text} time={msg.timestamp}");
                                }
                                break;
                            case TIMElemType.Image:
                                {
                                    Debug.Log($"[{i}]Image sub={msg.subType} text={msg.text} param={msg.param}");
                                    StartCoroutine(LoadImage(msg.text));
                                }
                                break;
                            case TIMElemType.Sound:
                                {
                                    var data = JsonMapper.ToObject<TIMSoundElemResp>(msg.param);
                                    Debug.Log($"[{i}]Sound uuid={data.uuid} path={data.path} duration={data.duration}");
                                }
                                break;
                            case TIMElemType.Location:
                                {
                                    var data = JsonMapper.ToObject<TIMLocationElemResp>(msg.text);
                                    Debug.Log($"[{i}]Location desc={data.desc} lat={data.latitude} lon={data.longitude}");
                                }
                                break;
                            case TIMElemType.SNSTips:
                                {
                                    Debug.Log($"[{i}]SNS subType={msg.subType}");
                                    switch (msg.subType)
                                    {
                                        case TIMSNSSystemType.INVALID: //0 //无效的消息
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND: //1 //与某人成为了好友
                                            {
                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND: //2 //与某人解除了好友
                                            {

                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND_REQ: //3 //收到好友申请
                                            {
                                                var array = JsonMapper.ToObject<TIMFriendPendencyInfo[]>(obj.data);
                                                for (int t = 0; t < array.Length; t++)
                                                {
                                                    Debug.Log($"未决[{t}]: fromUser={array[t].fromUser}, addSource={array[t].addSource}, fromUserNickName={array[t].fromUserNickName}, addWording={array[t].addWording}");
                                                    TIMFriendPendencyItem item = new TIMFriendPendencyItem();
                                                    item.identifier = array[t].fromUser;
                                                    item.addSource = array[t].addSource;
                                                    item.nickname = array[t].fromUserNickName;
                                                    item.addWording = array[t].addWording;
                                                }
                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND_REQ: //4 //拒绝好友申请
                                            {
                                                Debug.Log($"拒绝了申请，与{obj.data}无法成为好友"); //TODO: 从列表中删除该元素
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                            default:
                                Debug.Log($"[{i}]其它类型消息");
                                break;
                        }
                    }
                }
                break;
            case TimSdkMessage.Download: //消息中的文件下载完成（音频/视频/文件）
                {
                    var data = JsonMapper.ToObject<TIMDownloadResp>(obj.data);
                    Debug.Log($"异步获取文件url type={data.elemType} uuid={data.uuid} url={data.url}");
                    HttpManager.Download(data.url, OnDownload, data.uuid);
                }
                break;
            default:
                Debug.Log($"[JsonCallback] msg=[{obj.msg}]{(TimSdkMessage)obj.msg}, code={obj.code}, data={obj.data}");
                break;
        }
    }

    IEnumerator LoadImage(string path) 
    {
        yield return new WaitUntil(() => File.Exists(path));

        WWW www = new WWW("file://" + path);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) 
        {
            Debug.LogError($"err={www.error}");
            yield break;
        }
        byte[] bytes = www.bytes;
        www.Dispose();
        //m_ShowImage.texture = www.texture;
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        m_ShowImage.texture = texture;
    }

    public void PlayAudio()
    {
        m_AudioSource.Play();
    }

    void OnDownload(byte[] bytes)
    {
        AudioClip clip = WavUtility.ToAudioClip(bytes);
        m_AudioSource.clip = clip;
    }

    #endregion
}
