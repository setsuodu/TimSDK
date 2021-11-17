using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;
using SuperScrollView;

namespace Client
{
    /// <summary>
    /// 列表数据 //http请求返回的数据
    /// </summary>
    [System.Serializable]
    public class ArticleItemData : Article
    {
        public int mId;
        public string mName;
        public ArticleItemData setArticle(Article _article)
        {
            praised = _article.praised;
            user = _article.user;
            id = _article.id;
            type = _article.type;
            content = _article.content;
            status = _article.status;
            anonymous = _article.anonymous;
            topics = _article.topics;
            top = _article.top;
            dataUrl = _article.dataUrl;
            commentCnt = _article.commentCnt;
            praiseCnt = _article.praiseCnt;
            cover = _article.cover;
            publishTime = _article.publishTime;
            pics = _article.pics;
            essence = _article.essence;
            pri = _article.pri;
            addr = _article.addr;
            return this;
        }
    }

    /// <summary>
    /// 文章元素
    /// </summary>
    public class ArticleListItem : MonoBehaviour
    {
        public RectTransform rect;
        public LoopListViewItem2 item2;
        [SerializeField] protected HttpManager.ContentType type; //1图片，2视频，4纯文字

        public ArticleItemData mItemData;
        public int mItemDataIndex = -1;

        [SerializeField] private Sprite[] starSprites;
        [SerializeField] private Button mSelfBtn;
        [SerializeField] private RawImage mHeadImage;
        [SerializeField] private Text mNickText;
        [SerializeField] private Text mTimeText;
        [SerializeField] private Button mMoreBtn;
        private string[] m_SmallStringList = new string[] { "举报" };
        [SerializeField] private Text mContentText;
        [SerializeField] private Text mLocationText;
        [SerializeField] private Text mStarCountText;
        [SerializeField] private Button mStarBtn;
        [SerializeField] private Image mStarIcon;
        [SerializeField] private Text mCommentCountText;
        [SerializeField] private Button mCommentBtn;
        [SerializeField] private Button mShareBtn;
        [Header("图片")]
        [SerializeField] private RawImage mSingleImage;
        [SerializeField] private RawImage[] mMultiImages;
        [SerializeField] private AspectRatioFitter[] mMultiImagesAspect;
        [Header("音频")]
        [SerializeField] private Image soundImage;
        /*
        [Header("视频")]
        [SerializeField] private RawImage mediaRawImage;
        [SerializeField] private AspectRatioFitter mediaAspect;
        [SerializeField] private MediaPlayerCtrl mediaPlayer;
        [SerializeField] private Button m_PlayBtn;
        */
        [Header("动态隐藏")]
        //所有子Group单独对子物体适应
        [SerializeField] private float Height;
        [SerializeField] private RectTransform mProfileGroup;   //60 //fixed //永远存在
        [SerializeField] private RectTransform mTextGroup;      //30 //dynamic
        [SerializeField] private RectTransform mImageGroup;     //120 //dynamic //单图
        [SerializeField] private RectTransform mImagesGroup;    //250 //dynamic //多图
        [SerializeField] private RectTransform mSoundGroup;     //100 //fixed
        [SerializeField] private RectTransform mVideoGroup;     //100 //fixed
        [SerializeField] private RectTransform mLocGroup;       //30 //fixed
        [SerializeField] private RectTransform mShareGroup;     //30 //fixed //永远存在
        [SerializeField] private RectTransform mTopicGroup;     //30 //fixed

        void Awake()
        {
            rect = gameObject.GetComponent<RectTransform>();
            item2 = gameObject.GetComponent<LoopListViewItem2>();

            mSelfBtn.onClick.AddListener(OnSelfBtnClick);
            mMoreBtn.onClick.AddListener(CmdMore);
            mStarBtn.onClick.AddListener(CmdStar);
            mCommentBtn.onClick.AddListener(CmdComment);
            mShareBtn.onClick.AddListener(CmdShare);

            mSingleImage.GetComponent<Button>().onClick.AddListener(() => OnImageBtnClick(0));
            mMultiImagesAspect = new AspectRatioFitter[mMultiImages.Length];
            for (int i = 0; i < mMultiImages.Length; i++)
            {
                int index = i;
                mMultiImages[index].GetComponent<Button>().onClick.AddListener(() => OnImageBtnClick(index));
                mMultiImagesAspect[i] = mMultiImages[index].GetComponent<AspectRatioFitter>();
            }
        }

