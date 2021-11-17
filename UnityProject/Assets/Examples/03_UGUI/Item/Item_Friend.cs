using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Item_Friend : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private TIMUserProfileExt profile;
    [SerializeField] private Image m_HeadImage;
    [SerializeField] private Text m_NickNameText;
    [SerializeField] private Text m_RelationText;
    [SerializeField] private Button m_UnfriendBtn;
    [SerializeField] private Button m_SelfBtn;
    public UnityEvent onLongPress = new UnityEvent();

    void Awake()
    {
        m_UnfriendBtn.onClick.AddListener(Unfriend);
        m_SelfBtn.onClick.AddListener(OpenChat);
        onLongPress.AddListener(OnShow);
    }

    public void Init(TIMUserProfileExt friend)
    {
        this.profile = friend;

        //TODO: 根据 PlayerPrefs 获取时间戳，超时从服务器拉取最新数据。否则走本地。查询用户信息。
        m_NickNameText.text = profile.nickName;
        m_RelationText.text = $"亲密度={profile.amount}";
        FileManager.Download(profile.faceUrl, OnLoadHeadImage);
    }

    void Unfriend() 
    {
        Debug.Log("解除关系");
    }

    void OpenChat()
    {
        var script = PanelManager.Instance.CreatePanel<UI_Chat>(false, true);
        script.Init(profile);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MenuItemClass[] items = new MenuItemClass[]
        {
            new MenuItemClass("标记未读", Action1),
            new MenuItemClass("删除消息", Action2),
        };
        LongPressManager.Init(eventData.position, items);
        Invoke("OnLongPress", 1f); //长按1s
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke("OnLongPress");
    }

    void OnLongPress()
    {
        onLongPress.Invoke();
    }

    public void OnShow()
    {
        LongPressManager.Show();
    }

    void Action1() 
    {
        Debug.Log($"设置未读: {profile.identifier}");
    }

    void Action2()
    {
        Debug.Log($"删除会话: {profile.identifier}");

        bool result = TimSdkManager.Instance.DeleteConversation(profile.identifier, true);
        Debug.Log($"删除{(result ? "成功" : "失败")}");
    }

    void OnLoadHeadImage(byte[] bytes)
    {
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_HeadImage.sprite = sp;
    }
}
