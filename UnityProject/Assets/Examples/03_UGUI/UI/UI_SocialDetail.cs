using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Client;
using LitJson;
using DG.Tweening;

/// <summary>
/// 文章详情
/// </summary>
public class UI_SocialDetail : UIWidget
{
    public static UI_SocialDetail Instance;

    public int cid = 0; //点击输入框0，点击楼层传入楼的_id
    [SerializeField] private ArticleItemData mItemData;
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button mMoreBtn;
    [SerializeField] private InputField m_TextInput;
    [SerializeField] private RectTransform m_InputRect;
    [SerializeField] private Button m_SendBtn;
    [SerializeField] private Button m_EmojiBtn;

    void Awake()
    {
        Instance = this;

        base.SetAlpha(false);
        m_EmojiBtn.gameObject.SetActive(false);
        m_SendBtn.gameObject.SetActive(false);
        m_InputRect = m_TextInput.GetComponent<RectTransform>();
        m_InputRect.sizeDelta = new Vector2(328, 39);
        m_SendBtn.onClick.AddListener(CmdSend);
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        mMoreBtn.onClick.AddListener(OnMoreBtnClick);
        m_TextInput.onValueChanged.AddListener(OnInputChanged);
    }

    public void Init(ArticleItemData itemData)
    {
        this.mItemData = itemData;
        //Debug.Log($"文章id={itemData.id}");

        // 文章详情
        SocialDetailData.Get.LoadArticle(itemData);
        if (this.mItemData.commentCnt == 0)
        {
            //显示"目前还没有评论哦"
            Debug.Log("目前还没有评论哦");
        }
        else
        {
            //请求评论
            SocialDetailData.Get.RequestData(itemData.id);
        }

        StartCoroutine(OnRefresh());
    }

    IEnumerator OnRefresh()
    {
        //TODO: FadeIn，dial.hide()
        yield return new WaitForSeconds(TWEEN_TIME);
        SocialDetailData.Get.mLoopListView.RefreshAllShownItem();
        base.FadeIn();
    }

    void OnMoreBtnClick()
    {
        Debug.Log("更多");
    }

    void OnInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            m_EmojiBtn.gameObject.SetActive(false);
            m_SendBtn.gameObject.SetActive(false);
            m_InputRect.sizeDelta = new Vector2(328, 39);
        }
        else
        {
            m_EmojiBtn.gameObject.SetActive(true);
            m_SendBtn.gameObject.SetActive(true);
            m_InputRect.sizeDelta = new Vector2(255, 39);
        }
    }

    void CmdSend()
    {
        Debug.Log($"{m_TextInput.text}, aid={mItemData.id}, cid={0}");

        HttpManager.commentReply(mItemData.id, cid, m_TextInput.text, "", onCommentReply);
    }

    void onCommentReply(string json)
    {
        Debug.Log(json);

        var obj = JsonMapper.ToObject<HttpCallback<ArticleCommentReply>>(json);
        if (obj.code == 0)
        {
            //发表成功
            m_TextInput.text = string.Empty;
            OnKeyboardInvisible();

            //TODO: 刷新楼层
            SocialDetailItemData data = new SocialDetailItemData();
            data.mId = 1;
            data.mName = "插入评论";
            data.comment = new CommentItemData();
            //
            //public string[] replies { get; set; }	//NULL
            //public string content { get; set; }		//评论内容
            //public int total { get; set; }			//
            //public string nickname { get; set; }	//评论者昵称
            //public long uid { get; set; }            //评论者
            //public long createTime { get; set; }    //发布时间
            //public int baseId { get; set; }			//服务器优化用的字段
            //public bool praised { get; set; }		//我是否点赞
            //public string avatarUrl { get; set; }	//评论者头像
            //public int commentId { get; set; }		
            //public int praiseCount { get; set; }	//点赞数
            //public long replyCid { get; set; }		//该评论回复的对象

	        //public long _id { get; set; }			//评论唯一id
	        //public long updateTime { get; set; }	//发布时间？
	        //public int aid { get; set; }			//文章id
	        //public int type { get; set; }			//默认1，文字
	        //public string content { get; set; }		//评论内容
	        //public long userId { get; set; }		//评论者？
	        //public int status { get; set; }			//评论状态
	        //public long adminId { get; set; } 
	        //public int praiseCnt { get; set; }		//点赞数
	        //public long createTime { get; set; }	//发布时间？
	        //public int baseId { get; set; }			//服务器优化用的字段
	        //public long cid { get; set; }			//接楼id
	        //public long[] atUsers { get; set; }		//@（没有）
	        //public string pics { get; set; }		//图片（没有）
	        //public int replyCnt { get; set; }		//回复数

            data.comment.replies = null;
            data.comment.content = obj.data.content;
            data.comment.total = 0;//
            data.comment.nickname = UserManager.Instance.localPlayer.nickName;
            data.comment.uid = obj.data.userId;
            data.comment.createTime = obj.data.createTime;
            data.comment.baseId = obj.data.baseId;
            data.comment.praised = false;
            data.comment.avatarUrl = UserManager.Instance.localPlayer.faceUrl;
            data.comment.commentId = 1;//
            data.comment.praiseCount = obj.data.praiseCnt;
            data.comment.replyCid = obj.data.cid;
            SocialDetailData.Get.InsertData(1, data);
            SocialDetailData.Get.mLoopListView.SetListItemCount(SocialDetailData.Get.TotalItemCount + 1, false); //loading view不算ListItem
            //SocialDetailData.Get.mLoopListView.RefreshAllShownItem();
            SocialDetailData.Get.mLoopListView.MovePanelToItemIndex(0, 0); //跳到该元素
        }
    }

    #region 功能键盘

    [Header("Keyboard")]
    [SerializeField] private GameObject m_Mask;
    [SerializeField] private Transform keyboard;
    [SerializeField] private CanvasGroup m_emojiLayer;
    private bool isBoxRisen;
    private bool isEmoji;

    // Inspector中给Input添加EventTrigger绑定
    public void OnInputFieldClick()
    {
        cid = 0;

        //isEmoji = false;
        //m_emojiLayer.alpha = 0;
        //m_emojiLayer.interactable = false;
        //m_emojiLayer.blocksRaycasts = false;

        if (m_TextInput.isFocused && isBoxRisen == false) //TouchScreenKeyboard.visible
        {
            OnKeyboardVisible();
        }
        else if (!m_TextInput.isFocused && isBoxRisen == true) //TouchScreenKeyboard.visible
        {
            OnKeyboardInvisible();
        }
    }

    public void OnKeyboardVisible()
    {
        //Debug.Log("开启");
        if (!isBoxRisen)
        {
            m_Mask.SetActive(true);
            keyboard.DOLocalMoveY(270, 0.2f);
            //if (mContentFiller.listScrollRect.content.rect.height > 300)
            //    mContentFiller.listScrollRect.transform.DOLocalMoveY(270, 0.2f);
            isBoxRisen = true;
        }
    }
    // Inspector中给Mask添加EventTrigger绑定
    public void OnKeyboardInvisible()
    {
        //Debug.Log("关闭");
        if (isBoxRisen == true)
        {
            m_Mask.SetActive(false);
            keyboard.DOLocalMoveY(0, 0.2f);
            //mContentFiller.listScrollRect.transform.DOLocalMoveY(0, 0.2f);
            isBoxRisen = false;
        }
    }

    #endregion
}
