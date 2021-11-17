using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SuperScrollView;

namespace Client
{
    [System.Serializable]
    public class ImageViewerItemData
    {
        public int mId;
        public string mName;
        public string mImageUrl;
    }

    public class ImageViewerMgr : MonoBehaviour
    {
        static ImageViewerMgr instance = null;
        public static ImageViewerMgr Get
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<ImageViewerMgr>();
                }
                return instance;
            }
        }

        public LoopListView2 mLoopListView;

        List<ImageViewerItemData> mItemDataList = new List<ImageViewerItemData>();
        public int TotalItemCount { get { return mItemDataList.Count; } }

        public int mTotalDataCount = 9;

        private float _width;
        public int itemIndex = 0;

        void Awake()
        {
            _width = mLoopListView.GetComponent<RectTransform>().rect.width;
            //Debug.Log($"width={_width}");
        }

        public void LoadDataSource(ArticleItemData data, int jumpto)
        {
            this.itemIndex = jumpto;

            mItemDataList.Clear();
            mTotalDataCount = data.pics.Length;
            for (int i = 0; i < mTotalDataCount; ++i)
            {
                int index = i;
                var tData = new ImageViewerItemData();
                tData.mId = index;
                tData.mName = "Item" + index;
                tData.mImageUrl = data.pics[index];
                mItemDataList.Add(tData);
            }
        }

        void Start()
        {
            //Debug.Log($"初始化{TotalItemCount}个");
            mLoopListView.InitListView(TotalItemCount, OnGetItemByIndex);
            mLoopListView.mOnBeginDragAction = OnBeginDrag;
            mLoopListView.mOnDragingAction = OnDraging;
            mLoopListView.mOnEndDragAction = OnEndDrag;

            //Debug.Log($"跳转到{itemIndex}");
            mLoopListView.MovePanelToItemIndex(itemIndex, 0);
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0 || index >= TotalItemCount)
            {
                return null;
            }

            var itemData = GetItemDataByIndex(index);
            if (itemData == null)
            {
                return null;
            }
            //get a new item. Every item can use a different prefab, the parameter of the NewListViewItem is the prefab’name. 
            //And all the prefabs should be listed in ItemPrefabList in LoopListView2 Inspector Setting
            LoopListViewItem2 item = listView.NewListViewItem("ItemPrefab1");
            var itemScript = item.GetComponent<ImageViewerListItem>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true; //保证只执行一次
                //itemScript.Init();
            }
            itemScript.SetItemData(itemData, index);
            return item;
        }

        public ImageViewerItemData GetItemDataByIndex(int index)
        {
            if (index < 0 || index >= mItemDataList.Count)
            {
                return null;
            }
            return mItemDataList[index];
        }

        public ImageViewerItemData GetItemDataById(int itemId)
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

        public void RemoveData(int index)
        {
            mItemDataList.RemoveAt(index);
        }

        public void InsertData(int index, ImageViewerItemData data)
        {
            mItemDataList.Insert(index, data);
        }



        public float duration;
        public float move;

        void OnBeginDrag()
        {
            duration = Time.time;
            move = mLoopListView.ContainerTrans.anchoredPosition3D.x;
        }
        void OnDraging()
        {
        }
        void OnEndDrag()
        {
            var lasttime = Time.time;
            duration = lasttime - duration;
            var lastmove = mLoopListView.ContainerTrans.anchoredPosition3D.x;
            move = lastmove - move;
            //Debug.Log($"move={move}");

            if (duration < 0.1f && move > _width / 8) // 1/8屏幕
            {
                //Debug.Log("右滑");
                itemIndex--;
                float x = (_width - move) + mLoopListView.ContainerTrans.anchoredPosition3D.x;
                OnJumpBtnClicked(x);
            }
            if (duration < 0.1f && move < -_width / 8) // 1/8屏幕
            {
                //Debug.Log("左滑");
                itemIndex++;
                float x = -(_width + move) + mLoopListView.ContainerTrans.anchoredPosition3D.x;
                OnJumpBtnClicked(x);
            }
        }

        void OnJumpBtnClicked(float x)
        {
            if (itemIndex < 0 || itemIndex >= 9)
            {
                Debug.Log(itemIndex);
                return;
            }

            var tw = mLoopListView.ContainerTrans.DOAnchorPos3DX(x, 0.3f);
            tw.OnComplete(() =>
            {
                mLoopListView.MovePanelToItemIndex(itemIndex, 0);
                mLoopListView.FinishSnapImmediately();
            });
        }
    }
}