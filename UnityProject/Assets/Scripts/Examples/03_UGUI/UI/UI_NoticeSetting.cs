using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_NoticeSetting : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Toggle m_ShowNoticeTog;
    [SerializeField] private Toggle m_RecvTog1;
    [SerializeField] private Toggle m_RecvTog2;
    [SerializeField] private Toggle m_RecvTog3;
    private UserConfig tmpConfig; //对比，和原来一样，就不提交

    void Awake()
    {
        m_BackBtn.onClick.AddListener(OnSave);

        m_ShowNoticeTog.onValueChanged.AddListener(OnShowNoticeChanged);
        m_RecvTog1.onValueChanged.AddListener(OnNoticeRecv1);
        m_RecvTog2.onValueChanged.AddListener(OnNoticeRecv2);
        m_RecvTog3.onValueChanged.AddListener(OnNoticeRecv3);
    }

    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        Debug.Log(UserManager.Instance.localConfig.ToString());

        //tmpConfig = UserManager.Instance.localConfig; //深拷贝
        tmpConfig = new UserConfig();
        tmpConfig.Update(UserManager.Instance.localConfig); //浅拷贝

        m_ShowNoticeTog.isOn = (UserManager.Instance.localConfig.noticeShow == 1); //1:开，2:关
        m_RecvTog1.isOn = (UserManager.Instance.localConfig.noticeRecv == 1);
        m_RecvTog2.isOn = (UserManager.Instance.localConfig.noticeRecv == 2);
        m_RecvTog3.isOn = (UserManager.Instance.localConfig.noticeRecv == 3);
    }


    void OnShowNoticeChanged(bool value)
    {
        tmpConfig.noticeShow = value ? 1 : 2;
    }

    void OnNoticeRecv1(bool value)
    {
        if (value)
            tmpConfig.noticeRecv = 1;
    }

    void OnNoticeRecv2(bool value)
    {
        if (value)
            tmpConfig.noticeRecv = 2;
    }

    void OnNoticeRecv3(bool value)
    {
        if (value)
            tmpConfig.noticeRecv = 3;
    }


    // 提交保存请求
    void OnSave()
    {
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
            HttpManager.setNotice(tmpConfig.noticeShow, tmpConfig.noticeRecv, onUserConfig);
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