using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TimEventManager : MonoBehaviour
{
    private static TimEventManager eventManager;
    public static TimEventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(TimEventManager)) as TimEventManager;
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
    public class MyIntEvent : UnityEvent<TimCallback>
    {

    }

    private Dictionary<TimSdkMessage, MyIntEvent> eventDictionary;

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<TimSdkMessage, MyIntEvent>();
        }
    }

    public static void StartListening(TimSdkMessage eventName, UnityAction<TimCallback> listener)
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

    public static void StopListening(TimSdkMessage eventName, UnityAction<TimCallback> listener)
    {
        if (eventManager == null) return;
        MyIntEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void Notify(TimSdkMessage eventName, TimCallback obj)
    {
        MyIntEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(obj);
        }
    }
}
