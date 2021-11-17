using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using LitJson;

namespace Client
{
    public class POIData : MonoBehaviour
    {
        static POIData instance = null;
        public static POIData Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<POIData>();
                }
                return instance;
            }
        }

        public List<POI> mItemDataList = new List<POI>();
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
        void LoadMore(POI[] list)
        {
            DoLoadMoreDataSource(list);
            mLoopListView.SetListItemCount(TotalItemCount + 1, false);
            mLoopListView.RefreshAllShownItem();
        }

        #region Model

        public POI GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        public POI GetItemDataById(int itemId)
        {
            int count = mItemDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                if (int.Parse(mItemDataList[i].id) == itemId)
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

        public void InsertData(int index, POI data)
        {
            mItemDataList.Insert(index, data);
        }

        #endregion

        #region View

        public LoopListView2 mLoopListView;
        LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
        float mLoadingTipItemHeight = 50;

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
            var itemScript = item.GetComponent<POIListItem>();
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

        void OnJumpBtnClicked(int itemIndex)
        {
            if (itemIndex < 0) return;
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }

        #endregion

        #region Controller

        //TODO: 好友列表每天只请求一次，存入SQLite
        public void RequestData()
        {
            if (page == 0)
            {
                //TODO: 定位管理器
                double lat = 30.193834; //纬度
                double lng = 120.186414; //经度
                HttpManager.nearAddr(lat, lng, onNearAddr);
            }
        }

        public void RequestSearch(string value)
        {
            //if (page == 0)
            {
                HttpManager.lbsSearch(value, "杭州", 10, 0, onLbsSearch); //TODO: 地址在启动app定位后缓存
            }
        }

        public void DoRefreshDataSource(POI[] list)
        {
            mItemDataList.Clear(); //清理mItemDataList
            mTotalDataCount = list.Length;
            for (int i = 0; i < list.Length; ++i)
            {
                //这里不用转数据
                var tData = list[i];
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
                var tData = new POI();
                tData.id = $"{i}";
                tData.title = "杭州";
                tData.address = "杭州";
                tData.category = "立交桥";
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
        }
        // 真实数据
        public void DoLoadMoreDataSource(POI[] list)
        {
            if (list == null) return;
            //mLoadMoreCount = list.Length;
            //for (int i = 0; i < mLoadMoreCount; ++i)
            for (int i = 0; i < list.Length; ++i)
            {
                //这里不用转数据
                var tData = list[i];
                mItemDataList.Add(tData);
            }
            mTotalDataCount = mItemDataList.Count;
            page += 1; //刷完列表page自增
        }

        #endregion

        #region HTTP回调

        void onNearAddr(string json)
        {
            Debug.Log(json);
            //{"code":0,"msg":"OK","data":[{"id":"695671858669776040","title":"中兴立交桥","address":"浙江省杭州市滨江区","category":"基础设施:道路附属:桥","location":{"lat":30.194752,"lng":120.187042},"ad_info":{"adcode":"330108","province":"","city":"","district":""},"_distance":3,"_dir_desc":"东南"},{"id":"16384111024849558010","title":"中南城第六空间国际建材馆","address":"浙江省杭州市滨江区滨怡路","category":"购物:家具家居建材:建材","location":{"lat":30.196739,"lng":120.189392},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":207,"_dir_desc":"西南"},{"id":"7870995877617174994","title":"杭州市滨江实验小学","address":"浙江省杭州市滨江区平乐街80号","category":"教育学校:小学","location":{"lat":30.194744,"lng":120.182892},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":268,"_dir_desc":"东"},{"id":"7144361010411429618","title":"阿里中心(杭州滨江园区)","address":"浙江省杭州市滨江区滨兴路1866号","category":"房产小区:商务楼宇","location":{"lat":30.192381,"lng":120.190216},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":399,"_dir_desc":"西北"},{"id":"13814529078142314262","title":"江滨一号体育健身中心","address":"浙江省杭州市滨江区滨盛路","category":"运动健身:高尔夫场","location":{"lat":30.198332,"lng":120.183311},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":468,"_dir_desc":"东南"},{"id":"2710237208498113165","title":"神鲸空间","address":"浙江省杭州市滨江区滨兴路1866号阿里中心T2号楼","category":"房产小区:商务楼宇","location":{"lat":30.192396,"lng":120.189415},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":329,"_dir_desc":"西北"},{"id":"1572721794533973773","title":"第六空间家具馆","address":"浙江省杭州市滨江区江南大道1088号","category":"购物:家具家居建材:家具家居","location":{"lat":30.197109,"lng":120.191513},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":481,"_dir_desc":"西南"},{"id":"6041970606523426404","title":"萧山协办江三加油站","address":"浙江省杭州市滨江区滨怡路","category":"汽车:加油站:加油加气站","location":{"lat":30.19235,"lng":120.182449},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":371,"_dir_desc":"东"},{"id":"10339859532603328992","title":"阿里巴巴滨江园区","address":"浙江省杭州市滨江区网商路699号","category":"房产小区:产业园区","location":{"lat":30.189772,"lng":120.190262},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":443,"_dir_desc":"西北"},{"id":"3080352302706326591","title":"阿里巴巴","address":"浙江省杭州市滨江区网商路699号滨江新园区","category":"公司企业:公司企业","location":{"lat":30.18998,"lng":120.191643},"ad_info":{"adcode":"330108","province":"浙江省","city":"杭州市","district":"滨江区"},"_distance":424,"_dir_desc":"西北"}]}

            var obj = JsonMapper.ToObject<HttpCallback<POI[]>>(json);
            if (obj.code == 0)
            {
                Debug.Log($"poi count={obj.data.Length}");
                if (obj.data.Length <= 0)
                {
                    Debug.Log("没有更多数据");
                    HttpManager.OnHttpComplete("委托调用.没有更多数据");
                    return;
                }

                // 刷新列表
                LoadMore(obj.data);

                // 通知委托的Widget
                HttpManager.OnHttpComplete($"委托调用.请求完成 page={page}");
            }
        }

        void onLbsSearch(string json)
        {
            Debug.Log(json);

            //var obj = JsonMapper.ToObject<HttpCallback<POI[]>>(json);
        }

        #endregion
    }
}