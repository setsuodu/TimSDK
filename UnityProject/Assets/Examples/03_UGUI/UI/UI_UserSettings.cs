using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_UserSettings : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Text m_NickNameText;
    [SerializeField] private Button m_ModifyNickBtn;
    [SerializeField] private Text m_DescText;
    [SerializeField] private Image m_GenderImage;
    [SerializeField] private Sprite[] genderSprites;
    [SerializeField] private Image m_HeadImage;
    [SerializeField] private Button m_VIPBannerBtn;
    [SerializeField] private Button m_ModifyTagBtn;
    [SerializeField] private Button m_BindPhoneBtn;
    [SerializeField] private Button m_NoticeSettingBtn;
    [SerializeField] private Button m_PrivacySettingBtn;
    [SerializeField] private Button m_BlackListBtn;
    [SerializeField] private Button m_ClearCacheBtn;
    [SerializeField] private Text m_CacheText;
    [SerializeField] private Button m_LogoutBtn;
    [SerializeField] private Text m_VersionText;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_ModifyNickBtn.onClick.AddListener(OnModifyNick);
        m_VIPBannerBtn.onClick.AddListener(OnVIPBannar);
        m_ModifyTagBtn.onClick.AddListener(OnModifyTag);
        m_BindPhoneBtn.onClick.AddListener(OnBindPhone);
        m_NoticeSettingBtn.onClick.AddListener(OnNoticeSetting);
        m_PrivacySettingBtn.onClick.AddListener(OnPrivacySetting);
        m_BlackListBtn.onClick.AddListener(OnBlackList);
        m_ClearCacheBtn.onClick.AddListener(OnClearCache);
        m_LogoutBtn.onClick.AddListener(OnLogout);
    }

    void Start()
    {
        m_NickNameText.text = UserManager.Instance.localPlayer.nickName;
        m_GenderImage.sprite = genderSprites[(UserManager.Instance.localPlayer.gender == 1) ? 0 : 1];
        FileManager.Download(UserManager.Instance.localPlayer.faceUrl, OnLoadHeadImage);

        //测试数据
        //var datetime196514 = new System.DateTime(1965, 1, 4);
        //var long196514 = Utils.ToTimestamp(datetime196514);
        //UserManager.Instance.updateBirthday(long196514);
        System.DateTime birth = Utils.ConvertTimestamp(UserManager.Instance.localPlayer.birthday);
        DateParser dp = new DateParser(birth);
        string gen = Utils.Generation(birth);
        m_DescText.text = $"{dp.Constellation} {gen}";

        m_VersionText.text = $"版本号{Application.version}";
    }

    void OnEnable()
    {
        SystemEventManager.StartListening(SystemEventName.UserProfileChanged, OnSystemMessage); //用户资料变更
    }

    void OnDisable()
    {
        SystemEventManager.StopListening(SystemEventName.UserProfileChanged, OnSystemMessage);
    }

    public override void OnSystemMessage(int value)
    {
        switch ((SystemEventName)value)
        {
            case SystemEventName.UserProfileChanged:
                {
                    m_NickNameText.text = UserManager.Instance.localPlayer.nickName;
                }
                break;
        }
    }

    void OnLoadHeadImage(byte[] bytes)
    {
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_HeadImage.sprite = sp;
    }

    void OnVIPBannar()
    {
        ToastManager.Show("goto VIP", 0.5f, MaterialUIManager.UIRoot);
    }

    void OnModifyNick() 
    {
        PanelManager.Instance.CreatePanel<UI_ModifyNick>(false, true);
    }

    void OnModifyTag()
    {
        PanelManager.Instance.CreatePanel<UI_ModifyTag>(false, true);
    }

    void OnBindPhone()
    {
        PanelManager.Instance.CreatePanel<UI_BindPhone>(false, true);
    }

    void OnNoticeSetting()
    {
        PanelManager.Instance.CreatePanel<UI_NoticeSetting>(false, true);
    }

    void OnPrivacySetting()
    {
        PanelManager.Instance.CreatePanel<UI_PrivacySetting>(false, true);
    }

    void OnBlackList()
    {
        PanelManager.Instance.CreatePanel<UI_Blacks>(false, true);
    }

    void OnClearCache()
    {

    }

    void OnLogout() 
    {
        DialogManager.ShowAlert("确定退出登录？", 
            () => { TimSdkManager.Instance.Logout(); }, "确定", null, null,
            () => { }, "我再想想");
    }
}
