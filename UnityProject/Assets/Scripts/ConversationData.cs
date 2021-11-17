using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;

namespace Client
{
    public class ConversationData : MonoBehaviour
    {
        static ConversationData instance = null;
        public static ConversationData Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<ConversationData>();
                }
                return instance;
            }
        }

        public List<TIMUserProfileExt> mItemDataList = new List<TIMUserProfileExt>();
        public int TotalItemCount { get { return mItemDataList.Count; } }
        public int page = 0;

        System.Action mOnRefreshFinished = null;
        int mTotalDataCount = 5;
        float mDataRefreshLeftTime = 0;
        bool mIsWaittingRefreshData = false;

        System.Action mOnLoadMoreFinished = null;
        int mLoadMoreCount = 5;
        float mDataLoadLeftTime = 0;
        bool mIsWaitLoadingMoreData = false;

        public LoopListView2 mLoopListView;
        LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mLoadingTipItemHeight = 50;

        void Awake()
        {
            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;

            Reset();
        }

        void Update()
        {
            if (mIsWaittingRefreshData)
            {
                mDataRefreshLeftTime -= Time.deltaTime;
                if (mDataRefreshLeftTime <= 0)
                {
                    mIsWaittingRefreshData = false;
                    DoRefreshDataSource();
                    mOnRefreshFinished?.Invoke();
                }
            }
            if (mIsWaitLoadingMoreData)
            {
                mDataLoadLeftTime -= Time.deltaTime;
                if (mDataLoadLeftTime <= 0)
                {
                    mIsWaitLoadingMoreData = false;
                    DoLoadMoreDataSource();
                    mOnLoadMoreFinished?.Invoke();
                }
            }
        }

        //初始化
        void Reset()
        {
            mItemDataList.Clear();
            mLoopListView.InitListView(1, OnGetItemByIndex);
        }

        //刷新第一页
        public void LoadMore()
        {
            DoLoadMoreDataSource();
            mLoopListView.SetListItemCount(TotalItemCount + 1, false); //loading view不算ListItem
            mLoopListView.RefreshAllShownItem();
        }

        #region Model

        public TIMUserProfileExt GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        public TIMUserProfileExt GetItemDataById(int itemId)
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (int.Parse(mItemDataList[i].identifier) == itemId)
                {
                    return mItemDataList[i];
                }
            }
            return null;
        }

        public void RequestRefreshDataList(System.Action onReflushFinished)
        {
            mDataRefreshLeftTime = 1;
            mOnRefreshFinished = onReflushFinished;
            mIsWaittingRefreshData = true;
        }

        public void RequestLoadMoreDataList(int loadCount, System.Action onLoadMoreFinished)
        {
            mLoadMoreCount = loadCount;
            mDataLoadLeftTime = 1;
            mOnLoadMoreFinished = onLoadMoreFinished;
            mIsWaitLoadingMoreData = true;
        }

        public void RemoveData(int index)
        {
            mItemDataList.RemoveAt(index);
        }

        public void InsertData(int index, TIMUserProfileExt data)
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
                item = listView.NewListViewItem("ItemPrefab0");
                UpdateLoadingTip(item);
                return item;
            }

            var itemData = GetItemDataByIndex(index);
            if (itemData == null)
            {
                return null;
            }
            item = listView.NewListViewItem("ItemPrefab1");
            var itemScript = item.GetComponent<ConversationListItem>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //itemScript.Init();
            }
            if (index == TotalItemCount - 1)
            {
                item.Padding = 0;
            }
            itemScript.SetItemData(itemData, index);
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
            RequestLoadMoreDataList(mLoadMoreCount, OnDataSourceLoadMoreFinished);
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
                mLoopListView.SetListItemCount(TotalItemCount + 1, false);
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

        public void DoRefreshDataSource()
        {
            mItemDataList.Clear();
            for (int i = 0; i < mTotalDataCount; ++i)
            {
                var tData = new TIMUserProfileExt();
                tData.identifier = $"{i}";
                tData.nickName = "Item" + i;
                tData.lastMsg = "Item Desc For Item " + i;
                tData.amount = Random.Range(20, 999);
                //tData.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                //System.DateTime tm = new System.DateTime(Random.Range(2019, 2021), Random.Range(3, 5), Random.Range(1, 8), Random.Range(1, 11), 0, 0);
                System.DateTime tm = new System.DateTime(2020, 4, 6, 12, 0, 0);
                tData.timestamp = Utils.ToTimestamp(tm);
                tData.unreadMessageNum = Random.Range(0, 3);
                mItemDataList.Add(tData);
            }
        }

        public void DoLoadMoreDataSource()
        {
            int count = mItemDataList.Count;
            for (int k = 0; k < mLoadMoreCount; ++k)
            {
                int i = k + count;
                var tData = new TIMUserProfileExt();
                tData.identifier = $"{i}";
                tData.nickName = "Item" + i;
                tData.lastMsg = "Item Desc For Item " + i;
                tData.amount = Random.Range(20, 999);
                //tData.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                System.DateTime tm = new System.DateTime(Random.Range(2019, 2021), Random.Range(3, 5), Random.Range(1, 8), Random.Range(1, 11), 0, 0);
                tData.timestamp = Utils.ToTimestamp(tm);
                tData.unreadMessageNum = Random.Range(0, 3);
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
        }

        #endregion
    }
}