        public void SetItemData(ArticleItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;
            type = (HttpManager.ContentType)itemData.type;

            mNickText.text = itemData.user.nickname;
            mTimeText.text = Utils.ConvertTimestamp(itemData.publishTime / 1000).ToString();
            FileManager.Download(UserManager.Instance.localPlayer.faceUrl, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mHeadImage));
            mContentText.text = itemData.content;
            mLocationText.text = itemData.addr;
            mStarCountText.text = itemData.praiseCnt.ToString();
            mStarIcon.sprite = starSprites[itemData.praised ? 1 : 0];
            mCommentCountText.text = itemData.commentCnt.ToString();

            mMoreBtn.gameObject.SetActive(itemData.user.uid != long.Parse(UserManager.Instance.localPlayer.identifier));

            OnExpandBtnClicked();
        }

        void OnSelfBtnClick()
        {
            var script = PanelManager.Instance.CreatePanel<UI_SocialDetail>(false, true);
            script.Init(mItemData);
        }

        // 浏览图片
        void OnImageBtnClick(int index)
        {
            Debug.Log($"查看图片 {index}/{mItemData.pics.Length}");
            var viewer = PanelManager.Instance.CreatePanel<UI_ImageViewer>(false, true);
            viewer.Init(mItemData, index);
        }

        // 更多
        public void CmdMore()
        {
            DialogManager.ShowBottomList(m_SmallStringList, OnChatWhoValue);
        }

        void OnChatWhoValue(int selectedIndex)
        {
            ToastManager.Show($"selected:{m_SmallStringList[selectedIndex]} uid={mItemData.user.uid} ");

            //TODO: 点击事件
            //tmpConfig.chatWho = selectedIndex + 1;
            //m_ChatWhoText.text = m_SmallStringList[selectedIndex]; //1:全部 2:好友 3:自闭
        }

        // 点赞
        public void CmdStar()
        {
            Debug.Log($"点赞文章 aid={mItemData.id}");

            if (mItemData.praised)
            {
                //点过赞
                HttpManager.unPraise(mItemData.id, onUnPraise);
            }
            else
            {
                //没点过赞
                HttpManager.praise(mItemData.id, onPraise);
            }
        }

        void onPraise(string json)
        {
            Debug.Log(json);

            var obj = JsonMapper.ToObject<HttpCallback>(json);
            if (obj.code == 0)
            {
                //点赞成功，显示红色
                mItemData.praised = true;
                mItemData.praiseCnt += 1;

                mStarIcon.sprite = starSprites[1];
                mStarCountText.text = $"{mItemData.praiseCnt}";
            }
        }

        void onUnPraise(string json)
        {
            Debug.Log(json);

            var obj = JsonMapper.ToObject<HttpCallback>(json);
            if (obj.code == 0)
            {
                //取消点赞成功，显示灰色
                mItemData.praised = false;
                mItemData.praiseCnt -= 1;

                mStarIcon.sprite = starSprites[0];
                mStarCountText.text = $"{mItemData.praiseCnt}";
            }
        }

        // 评论
        public void CmdComment()
        {
            Debug.Log("评论");
        }

        // 分享
        public void CmdShare()
        {
            Debug.Log("分享");
        }

