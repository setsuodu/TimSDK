using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using Client;

namespace SuperScrollView
{
    public class ArticleListItemSticky : MonoBehaviour
    {
        public ArticleItemData mItemData;
        public int mItemDataIndex = -1;

        public Button mDiscoverBtn;
        public Button mNearByBtn;
        public Button mHiBtn;
        public Button mCatBtn;
        public Button mDogBtn;
        public Button mSearchBtn;

        void Awake()
        {
            mDiscoverBtn.onClick.AddListener(CmdDiscover);
            mNearByBtn.onClick.AddListener(CmdNearBy);
            mHiBtn.onClick.AddListener(CmdHiGroup);
            mCatBtn.onClick.AddListener(CmdCatGroup);
            mDogBtn.onClick.AddListener(CmdDogGroup);
            mSearchBtn.onClick.AddListener(CmdSearch);
        }

        public void SetItemData(ArticleItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;
        }

        // 推荐
        void CmdDiscover()
        {
            HttpManager.discover(0, 20, 0, onDiscover);
        }

        // 同城
        void CmdNearBy()
        {
            double lat = 30.193834; //纬度
            double lng = 120.186414; //经度
            HttpManager.nearby(0, 20, 0, lat, lng, onNearby);
        }

        // Hi圈
        void CmdHiGroup()
        {

        }

        // 喵圈
        void CmdCatGroup()
        {

        }

        // 汪圈
        void CmdDogGroup()
        {

        }

        // 搜索
        void CmdSearch()
        {

        }

        #region HTTP回调

        void onDiscover(string json)
        {
            Debug.Log(json);
            //{"total":2,"size":20,"url":null,"list":[{"praised":false,"user":{"uid":2,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","no":"4","sex":1,"nickname":"圣诞w@@#^sss"},"id":2,"type":1,"content":"啦啦啦啦","status":3,"anonymous":0,"topics":null,"top":0,"dataUrl":"","commentCnt":0,"cover":"","publishTime":1586831183613,"pics":null,"essence":1,"pri":100,"addr":""},{"praised":false,"user":{"uid":2,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","no":"4","sex":1,"nickname":"圣诞w@@#^sss"},"id":1,"type":1,"content":"hello world","status":3,"anonymous":0,"topics":null,"top":0,"dataUrl":"","commentCnt":0,"cover":"","publishTime":1586763183447,"pics":null,"essence":1,"pri":100,"addr":""}],"curPage":1,"totalPage":1}

            var obj = JsonMapper.ToObject<Articles>(json);
            Debug.Log($"{obj.list.Length}篇文章");

            //TODO: 刷新列表
            //SocialData.Get.LoadMore();
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
