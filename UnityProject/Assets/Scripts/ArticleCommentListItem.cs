using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using SuperScrollView;

namespace Client
{
    /// <summary>
    /// 列表数据 //http请求返回的数据
    /// </summary>
    [System.Serializable]
    public class CommentItemData : Comment
    {
        public int mId; //实例化标记位
        public string mName;
        public Comment setComment(Comment comment)
        {
            this.replies = comment.replies;
            this.content = comment.content;
            this.total = comment.total;
            this.nickname = comment.nickname;
            this.uid = comment.uid;
            this.createTime = comment.createTime;
            this.baseId = comment.baseId;
            this.praised = comment.praised;
            this.avatarUrl = comment.avatarUrl;
            this.commentId = comment.commentId;
            this.praiseCount = comment.praiseCount;
            this.replyCid = comment.replyCid;
            return this;
        }
    }

    /// <summary>
    /// 文章评论
    /// </summary>
    public class ArticleCommentListItem : MonoBehaviour
    {
        public RectTransform rect;
        public LoopListViewItem2 item2;

        public CommentItemData mItemData;
        public int mItemDataIndex = -1;
        public int cid = 0;

        [SerializeField] private Button mSelfBtn;
        [SerializeField] private Sprite[] starSprites;
        [SerializeField] private RawImage mHeadImage;
        private Button mHeadBtn;
        [SerializeField] private Text mNickText;
        [SerializeField] private Text mTimeText;
        [SerializeField] private Text mContentText;
        [SerializeField] private Text mStarCountText;
        [SerializeField] private Image mStarIcon;
        [SerializeField] private Button mStarBtn;
        [Header("动态隐藏")]
        [SerializeField] private float Height;
        [SerializeField] private RectTransform mTextGroup;      //30 //dynamic

        void Awake()
        {
            rect = gameObject.GetComponent<RectTransform>();
            item2 = gameObject.GetComponent<LoopListViewItem2>();

            mHeadBtn = mHeadImage.GetComponent<Button>();
            mHeadBtn.onClick.AddListener(OnHeadBtnClick);
            mSelfBtn.onClick.AddListener(CmdComment);
            mStarBtn.onClick.AddListener(CmdStar);
        }

        public void SetItemData(CommentItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;

            cid = mItemData.commentId;

            mNickText.text = itemData.nickname;
            mTimeText.text = Utils.ConvertTimestamp(itemData.createTime / 1000).ToString();
            FileManager.Download(itemData.avatarUrl, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mHeadImage));
            mContentText.text = itemData.content;
            mStarCountText.text = itemData.praiseCount.ToString();
            mStarIcon.sprite = starSprites[itemData.praised ? 1 : 0];

            OnExpandBtnClicked();
        }

        [ContextMenu("OnExpandBtnClicked")]
        void OnExpandBtnClicked()
        {
            var data = SocialDetailData.Get.GetItemDataByIndex(mItemDataIndex);
            if (data == null) return;

            // 设置子物体组开关
            Height = 60 + mTextGroup.rect.height;

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
            item2.ParentListView.OnItemSizeChanged(item2.ItemIndex);
            //Debug.Log($"index={mItemDataIndex} rect={rect.rect.height}/Height={Height}");
        }

        // 点赞
        public void CmdStar()
        {
            Debug.Log($"点赞评论 commentId={mItemData.commentId}, baseId={mItemData.baseId}");

            if (mItemData.praised)
            {
                //点过赞
                Debug.Log("点过赞了");
                HttpManager.commentUnPraise(cid, onCommentUnPraise);
            }
            else
            {
                //没点过赞
                HttpManager.commentPraise(cid, onCommentPraise);
            }
        }

        void onCommentPraise(string json)
        {
            Debug.Log(json);
            //{"code":0,"msg":"OK","data":null}
            var obj = JsonMapper.ToObject<HttpCallback>(json);
            if (obj.code == 0)
            {
                mItemData.praised = true;
                mItemData.praiseCount += 1;

                mStarIcon.sprite = starSprites[1];
                mStarCountText.text = $"{mItemData.praiseCount}";
            }
        }

        void onCommentUnPraise(string json)
        {
            Debug.Log(json);
            var obj = JsonMapper.ToObject<HttpCallback>(json);
            if (obj.code == 0)
            {
                //取消点赞成功，显示灰色
                mItemData.praised = false;
                mItemData.praiseCount -= 1;

                mStarIcon.sprite = starSprites[0];
                mStarCountText.text = $"{mItemData.praiseCount}";
            }
        }

        // 评论
        public void CmdComment()
        {
            Debug.Log("输入评论...");

            //UI_SocialDetail.Instance.OnInputFieldClick();
            UI_SocialDetail.Instance.OnKeyboardVisible();
        }

        void OnHeadBtnClick()
        {
            Debug.Log($"进入id={mItemData.uid}的个人空间");
        }
    }
}
