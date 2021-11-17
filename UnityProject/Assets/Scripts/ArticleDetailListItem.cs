using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using SuperScrollView;

namespace Client
{
    /// <summary>
    /// 文章详情
    /// </summary>
    public class ArticleDetailListItem : MonoBehaviour
    {
        public RectTransform rect;
        public LoopListViewItem2 item2;

        public ArticleItemData mItemData;
        public int mItemDataIndex = -1;

        [SerializeField] private Sprite[] starSprites;
        [SerializeField] private RawImage mHeadImage;
        [SerializeField] private Text mNickText;
        [SerializeField] private Text mTimeText;
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
        [Header("音频")]
        [SerializeField] private Image soundImage;
        [Header("视频")]
        [SerializeField] private RawImage mediaRawImage;
        [SerializeField] private RawImage coverRawImage;
        [SerializeField] private AspectRatioFitter mediaAspect;
        [SerializeField] private MediaPlayerCtrl mediaPlayer;
        //[SerializeField] private CanvasGroup mediaCanvasGroup;
        [SerializeField] private Button m_PlayBtn;
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
        [SerializeField] private RectTransform mTopicGroup;     //30 //fixed
        [SerializeField] private RectTransform mShareGroup;     //30 //fixed //永远存在

        void Awake()
        {
            rect = gameObject.GetComponent<RectTransform>();
            item2 = gameObject.GetComponent<LoopListViewItem2>();

            //mediaCanvasGroup.alpha = 0;
            //mediaCanvasGroup.interactable = false;
            //mediaCanvasGroup.blocksRaycasts = false;
            mediaPlayer.OnReady = () =>
            {
                Debug.Log($"OnReady status={mediaPlayer.GetCurrentState()}"); //NOT_READY
            };
            mediaPlayer.OnResize = () =>
            {
                Debug.Log($"OnResize status={mediaPlayer.GetCurrentState()}"); //PLAYING
                mediaRawImage.uvRect = new Rect(0, 1, 1, -1); //视频的Rect
                mediaRawImage.transform.localScale = Vector3.one;
            };
            mediaPlayer.OnVideoBuffering = () =>
            {
                Debug.Log($"OnVideoBuffering status={mediaPlayer.GetCurrentState()}, perc={mediaPlayer.GetCurrentSeekPercent()}%"); //PLAYING //END,100%
            };
            mediaPlayer.OnVideoFirstFrameReady = () =>
            {
                Debug.Log($"OnVideoFirstFrameReady status={mediaPlayer.GetCurrentState()}"); //PLAYING
                mediaRawImage.uvRect = new Rect(0, 1, 1, -1); //视频的Rect
                mediaRawImage.transform.localScale = Vector3.one;
                //mediaCanvasGroup.alpha = 1;
                //mediaCanvasGroup.interactable = true;
                //mediaCanvasGroup.blocksRaycasts = true;
                coverRawImage.gameObject.SetActive(false);
            };
            mediaPlayer.OnVideoBufferingEnd = () =>
            {
                Debug.Log($"OnVideoBufferingEnd  status={mediaPlayer.GetCurrentState()}, perc={mediaPlayer.GetCurrentSeekPercent()}%"); //PLAYING
                Debug.Log($"rect={mediaRawImage.uvRect}");
            };
            mediaPlayer.OnEnd = () =>
            {
                Debug.Log($"OnVideoBufferingEnd  status={mediaPlayer.GetCurrentState()}, perc={mediaPlayer.GetCurrentSeekPercent()}%"); //END,100%
                //mediaPlayer.Stop();
                m_PlayBtn.gameObject.SetActive(true);
            };

            mStarBtn.onClick.AddListener(CmdStar);
            mCommentBtn.onClick.AddListener(CmdComment);
            mShareBtn.onClick.AddListener(CmdShare);

            m_PlayBtn.onClick.AddListener(OnPlayVideoBtnClick);
        }

        public void SetItemData(ArticleItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;

            mNickText.text = itemData.user.nickname;
            mTimeText.text = Utils.ConvertTimestamp(itemData.publishTime / 1000).ToString();
            FileManager.Download(UserManager.Instance.localPlayer.faceUrl, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mHeadImage));
            mContentText.text = itemData.content;
            mLocationText.text = itemData.addr;
            mStarCountText.text = itemData.praiseCnt.ToString();
            mStarIcon.sprite = starSprites[itemData.praised ? 1 : 0];
            mCommentCountText.text = itemData.commentCnt.ToString();

