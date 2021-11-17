using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// App内部消息传递（无数据）
public enum SystemEventName 
{
    BackPressed         = 0, //按下返回键
    UserProfileChanged  = 1, //用户资料变更（所有显示头像、昵称的页面都需要添加监听）UI_Home, UI_UserSettings
    Drag                = 2, //下拉加载聊天记录
}

public class SystemEventManager : MonoBehaviour
{
    private static SystemEventManager eventManager;
    public static SystemEventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(SystemEventManager)) as SystemEventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

    [System.Serializable]
    public class MyIntEvent : UnityEvent<int>
    {

    }

    private Dictionary<SystemEventName, MyIntEvent> eventDictionary;

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<SystemEventName, MyIntEvent>();
        }
    }

    public static void StartListening(SystemEventName eventName, UnityAction<int> listener)
    {
        MyIntEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new MyIntEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(SystemEventName eventName, UnityAction<int> listener)
    {
        if (eventManager == null) return;
        MyIntEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(SystemEventName eventName)
    {
        MyIntEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke((int)eventName);
        }
    }
}
