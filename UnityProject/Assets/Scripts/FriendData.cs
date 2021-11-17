using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using LitJson;

namespace Client
{
    public class FriendData : MonoBehaviour
    {
        static FriendData instance = null;
        public static FriendData Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<FriendData>();
                }
                return instance;
            }
        }

        public LoopListView2 mLoopListView;
        LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mLoadingTipItemHeight = 50;

        public List<TIMUserProfileExt> mItemDataList = new List<TIMUserProfileExt>();
        public int TotalItemCount { get { return mItemDataList.Count; } }
        public int page = 0;

        System.Action mOnRefreshFinished = null;
        int mTotalDataCount = 5; //初始化数量
        float mDataRefreshLeftTime = 0;
        bool mIsWaittingRefreshData = false; //刷新

        System.Action mOnLoadMoreFinished = null;
        int mLoadMoreCount = 5; //分页增量
        float mDataLoadLeftTime = 0;
        bool mIsWaitLoadingMoreData = false; //加载

        void Awake()
        {
            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;

            Reset();
        }

        void OnDestroy()
        {
            mLoopListView.mOnBeginDragAction = null;
            mLoopListView.mOnDragingAction = null;
            mLoopListView.mOnEndDragAction = null;
        }

        void Update()
        {
            if (mIsWaittingRefreshData)
            {
                mDataRefreshLeftTime -= Time.deltaTime;
                if (mDataRefreshLeftTime <= 0)
                {
                    mIsWaittingRefreshData = false;
                    //DoRefreshDataSource();
                    mOnRefreshFinished?.Invoke();
                }
            }
            if (mIsWaitLoadingMoreData)
            {
                mDataLoadLeftTime -= Time.deltaTime;
                if (mDataLoadLeftTime <= 0)
                {
                    mIsWaitLoadingMoreData = false;
                    //DoLoadMoreDataSource();
                    RequestData();
                    mOnLoadMoreFinished?.Invoke();
                    Debug.Log("拖拽结束");
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
        void LoadMore(MyFriendsItem[] list)
        {
            DoLoadMoreDataSource(list);
            mLoopListView.SetListItemCount(TotalItemCount + 1, false);
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
            var itemScript = item.GetComponent<FriendListItem>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
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
            LoopListViewItem2 item0 = mLoopListView.GetShownItemByItemIndex(TotalItemCount);
            if (item0 == null) return;
            LoopListViewItem2 item1 = mLoopListView.GetShownItemByItemIndex(TotalItemCount - 1);
            if (item1 == null) return;

            float y = mLoopListView.GetItemCornerPosInViewPort(item1, ItemCornerEnum.LeftBottom).y;
            if (y + mLoopListView.ViewPortSize >= mLoadingTipItemHeight)
            {
                if (mLoadingTipStatus != LoadingTipStatus.None) return;
                mLoadingTipStatus = LoadingTipStatus.WaitRelease;
                UpdateLoadingTip(item0);
            }
            else
            {
                if (mLoadingTipStatus != LoadingTipStatus.None) return;
                mLoadingTipStatus = LoadingTipStatus.WaitRelease;
                UpdateLoadingTip(item0);
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

        #endregion

        #region Controller

        //TODO: 好友列表每天只请求一次，存入SQLite
        public void RequestData()
        {
            if (page == 0)
            {
                //string json = "{\"total\":1,\"size\":20,\"url\":null,\"list\":[{\"no\":\"5\",\"avatar\":\"http://avatar.zd1312.com/def/women_320_320.png\",\"r\":3,\"amount\":20,\"uid\":1,\"nickname\":\"用户5\"}],\"curPage\":1,\"totalPage\":1}";
                //onMyFriends(json);
                //return;

                HttpManager.myFriends(page, (page + 1) * 20, onMyFriends);
            }
            else if (page > 0 && page < 4)
            {
                //{"total":1,"size":20,"url":null,"list":[{"no":"5","avatar":"","r":3,"amount":25,"uid":3,"nickname":"用户5"}],"curPage":1,"totalPage":1}

                List<MyFriendsItem> array = new List<MyFriendsItem>();
                for (int i = 0; i < 5; i++)
                {
                    var item = new MyFriendsItem();
                    item.no = $"{mItemDataList.Count + i}";
                    item.avatar = "http://avatar.zd1312.com/def/women_320_320.png";
                    item.r = 3;
                    item.amount = 10;
                    item.uid = mItemDataList.Count + i;
                    item.nickname = $"用户{mItemDataList.Count + i}";
                    array.Add(item);
                }
                MyFriendsCallback cbk = new MyFriendsCallback()
                {
                    total = 1,
                    size = 20,
                    url = null,
                    list = array.ToArray(),
                    curPage = 1,
                    totalPage = 1,
                };
                string json = JsonMapper.ToJson(cbk);
                onMyFriends(json);
            }
            else
            {
                string json = "{\"total\":1,\"size\":20,\"url\":null,\"list\":[],\"curPage\":1,\"totalPage\":1}";
                onMyFriends(json);
            }
        }

        public void DoRefreshDataSource(MyFriendsItem[] list)
        {
            mItemDataList.Clear(); //清理mItemDataList
            mTotalDataCount = list.Length;
            for (int i = 0; i < list.Length; ++i)
            {
                var tData = new TIMUserProfileExt();
                tData.identifier = list[i].no;
                tData.nickName = list[i].nickname;
                tData.faceUrl = list[i].avatar;
                tData.amount = list[i].amount;
                tData.lastMsg = "Item Desc For Item " + i;
                //tData.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                //System.DateTime tm = new System.DateTime(Random.Range(2019, 2021), Random.Range(3, 5), Random.Range(1, 8), Random.Range(1, 11), 0, 0);
                System.DateTime tm = new System.DateTime(2020, 4, 6, 12, 0, 0);
                tData.timestamp = Utils.ToTimestamp(tm);
                tData.unreadMessageNum = Random.Range(0, 3);
                mItemDataList.Add(tData);
            }
            page += 1; //刷完列表page自增
        }

        // 测试数据
        public void DoLoadMoreDataSource()
        {
            int count = mItemDataList.Count;
            for (int k = 0; k < mLoadMoreCount; ++k)
            {
                int i = k + count;
                var tData = new TIMUserProfileExt();
                tData.identifier = $"{i}";
                tData.nickName = "Item" + i;
                tData.faceUrl = "http://avatar.zd1312.com/def/women_320_320.png";
                tData.lastMsg = "Item Desc For Item " + i;
                tData.relation = 3;
                tData.amount = Random.Range(20, 999);
                //tData.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                System.DateTime tm = new System.DateTime(Random.Range(2019, 2021), Random.Range(3, 5), Random.Range(1, 8), Random.Range(1, 11), 0, 0);
                tData.timestamp = Utils.ToTimestamp(tm);
                tData.unreadMessageNum = Random.Range(0, 3);
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
        }
        // 真实数据
        public void DoLoadMoreDataSource(MyFriendsItem[] list)
        {
            if (list == null) return;
            //mLoadMoreCount = list.Length;
            //for (int i = 0; i < mLoadMoreCount; ++i)
            for (int i = 0; i < list.Length; ++i)
            {
                var tData = new TIMUserProfileExt();
                tData.identifier = $"{list[i].uid}";
                tData.nickName = list[i].nickname;
                tData.faceUrl = list[i].avatar;
                tData.relation = list[i].r;
                tData.amount = list[i].amount;
                tData.lastMsg = "Item Desc For Item " + i;
                //tData.timestamp = Utils.ToTimestamp(System.DateTime.Now);
                System.DateTime tm = new System.DateTime(Random.Range(2019, 2021), Random.Range(3, 5), Random.Range(1, 8), Random.Range(1, 11), 0, 0);
                tData.timestamp = Utils.ToTimestamp(tm);
                tData.unreadMessageNum = Random.Range(0, 3);
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
            page += 1; //刷完列表page自增
        }

        void OnJumpBtnClicked(int itemIndex)
        {
            if (itemIndex < 0) return;
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }

        #endregion

        #region HTTP回调

        void onMyFriends(string json)
        {
            Debug.Log(json);

            // 解析数据
            var obj = JsonMapper.ToObject<MyFriendsCallback>(json);
            if (obj.list.Length <= 0)
            {
                Debug.Log("没有更多数据");
                HttpManager.OnHttpComplete("委托调用.没有更多数据");
                return;
            }

            // 缓存数据
            for (int i = 0; i < obj.list.Length; i++)
            {
                TIMUserProfileExt friend = new TIMUserProfileExt()
                {
                    identifier = obj.list[i].uid.ToString(),
                    nickName = obj.list[i].nickname,
                    gender = 1,
                    faceUrl = obj.list[i].avatar,
                    relation = obj.list[i].r,
                    amount = obj.list[i].amount,
                };
                //TimSdkManager.Instance.GetLastMsg(profile.identifier);
                if (!UserManager.Instance.friends.ContainsKey(long.Parse(friend.identifier)))
                    UserManager.Instance.friends.Add(long.Parse(friend.identifier), friend);
            }

            // 刷新列表
            LoadMore(obj.list);

            // 通知委托的Widget
            HttpManager.OnHttpComplete($"委托调用.请求完成 page={page}");
        }

        #endregion
    }
}