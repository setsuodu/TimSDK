using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_ModifyNick : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_ClearBtn;
    [SerializeField] private Button m_SaveBtn;
    [SerializeField] private Text m_SaveText;
    [SerializeField] private InputField m_NickNameInput;
    private Color disableColor = new Color32(204, 204, 204, 255);
    private Color activeColor = new Color32(50, 50, 50, 255);

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_ClearBtn.onClick.AddListener(OnClear);
        m_ClearBtn.gameObject.SetActive(false);
        m_SaveBtn.onClick.AddListener(CmdCommit);
        m_NickNameInput.onValueChanged.AddListener(OnNickNameChanged);

        RefreshView(); //在Awake中初始化完成
    }

    void OnEnable()
    {
        SystemEventManager.StartListening(SystemEventName.UserProfileChanged, OnSystemMessage); //用户资料变更
    }

    void OnDisable()
    {
        SystemEventManager.StopListening(SystemEventName.UserProfileChanged, OnSystemMessage);
    }

    void RefreshView()
    {
        m_NickNameInput.text = UserManager.Instance.localPlayer.nickName;

        m_SaveBtn.interactable = false;
        m_SaveText.color = disableColor;
    }

    void OnClear()
    {
        m_NickNameInput.text = string.Empty;
    }

    void CmdCommit()
    {
        //a.用户昵称长度最长可以是6个汉字或者12个字符，最短为1个汉字或1个字符。
        //b.没有输入昵称确认修改按钮无法点击。

        // 长度/格式校验
        if (string.IsNullOrEmpty(m_NickNameInput.text))
        {
            ToastManager.Show("昵称长度不足", 0.5f, MaterialUIManager.UIRoot);
            return;
        }
        if (Utils.GetLength(m_NickNameInput.text) > 12)
        {
            ToastManager.Show("昵称最长6个汉字或者12个字符", 0.5f, MaterialUIManager.UIRoot);
            return;
        }

        HttpManager.userUpdate(m_NickNameInput.text, onUserUpdate);
    }

    public void onUserUpdate(string json)
    {
        Debug.Log(json);
        //{"code":0,"msg":"OK","data":{"coin":0,"diamond":0,"city":null,"province":null,"signal":null,"vip":0,"avatarId":0,"exp":0,"uid":2,"nickname":"lala","sex":1,"no":"4","avatar":"http://avatar.zd1312.com/def/women_320_320.png"}}
        var obj = JsonMapper.ToObject<HttpCallback<InfoCallback>>(json);
        Debug.Log(obj.data.ToString());

        //TODO: 全局事件通知，写入本地SQL
        if (obj.code == 0)
        {
            UserManager.Instance.updateNick(obj.data.nickname);
            SystemEventManager.TriggerEvent(SystemEventName.UserProfileChanged);

            ToastManager.Show("修改成功", 0.5f, MaterialUIManager.UIRoot);

            base.Close(true);
        }
        else
        {
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    void OnNickNameChanged(string value)
    {
        if (!m_SaveBtn.interactable)
        {
            m_SaveBtn.interactable = true;
            m_SaveText.color = activeColor;
        }

        bool submit = !string.IsNullOrEmpty(value);
        m_ClearBtn.gameObject.SetActive(submit);
        m_SaveText.color = !submit ? disableColor : activeColor;
    }
}
