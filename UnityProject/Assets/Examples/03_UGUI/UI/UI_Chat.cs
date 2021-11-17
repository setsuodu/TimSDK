using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using DG.Tweening;
using Client;
using MaterialUI;

public class UI_Chat : UIWidget
{
    public Client.MessagingContentFiller mContentFiller;
    [SerializeField] private Image m_HeadImage;
    [SerializeField] private Text m_NickNameText;
    [SerializeField] private InputField m_TextInput;
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_RequestCPBtn;
    [SerializeField] private Button m_MoreBtn;
    [SerializeField] private Button m_SoundBtn;
    [SerializeField] private Button m_EmojiBtn;
    [SerializeField] private Button m_SendBtn;
    [SerializeField] private TIMUserProfileExt profile; //对方账号
    private List<TIMChatMsg> msgList;
    private int pageCount = 5; //每页消息数
    [SerializeField] private bool hasRequestToFriend = true; //是否请求了好友
#if UNITY_EDITOR
    public InputField TextInput { get { return m_TextInput; } }
    public int idx;
#endif

    void Awake()
    {
        m_Mask.SetActive(false);
        m_SendBtn.gameObject.SetActive(false);
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_RequestCPBtn.onClick.AddListener(OnRequestCP);
        m_MoreBtn.onClick.AddListener(OnMore);
        //m_RevokeBtn.onClick.AddListener(RevokeMessage);
        m_TextInput.onValueChanged.AddListener(InputChanged);
        m_SoundBtn.onClick.AddListener(SendSound);
        m_EmojiBtn.onClick.AddListener(OnEmojiButtonClick);
        m_SendBtn.onClick.AddListener(SendText);

        m_GalleryBtn.onClick.AddListener(SendImage);
    }

    void Start()
    {
#if UNITY_EDITOR
        return;
#endif
        var unreadNum = (int)TimSdkManager.Instance.GetUnreadMessageNum(profile.identifier, (int)TIMConversationType.C2C);
        //Debug.Log($"拉取漫游消息 unreadNum={unreadNum}");

        //拉取云端（7天），再拉取本地
        TimSdkManager.Instance.GetMessageFirst(profile.identifier, unreadNum); //拉取所有漫游
        ////TODO: 没网直接获取本地消息
        //TimSdkManager.Instance.GetLocalMessageFirst(profile.identifier, pageCount);
    }

    void OnEnable()
    {
        SystemEventManager.StartListening(SystemEventName.Drag, OnSystemMessage);

        TimEventManager.StartListening(TimSdkMessage.SendTextElement, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.GetMessage, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.GetLocalMessage, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.SetReadMessage, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.Download, OnTimSdkMessage);
    }

    void OnDisable()
    {
        SystemEventManager.StopListening(SystemEventName.Drag, OnSystemMessage);

        TimEventManager.StopListening(TimSdkMessage.SendTextElement, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.GetMessage, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.GetLocalMessage, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.SetReadMessage, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.Download, OnTimSdkMessage);
    }

