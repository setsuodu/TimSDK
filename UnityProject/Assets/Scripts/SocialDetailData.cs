using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using LitJson;
using MaterialUI;

namespace Client
{
    //让 ArticleItemData, CommentItemData共存
    [System.Serializable]
    public class SocialDetailItemData
    {
        public int mId;
        public string mName;
        public ArticleItemData article;
        public CommentItemData comment;
        public SocialDetailItemData()
        {
            this.article = new ArticleItemData();
            this.comment = new CommentItemData();
        }
    }

    public class SocialDetailData : MonoBehaviour
    {
        static SocialDetailData instance = null;
        public static SocialDetailData Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<SocialDetailData>();
                }
                return instance;
            }
        }

        public List<SocialDetailItemData> mItemDataList = new List<SocialDetailItemData>();
        public int TotalItemCount { get { return mItemDataList.Count; } }
        private int size = 10; //自定义每页条目数
        private int total = 0;
        private int curPage = 0;
        private int totalPage = 1;

        //没有下拉刷新
        //System.Action mOnRefreshFinished = null;
        //float mDataRefreshLeftTime = 0;
        //bool mIsWaittingRefreshData = false;

        System.Action mOnLoadMoreFinished = null;
        float mDataLoadLeftTime = 0;
        public bool mIsWaitLoadingMoreData = false;

        public LoopListView2 mLoopListView;
        LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mLoadingTipItemHeight = 50;

        public Dictionary<int, CommentItemData> comments { get; set; }

        void Awake()
        {
            comments = new Dictionary<int, CommentItemData>();

            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;

            Reset();
        }

        void Update()
        {
            //if (mIsWaittingRefreshData)
            //{
            //    mDataRefreshLeftTime -= Time.deltaTime;
            //    if (mDataRefreshLeftTime <= 0)
            //    {
            //        mIsWaittingRefreshData = false;
            //        DoRefreshDataSource();
            //        mOnRefreshFinished?.Invoke();
            //    }
            //}

            //上拉加载
            if (mIsWaitLoadingMoreData)
            {
                mDataLoadLeftTime -= Time.deltaTime;
                if (mDataLoadLeftTime <= 0)
                {
                    Debug.Log("上拉加载等待时间到，刷新视图" + (mOnLoadMoreFinished != null));

                    mIsWaitLoadingMoreData = false;
                    // 基于数据刷新视图
                    mLoopListView.SetListItemCount(TotalItemCount + 1, false);
                    mOnLoadMoreFinished?.Invoke();
                }
            }
        }

        // 初始化
        void Reset()
        {
            mItemDataList.Clear();
            mLoopListView.InitListView(1, OnGetItemByIndex);
        }

        // 刷新第一页（文章内容）
        public void LoadArticle(ArticleItemData itemData)
        {
            mItemDataList.Clear();
            var tData = new SocialDetailItemData();
            tData.mId = 0; //列表id
            tData.mName = "文章";
            tData.article.setArticle(itemData);
            mItemDataList.Add(tData);

            mLoopListView.SetListItemCount(1, false); //loading view不算ListItem
            //mLoopListView.RefreshAllShownItem();
        }
        // 刷新翻页
        void LoadComments(CommentItemData[] list)
        {
            DoLoadMoreDataSource(list);
            if (!mIsWaitLoadingMoreData)
                mLoopListView.SetListItemCount(TotalItemCount + 1, false);
            //mLoopListView.RefreshAllShownItem();
        }

        #region Model

        public SocialDetailItemData GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        public SocialDetailItemData GetItemDataById(int itemId)
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mItemDataList[i].mId == itemId)
                {
                    return mItemDataList[i];
                }
            }
            return null;
        }

        public void RequestLoadMoreDataList(int loadCount, System.Action onLoadMoreFinished)
        {
            //size = loadCount;
            mDataLoadLeftTime = 1;
            mOnLoadMoreFinished = onLoadMoreFinished;
            mIsWaitLoadingMoreData = true;

            //发起网络请求
            RequestData(0);
        }

        public void RemoveData(int index)
        {
            mItemDataList.RemoveAt(index);
        }

        public void InsertData(int index, SocialDetailItemData data)
        {
            mItemDataList.Insert(index, data);
        }

        #endregion

        #region View

        // 初始化委托
        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0)
            {
                return null;
            }

            LoopListViewItem2 item = null;

            if (index == TotalItemCount)
            {
                //Debug.Log("0=>加载");
                item = listView.NewListViewItem("ItemPrefab0");
                UpdateLoadingTip(item);
                return item;
            }

            var itemData = GetItemDataByIndex(index);
            if (itemData == null)
            {
                return null;
            }

            if (itemData.mId == 0)
            {
                item = listView.NewListViewItem("ItemPrefab3"); //贴子元素 //动态layout显示异常，作为固定元素
                var itemScript = item.GetComponent<ArticleDetailListItem>();
                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                }
                itemScript.SetItemData(itemData.article, index);
            }
            else 
            {
                item = listView.NewListViewItem("ItemPrefab4");
                var itemScript = item.GetComponent<ArticleCommentListItem>(); //评论元素
                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                }
                itemScript.SetItemData(itemData.comment, index);
            }
            return item;
        }

        void UpdateLoadingTip(LoopListViewItem2 item)
        {
            if (item == null)
            {
                return;
            }
            var itemScript0 = item.GetComponent<ListItem0>();
            if (itemScript0 == null)
            {
                return;
            }
            if (mLoadingTipStatus == LoadingTipStatus.None)
            {
                itemScript0.mRoot.SetActive(false);
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            }
            else if (mLoadingTipStatus == LoadingTipStatus.WaitRelease)
            {
                itemScript0.mRoot.SetActive(true);
                itemScript0.mText.text = "Release to Load More";
                itemScript0.mArrow.SetActive(true);
                itemScript0.mWaitingIcon.SetActive(false);
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight);
            }
            else if (mLoadingTipStatus == LoadingTipStatus.WaitLoad)
            {
                itemScript0.mRoot.SetActive(true);
                itemScript0.mArrow.SetActive(false);
                itemScript0.mWaitingIcon.SetActive(true);
                itemScript0.mText.text = "Loading ...";
                item.CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mLoadingTipItemHeight);
            }
        }

        void OnBeginDrag()
        {

        }
        void OnDraging()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus != LoadingTipStatus.None && mLoadingTipStatus != LoadingTipStatus.WaitRelease)
            {
                return;
            }
            LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(TotalItemCount);
            if (item == null)
            {
                return;
            }
            LoopListViewItem2 item1 = mLoopListView.GetShownItemByItemIndex(TotalItemCount - 1);
            if (item1 == null)
            {
                return;
            }
            float y = mLoopListView.GetItemCornerPosInViewPort(item1, ItemCornerEnum.LeftBottom).y;
            if (y + mLoopListView.ViewPortSize >= mLoadingTipItemHeight)
            {
                if (mLoadingTipStatus != LoadingTipStatus.None)
                {
                    return;
                }
                mLoadingTipStatus = LoadingTipStatus.WaitRelease;
                UpdateLoadingTip(item);
            }
            else
            {
                if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
                {
                    return;
                }
                mLoadingTipStatus = LoadingTipStatus.None;
                UpdateLoadingTip(item);
            }
        }
        void OnEndDrag()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus != LoadingTipStatus.None && mLoadingTipStatus != LoadingTipStatus.WaitRelease)
            {
                return;
            }
            LoopListViewItem2 item0 = mLoopListView.GetShownItemByItemIndex(TotalItemCount);
            if (item0 == null)
            {
                return;
            }
            mLoopListView.OnItemSizeChanged(item0.ItemIndex);
            if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
            {
                return;
            }
            mLoadingTipStatus = LoadingTipStatus.WaitLoad;
            UpdateLoadingTip(item0);

            RequestLoadMoreDataList(size, OnDataSourceLoadMoreFinished);
        }

        void OnDataSourceLoadMoreFinished()
        {
            if (mLoopListView.ShownItemCount == 0)
            {
                return;
            }
            if (mLoadingTipStatus == LoadingTipStatus.WaitLoad)
            {
                mLoadingTipStatus = LoadingTipStatus.None;
                //mLoopListView.SetListItemCount(TotalItemCount + 1, false);
                mLoopListView.RefreshAllShownItem();
            }
        }

        void OnJumpBtnClicked(int itemIndex)
        {
            if (itemIndex < 0) return;
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }

        #endregion

        #region Controller

        // 请求评论
        public void RequestData(int aid)
        {
            //curPage==totalPage，在最后一页
            //skip当前取得的条目数

            //请求数据 skip = 30, limit = 10
            Debug.Log($"<color=blue>请求数据 curPage={curPage}, totalPage={totalPage} skip={(curPage) * size}, limit={size}</color>");
            HttpManager.commentList((curPage) * size, size, aid, onCommentList);


            ////测试数据tmp
            //Debug.Log("测试数据tmp");
            //var ta = Resources.Load<TextAsset>("social");
            //onCommentList(ta.text);
        }

        // 测试数据
        public void DoLoadMoreDataSource()
        {
            int count = mItemDataList.Count;
            Debug.Log($"测试数据 count={count}");

            for (int k = 0; k < size; ++k)
            {
                int i = k + count;
                var tData = new SocialDetailItemData();
                tData.mId = i;
                tData.mName = "评论";
                //{"replies":null,"content":"abc","total":0,"nickname":"圣诞w@@#^sss","uid":2,
                //"createTime":1587451392078,"baseId":0,"praised":false,"avatarUrl":"http://avatar.zd1312.com/def/women_320_320.png","commentId":1,"praiseCount":0,"replyCid":0}
                var comment = new CommentItemData();
                comment.replies = null;
                comment.content = "abc";
                comment.total = 0;
                comment.nickname = "圣诞w@@#^sss";
                comment.uid = 2;
                comment.createTime = 1587451392078;
                comment.baseId = 0;
                comment.praised = false;
                comment.avatarUrl = "http://avatar.zd1312.com/def/women_320_320.png";
                comment.commentId = 1;
                comment.praiseCount = 0;
                comment.replyCid = 0;
                tData.comment = comment;
                mItemDataList.Add(tData);
            }
            //size = mItemDataList.Count;
        }
        // 真实数据
        public void DoLoadMoreDataSource(CommentItemData[] list)
        {
            if (list == null) return;
            int count = mItemDataList.Count;
            for (int k = 0; k < list.Length; ++k)
            {
                int i = k + count;
                var tData = new SocialDetailItemData();
                tData.mId = i;
                tData.mName = "评论";
                tData.comment.setComment(list[k]);
                mItemDataList.Add(tData);
            }
            //size = mItemDataList.Count;
        }

        #endregion

        #region HTTP回调

        void onCommentList(string json)
        {
            Debug.Log(json);
            //{"total":1,"size":10,"url":null,"list":[{"replies":null,"content":"abc","total":0,"nickname":"圣诞w@@#^sss","uid":2,"createTime":1587451392078,"baseId":0,"praised":false,"avatarUrl":"http://avatar.zd1312.com/def/women_320_320.png","commentId":1,"praiseCount":0,"replyCid":0}],"curPage":1,"totalPage":1}

            // 解析数据
            var obj = JsonMapper.ToObject<CommentList>(json);
            if (obj.list.Length <= 0)
            {
                Debug.Log("没有更多数据");
                ToastManager.Show("没有更多数据", 0.5f, MaterialUIManager.UIRoot);
                return;
            }
            this.total = obj.total;
            this.curPage = obj.curPage;
            this.totalPage = obj.totalPage;
            Debug.Log($"<color=green>回调数据 curPage={curPage}, totalPage={totalPage} skip={(curPage) * size}, limit={size} {obj.list.Length}篇评论</color>");

            // 缓存数据
            List<CommentItemData> tmp = new List<CommentItemData>();
            for (int i = 0; i < obj.list.Length; i++)
            {
                CommentItemData data = new CommentItemData();
                data.setComment(obj.list[i]);

                if (!comments.ContainsKey(obj.list[i].commentId))
                {
                    comments.Add(obj.list[i].commentId, data);
                    tmp.Add(data);
                }
            }

            // 刷新列表
            LoadComments(tmp.ToArray());
        }

        #endregion
    }
}