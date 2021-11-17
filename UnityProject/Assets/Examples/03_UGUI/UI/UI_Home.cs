using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Home : UIWidget
{
    public Text m_IdText;
    public Text m_NickNameText;
    public Image m_HeadImage;
    public Button m_TaskBtn;
    public Button m_ClothBtn;
    public Button m_SettingsBtn;

    void Awake()
    {
        m_TaskBtn.onClick.AddListener(OnTask);
        m_ClothBtn.onClick.AddListener(OnCloth);
        m_SettingsBtn.onClick.AddListener(OnSettings);
    }

    void OnEnable()
    {
        SystemEventManager.StartListening(SystemEventName.UserProfileChanged, OnSystemMessage); //用户资料变更
    }

    void OnDisable()
    {
        SystemEventManager.StopListening(SystemEventName.UserProfileChanged, OnSystemMessage);
    }

    void Start()
    {
        m_IdText.text = UserManager.Instance.localPlayer.identifier;
        m_NickNameText.text = UserManager.Instance.localPlayer.nickName;
        FileManager.Download(UserManager.Instance.localPlayer.faceUrl, OnLoadHeadImage);
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

    void OnTask()
    {
        Debug.Log("任务");
    }

    void OnCloth()
    {
        Debug.Log("服饰");
    }

    void OnSettings()
    {
        //Debug.Log("设置");
        PanelManager.Instance.CreatePanel<UI_UserSettings>(false, true);
    }
}
