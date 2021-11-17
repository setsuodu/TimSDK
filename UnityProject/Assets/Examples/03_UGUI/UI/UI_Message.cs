using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using Client;
using MaterialUI;

public class UI_Message : UIWidget
{
    [SerializeField] private List<TIMFriendPendencyItem> pendencyDataList;
    [SerializeField] private Button m_SearchBtn;
    [SerializeField] private Button m_FriendListBtn;
    [SerializeField] private Button m_SortBtn;
    [SerializeField] private Button m_VisitorsBtn;

    void Awake()
    {
        //prefab = Resources.Load<GameObject>("prefabs/item/item_message");
        pendencyDataList = new List<TIMFriendPendencyItem>();

        m_SearchBtn.onClick.AddListener(CommandSearch);
        m_FriendListBtn.onClick.AddListener(CommandFriendList);
        m_SortBtn.onClick.AddListener(CommandSort);
        m_VisitorsBtn.onClick.AddListener(CommandVisitors);
    }

    void OnEnable()
    {
        TimEventManager.StartListening(TimSdkMessage.GetConversationList, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.DoFriendResponse, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StartListening(TimSdkMessage.SetReadMessage, OnTimSdkMessage);
    }

    void OnDisable()
    {
        TimEventManager.StopListening(TimSdkMessage.GetConversationList, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.DoFriendResponse, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.RecvNewMessages, OnTimSdkMessage);
        TimEventManager.StopListening(TimSdkMessage.SetReadMessage, OnTimSdkMessage);
    }

    void Start()
    {
        RefreshConversation(); //必须在注册监听之后去获取
    }

    //TODO: 小助手/官方消息/聊天
    public void RefreshConversation()
    {
#if UNITY_EDITOR
        string json = "";
        var sendData = new TimCallback(TimSdkMessage.GetConversationList, 0, json);
        var sendJson = JsonMapper.ToJson(sendData);
        TimSdkManager.Instance.JsonCallback(sendJson);
        return;
#endif
        TimSdkManager.Instance.GetConversationList();
    }

    // 初始化列表
    public IEnumerator OnRefresh()
    {
        if (ConversationData.Get.TotalItemCount > 0) yield break;

        DialogProgress dialog = DialogManager.ShowProgressCircular("Loading");

        //请求数据
        base.SetAlpha(false);
        ConversationData.Get.LoadMore();

        //等待ui创建
        yield return new WaitForSeconds(0.5f);
        dialog.Hide();

        //数据加载完成，View淡入
        yield return new WaitForSeconds(0.3f);
        base.FadeIn();
    }

    public override void OnTimSdkMessage(TimCallback obj)
    {
        switch ((TimSdkMessage)obj.msg)
        {
            case TimSdkMessage.GetConversationList:
                {
                    if (obj.code == 0)
                    {
                        Debug.Log($"<color=green>获取所有会话.成功：{obj.data}</color>");

                        //TODO: 会话列表会一直收到
                        StartCoroutine(OnRefresh());
                    }
                    else
                    {
                        Debug.LogError($"获取所有会话.失败：code={obj.code}, data={obj.data}");
                    }
                }
                break;
            case TimSdkMessage.DoFriendResponse: //同意/拒绝好友申请
                {
                    switch (obj.code)
                    {
                        case TIMFriendStatus.TIM_FRIEND_STATUS_SUCC: //0 //同意/拒绝操作成功
                            break;
                        case TIMFriendStatus.TIM_RESPONSE_FRIEND_STATUS_NO_REQ: //30614
                            break;
                        case TIMFriendStatus.TIM_ADD_FRIEND_STATUS_SELF_FRIEND_FULL: //30010
                            break;
                        case TIMFriendStatus.TIM_ADD_FRIEND_STATUS_THEIR_FRIEND_FULL: //30014
                            break;
                        default:
                            break;
                    }
                }
                break;
            case TimSdkMessage.RecvNewMessages: //收到新消息
                {
                    var arr = JsonMapper.ToObject<TIMMessageResp[]>(obj.data);
                    Debug.Log($"收到新消息={arr.Length}条");
                    for (int i = 0; i < arr.Length; i++)
                    {
                        TIMChatMsg msg = new TIMChatMsg();
                        msg.seq = arr[i].seq;
                        msg.rand = arr[i].rand;
                        msg.timestamp = arr[i].timestamp;
                        msg.isSelf = arr[i].isSelf;
                        msg.isRead = arr[i].isRead;
                        msg.isPeerReaded = arr[i].isPeerReaded;
                        msg.elemType = arr[i].elemType;
                        msg.subType = arr[i].subType;
                        msg.text = arr[i].text;
                        msg.param = arr[i].param;

                        switch ((TIMElemType)msg.elemType)
                        {
                            case TIMElemType.Text:
                                {
                                    Debug.Log($"Text msg={msg.text}");
                                }
                                break;
                            case TIMElemType.Image:
                                {
                                    Debug.Log($"Image sub={msg.subType} text={msg.text} param={msg.param}");
                                }
                                break;
                            case TIMElemType.SNSTips:
                                {
                                    Debug.Log($"SNS subType={msg.subType}");
                                    switch (msg.subType)
                                    {
                                        case TIMSNSSystemType.INVALID: //0 //无效的消息
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND: //1 //与某人成为了好友
                                            {

                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND: //2 //与某人解除了好友
                                            {

                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND_REQ: //3 //收到好友申请
                                            {
                                                var array = JsonMapper.ToObject<TIMFriendPendencyInfo[]>(msg.param);
                                                for (int t = 0; t < array.Length; t++)
                                                {
                                                    Debug.Log($"未决[{t}]: fromUser={array[t].fromUser}, addSource={array[t].addSource}, fromUserNickName={array[t].fromUserNickName}, addWording={array[t].addWording}");
                                                    TIMFriendPendencyItem item = new TIMFriendPendencyItem();
                                                    item.identifier = array[t].fromUser;
                                                    item.addSource = array[t].addSource;
                                                    item.nickname = array[t].fromUserNickName;
                                                    item.addWording = array[t].addWording;
                                                    pendencyDataList.Add(item);
                                                }
                                                //RefreshPendency(); //添加元素后，刷新列表
                                            }
                                            break;
                                        case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND_REQ: //4 //拒绝好友申请
                                            {
                                                Debug.Log($"拒绝了申请，与{msg.param}无法成为好友"); //TODO: 从列表中删除该元素
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                            default:
                                Debug.Log("其它类型消息");
                                break;
                        }
                    }
                }
                break;
            case TimSdkMessage.SetReadMessage: //我的消息对方已读
                {

                }
                break;
        }
    }

    void CommandSearch()
    {
        PanelManager.Instance.CreatePanel<UI_Search>(false, true);
    }

    void CommandFriendList()
    {
        PanelManager.Instance.CreatePanel<UI_FriendList>(false, true);
    }

    void CommandSort()
    {
        Debug.Log("消息列表排序");
    }

    void CommandVisitors()
    {
        PanelManager.Instance.CreatePanel<UI_Visitors>(false, true);
    }
}
