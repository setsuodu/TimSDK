using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ChangeItemHeight))]
public class ChangeItemHeightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        ChangeItemHeight demo = (ChangeItemHeight)target;
        if (GUILayout.Button("RequestData"))
        {
            demo.RequestData();
        }
    }
}
#endif

public class ChangeItemHeight : MonoBehaviour
{
    static ChangeItemHeight instance = null;
    public static ChangeItemHeight Get
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindObjectOfType<ChangeItemHeight>();
            }
            return instance;
        }
    }

    public LoopListView2 mLoopListView;
    LoadingTipStatus mLoadingTipStatus = LoadingTipStatus.None;
    float mLoadingTipItemHeight = 100;
    public int page = 0;
    public int size = 5;

    public List<ItemData> mItemDataList = new List<ItemData>();
    public int TotalItemCount { get { return mItemDataList.Count; } }

    System.Action mOnRefreshFinished = null;
    float mDataLoadLeftTime = 0;
    bool mIsWaittingRefreshData = false;

    float mDataRefreshLeftTime = 0;
    bool mIsWaitLoadingMoreData = false;
    System.Action mOnLoadMoreFinished = null;

    void Awake()
    {
        //Reset();
        DoRefreshDataSource();
    }

    void Start()
    {
        // totalItemCount +1 because the last "load more" banner is also a item.
        mLoopListView.InitListView(TotalItemCount + 1, OnGetItemByIndex);
        mLoopListView.mOnBeginDragAction = OnBeginDrag;
        mLoopListView.mOnDragingAction = OnDraging;
        mLoopListView.mOnEndDragAction = OnEndDrag;
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
                mOnLoadMoreFinished.Invoke();
            }
        }
    }

    //初始化
    void Reset()
    {
        //mItemDataList.Clear();
        //mLoopListView.InitListView(1, OnGetItemByIndex);

        //DoRefreshDataSource();
    }

    //刷新第一页
    void LoadMore()
    {
        DoLoadMoreDataSource();
        mLoopListView.SetListItemCount(TotalItemCount + 1, false);
        mLoopListView.RefreshAllShownItem();
    }

    #region Model

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0)
        {
            return null;
        }

        LoopListViewItem2 item = null;

        if (index == TotalItemCount)
        {
            //Debug.Log($"{index}加载");
            item = listView.NewListViewItem("ItemPrefab0");
            UpdateLoadingTip(item);
            return item;
        }

        ItemData itemData = GetItemDataByIndex(index);
        if (itemData == null) return null;

        //Debug.Log($"{index}元素");
        item = listView.NewListViewItem("ItemPrefab1");
        ChangeListItem itemScript = item.GetComponent<ChangeListItem>();
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

    public ItemData GetItemDataByIndex(int index)
    {
        if (index < 0 || index >= mItemDataList.Count)
        {
            return null;
        }
        return mItemDataList[index];
    }

    public ItemData GetItemDataById(int itemId)
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

    public void RequestRefreshDataList(System.Action onReflushFinished)
    {
        mDataRefreshLeftTime = 1;
        mOnRefreshFinished = onReflushFinished;
        mIsWaittingRefreshData = true;
    }

    public void RequestLoadMoreDataList(int loadCount, System.Action onLoadMoreFinished)
    {
        //size = loadCount;
        mDataLoadLeftTime = 1;
        mOnLoadMoreFinished = onLoadMoreFinished;
        mIsWaitLoadingMoreData = true;
    }

    public void SetDataTotalCount(int count)
    {
        size = count;
        DoRefreshDataSource();
    }

    public void RemoveData(int index)
    {
        mItemDataList.RemoveAt(index);
    }

    public void InsertData(int index, ItemData data)
    {
        mItemDataList.Insert(index, data);
    }

    public void CheckAllItem()
    {
        int count = mItemDataList.Count;
        for (int i = 0; i < count; ++i)
        {
            mItemDataList[i].mChecked = true;
        }
    }

    public void UnCheckAllItem()
    {
        int count = mItemDataList.Count;
        for (int i = 0; i < count; ++i)
        {
            mItemDataList[i].mChecked = false;
        }
    }

    public bool DeleteAllCheckedItem()
    {
        int oldCount = mItemDataList.Count;
        mItemDataList.RemoveAll(it => it.mChecked);
        return (oldCount != mItemDataList.Count);
    }

    #endregion

    #region View

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
        LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(TotalItemCount);
        if (item == null)
        {
            return;
        }
        mLoopListView.OnItemSizeChanged(item.ItemIndex);
        if (mLoadingTipStatus != LoadingTipStatus.WaitRelease)
        {
            return;
        }
        mLoadingTipStatus = LoadingTipStatus.WaitLoad;
        UpdateLoadingTip(item);
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
            mLoopListView.SetListItemCount(TotalItemCount + 1, false);
            mLoopListView.RefreshAllShownItem();
        }
    }

    #endregion

    #region Controller

    public void RequestData()
    {
        LoadMore();
    }

    void DoRefreshDataSource()
    {
        mItemDataList.Clear();
        for (int i = 0; i < size; ++i)
        {
            ItemData tData = new ItemData();
            tData.mId = i;
            tData.mName = "Item" + i;
            tData.mDesc = "Item Desc For Item " + i;
            //tData.mIcon = ResManager.Get.GetSpriteNameByIndex(Random.Range(0, 24));
            tData.mStarCount = Random.Range(0, 6);
            tData.mFileSize = Random.Range(20, 999);
            tData.mChecked = false;
            tData.mIsExpand = false;
            mItemDataList.Add(tData);
        }
    }

    void DoLoadMoreDataSource()
    {
        int count = mItemDataList.Count;
        for (int k = 0; k < size; ++k)
        {
            int i = k + count;
            ItemData tData = new ItemData();
            tData.mId = i;
            tData.mName = "Item" + i;
            tData.mDesc = "Item Desc For Item " + i;
            //tData.mIcon = ResManager.Get.GetSpriteNameByIndex(Random.Range(0, 24));
            tData.mStarCount = Random.Range(0, 6);
            tData.mFileSize = Random.Range(20, 999);
            tData.mChecked = false;
            tData.mIsExpand = false;
            mItemDataList.Add(tData);
        }
        //size = mItemDataList.Count;
    }

    void OnJumpBtnClicked(int itemIndex)
    {
        mLoopListView.MovePanelToItemIndex(itemIndex, 0);
    }

    void OnAddItemBtnClicked(int addNum)
    {
        if (mLoopListView.ItemTotalCount < 0)
        {
            return;
        }
        int count = mLoopListView.ItemTotalCount + addNum;
        if (count < 0 || count > ChangeItemHeight.Get.TotalItemCount)
        {
            return;
        }
        mLoopListView.SetListItemCount(count, false);
    }

    void OnSetItemCountBtnClicked(int count)
    {
        if (count < 0 || count > ChangeItemHeight.Get.TotalItemCount)
        {
            return;
        }
        mLoopListView.SetListItemCount(count, false);
    }

    #endregion
}
