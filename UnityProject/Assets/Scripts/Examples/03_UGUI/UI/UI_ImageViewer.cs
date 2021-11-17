using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Client;
using LitJson;

/// <summary>
/// 图片浏览器
/// </summary>
public class UI_ImageViewer : UIWidget
{
    // 最多9张图

    public ArticleItemData mItemData;

    [SerializeField] private Text m_PageText;
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_MoreBtn;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_MoreBtn.onClick.AddListener(OnMoreBtnClick);
    }

    void OnMoreBtnClick()
    {
        Debug.Log("更多");
    }

    public void Init(ArticleItemData itemData, int index)
    {
        this.mItemData = itemData;

        m_PageText.text = $"{index + 1}/{itemData.pics.Length}";

        ImageViewerMgr.Get.LoadDataSource(itemData, index);
    }

    [ContextMenu("Print")]
    void Print()
    {
        Debug.Log(ImageViewerMgr.Get.mLoopListView.ScrollRect.verticalScrollbar.numberOfSteps);
    }
}