    public override void OnSystemMessage(int value)
    {
        switch (value)
        {
            case (int)SystemEventName.Drag:
                NextPage();
                break;
        }
    }
    public override void OnTimSdkMessage(TimCallback obj)
    {
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.SendTextElement: //发送消息
                {
                    Debug.Log($"发送消息 code={obj.code} data={obj.data}");
                    var resp = JsonMapper.ToObject<TIMMessageResp>(obj.data);
                    if (obj.code == 0)
                    {
                        TIMChatMsg msg = new TIMChatMsg();
                        msg.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                        msg.isSelf = true;
                        msg.isRead = resp.isRead;
                        msg.isPeerReaded = resp.isPeerReaded;
                        msg.elemType = resp.elemType;
                        msg.subType = resp.subType;
                        msg.text = resp.text;
                        msg.param = resp.param;
                        mContentFiller.OnSendButtonClicked(msg);

                        m_TextInput.text = string.Empty;
                    }
                    else
                    {
                        Debug.LogError($"发送文本消息.失败：code={obj.code}, data={obj.data}");
                    }
                }
                break;
            case TimSdkMessage.RecvNewMessages: //收到新消息
                {
                    Debug.Log($"收到新消息 code={obj.code} data={obj.data}");
                    var arr = JsonMapper.ToObject<TIMMessageResp[]>(obj.data);
                    if (obj.code == 0)
                    {
                        TIMChatMsg msg = new TIMChatMsg();
                        msg.seq = arr[0].seq;
                        msg.rand = arr[0].rand;
                        msg.timestamp = arr[0].timestamp;
                        msg.isSelf = arr[0].isSelf;
                        msg.isRead = arr[0].isRead;
                        msg.isPeerReaded = arr[0].isPeerReaded;
                        msg.elemType = arr[0].elemType;
                        msg.subType = arr[0].subType;
                        msg.text = arr[0].text;
                        msg.param = arr[0].param;
                        mContentFiller.OnSendButtonClicked(msg);

                        //聊天面板中收到消息，直接设置为已读
                        TimSdkManager.Instance.SetReadMessage(profile.identifier);
                    }
                    else
                    {
                        Debug.LogError($"接收新消息.失败：code={obj.code}, data={obj.data}");
                    }
                }
                break;
            case TimSdkMessage.GetMessage: //获取漫游消息
                {
                    var arr = JsonMapper.ToObject<TIMMessageResp[]>(obj.data);
                    Debug.Log($"漫游消息={arr.Length}条");

                    //漫游消息不解析，拉取完后标记已读，避免重复拉取和无法删除聊天记录
                    TimSdkManager.Instance.SetReadMessage(profile.identifier);
                    TimSdkManager.Instance.GetLocalMessageFirst(profile.identifier, pageCount);
                }
                break;
            case TimSdkMessage.GetLocalMessage: //获取本地消息
                {
                    var arr = JsonMapper.ToObject<TIMMessageResp[]>(obj.data);
                    Debug.Log($"GetLocalMessage count={arr.Length} data={obj.data}");

                    msgList = new List<TIMChatMsg>();
                    for (int i = 0; i < arr.Length; i++)
                    {
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

                        Debug.Log($"{i} main=[{msg.elemType}]{(TIMElemType)msg.elemType} sub={msg.subType} text={msg.text} param={msg.param}");
                        switch ((TIMElemType)msg.elemType)
                        {
                            case TIMElemType.Text:
                            case TIMElemType.Image:
                                msgList.Add(msg);
                                break;
                            case TIMElemType.Location:
                            case TIMElemType.SNSTips:
                            default:
                                Debug.Log("其它类型消息");
                                break;
                        }
                    }
                    RefreshView(msgList);
                }
                break;
            case TimSdkMessage.SetReadMessage: //我的消息对方已读
                {
                    //界面上所有我发的消息设置为已读
                    mContentFiller.SetRead();
                }
                break;
            case TimSdkMessage.Download: //消息文件下载
                {
                    var data = JsonMapper.ToObject<TIMDownloadResp>(obj.data);
                    Debug.Log($"异步获取文件url type={data.elemType} uuid={data.uuid} url={data.url}");
                    //HttpManager.Download(data.url, OnDownload, data.uuid);
                }
                break;
        }
    }

    void RefreshView(List<TIMChatMsg> list)
    {
        //Debug.Log($"刷新{list.Count}条");
        //list.Sort((x, y) => x.timestamp.CompareTo(y.timestamp)); //排序
        mContentFiller.LoadChatData(list, pageCount);
    }

    public void Init(TIMUserProfileExt userProfile)
    {
        Debug.Log($"与{userProfile.identifier}的聊天页面 关系={userProfile.relation}");
        this.profile = userProfile;

        FileManager.Download(profile.faceUrl, OnLoadHeadImage);
        m_NickNameText.text = profile.nickName;

        hasRequestToFriend = (profile.relation != 0 && profile.relation != 1);
    }

    void OnLoadHeadImage(byte[] bytes)
    {
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_HeadImage.sprite = sp;
    }

    void OnDownload(byte[] bytes)
    {
        //AudioClip clip = WavUtility.ToAudioClip(bytes);
        //m_AudioSource.clip = clip;
        Debug.Log($"下载完成 size={bytes.Length}");
    }

    void OnRequestCP() 
    {
        //TODO: 服务器发送RESTAPI，进行通知。不用由客户端走sdk发送。
        string sex = (profile.gender == -1) ? "他" : "她";
        DialogManager.ShowAlert("一天只能发起一次cp邀请",
            () => { HttpManager.applyCp(profile.identifier, onApplyCp); }, "确定",
            $"确定要邀请{sex}成为cp吗？", null,
            () => { }, "我再想想");
    }

    void OnMore()
    {
        Debug.Log("更多");
        //mContentFiller.listScrollRect.StopMovement(); //测试用tmp
        Report(); //测试用tmp
    }

    public void RevokeMessage()
    {
        Debug.Log("撤回上一条消息");
        string json = "";
        TimSdkManager.Instance.RevokeMessage(profile.identifier, json);
    }

    public void SendText()
    {
        string content = m_TextInput.text;
        if (string.IsNullOrEmpty(content)) return;
        //Debug.Log($"发送{content}");

        //TODO: 测试加好友
        //TODO: 每次打开聊天页，判断不是好友且不在黑名单，发送第一条消息时，同时发送toFriend
        if (!hasRequestToFriend)
            HttpManager.toFriend(profile.identifier, onToFriend);

#if UNITY_EDITOR
        TIMMessageResp resp = new TIMMessageResp();
        resp.setMessage(0, 0, Utils.ToTimestamp(System.DateTime.Now), true, false, false)
                .setType((int)TIMElemType.Text, -1)
                .setText(content);
        string json = JsonMapper.ToJson(resp);

        var sendData = new TimCallback(TimSdkMessage.SendTextElement, 0, json);
        var sendJson = JsonMapper.ToJson(sendData);
        TimSdkManager.Instance.JsonCallback(sendJson);
        return;
#endif

        TimSdkManager.Instance.SendTextElement(content, profile.identifier, 1);
    }

    public void SendSound() 
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Clip192.mp3");
        Debug.Log($"发送语音 path={path} exist={System.IO.File.Exists(path)}");

        TIMSoundElemResp cmd = new TIMSoundElemResp();
        cmd.path = path;
        cmd.duration = 3;
        string json = JsonMapper.ToJson(cmd);

