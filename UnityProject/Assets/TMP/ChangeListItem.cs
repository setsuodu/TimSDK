using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;

public class ChangeListItem : MonoBehaviour
{
    RectTransform rect;
    LoopListViewItem2 item2;

    public Text mNameText;
    public Image mIcon;
    public Text mDescText;
    public GameObject mExpandContentRoot;
    public Text mClickTip;
    public Button mExpandBtn;
    public int mItemDataIndex = -1;
    bool mIsExpand;

    void Awake()
    {
        rect = gameObject.GetComponent<RectTransform>();
        item2 = gameObject.GetComponent<LoopListViewItem2>();

        mExpandBtn.onClick.AddListener(OnExpandBtnClicked);
    }

    void OnExpandBtnClicked()
    {
        //Debug.Log($"click{mItemDataIndex}");
        ItemData data = ChangeItemHeight.Get.GetItemDataByIndex(mItemDataIndex);
        if (data == null) return;

        mIsExpand = !mIsExpand;
        data.mIsExpand = mIsExpand;

        OnExpandChanged();

        item2.ParentListView.OnItemSizeChanged(item2.ItemIndex);
    }

    public void SetItemData(ItemData itemData, int itemIndex)
    {
        mItemDataIndex = itemIndex;
        mNameText.text = itemData.mName;
        mDescText.text = itemData.mFileSize.ToString() + "KB";
        mIsExpand = itemData.mIsExpand;

        OnExpandChanged();
    }

    public void OnExpandChanged()
    {
        if (mIsExpand)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 284f);
            mExpandContentRoot.SetActive(true);
            mClickTip.text = "Shrink";
        }
        else
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 143f);
            mExpandContentRoot.SetActive(false);
            mClickTip.text = "Expand";
        }
    }
}