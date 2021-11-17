using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MaterialUI;
using LitJson;

public class UI_Login : UIWidget
{
#if UNITY_EDITOR
    public InputField UserNameInput { get { return m_UserNameInput; } }
    public InputField PasswordInput { get { return m_PasswordInput; } }
#endif
    [SerializeField] private InputField m_UserNameInput;
    [SerializeField] private InputField m_PasswordInput; //不要低于手机键盘
    [SerializeField] private Button m_LoginBtn;

    void Awake()
    {
        m_LoginBtn.onClick.AddListener(OnLoginButtonClick);
    }

    void OnEnable()
    {
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;
        TimEventManager.StartListening(TimSdkMessage.Login, OnTimSdkMessage); //注册该UI相关的插件消息
    }

    void OnDisable()
    {
        TimEventManager.StopListening(TimSdkMessage.Login, OnTimSdkMessage);
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.Login:
                {
                    if (obj.code == 0)
                    {
                        HttpManager.userConfig(onUserConfig);

                        PanelManager.Instance.CreatePanel<UI_Main>();

                        UserManager.CreateUserRoot();
                        UserManager.Instance.InitDatabase();
                        UserManager.Instance.ds.CreateTable<TIMUserProfileExt>();
                        var ext = new TIMUserProfileExt();
                        ext.identifier = UserManager.Instance.localPlayer.identifier;
                        ext.nickName = UserManager.Instance.localPlayer.nickName;
                        if (UserManager.Instance.ds.QueryProfile(ext.identifier) == null)
                        {
                            //Debug.Log("不存在，写入数据");
                            UserManager.Instance.ds.InsertProfile(ext);
                        }
                        else
                        {
                            //Debug.Log("已经存在，更新数据");
                        }

                        HttpManager.Hide();
                    }
                    else
                    {
                        Debug.LogError($"登录.失败：code={obj.code}, data={obj.data}");
                        //6017  userID or userSig is empty
                        //70013 tlssdk Unpack error
                        ToastManager.Show(obj.data, 0.5f, MaterialUIManager.UIRoot);
                    }
                }
                break;
        }
    }

    void OnLoginButtonClick()
    {
        //TODO: 使用MaterialUI自带的判断提示 LoginLetterValidation
        if (string.IsNullOrEmpty(m_UserNameInput.text))
        {
            Debug.LogError("用户名未填");
            ToastManager.Show("用户名未填", 0.5f, MaterialUIManager.UIRoot);
            return;
        }
        if (string.IsNullOrEmpty(m_PasswordInput.text))
        {
            Debug.LogError("密码未填");
            ToastManager.Show("密码未填", 0.5f, MaterialUIManager.UIRoot);
            return;
        }

        int loginType = 1; //1手机验证码 2手机密码 10微信 11qq 12微博
        string phone = m_UserNameInput.text; //18069828910;
        string code = m_PasswordInput.text; //999000; //验证码或者密码
        HttpManager.login(loginType, phone, code, onLogin);
        HttpManager.ShowDialog("Loading");
    }

    #region HTTP回调

    void onLogin(string json)
    {
        //"code": 0,
        //"msg": "OK",
        //"data": "e9fee037d2116b57495d820db1d8b310" //token
        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            HttpManager.token = obj.data;
            Debug.Log($"token={HttpManager.token}");

            HttpManager.myinfo(onMyInfo);
        }
        else
        {
            Debug.LogError($"onLogin err code={obj.code} msg={obj.msg}");
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    void onMyInfo(string json)
    {
        //Debug.Log($"myinfo={json}");
        var obj = JsonMapper.ToObject<HttpCallback<MyInfoCallback>>(json);
        if (obj.code == 0)
        {
            UserManager.Instance
                .updateIdentifier(obj.data.uid.ToString())
                .updateNick(obj.data.nickname)
                .updateGender(obj.data.sex)
                .updateFaceUrl(obj.data.avatar);

            HttpManager.getSig(onGetSig);
        }
        else
        {
            Debug.LogError($"err code={obj.code} msg={obj.msg}");
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    void onGetSig(string json)
    {
        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            TimSdkManager.Instance.Login(UserManager.Instance.localPlayer.identifier, obj.data);
        }
        else
        {
            Debug.LogError($"onGetSig err code={obj.code} msg={obj.msg}");
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    void onUserConfig(string json)
    {
        Debug.Log($"config={json}");

        var obj = JsonMapper.ToObject<HttpCallback<UserConfig>>(json);
        //Debug.Log($"user/config={obj.data.ToString()}");
        UserManager.Instance.updateConfig(obj.data);
    }

    #endregion
}
