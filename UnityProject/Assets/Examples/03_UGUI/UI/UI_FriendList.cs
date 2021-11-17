using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Client;
using MaterialUI;

public class UI_FriendList : UIWidget
{
    [SerializeField] private Button m_BackBtn;

    private DialogProgress dialog;
    private Coroutine _timeout;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));

        HttpManager.CallDelegate += OnComplete;
    }

    void OnDestroy()
    {
        HttpManager.CallDelegate -= OnComplete;
    }

    void OnEnable()
    {
        TimEventManager.StartListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.GetLastMsg, OnTimSdkMessage);
    }

    void OnDisable()
    {
        TimEventManager.StopListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.GetLastMsg, OnTimSdkMessage);
    }

    IEnumerator Start()
    {
        //等待Tween的动画结束，发起初始化请求
        yield return new WaitForSeconds(TWEEN_TIME);
        StartRequest();
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.GetConversationList:
                {
                    if (obj.code == 0)
                    {
                        //Debug.Log($"<color=green>获取所有会话.成功：{obj.data}</color>");
                    }
                    else
                    {
                        Debug.LogError($"获取所有会话.失败：code={obj.code}, data={obj.data}");
                    }
                }
                break;
            case TimSdkMessage.RecvNewMessages: //收到新消息
                {
                    //TODO: 顶置并显示红点
                }
                break;
            case TimSdkMessage.GetLastMsg:
                {
                    //TODO: 显示灰字，最后一条消息
                    //Debug.Log($"获取最后一条消息成功{obj.data}");
                }
                break;
        }
    }

    void StartRequest()
    {
        dialog = DialogManager.ShowProgressCircular("Loading");
        base.SetAlpha(false);

        // 开启超时检查
        _timeout = StartCoroutine(OnTimeOut());

        // 请求数据
        FriendData.Get.RequestData();
    }

    // 请求结果回调 //1.请求异常/2.没有数据/3.刷新数据
    void OnComplete(string info)
    {
        Debug.Log($"UIWidget info={info} dialog={(dialog != null)}");

        if (dialog != null)
        {
            dialog?.Hide();
        }

        // 解除超时检查
        if (_timeout != null)
        {
            StopCoroutine(_timeout);
            _timeout = null;
        }
    }

    // 请求超时
    IEnumerator OnTimeOut()
    {
        yield return new WaitForSeconds(HTTP_TIME_OUT);
        dialog?.Hide();
        yield return new WaitForSeconds(0.3f);
        ToastManager.Show("请求超时，请稍后再试");
        _timeout = null;
    }
}
