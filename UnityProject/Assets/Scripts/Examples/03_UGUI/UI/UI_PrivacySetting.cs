using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_PrivacySetting : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Toggle m_ShieldListTog;
    [SerializeField] private Toggle m_MatchedTog;
    [SerializeField] private Button m_ChatWhoBtn;
    [SerializeField] private Text m_ChatWhoText;
    private string[] m_SmallStringList = new string[] { "所有人", "好友", "自闭" };
    private UserConfig tmpConfig; //对比，和原来一样，就不提交

    void Awake()
    {
        m_BackBtn.onClick.AddListener(OnSave);

        m_ShieldListTog.onValueChanged.AddListener(OnShieldListChanged);
        m_MatchedTog.onValueChanged.AddListener(OnMatchedChanged);
        m_ChatWhoBtn.onClick.AddListener(OnChatWho);
    }

    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        Debug.Log(UserManager.Instance.localConfig.ToString());

        tmpConfig = new UserConfig();
        tmpConfig.Update(UserManager.Instance.localConfig); //浅拷贝

        m_ShieldListTog.isOn = UserManager.Instance.localConfig.shieldList == 2; //2:不屏蔽
        m_MatchedTog.isOn = UserManager.Instance.localConfig.onlineMatched == 1; //1:可匹配
        m_ChatWhoText.text = m_SmallStringList[UserManager.Instance.localConfig.chatWho - 1]; //1:全部 2:好友 3:自闭
    }


    void OnShieldListChanged(bool value)
    {
        tmpConfig.shieldList = value ? 2 : 1;
    }

    void OnMatchedChanged(bool value)
    {
        tmpConfig.onlineMatched = value ? 1 : 2;
    }

    void OnChatWho()
    {
        DialogManager.ShowBottomList(m_SmallStringList, OnChatWhoValue);
    }

    void OnChatWhoValue(int selectedIndex)
    {
        //ToastManager.Show("Item #" + selectedIndex + " selected: " + m_SmallStringList[selectedIndex]); //TODO: 点击事件

        tmpConfig.chatWho = selectedIndex + 1;

        m_ChatWhoText.text = m_SmallStringList[selectedIndex]; //1:全部 2:好友 3:自闭
    }


    void OnSave()
    {
        //Debug.Log($"shieldList={m_ShieldListTog.isOn}, onlineMatched={m_MatchedTog.isOn}");

        if (UserManager.Instance.localConfig.Equals(tmpConfig))
        {
            // 和原来一样，直接关闭
            Debug.Log("没有改动");
            base.Close(true);
        }
        else
        {
            // 不一样就先提交，返回结果时关闭
            Debug.Log("提交改动");
            HttpManager.setPrivacy(tmpConfig.shieldList, tmpConfig.onlineMatched, tmpConfig.chatWho, onUserConfig);
        }
    }

    void onUserConfig(string json)
    {
        Debug.Log($"config={json}");

        var obj = JsonMapper.ToObject<HttpCallback<UserConfig>>(json);
        if (obj.code == 0)
        {
            Debug.Log("修改成功");

            UserManager.Instance.updateConfig(obj.data);

            base.Close(true);
        }
        else
        {
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }
}