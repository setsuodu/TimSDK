using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using Client;

/// <summary>
/// 话题
/// </summary>
public class UI_ArticleTopic : UIWidget
{
    [SerializeField] private InputField m_SearchInput;
    [SerializeField] private Button m_BackBtn;

    [SerializeField] private GameObject recGroup;
    [SerializeField] private GameObject hotGroup;
    [SerializeField] private GameObject createGroup;
    [SerializeField] private GameObject moreGroup;

    [SerializeField] private List<Text> recTopics; //推荐话题
    [SerializeField] private List<Text> hotTopics; //热门话题
    [SerializeField] private Button createBtn; //创建话题
    [SerializeField] private Text createText; //创建话题
    [SerializeField] private List<Text> moreTopics; //更多话题

    void Awake()
    {
        SearchEnable("");

        m_SearchInput.onValueChanged.AddListener(OnInputChanged);
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        createBtn.onClick.AddListener(CmdCreateTopic);

        HttpManager.CallDelegate += OnComplete;


        for (int i = 0; i < recTopics.Count; i++)
        {
            int index = i;
            recTopics[i].GetComponentInParent<Button>().onClick.AddListener(()=>
            {
                Debug.Log($"选择:{recTopics[index].text}");
            });
        }
    }

    void OnDestroy()
    {
        HttpManager.CallDelegate -= OnComplete;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(TWEEN_TIME);

        HttpManager.topicList(0, 20, "", onTopicList);
    }

    void onTopicList(string json)
    {
        //Debug.Log(json);
        //{"total":1,"size":20,"url":null,"list":[{"_id":"青花瓶","count":0,"sort":0.0,"createTime":1587107768621,"status":1,"updateTime":1587107768621}],"totalPage":1,"curPage":1}

        var obj = JsonMapper.ToObject<ArticleTopicList>(json);
        Debug.Log($"{obj.list.Length}个话题");

        for (int i = 0; i < recTopics.Count; i++)
        {
            if (i >= obj.list.Length)
            {
                recTopics[i].transform.parent.gameObject.SetActive(false);
            }
            else
            {
                recTopics[i].transform.parent.gameObject.SetActive(true);
                recTopics[i].text = obj.list[i]._id;
            }
        }

        for (int i = 0; i < hotTopics.Count; i++)
        {
            hotTopics[i].transform.parent.gameObject.SetActive(false);
        }
    }

    void CmdCreateTopic()
    {
        HttpManager.topicCreate(createText.text, onTopicCreate);
    }

    void onTopicCreate(string json)
    {
        Debug.Log(json);
    }

    // 请求结果回调 //1.请求异常/2.没有数据/3.刷新数据
    void OnComplete(string info)
    {
        Debug.Log($"UIWidget info={info}");

        //TODO: 没有数据显示"没有搜索到相关地点"
    }

    void OnInputChanged(string value)
    {
        SearchEnable(value);
        createText.text = value;

        if (async != null)
        {
            // 如果在指定时间内，有新的操作，取消上一个协程
            StopCoroutine(async);
        }
        async = StartCoroutine(DelayedAction(value));
    }

    void SearchEnable(string value) 
    {
        if (string.IsNullOrEmpty(value))
        {
            recGroup.SetActive(true);
            hotGroup.SetActive(true);
            createGroup.SetActive(false);
            moreGroup.SetActive(false);
        }
        else
        {
            recGroup.SetActive(false);
            hotGroup.SetActive(false);
            createGroup.SetActive(true);
            moreGroup.SetActive(true);
        }
    }

    //频繁调用时只执行最后一次
    private Coroutine async;
    private const float threshold = 0.5f; //时间阈值，小于该时间，则只执行后一次
    IEnumerator DelayedAction(string value)
    {
        yield return new WaitForSeconds(threshold);
        Debug.Log("search: " + value);
        async = null;

        //TODO: 模糊搜索
        Debug.Log("模糊搜索");
    }
}
