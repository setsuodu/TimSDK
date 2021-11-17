using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MaterialUI;
using LitJson;

public class UI_Search : UIWidget
{
    [SerializeField] private InputField m_SearchInput;
    [SerializeField] private Button m_CancelBtn;
    [SerializeField] private Button m_SearchBtn;
    [SerializeField] private Text m_SearchText;

    void Awake()
    {
        m_SearchInput.onValueChanged.AddListener(OnValueChanged);
        m_CancelBtn.onClick.AddListener(() => base.Close(true));
        m_SearchBtn.onClick.AddListener(Search);
    }

    void OnEnable()
    {
        //TimEventManager.StartListening(TimSdkMessage.GetUsersProfile, OnTimSdkMessage); //通过info获取
    }

    void OnDisable()
    {
        //TimEventManager.StopListening(TimSdkMessage.GetUsersProfile, OnTimSdkMessage);
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        //if (obj.code == 0)
        //{
        //    var user = JsonMapper.ToObject<TIMUserProfileExt>(obj.data);
        //    if (string.IsNullOrEmpty(user.nickName))
        //    {
        //        Debug.LogError("该用户不存在");
        //        ToastManager.Show("该用户不存在", 0.5f, MaterialUIManager.UIRoot);
        //        return;
        //    }
        //    Debug.Log("跳转到用户资料页");
        //    var scirpt = PanelManager.Instance.CreatePanel<UI_User>(false, true);
        //    scirpt.Init(user);
        //}
        //else
        //{
        //    Debug.LogError($"获取别人资料.失败：code={obj.code}, data={obj.data}");
        //    ToastManager.Show("获取资料失败", 0.5f, MaterialUIManager.UIRoot);
        //}
    }

    void OnValueChanged(string msg)
    {
        m_SearchBtn.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        m_SearchText.text = $"查找：{msg}";
    }

    void Search() 
    {
        if (string.IsNullOrEmpty(m_SearchInput.text))
        {
            ToastManager.Show("用户名为空", 0.5f, MaterialUIManager.UIRoot);
            return;
        }
        Debug.Log($"搜索 {m_SearchInput.text}");

        int rid = int.Parse(m_SearchInput.text);
        HttpManager.info(rid, onInfo);
    }

    #region HTTP回调

    public void onInfo(string json)
    {
        Debug.Log(json);
        var obj = JsonMapper.ToObject<HttpCallback<InfoCallback>>(json);
        if (obj.code == 0)
        {
            var scirpt = PanelManager.Instance.CreatePanel<UI_User>(false, true);
            scirpt.Init(obj.data);
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    #endregion
}
