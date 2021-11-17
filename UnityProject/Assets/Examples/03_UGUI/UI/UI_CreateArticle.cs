using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MaterialUI;
using LitJson;
using Client;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_CreateArticle : UIWidget
{
    public static UI_CreateArticle Instance;

    private static Color m_Gray200 = new Color32(200, 200, 200, 255);
    private static Color m_Gray60 = new Color32(153, 153, 153, 255);
    private static Color m_Gray50 = new Color32(50, 50, 50, 255);
    private static Color m_Gray20 = new Color32(20, 20, 20, 255);
    private static Color m_PurpleColor = new Color32(117, 84, 229, 255);

    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_PublishBtn;
    [SerializeField] private Text m_PublishText;
    [SerializeField] private Toggle[] m_PrivacyTogs;
    [SerializeField] private Image m_LocationIcon;
    [SerializeField] private Text m_LocationText;

    [Header("帖子属性")]
    [SerializeField] private HttpManager.ArticlePub article;
    public static string Address
    {
        get { return Instance.article.addr; }
        set
        {
            //Debug.Log($"选择了地址：{value}");

            Instance.article.addr = value;

            if (string.IsNullOrEmpty(value))
            {
                //你在哪里，置灰
                Instance.m_LocationText.text = "你在哪里";
                Instance.m_LocationText.color = m_Gray60;
                Instance.m_LocationIcon.color = m_Gray60;
            }
            else
            {
                //赋值，填充紫色
                Instance.m_LocationText.text = value;
                Instance.m_LocationText.color = m_PurpleColor;
                Instance.m_LocationIcon.color = m_PurpleColor;
            }
        }
    }
    public static string Topic
    {
        //最萌宝宝,可爱
        get { return Instance.article.addr; }
    }
    [SerializeField] private string m_SoundPath;
    [SerializeField] private string m_VideoPath;
    [SerializeField] private List<string> m_ImagePathList;
    [SerializeField] private GameObject m_AddItem;
    [SerializeField] private GameObject m_ImagePrefab;
    [SerializeField] private List<Item_GridImage> objectPool; //对象池
    [SerializeField] private List<Item_GridImage> recyclePool; //回收池
    public Item_GridImage Spawn()
    {
        Item_GridImage script = null;
        if (recyclePool.Count > 0)
        {
            script = recyclePool[recyclePool.Count - 1];
            recyclePool.Remove(script);
            script.gameObject.SetActive(true);
        }
        else
        {
            GameObject item = Instantiate(m_ImagePrefab);
            item.transform.SetParent(m_ImagesGroup.transform);
            item.SetActive(true);
            script = item.GetComponent<Item_GridImage>();
        }
        objectPool.Add(script);
        UpdateAddBtn();
        return script;
    }
    public void Despawn(Item_GridImage obj)
    {
        m_ImagePathList.Remove(obj.filePath);
        objectPool.Remove(obj);
        recyclePool.Add(obj);
        obj.Reset();
        obj.gameObject.SetActive(false);
        UpdateAddBtn();
    }
    void UpdateAddBtn()
    {
        m_AddItem.SetActive(m_ImagePathList.Count > 0 && m_ImagePathList.Count < 9);
        if (m_AddItem.activeSelf)
            m_AddItem.transform.SetAsLastSibling();
    }
    [SerializeField] private RawImage mediaRawImage;
    [SerializeField] private AspectRatioFitter mediaAspect;
    //[SerializeField] private MediaPlayerCtrl mediaPlayer;
    [SerializeField] private Button m_PlayBtn;
    [Header("动态隐藏")]
    [SerializeField] private InputField m_TextInput;
    [SerializeField] private GameObject m_ImagesGroup;
    [SerializeField] private GameObject m_SoundGroup;
    [SerializeField] private GameObject m_VideoGroup;
    [Header("键盘按钮")]
    [SerializeField] private Button m_LocationBtn;  //全屏
    [SerializeField] private Button m_SoundBtn;     //半屏
    [SerializeField] private Button m_GalleryBtn;   //全屏
    [SerializeField] private Button m_VideoBtn;     //全屏
    [SerializeField] private Button m_TopicBtn;     //全屏
    [SerializeField] private Button m_PrivacyBtn;   //半屏
    [SerializeField] private Button m_EmojiBtn;     //半屏
    [Header("Keyboard")]
    //半屏页：语音，隐私，emoji
    [SerializeField] private HalfScreenView halfScreen = HalfScreenView.None;
    private enum HalfScreenView
    {
        None = 0,
        Sound = 1,
        Privacy = 2,
        Emoji = 3,
    }
    private bool isBoxRisen;
    [SerializeField] private GameObject m_Mask;
    [SerializeField] private Transform keyboard; //TODO: 进入时，键盘直接弹出
    [SerializeField] private CanvasGroup m_RecordGroup;
    [SerializeField] private CanvasGroup m_PrivacyGroup;
    [SerializeField] private CanvasGroup m_EmojiGroup;
    [Header("语音面板")]
    [SerializeField] private Image m_RecordProgress;
    [SerializeField] private Text m_TimeText;
    [SerializeField] private Text m_StatusText;
    [SerializeField] private Button m_RecordStartBtn;
    [SerializeField] private Button m_RecordStopBtn;
    [SerializeField] private Button m_RecordPlayBtn;
    [SerializeField] private Button m_RecordDel;
    [SerializeField] private Button m_RecordSend;

    void Awake()
    {
        Instance = this;

        article = new HttpManager.ArticlePub();
        Address = string.Empty;
        m_ImagePathList = new List<string>();
        objectPool = new List<Item_GridImage>();
        recyclePool = new List<Item_GridImage>();
        UpdateImageGrid();

        m_Mask.SetActive(false);
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_PublishBtn.onClick.AddListener(CmdPublish);
        UpdatePublish(false); //初始化

        m_TextInput.onValueChanged.AddListener(OnInputChanged);
        m_LocationBtn.onClick.AddListener(OnLocationBtnClick);
        m_SoundBtn.onClick.AddListener(OnSoundBtnClick);
        m_GalleryBtn.onClick.AddListener(OnGalleryBtnClick);
        m_TopicBtn.onClick.AddListener(OnTopicBtnClick);
        m_PrivacyBtn.onClick.AddListener(OnPrivacyBtnClick);
        m_EmojiBtn.onClick.AddListener(OnEmojiBtnClick);

        for (int i = 0; i < m_PrivacyTogs.Length; i++)
        {
            int index = i;
            m_PrivacyTogs[index].onValueChanged.AddListener((bool value) => OnPrivacyTogChanged(value, index));
        }

        //语音面板
        m_RecordStartBtn.onClick.AddListener(OnRecordStart);
        m_RecordStopBtn.onClick.AddListener(OnRecordStop);
        m_RecordPlayBtn.onClick.AddListener(OnRecordPlay);
        m_RecordDel.onClick.AddListener(OnRecordDelete);
        m_RecordSend.onClick.AddListener(OnRecordSend);

        //m_PlayBtn.onClick.AddListener(OnPlayVideoBtnClick);
    }

    void Start()
    {
        //TODO: 弹出键盘
        m_ImagePrefab.SetActive(false);
    }

    void OnEnable()
    {
        TimEventManager.StartListening(TimSdkMessage.Image, OnTimSdkMessage); //注册该UI相关的插件消息
        TimEventManager.StartListening(TimSdkMessage.Sound, OnTimSdkMessage); //声音是录制的，不是选择文件回调
        TimEventManager.StartListening(TimSdkMessage.Video, OnTimSdkMessage);
    }

    void OnDisable()
    {
        TimEventManager.StopListening(TimSdkMessage.Image, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.Sound, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.Video, OnTimSdkMessage);
    }

    void Update()
    {
        if (Microphone.devices.Length > 0 && Microphone.IsRecording(null)) //正在录音
        {
            m_RecordProgress.fillAmount = (float)MicphoneManager.Instance.audioLength / 90f;
            m_TimeText.text = $"{MicphoneManager.Instance.audioLength}s";
        }
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.Image:
                {
                    if (obj.code == 0)
                    {
                        //Editor中闪是窗口问题

                        //Debug.Log($"Image path={obj.data}"); //C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png
                        if (string.IsNullOrEmpty(obj.data))
                        {
                            Debug.LogError($"图片地址为空");
                            return;
                        }
                        m_ImagePathList.Add(obj.data);
                        UpdateImageGrid();
                        UpdatePublish(true);

                        Item_GridImage script = Spawn();
                        script.Init(obj.data);
                    }
                    else
                    {
                        ToastManager.Show(obj.data, 0.5f, MaterialUIManager.UIRoot);
                    }
                }
                break;
            case TimSdkMessage.Sound:
                {
                    if (obj.code == 0)
                    {
                        Debug.Log($"Sound path={obj.data}");
                    }
                    else
                    {
                        ToastManager.Show(obj.data, 0.5f, MaterialUIManager.UIRoot);
                    }
                }
                break;
            case TimSdkMessage.Video:
                {
                    if (obj.code == 0)
                    {
                        /*
                        Debug.Log($"Video path={obj.data}");
                        //C:/Users/Administrator/Videos/We've been working on a slow introduction scene for our post-apocalyptic game..mp4
                        UpdateImageGrid();
                        UpdatePublish(true);

                        LoadVideo(obj.data);
                        */
                    }
                    else
                    {
                        ToastManager.Show(obj.data, 0.5f, MaterialUIManager.UIRoot);
                    }
                }
                break;
        }
    }

    
    // 输入文本，上传图片，语音，视频后状态激活
    void UpdatePublish(bool value)
    {
        m_PublishBtn.interactable = value;
        m_PublishText.color = value ? m_Gray50 : m_Gray200;
    }
    void UpdateImageGrid()
    {
        bool active = m_ImagePathList.Count > 0;
        if (m_ImagesGroup.activeSelf == active) return;
        m_ImagesGroup.SetActive(active);
    }


    void OnInputChanged(string value)
    {
        if (m_TextInput.text.Length > 0)
        {
            UpdatePublish(true);
        }
        else
        {
            //TODO: 没有文字，同时没有图片、音频、视频，不可发布
            if (m_ImagePathList.Count.Equals(0))
            {
                UpdatePublish(false);
            }
        }
    }

    // 发布文章
    void CmdPublish()
    {
        //TODO: 根据内容自动判断帖子类型
        if (m_ImagePathList.Count > 0 && string.IsNullOrEmpty(m_VideoPath))
        {
            article.type = (int)HttpManager.ArticlePub.TYPE.TYPE_IMG;
        }
        else if (!string.IsNullOrEmpty(m_SoundPath) && m_ImagePathList.Count == 0)
        {
            article.type = (int)HttpManager.ArticlePub.TYPE.TYPE_AUDIO;
        }
        else if (!string.IsNullOrEmpty(m_VideoPath) && m_ImagePathList.Count == 1)
        {
            article.type = (int)HttpManager.ArticlePub.TYPE.TYPE_VIDEO;
        }
        else
        {
            article.type = (int)HttpManager.ArticlePub.TYPE.TYPE_TXT;
        }
        article.content = m_TextInput.text;
        article.addr = Address;


        //TODO: 内容非空判断：文字，图片，音频，视频
        if (string.IsNullOrEmpty(article.content) && m_ImagePathList.Count.Equals(0))
        {
            ToastManager.Show("内容为空", 0.5f, MaterialUIManager.UIRoot);
            return;
        }
        

        // 图片、音频、视频数据上传
        if (article.type == (int)HttpManager.ArticlePub.TYPE.TYPE_IMG)
        {
            //string url = $"{ConstValue.api_route}/api/upload/article";
            //HttpManager.HttpUploadQueue(url, m_ImagePathList.ToArray(), HttpManager.ContentType.img, onUploadQueue); //协程模式//OK
            //HttpManager.UploadFile(m_ImagePathList[0], HttpManager.ContentType.img, onUploadFile); //搭搭模式//OK
            //HttpManager.UploadTask(m_ImagePathList[0], HttpManager.ContentType.img, onUploadFile); //Task模式//OK
            HttpManager.UploadTasks(m_ImagePathList.ToArray(), HttpManager.ContentType.img, onUploadQueue); //Task模式//OK
        }
        else if (article.type == (int)HttpManager.ArticlePub.TYPE.TYPE_AUDIO)
        {
            //HttpManager.UploadTask(m_ImagePathList.ToArray(), HttpManager.ContentType.audio, onUploadFile); //Task模式//OK
        }
        else if (article.type == (int)HttpManager.ArticlePub.TYPE.TYPE_VIDEO)
        {
            HttpManager.UploadVideo(m_ImagePathList[0], m_VideoPath, onUploadVideo); //Task模式//OK
        }
        else if (article.type == (int)HttpManager.ArticlePub.TYPE.TYPE_TXT)
        {
            HttpManager.pub(article, onPub);
        }
    }

    void OnLocationBtnClick()
    {
        PanelManager.Instance.CreatePanel<UI_POI>(false, true);
    }

    void OnGalleryBtnClick()
    {
        //TimSdkManager.Instance.OpenGallery();
        TimSdkManager.Instance.OpenVideo();
    }

    void OnTopicBtnClick()
    {
        PanelManager.Instance.CreatePanel<UI_ArticleTopic>(false, true);
    }

    void OnPrivacyTogChanged(bool value, int index)
    {
        if (value)
        {
            //Debug.Log($"id={index} 激活");
            m_PrivacyTogs[index].targetGraphic.color = m_PurpleColor;
            m_PrivacyTogs[index].graphic.color = m_PurpleColor;

            //toggle index
            //0社区可见     --  100全部可见     ++ anonymous0
            //1主页可见     --  2空间可见       ++ anonymous0
            //2自己可见     --  1自己可见       ++ anonymous0
            //3仅陌生人可见 --  4好友不可见     ++ anonymous0
            //4伴侣可见     --  3伴侣可见       ++ anonymous0 以上均不匿名
            //5匿名         --  100全部可见？？ ++ anonymous1

            switch (index)
            {
                case 0://社区可见
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_ALL;//全部可见
                    article.anonymous = 0;
                    break;
                case 1://主页可见
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_ZONE;//空间可见
                    article.anonymous = 0;
                    break;
                case 2://自己可见
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_MYSELF;//自己可见
                    article.anonymous = 0;
                    break;
                case 3://仅陌生人可见
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_NOT_FRIEND;//仅陌生人可见
                    article.anonymous = 0;
                    break;
                case 4://伴侣可见
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_CP;//伴侣可见
                    article.anonymous = 0;
                    break;
                case 5://匿名
                    article.pri = (int)HttpManager.ArticlePub.Pri.PRI_ALL;//全部可见
                    article.anonymous = 1;
                    break;
            }
        }
        else
        {
            m_PrivacyTogs[index].targetGraphic.color = m_Gray20;
            m_PrivacyTogs[index].graphic.color = m_Gray20;
        }
    }
    /*
    void OnPlayVideoBtnClick()
    {
        mediaPlayer.OnEnd = () => { mediaPlayer.Stop(); };

        if (mediaPlayer.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
            mediaPlayer.Pause();
        else
            mediaPlayer.Play();
    }

    async void LoadVideo(string path)
    {
        m_VideoPath = path; //TODO: 删除视频后置空

        mediaPlayer.m_strFileName = $"file://{path}";
        mediaPlayer.Play();
        await Task.Delay(TimeSpan.FromSeconds(1));
        //TODO: MediaPlayerCtrl.DownloadStreamingVideoAndLoad //写成Task，等待加载后获取图片
        //TODO: 该过程转圈


        //mediaRawImage.texture = mediaPlayer.GetVideoTexture();
        int width = mediaPlayer.GetVideoWidth();
        int height = mediaPlayer.GetVideoHeight();
        float ratio = (float)width / (float)height;

        //var t2d = mediaPlayer.GetVideoTexture(); ;
        //mediaRawImage.texture = t2d;
        //mediaRawImage.transform.localScale = Vector3.one;
        //mediaAspect.aspectRatio = (float)mediaRawImage.texture.width/(float)mediaRawImage.texture.height;
        //Debug.Log($"status={mediaPlayer.GetCurrentState()}, {mediaRawImage.texture.width}x{mediaRawImage.texture.height}");
        //mediaPlayer.Pause();
        Texture mainTexture = mediaPlayer.GetVideoTexture();
        preview = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 24);
        Graphics.Blit(mainTexture, renderTexture);
        RenderTexture.active = renderTexture;
        preview.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        preview.Apply();
        mediaPlayer.Pause();
        RenderTexture.active = null;

        preview = Utils.FlipVertical(preview);
        preview = TextureScale.Bilinear(preview, 480, Mathf.RoundToInt(480f / ratio));
        mediaRawImage.texture = preview;
        mediaAspect.aspectRatio = ratio;
        mediaAspect.transform.localScale = Vector3.one;
        mediaRawImage.uvRect = new Rect(0, 0, 1, 1); //图片的Rect
        //mediaRawImage.uvRect = new Rect(0, 1, 1, -1); //这里的图片是从视频上拍屏的，用视频的Rect
        Debug.Log($"origin={width}x{height}, comp={preview.width}x{preview.height}"); //1920x1080


        byte[] data = preview.EncodeToJPG();
        long currentTicks = DateTime.Now.Ticks;
        DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;
        string fileName = $"{UserManager.Instance.localPlayer.identifier}_{currentMillis}.jpg";
        string savePath = $"{ConstValue.image_root}/{fileName}";
        System.IO.File.WriteAllBytes(savePath, data);
        m_ImagePathList.Add(savePath);
    }

    public Texture2D preview;
    */

    #region 语音面板

    void SetRecordStatus(int status)
    {
        switch (status)
        {
            case 0:
                {
                    m_RecordProgress.gameObject.SetActive(false);
                    m_TimeText.text = "";
                    m_RecordStartBtn.gameObject.SetActive(true);
                    m_RecordStopBtn.gameObject.SetActive(false);
                    m_RecordPlayBtn.gameObject.SetActive(false);
                    m_RecordDel.gameObject.SetActive(false);
                    m_RecordSend.gameObject.SetActive(false);
                    m_StatusText.text = "点击录音";
                    break;
                }
            case 1:
                {
                    m_RecordProgress.gameObject.SetActive(true);
                    m_TimeText.text = "0s";
                    m_RecordStartBtn.gameObject.SetActive(false);
                    m_RecordStopBtn.gameObject.SetActive(true);
                    m_RecordPlayBtn.gameObject.SetActive(false);
                    m_RecordDel.gameObject.SetActive(false);
                    m_RecordSend.gameObject.SetActive(false);
                    m_StatusText.text = "录音中";
                    break;
                }
            case 2:
                {
                    m_RecordProgress.gameObject.SetActive(true);
                    m_TimeText.text = "12s";
                    m_RecordStartBtn.gameObject.SetActive(false);
                    m_RecordStopBtn.gameObject.SetActive(false);
                    m_RecordPlayBtn.gameObject.SetActive(true);
                    m_RecordDel.gameObject.SetActive(true);
                    m_RecordSend.gameObject.SetActive(true);
                    m_StatusText.text = "点击播放";
                    break;
                }
        }
    }

    void OnRecordStart()
    {
        Debug.Log("Start");
        SetRecordStatus(1);

        MicphoneManager.Instance.StartRecord();
    }

    void OnRecordStop()
    {
        Debug.Log("Stop");
        SetRecordStatus(2);

        MicphoneManager.Instance.StopRecord();

        m_TimeText.text = $"{MicphoneManager.Instance.audioLength}s";
    }

    void OnRecordPlay()
    {
        Debug.Log("Play");

        MicphoneManager.Instance.PlayRecord();
    }

    void OnRecordDelete()
    {
        Debug.Log("Delete");
        SetRecordStatus(0);
    }

    void OnRecordSend()
    {
        Debug.Log("Send");

        string filePath = MicphoneManager.filePath;
        var data = MicphoneManager.Instance.GetClipData();
        Debug.Log($"filePath={filePath}, data={data.Length}");

        // 发送流数据
        //HttpManager.upload(filePath, data, HttpManager.ContentType.audio, onUploadAudio);
        HttpManager.UploadTask(filePath, HttpManager.ContentType.audio, onUploadAudio); //Task模式//OK
    }

    #endregion

    #region HTTP回调

    void onUploadFile(string url)
    {
        Debug.Log($"onUploadFile url={url}");
    }

    void onUploadQueue(string[] array)
    {
        string pics = string.Empty;

        for (int i = 0; i < array.Length; i++)
        {
            //Debug.Log(array[i]);
            pics += array[i];
            if (i < array.Length - 1)
            {
                pics += ",";
            }
        }

        Debug.Log($"cover={array[0]} pics={pics}");
        article.cover = array[0];
        article.pics = pics;
        //Debug.Log(article.ToString());

        //return;
        HttpManager.pub(article, onPub);
    }

    void onUploadAudio(string url)
    {
        Debug.Log($"onUploadAudio url={url}");

        //TODO: 关闭键盘
    }

    void onUploadVideo(string[] urlArray)
    {
        Debug.Log($"onUploadVideo cover={urlArray[0]} video={urlArray[1]}");

        article.cover = urlArray[0];
        article.dataUrl = urlArray[1];
        Debug.Log(article.ToString());

        //return;
        HttpManager.pub(article, onPub);
    }

    void onPub(string json)
    {
        Debug.Log(json);
        //{"code":0,"msg":"OK","data":
        //{"_id":1,"updateTime":1586763183464,"type":1,"content":"hello world","cover":"","pics":null,"dataUrl":"","userId":2,"status":3,"essence":1,"audited":1,"adminId":0,"sort":0.0,"readCnt":0,"praiseCnt":0,"commentCnt":0,"shareCnt":0,"createTime":1586763183464,"publishTime":1586763183447,"topics":null,"pri":0,"addr":"","picFormat":null,"top":0,"anonymous":0,"loc":{"type":"Point","coordinates":[0.0,0.0]},"group":0}}

        var obj = JsonMapper.ToObject<HttpCallback<ArticlePub>>(json);
        if (obj.code == 0)
        {
            ToastManager.Show("发表成功", 0.5f, MaterialUIManager.UIRoot);
            base.Close(true); // 关闭页面

            //TODO: 更新文章列表
            //SocialData.(ArticleItemData)mItemDataList
            ArticleItemData tData = new ArticleItemData();
            tData.mName = "文章";
            tData.mId = 2;
            tData.praised = false;
            tData.user = new ArticleUser()
            { 
                uid = obj.data.userId,
                avatar = UserManager.Instance.localPlayer.faceUrl,
                no = "0",//
                sex = UserManager.Instance.localPlayer.gender,
                nickname = UserManager.Instance.localPlayer.nickName,
            };
            tData.id = obj.data._id;
            tData.type = obj.data.type;
            tData.content = obj.data.content;
            tData.status = obj.data.status;
            tData.anonymous = obj.data.anonymous;
            tData.topics = obj.data.topics;
            tData.top = obj.data.top;
            tData.dataUrl = obj.data.dataUrl;
            tData.commentCnt = obj.data.commentCnt;
            tData.praiseCnt = obj.data.praiseCnt;
            tData.cover = obj.data.cover;
            tData.publishTime = obj.data.publishTime;
            tData.pics = obj.data.pics;
            tData.essence = obj.data.essence;
            tData.pri = obj.data.pri;
            tData.addr = obj.data.addr;
            if (GameObject.FindObjectOfType<UI_Social>() != null)
            {
                SocialData.Get.InsertData(2, tData); //TODO: 只有当前在导航/社区分页，才进行插入
                SocialData.Get.mLoopListView.SetListItemCount(SocialData.Get.TotalItemCount + 1, false);
                //SocialData.Get.mLoopListView.RefreshAllShownItem();
                SocialData.Get.mLoopListView.MovePanelToItemIndex(0, 0);
            }
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
        }
    }

    #endregion

    #region 功能键盘

    void ShowKeyboard()
    {
        //m_ContentInput.ActivateInputField();
    }

    void ShowKeyboard2()
    {
        //string value = m_ContentInput.text;
        ////string placeholder = "分享你的生活...";
        //TouchScreenKeyboard.hideInput = true;
        //screenKeyboard = TouchScreenKeyboard.Open(value);

        //screenKeyboard.text = "abc";
    }

    void HideKeyboard()
    {
        //m_ContentInput.DeactivateInputField();
    }

    // Inspector中给Input添加EventTrigger绑定
    public void OnInputFieldClick()
    {
        halfScreen = HalfScreenView.None;
        HideAllHalfGroups();

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
        if (!isBoxRisen)
        {
            m_Mask.SetActive(true);
            keyboard.DOLocalMoveY(270, 0.2f);
            isBoxRisen = true;
        }
    }
    // Inspector中给Mask添加EventTrigger绑定
    public void OnKeyboardInvisible()
    {
        //if (isEmoji == true)
        //{
        //    // 切换回文字输入
        //    isEmoji = false;
        //    m_emojiLayer.alpha = 0;
        //    m_emojiLayer.interactable = false;
        //    m_emojiLayer.blocksRaycasts = false;
        //    //m_TextInput.ActivateInputField();
        //    return;
        //}

        halfScreen = HalfScreenView.None;

        //Debug.Log("关闭");
        if (isBoxRisen == true)
        {
            m_Mask.SetActive(false);
            keyboard.DOLocalMoveY(0, 0.2f);
            isBoxRisen = false;
        }
    }

    void OnSoundBtnClick()
    {
        if (halfScreen != HalfScreenView.Sound)
        {
            halfScreen = HalfScreenView.Sound;
            HideAllHalfGroups();
            m_RecordGroup.alpha = 1;
            m_RecordGroup.interactable = true;
            m_RecordGroup.blocksRaycasts = true;
        }
        OnKeyboardVisible();
        SetRecordStatus(0);
    }

    void OnPrivacyBtnClick()
    {
        if (halfScreen != HalfScreenView.Privacy)
        {
            halfScreen = HalfScreenView.Privacy;
            HideAllHalfGroups();
            m_PrivacyGroup.alpha = 1;
            m_PrivacyGroup.interactable = true;
            m_PrivacyGroup.blocksRaycasts = true;
        }
        OnKeyboardVisible();
    }

    void OnEmojiBtnClick()
    {
        if (halfScreen != HalfScreenView.Emoji)
        {
            halfScreen = HalfScreenView.Emoji;
            HideAllHalfGroups();
            m_EmojiGroup.alpha = 1;
            m_EmojiGroup.interactable = true;
            m_EmojiGroup.blocksRaycasts = true;
        }
        OnKeyboardVisible();
    }

    void HideAllHalfGroups()
    {
        m_RecordGroup.alpha = 0;
        m_RecordGroup.interactable = false;
        m_RecordGroup.blocksRaycasts = false;

        m_PrivacyGroup.alpha = 0;
        m_PrivacyGroup.interactable = false;
        m_PrivacyGroup.blocksRaycasts = false;

        m_EmojiGroup.alpha = 0;
        m_EmojiGroup.interactable = false;
        m_EmojiGroup.blocksRaycasts = false;
    }

    #endregion

    #region 测试方法

    public void Print()
    {
        Debug.Log(article.ToString());
    }

    public void Add()
    {
        //Debug.Log($"path={obj.data}"); //C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png
        string path = "C:/Users/Administrator/Pictures/4c14ce926ac93baf4c02f7e0119bae94.jpg";
        m_ImagePathList.Add(path);
        UpdateImageGrid();
        UpdatePublish(true);

        Item_GridImage script = Spawn();
        script.Init(path);
    }

    #endregion
}
