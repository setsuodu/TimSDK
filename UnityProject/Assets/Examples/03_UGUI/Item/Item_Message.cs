using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 未决元素
public class Item_Message : MonoBehaviour
{
    private TIMFriendPendencyItem PendencyItem;
    //private string m_HeadUrl;
    //private Texture2D m_HeadTex;
    [SerializeField] private Text m_MessageTitle;
    [SerializeField] private Text m_MessageContent;
    [SerializeField] private Text m_LastTimeText;
    [SerializeField] private Button m_AgreeBtn;
    [SerializeField] private Button m_DenyBtn;

    void Awake()
    {
        var left = transform.Find("LeftPanel");
        var center = transform.Find("CenterPanel");
        var right = transform.Find("RightPanel");
        //m_HeadTex = left.Find("Head").GetComponent<Texture2D>();
        m_MessageTitle = center.Find("MessageTitle").GetComponent<Text>();
        m_MessageContent = center.Find("MessageContent").GetComponent<Text>();
        m_LastTimeText = right.Find("LastTimeText").GetComponent<Text>();
        m_AgreeBtn = transform.Find("AgreeBtn").GetComponent<Button>();
        m_DenyBtn = transform.Find("DenyBtn").GetComponent<Button>();

        m_AgreeBtn.onClick.AddListener(OnAgree);
        m_DenyBtn.onClick.AddListener(OnDeny);
    }

    public void Init(TIMFriendPendencyItem item)
    {
        this.PendencyItem = item;

        m_MessageTitle.text = PendencyItem.identifier;
        m_MessageContent.text = PendencyItem.addWording;
        m_LastTimeText.text = Utils.ConvertTimestamp(PendencyItem.addTime).ToString();
    }

    void OnAgree() 
    {
        Debug.Log("同意");

        TimSdkManager.Instance.DoFriendResponse(PendencyItem.identifier, 1);
    }

    void OnDeny()
    {
        Debug.Log("拒绝");

        TimSdkManager.Instance.DoFriendResponse(PendencyItem.identifier, 2);
    }
}
