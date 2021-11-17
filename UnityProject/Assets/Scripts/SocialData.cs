using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using LitJson;
using MaterialUI;

namespace Client
{
    public class SocialData : MonoBehaviour
    {
        static SocialData instance = null;
        public static SocialData Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<SocialData>();
                }
                return instance;
            }
        }

        public List<ArticleItemData> mItemDataList = new List<ArticleItemData>();
        public int TotalItemCount { get { return mItemDataList.Count; } }
        private int size = 10; //自定义每页条目数
        private int curPage = 0;
        private int totalPage = 1;

        System.Action mOnLoadMoreFinished = null;
        float mDataLoadLeftTime = 0;
        public bool mIsWaitLoadingMoreData = false;

        public LoopListView2 mLoopListView;
        public ArticleListItemSticky mStickeyHeadItem;
        LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mLoadingTipItemHeight = 50;

        public Dictionary<int, ArticleItemData> articles { get; set; }

        void Awake()
        {
            articles = new Dictionary<int, ArticleItemData>();

            mItemDataList.Clear();
            mLoopListView.InitListView(1, OnGetItemByIndex);
            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;
            mLoopListView.ScrollRect.onValueChanged.AddListener((Vector2 pos) => UpdateStickeyHeadPos()); //监听滚动
            UpdateStickeyHeadPos();
        }

        void Update()
        {
            //上拉加载
            if (mIsWaitLoadingMoreData)
            {
                mDataLoadLeftTime -= Time.deltaTime;
                if (mDataLoadLeftTime <= 0)
                {
                    //Debug.Log("上拉加载等待时间到，刷新视图" + (mOnLoadMoreFinished != null));

                    mIsWaitLoadingMoreData = false;
                    // 基于数据刷新视图
                    mLoopListView.SetListItemCount(TotalItemCount + 1, false);
                    mOnLoadMoreFinished?.Invoke();
                }
            }
        }

        #region Model

        public ArticleItemData GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        public ArticleItemData GetItemDataById(int itemId)
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
            //Debug.Log("松手，发起网络请求");
            RequestData(0);
        }

        public void RemoveData(int index)
        {
            mItemDataList.RemoveAt(index);
        }

        public void InsertData(int index, ArticleItemData data)
        {
            mItemDataList.Insert(index, data);
        }

        #endregion

        #region View

        void UpdateStickeyHeadPos()
        {
            //置顶元素的index，该元素的y超过sticky的位置，sticky就显示，反之隐藏
            //Debug.Log(Mathf.RoundToInt(mLoopListView.ContainerTrans.anchoredPosition.y)); //>0号元素的高度，就置顶。
            bool stick = mLoopListView.ContainerTrans.anchoredPosition.y > 180;
            mStickeyHeadItem.gameObject.SetActive(stick); //滑动太快过会消失
        }

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
                //Debug.Log($"1=>表白墙 id={itemData.id}");
                item = listView.NewListViewItem("ItemPrefab1"); //表白墙
                var itemScript = item.GetComponent<ArticleListItemWall>();
                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                }
                itemScript.SetItemData(itemData, index);
            }
            else if (itemData.mId == 1)
            {
                //Debug.Log($"2=>导航栏 id={itemData.id}");
                item = listView.NewListViewItem("ItemPrefab2"); //导航栏
                var itemScript = item.GetComponent<ArticleListItemSticky>();
                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                }
                itemScript.SetItemData(itemData, index);
            }
            else
            {
                item = listView.NewListViewItem("ItemPrefab3"); //文章元素
                var itemScript = item.GetComponent<ArticleListItem>();
                if (item.IsInitHandlerCalled == false)
                {
                    item.IsInitHandlerCalled = true;
                }
                itemScript.SetItemData(itemData, index);
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
                //Debug.Log("加载结束");
            }
        }

        void OnJumpBtnClicked(int itemIndex)
        {
            if (itemIndex < 0) return;
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }

        #endregion

        #region Controller

        // 刷新表白墙和置顶导航
        public void LoadReset()
        {
            mItemDataList.Clear();
            for (int i = 0; i < 2; ++i)
            {
                var tData = new ArticleItemData();
                tData.mId = i;
                if (i == 0)
                {
                    tData.mName = "表白墙";
                }
                else if (i == 1)
                {
                    tData.mName = "置顶";
                }
                mItemDataList.Add(tData);
            }
        }

        // 刷新翻页
        private void LoadMore(Article[] list)
        {
            DoLoadMoreDataSource(list);
            Debug.Log($"刷新翻页 {mIsWaitLoadingMoreData}");
            if (!mIsWaitLoadingMoreData)
            {
                mLoopListView.SetListItemCount(TotalItemCount + 1, false);
                mLoopListView.RefreshAllShownItem();
            }
        }

        /// <summary>
        /// 请求数据
        /// </summary>
        /// <param name="type">板块</param>
        public void RequestData(int type)
        {
            //Debug.Log($"<color=blue>请求数据 curPage={curPage}, totalPage={totalPage} skip={(curPage) * size}, limit={size}</color>");
            HttpManager.discover((curPage) * size, size, type, onDiscover);

            ////测试数据tmp
            //Debug.Log("测试数据tmp");
            //var ta = Resources.Load<TextAsset>("social");
            //onDiscover(ta.text);
        }

        public void DoLoadMoreDataSource(Article[] list)
        {
            if (list == null) return;
            int count = mItemDataList.Count;
            for (int k = 0; k < list.Length; ++k)
            {
                int i = k + count;
                var tData = new ArticleItemData();
                tData.mId = i;
                if (i == 0)
                {
                    tData.mName = "表白墙";
                }
                else if (i == 1)
                {
                    tData.mName = "置顶";
                }
                else
                {
                    tData.mName = "帖子";
                }
                tData.setArticle(list[k]);
                mItemDataList.Add(tData);
            }
        }

        #endregion

        #region HTTP回调

        void onDiscover(string json)
        {
            //Debug.Log(json);

            // 解析数据
            var obj = JsonMapper.ToObject<Articles>(json);
            if (obj.list.Length <= 0)
            {
                Debug.Log("没有更多数据");
                ToastManager.Show("没有更多数据", 0.5f, MaterialUIManager.UIRoot);
                //TODO: 此时要初始化表白墙和置顶导航
                HttpManager.OnHttpComplete($"请求回调 page={curPage}");
                return;
            }
            //this.total = obj.total;
            this.curPage = obj.curPage;
            this.totalPage = obj.totalPage;
            Debug.Log($"<color=green>回调数据 curPage={curPage}, totalPage={totalPage} skip={(curPage) * size}, limit={size} {obj.list.Length}篇文章</color>");

            // 缓存数据
            List<Article> tmp = new List<Article>();
            for (int i = 0; i < obj.list.Length; i++)
            {
                ArticleItemData data = new ArticleItemData();
                data.setArticle(obj.list[i]);

                if (!articles.ContainsKey(obj.list[i].id))
                {
                    articles.Add(obj.list[i].id, data);
                    tmp.Add(obj.list[i]);
                }
            }

            // 刷新列表
            LoadMore(tmp.ToArray());

            // 通知委托的Widget
            HttpManager.OnHttpComplete($"请求回调 page={curPage}");
        }

        void onNearby(string json)
        {
            Debug.Log(json);
        }

        void onGroup(string json)
        {
            Debug.Log(json);
        }

        #endregion
    }
}