            OnExpandBtnClicked();
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

            Print();
        }

        // 播放视频
        public void OnPlayVideoBtnClick()
        {
            Debug.Log($"{mediaPlayer.GetCurrentState()}，播放");

            m_PlayBtn.gameObject.SetActive(false);
            mediaPlayer.Play();
        }

        // 动态扩展
        [ContextMenu("OnExpandBtnClicked")]
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
                if (mItemData.pics != null && mItemData.pics.Length == 1)
                {
                    //单图显示
                    FileManager.Download(mItemData.pics[0], (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mSingleImage));
                    //Debug.Log($"{mSingleImage.texture.width}*{mSingleImage.texture.height}");
                    var m_AspectRatio = mSingleImage.GetComponent<AspectRatioFitter>();
                    if (m_AspectRatio != null)
                    {
                        //小的控制大的
                        float ratio = 0;
                        m_AspectRatio.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                        ratio = (float)mSingleImage.texture.width / (float)mSingleImage.texture.height;
                        //m_Thumbnail.rectTransform.sizeDelta = new Vector2(78, 78);
                        m_AspectRatio.aspectRatio = ratio;
                    }
                }
                Height += (imageActive ? mImageGroup.rect.height : 0);

                bool imagesActive = (mItemData.pics != null && mItemData.pics.Length > 1);
                mImagesGroup.gameObject.SetActive(imagesActive);
                if (mItemData.pics != null && mItemData.pics.Length > 1)
                {
                    //多图显示
                    for (int i = 0; i < mMultiImages.Length; i++)
                    {
                        if (i >= mItemData.pics.Length)
                        {
                            mMultiImages[i].gameObject.SetActive(false);
                        }
                        else
                        {
                            mMultiImages[i].gameObject.SetActive(true);
                            FileManager.Download(mItemData.pics[i], (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mMultiImages[i]));
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

                bool videoActive = (!string.IsNullOrEmpty(mItemData.dataUrl) && !string.IsNullOrEmpty(mItemData.cover));
                mVideoGroup.gameObject.SetActive(videoActive);
                if (videoActive)
                {
                    //Debug.Log($"视频封面 {mItemData.cover}");
                    FileManager.Download(mItemData.cover, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, coverRawImage, () =>
                    {
                        //Debug.Log($"{mSingleImage.texture.width}*{mSingleImage.texture.height}");
                        if (mediaAspect != null)
                        {
                            mediaAspect.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                            float ratio = (float)coverRawImage.texture.width / (float)coverRawImage.texture.height;
                            mediaAspect.aspectRatio = ratio;
                            //mediaRawImage.uvRect = new Rect(0, 0, 1, 1); //图片的Rect
                            //mediaRawImage.transform.localScale = Vector3.one;
                        }
                    }));
                    mediaPlayer.m_strFileName = mItemData.dataUrl;
                }
                Height += (videoActive ? mVideoGroup.rect.height : 0);

                bool locActive = (!string.IsNullOrEmpty(mItemData.addr));
                mLocGroup.gameObject.SetActive(locActive);
                Height += (locActive ? mLocGroup.rect.height : 0);

                bool topicActive = (!string.IsNullOrEmpty(mItemData.topics));
                mTopicGroup.gameObject.SetActive(topicActive);
                Height += (topicActive ? mTopicGroup.rect.height : 0);

                //share
                mShareGroup.gameObject.SetActive(true);
                Height += 30;
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
            item2.ParentListView.OnItemSizeChanged(item2.ItemIndex);
        }

        [ContextMenu("Print")]
        void Print()
        {
            //Debug.Log($"text={mTextGroup.rect.height}, image={mImageGroup.rect.height}, images={mImagesGroup.rect.height}," +
            //    $" loc={mLocGroup.rect.height}, topic={mTopicGroup.rect.height}, share={mShareGroup.rect.height}" +
            //    $" Height={Height} Sum={mTextGroup.rect.height+ mImageGroup.rect.height+ mImagesGroup.rect.height + mLocGroup.rect.height + mTopicGroup.rect.height + mShareGroup.rect.height}");

            Debug.Log($"rect0={mediaRawImage.uvRect}");
            //mediaRawImage.uvRect = new Rect(0, 1, 1, -1); //视频的Rect
            mediaRawImage.uvRect = new Rect(0, 0, 1, 1); //视频的Rect
            mediaRawImage.transform.localScale = Vector3.one;
            Debug.Log($"rect1={mediaRawImage.uvRect}");
        }
    }
}
