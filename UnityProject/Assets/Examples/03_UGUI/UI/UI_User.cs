using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_User : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_ChatBtn;
    [SerializeField] private Text m_NickNameText;
    [SerializeField] private Text m_GenderText;
    [SerializeField] private Text m_BirthText;
    [SerializeField] private Image m_HeadImage;
    [SerializeField] private TIMUserProfileExt profile;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_ChatBtn.onClick.AddListener(Chat);
    }

    void OnEnable()
    {
        //TimEventManager.StartListening(TimSdkMessage.AddFriend, OnTimSdkMessage);
        //TimEventManager.StartListening(TimSdkMessage.CheckFriends, OnTimSdkMessage);
    }

    void OnDisable()
    {
        //TimEventManager.StopListening(TimSdkMessage.AddFriend, OnTimSdkMessage);
        //TimEventManager.StopListening(TimSdkMessage.CheckFriends, OnTimSdkMessage);
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        //switch ((TimSdkMessage)obj.msg) 
        //{
        //    case TimSdkMessage.AddFriend:
        //        {
        //            if (obj.code == 0)
        //            {
        //                Debug.Log($"<color=green>添加操作.成功：{obj.data}</color>");

        //                var result = JsonMapper.ToObject<TIMFriendResult>(obj.data);
        //                string errMsg = string.Empty;
        //                switch (result.ResultCode)
        //                {
        //                    case 0: //操作成功
        //                        break;
        //                    case 30001: //请求参数错误，请根据错误描述检查请求是否正确
        //                        errMsg = "请根据错误描述检查请求是否正确";
        //                        break;
        //                    case 30010: //加好友、响应好友时有效：自己的好友数已达系统上限
        //                        errMsg = "自己的好友数已达系统上限";
        //                        break;
        //                    case 30014: //加好友、响应好友时有效：对方的好友数已达系统上限
        //                        errMsg = "对方的好友数已达系统上限";
        //                        break;
        //                    case 30515: //加好友时有效：被加好友在自己的黑名单中
        //                        errMsg = "被加好友在自己的黑名单中";
        //                        break;
        //                    case 30516: //加好友时有效：被加好友设置为禁止加好友
        //                        errMsg = "被加好友设置为禁止加好友";
        //                        break;
        //                    case 30525: //加好友时有效：已被被添加好友设置为黑名单
        //                        errMsg = "已被被添加好友设置为黑名单";
        //                        break;
        //                    case 30539: //加好友时有效：等待好友审核同意
        //                        errMsg = "等待好友审核同意";
        //                        break;
        //                    default: //未知错误
        //                        errMsg = "未知错误";
        //                        break;
        //                }
        //                ToastManager.Show(errMsg, 0.5f, MaterialUIManager.UIRoot);
        //            }
        //            else
        //            {
        //                Debug.LogError($"添加操作.失败：code={obj.code}, data={obj.data}");
        //            }
        //        }
        //        break;
        //    case TimSdkMessage.CheckFriends:
        //        {
        //            if (obj.code == 0)
        //            {
        //                Debug.Log($"<color=green>检查好友.成功：{obj.data}</color>");

        //                // 0=不是好友，1=对方在我的好友列表中，2=我在对方的好友列表中，3=互为好友
        //                var resp = JsonMapper.ToObject<TIMCheckFriendResult[]>(obj.data);
        //                Debug.Log($"我与该用户的关系：{resp[0].resultType}");

        //                bool notFriend = resp[0].resultType == 0;
        //                //m_AddBtn.gameObject.SetActive(notFriend);
        //                //m_ChatBtn.gameObject.SetActive(!notFriend);
        //            }
        //            else
        //            {
        //                Debug.LogError($"检查好友.失败：code={obj.code}, data={obj.data}");
        //            }
        //        }
        //        break;
        //}
    }

    //public void Init(TIMUserProfileExt _profile) 
    //{
    //    this.profile = _profile;
    //    m_NickNameText.text = profile.nickName;
    //    m_GenderText.text = profile.gender == 0 ? "未知" : profile.gender == 1 ? "男" : "女";
    //    m_BirthText.text = profile.birthday.ToString();

    //    List<string> users = new List<string>();
    //    users.Add(profile.identifier);
    //    string usersjson = JsonMapper.ToJson(users);
    //    TimSdkManager.Instance.CheckFriends(2, usersjson);
    //}
    public void Init(InfoCallback _profile)
    {
        bool result = UserManager.Instance.friends.TryGetValue(_profile.uid, out this.profile);
        if (!result)
        {
            this.profile = new TIMUserProfileExt()
            {
                identifier = _profile.uid.ToString(),
                nickName = _profile.nickname,
                gender = _profile.sex,
                relation = 0,
                amount = 0,
                faceUrl = "http://avatar.zd1312.com/def/women_320_320.png",
            };
        }
        m_NickNameText.text = $"{profile.nickName}{(profile.relation == 3 ? "(好友)" : "")}"; //测试tmp
        m_GenderText.text = profile.gender == 0 ? "未知" : profile.gender == 1 ? "男" : "女";
        //m_BirthText.text = $"{profile.province} {profile.city}";

        FileManager.Download(profile.faceUrl, OnLoadHeadImage);
    }

    void OnLoadHeadImage(byte[] bytes)
    {
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_HeadImage.sprite = sp;
    }

    void Add() 
    {
        Debug.Log($"申请好友：{profile.identifier}");
        TimSdkManager.Instance.AddFriend(profile.identifier, $"{profile.nickName} 请求与您成为好友");
    }

    void Chat()
    {
        Debug.Log($"跳转聊天页：{profile.identifier}");
        var script = PanelManager.Instance.CreatePanel<UI_Chat>(false, true);
        script.Init(profile);
    }
}
