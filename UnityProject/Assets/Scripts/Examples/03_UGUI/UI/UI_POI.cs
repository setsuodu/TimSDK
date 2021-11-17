using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MaterialUI;
using LitJson;
using Client;

/// <summary>
/// POI
/// </summary>
public class UI_POI : UIWidget
{
    [SerializeField] private InputField m_SearchInput;
    [SerializeField] private Button m_HideLocBtn;
    [SerializeField] private Button m_BackBtn;

    void Awake()
    {
        m_SearchInput.onValueChanged.AddListener(OnInputChanged);
        m_HideLocBtn.onClick.AddListener(OnHideLocBtnClick);
        m_BackBtn.onClick.AddListener(() => base.Close(true));

        HttpManager.CallDelegate += OnComplete;
    }

    void OnDestroy()
    {
        HttpManager.CallDelegate -= OnComplete;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(TWEEN_TIME);

        yield return LocationManager.Instance.GetLocationInfo();
        if (LocationManager.Instance.LocationOK == true)
        {
            POIData.Get.RequestData();
        }
        else
        {
            // 跳转系统GPS设置
            DialogManager.ShowAlert(
                "无法定位，你可以通过以下操作提高定位精确度：在位置设置中打开GPS和无线网络",
                () =>
                {
                    TimSdkManager.Instance.OpenGPSSetting();
                },
                "去设置",
                "提示",
                null,
                () => { /* 取消*/ },
                "取消");
        }
    }

    // 请求结果回调 //1.请求异常/2.没有数据/3.刷新数据
    void OnComplete(string info)
    {
        Debug.Log($"UIWidget info={info}");

        //TODO: 没有数据显示"没有搜索到相关地点"
    }

    void OnInputChanged(string value)
    {
        if (async != null)
        {
            // 如果在指定时间内，有新的操作，取消上一个协程
            StopCoroutine(async);
        }

        async = StartCoroutine(DelayedAction(value));
    }

    // 隐藏位置
    void OnHideLocBtnClick()
    {
        UI_CreateArticle.Address = string.Empty;
        base.Close(true);
    }

    //频繁调用时只执行最后一次
    private Coroutine async;
    private const float threshold = 0.5f; //时间阈值，小于该时间，则只执行后一次
    IEnumerator DelayedAction(string value)
    {
        yield return new WaitForSeconds(threshold);
        Debug.Log("search: " + value);
        async = null;

        //TODO: 搜索请求放到 POIData 中
        //HttpManager.lbsSearch(value, "杭州", 10, 0, onLbsSearch);
        POIData.Get.RequestSearch(value);
    }
}
