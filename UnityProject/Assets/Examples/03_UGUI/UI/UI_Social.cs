using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Client;

public class UI_Social : UIWidget
{
    [Header("导航栏")]
    //[SerializeField] private Button m_Tab0Btn; //推荐
    //[SerializeField] private Button m_Tab1Btn; //同城
    //[SerializeField] private Button m_Tab2Btn; //Hi圈
    //[SerializeField] private Button m_Tab3Btn; //喵圈
    //[SerializeField] private Button m_Tab4Btn; //汪圈
    [SerializeField] private Button m_SearchBtn;

    void Awake()
    {
        //m_Tab0Btn.onClick.AddListener(CmdDiscover);
        //m_Tab1Btn.onClick.AddListener(CmdNearBy);
        //m_Tab2Btn.onClick.AddListener(CmdHiGroup);
        //m_Tab3Btn.onClick.AddListener(CmdCatGroup);
        //m_Tab4Btn.onClick.AddListener(CmdDogGroup);
        m_SearchBtn.onClick.AddListener(CmdSearch);

        HttpManager.CallDelegate += OnComplete;
    }

    void OnDestroy()
    {
        HttpManager.CallDelegate -= OnComplete;
    }

    void Start()
    {
        base.SetAlpha(false);
        HttpManager.ShowDialog("Loading");
        SocialData.Get.LoadReset();
        SocialData.Get.RequestData(0);
    }

    // 请求结果回调 //1.请求异常/2.没有数据/3.刷新数据
    void OnComplete(string info)
    {
        StartCoroutine(OnRefresh());
    }
    public IEnumerator OnRefresh()
    {
        //等待ui创建
        yield return new WaitForSeconds(TWEEN_TIME);
        //SocialData.Get.mLoopListView.RefreshAllShownItem();
        HttpManager.Hide();

        if (!SocialData.Get.mIsWaitLoadingMoreData)
        {
            //数据加载完成，View淡入
            yield return new WaitForEndOfFrame();
            base.FadeIn();
        }
    }

    // 搜索
    void CmdSearch()
    {
    
    }
}