        // 动态高度调节
        void OnExpandBtnClicked()
        {
            var data = SocialData.Get.GetItemDataByIndex(mItemDataIndex);
            if (data == null) return;
            //Debug.Log(data.id);


            // 设置子物体组开关
            {
                Height = 0;

                mProfileGroup.gameObject.SetActive(true); //60
                Height += 60;

                bool textActive = !string.IsNullOrEmpty(mItemData.content);
                mTextGroup.gameObject.SetActive(textActive);
                Height += (textActive ? mTextGroup.rect.height : 0);

                bool imageActive = (mItemData.pics != null && mItemData.pics?.Length == 1);
                mImageGroup.gameObject.SetActive(imageActive);
                if (imageActive)
                {
                    //单图显示
                    FileManager.Download(mItemData.pics[0], (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mSingleImage, () =>
                    {
                        //Debug.Log($"{mSingleImage.texture.width}*{mSingleImage.texture.height}");
                        var m_AspectRatio = mSingleImage.GetComponent<AspectRatioFitter>();
                        if (m_AspectRatio != null)
                        {
                            //小的控制大的
                            float ratio = 0;
                            m_AspectRatio.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                            ratio = (float)mSingleImage.texture.width / (float)mSingleImage.texture.height;
                            //Debug.Log($"单图 {mSingleImage.texture.width}x{mSingleImage.texture.height}, ratio={ratio}");
                            m_AspectRatio.aspectRatio = ratio;
                        }
                    }));
                }
                Height += (imageActive ? mImageGroup.rect.height : 0);

                bool imagesActive = (mItemData.pics != null && mItemData.pics.Length > 1);
                mImagesGroup.gameObject.SetActive(imagesActive);
                if (imagesActive)
                {
                    //多图显示
                    //Debug.Log($"多图 length={mItemData.pics.Length}");
                    for (int i = 0; i < mMultiImages.Length; i++) //9
                    {
                        if (i >= mItemData.pics.Length)
                        {
                            mMultiImages[i].transform.parent.gameObject.SetActive(false);
                        }
                        else
                        {
                            mMultiImages[i].transform.parent.gameObject.SetActive(true);
                            FileManager.Download(mItemData.pics[i], (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mMultiImages[i], () =>
                            {
                                //var aspect = mMultiImages[i].GetComponent<AspectRatioFitter>();
                                float ratio = (float)mMultiImages[i].texture.width / (float)mMultiImages[i].texture.height;
                                if (mMultiImages[i].texture.width < mMultiImages[i].texture.height)
                                {
                                    mMultiImagesAspect[i].aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                                }
                                else
                                {
                                    mMultiImagesAspect[i].aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                                }
                                //mMultiImages[i].rectTransform.sizeDelta = new Vector2(78, 78);
                                mMultiImagesAspect[i].aspectRatio = ratio;
                            }));
                        }
                    }

                    //1-3 //84
                    //4-6 //84+78+5=167
                    //7-9 //167+78+5=250
                    if (mItemData.pics.Length > 1 && mItemData.pics.Length <= 3)
                    {
                        mImagesGroup.sizeDelta = new Vector2(mImagesGroup.rect.width, 84);
                    }
                    else if (mItemData.pics.Length > 3 && mItemData.pics.Length <= 6)
                    {
                        mImagesGroup.sizeDelta = new Vector2(mImagesGroup.rect.width, 167);
                    }
                    else if (mItemData.pics.Length > 6 && mItemData.pics.Length <= 9)
                    {
                        mImagesGroup.sizeDelta = new Vector2(mImagesGroup.rect.width, 250);
                    }
                    //Debug.Log($"多图({mItemData.pics.Length})={mImagesGroup.rect.height}");
                }
                Height += (imagesActive ? mImagesGroup.rect.height : 0);

                //bool soundActive = (!string.IsNullOrEmpty(mItemData.dataUrl) && string.IsNullOrEmpty(mItemData.cover));
                bool soundActive = mItemData.type == (int)HttpManager.ContentType.audio;
                mSoundGroup.gameObject.SetActive(soundActive);
                Height += (soundActive ? mSoundGroup.rect.height : 0);

                bool videoActive = (!string.IsNullOrEmpty(mItemData.dataUrl) && !string.IsNullOrEmpty(mItemData.cover));
                mVideoGroup.gameObject.SetActive(videoActive);
                if (videoActive)
                {
                    /*
                    //Debug.Log($"视频封面 {mItemData.cover}");
                    FileManager.Download(mItemData.cover, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mediaRawImage, () =>
                    {
                        //Debug.Log($"{mSingleImage.texture.width}*{mSingleImage.texture.height}");
                        if (mediaAspect != null)
                        {
                            mediaAspect.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                            float ratio = (float)mediaRawImage.texture.width / (float)mediaRawImage.texture.height;
                            mediaAspect.aspectRatio = ratio;
                            mediaRawImage.uvRect = new Rect(0, 0, 1, 1); //图片的Rect
                            mediaRawImage.transform.localScale = Vector3.one;
                        }
                    }));
                    */
                }
                Height += (videoActive ? mVideoGroup.rect.height : 0);

                bool locActive = (!string.IsNullOrEmpty(mItemData.addr));
                mLocGroup.gameObject.SetActive(locActive);
                mShareGroup.gameObject.SetActive(true);
                Height += 30;

                bool topicActive = (!string.IsNullOrEmpty(mItemData.topics));
                mTopicGroup.gameObject.SetActive(topicActive);
                Height += (topicActive ? mTopicGroup.rect.height : 0);
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
            item2.ParentListView.OnItemSizeChanged(item2.ItemIndex);

            //Debug.Log($"index={mItemDataIndex} rect={rect.rect.height}/Height={Height}");
        }

        [ContextMenu("Print")]
        void Print()
        {
            Debug.Log($"类型={mItemData.type}");
        }
    }
}
