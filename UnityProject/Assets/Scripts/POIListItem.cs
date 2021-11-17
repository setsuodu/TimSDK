using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;

namespace Client
{
    public class POIListItem : MonoBehaviour
    {
        int mItemDataIndex = -1;
        POI mItemData;

        [SerializeField] private Button mSelfBtn;
        [SerializeField] private Text mNameText;
        [SerializeField] private Text mLocationText;

        void Awake()
        {
            mSelfBtn.onClick.AddListener(OnSelfBtnClick);
        }

        public void SetItemData(POI itemData,int itemIndex)
        {
            this.mItemData = itemData;
            mItemDataIndex = itemIndex;

            mNameText.text = itemData.title;
            mLocationText.text = itemData.address;
        }

        void OnSelfBtnClick()
        {
            Debug.Log($"选中{mItemData.address}");

            UI_CreateArticle.Address = mItemData.title;
            PanelManager.Instance.CloseLast(true); //关闭UI_POI
        }
    }
}