#if UNITY_EDITOR
        //path = System.IO.Path.Combine(Application.dataPath, "Resources/Sounds/Clip192.mp3");
        TIMMessageResp resp = new TIMMessageResp();
        resp.setMessage(0, 0, Utils.ToTimestamp(System.DateTime.Now), true, false, false)
                .setType((int)TIMElemType.Sound, -1)
                .setText(path)
                .setParam(json);
        string jsonStr = JsonMapper.ToJson(resp);

        var sendData = new TimCallback(TimSdkMessage.SendTextElement, 0, jsonStr);
        var sendJson = JsonMapper.ToJson(sendData);
        TimSdkManager.Instance.JsonCallback(sendJson);
        return;
#endif

        TimSdkManager.Instance.SendSoundElement(json, profile.identifier, (int)TIMConversationType.C2C);
    }

    public void SendImage()
    {
        //TODO: 发起相册选择，java层传递图片地址tmp
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Splash.png");
        Debug.Log($"path={path} exist={System.IO.File.Exists(path)}");

#if UNITY_EDITOR
        path = System.IO.Path.Combine(Application.dataPath, "Resources/Sprites/Splash.png");
        TIMMessageResp resp = new TIMMessageResp();
        resp.setMessage(0, 0, Utils.ToTimestamp(System.DateTime.Now), true, false, false)
                .setType((int)TIMElemType.Image, -1)
                .setText(path);
        string json = JsonMapper.ToJson(resp);

        var sendData = new TimCallback(TimSdkMessage.SendTextElement, 0, json);
        var sendJson = JsonMapper.ToJson(sendData);
        TimSdkManager.Instance.JsonCallback(sendJson);
        return;
#endif
        TimSdkManager.Instance.SendImageElement(path, profile.identifier, 1);
    }

    public void NextPage()
    {
#if UNITY_EDITOR
        mContentFiller.LoadMoreData(pageCount);
        //mContentFiller.LoadMoreData(pageCount - 1);
        //Debug.Log($"goto={pageCount}");
        return;
#endif
        TimSdkManager.Instance.GetLocalMessageNext(profile.identifier, pageCount);
    }

    public void Report()
    {
        if (msgList.Count <= 0) return;
        List<ChatLogs> array = new List<ChatLogs>();
        for (int i = 0; i < msgList.Count; i++)
        {
            ChatLogs log = new ChatLogs();
            log.sendUid = long.Parse(UserManager.Instance.localPlayer.identifier);
            log.recvUid = long.Parse(profile.identifier);
            log.seq = msgList[i].seq;
            log.timestamp = msgList[i].timestamp;
            array.Add(log);
        }
        string json = JsonMapper.ToJson(array);
        Debug.Log($"Report {UserManager.Instance.localPlayer.identifier} -> {profile.identifier}\njson={json}");
        HttpManager.chatLogs(json, onChatLogs);
    }

    #region HTTP回调

    public void onToFriend(string json)
    {
        Debug.Log(json);
        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            Debug.Log($"onToFriend={obj.data}");
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
        }
    }

    public void onChatLogs(string json)
    {
        Debug.Log(json);

        //TODO: data增长了亲密度，刷新
    }

    public void onApplyCp(string json)
    {
        Debug.Log(json);

        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            ToastManager.Show("申请成功", 0.5f, MaterialUIManager.UIRoot);
        }
    }

    #endregion

    #region 功能键盘

    [Header("Keyboard")]
    [SerializeField] private GameObject m_Mask;
    [SerializeField] private Transform keyboard;
    [SerializeField] private CanvasGroup m_emojiLayer;
    [SerializeField] private Button m_GalleryBtn;
    [SerializeField] private Button m_CameraBtn;
    [SerializeField] private Button m_PhoneBtn;
    [SerializeField] private Button m_PetBtn;
    private bool isBoxRisen;
    private bool isEmoji;

    void InputChanged(string value) 
    {
        if (string.IsNullOrEmpty(value))
        {
            //TODO: 用变量缓存状态
            m_EmojiBtn.image.rectTransform.DOLocalMoveX(136f, 0.1f);
            m_SendBtn.gameObject.SetActive(false);
        }
        else
        {
            m_EmojiBtn.image.rectTransform.DOLocalMoveX(96f, 0.1f);
            m_SendBtn.gameObject.SetActive(true);
        }
    }

    // Inspector中给Input添加EventTrigger绑定
    public void OnInputFieldClick()
    {
        isEmoji = false;
        m_emojiLayer.alpha = 0;
        m_emojiLayer.interactable = false;
        m_emojiLayer.blocksRaycasts = false;

        //m_moreLayer.alpha = 0;
        //m_moreLayer.interactable = false;
        //m_moreLayer.blocksRaycasts = false;

        //m_commonlyLayer.alpha = 0;
        //m_commonlyLayer.interactable = false;
        //m_commonlyLayer.blocksRaycasts = false;

        if (m_TextInput.isFocused && isBoxRisen == false) //TouchScreenKeyboard.visible
        {
            OnKeyboardVisible();
        }
        else if (!m_TextInput.isFocused && isBoxRisen == true) //TouchScreenKeyboard.visible
        {
            OnKeyboardInvisible();
        }
    }

    public void OnKeyboardVisible()
    {
        //Debug.Log("开启");
        if (!isBoxRisen)
        {
            m_Mask.SetActive(true);
            keyboard.DOLocalMoveY(270, 0.2f);
            //Debug.Log(mContentFiller.listScrollRect.content.childCount);
            //if (mContentFiller.listScrollRect.content.childCount > 3)
            if (mContentFiller.listScrollRect.content.rect.height > 300)
                    mContentFiller.listScrollRect.transform.DOLocalMoveY(270, 0.2f);
            isBoxRisen = true;
        }
    }
    // Inspector中给Mask添加EventTrigger绑定
    public void OnKeyboardInvisible()
    {
        //Debug.Log("关闭");
        if (isBoxRisen == true)
        {
            m_Mask.SetActive(false);
            keyboard.DOLocalMoveY(0, 0.2f);
            mContentFiller.listScrollRect.transform.DOLocalMoveY(0, 0.2f);
            isBoxRisen = false;
        }
    }

    void OnEmojiButtonClick()
    {
        if (!isEmoji)
        {
            isEmoji = true;
            m_emojiLayer.alpha = 1;
            m_emojiLayer.interactable = true;
            m_emojiLayer.blocksRaycasts = true;

            //m_moreLayer.alpha = 0;
            //m_moreLayer.interactable = false;
            //m_moreLayer.blocksRaycasts = false;

            //m_commonlyLayer.alpha = 0;
            //m_commonlyLayer.interactable = false;
            //m_commonlyLayer.blocksRaycasts = false;
        }
        OnKeyboardVisible();
        //OnTextButtonClick();
    }

    #endregion
}
