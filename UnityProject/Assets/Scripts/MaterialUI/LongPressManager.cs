using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongPressManager : MonoBehaviour
{
    private static LongPressManager m_Instance;
    public static LongPressManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                GameObject go = new GameObject();
                go.name = "LongPressManager";
                m_Instance = go.AddComponent<LongPressManager>();
            }
            return m_Instance;
        }
    }

    private static bool Inited;
    [SerializeField] private Canvas m_ParentCanvas;
    [SerializeField] private RectTransform m_Background;
    [SerializeField] private RectTransform m_Rect;
    [SerializeField] private Item_Menu m_Menu;

    void Awake()
    {
        Canvas[] arr = FindObjectsOfType<Canvas>(); //2个
        m_ParentCanvas = (arr[0].sortingOrder > arr[1].sortingOrder) ? arr[0] : arr[1];

        var asset = Resources.Load<GameObject>("1920/Menu");
        var obj = Instantiate(asset);
        m_Background = obj.GetComponent<RectTransform>();
        m_Menu = obj.GetComponent<Item_Menu>();
        m_Rect = obj.transform.GetChild(0).GetComponent<RectTransform>();
    }

    // 测试初始化
    public static void Init()
    {
        Canvas canvas = Instance.m_ParentCanvas;
        Instance.m_Background.SetParent(canvas.transform);
        Instance.m_Background.anchorMin = Vector2.zero;
        Instance.m_Background.anchorMax = Vector2.one;
        Instance.m_Background.localPosition = Vector3.zero;
        Instance.m_Background.anchoredPosition = Vector3.zero;
        Instance.m_Background.sizeDelta = Vector2.zero;
    }

    public static void Init(Vector3 position, MenuItemClass[] array)
    {
        if (!Inited)
        {
            Inited = true;
            Canvas canvas = Instance.m_ParentCanvas;
            Instance.m_Background.SetParent(canvas.transform);
            Instance.m_Background.anchorMin = Vector2.zero;
            Instance.m_Background.anchorMax = Vector2.one;
            Instance.m_Background.localPosition = Vector3.zero;
            Instance.m_Background.anchoredPosition = Vector3.zero;
            Instance.m_Background.sizeDelta = Vector2.zero;
        }

        Instance.m_Rect.position = position;
        Instance.m_Rect.pivot = new Vector2(position.x / Screen.width, position.y / Screen.height);
        Instance.m_Background.gameObject.SetActive(false);
        Instance.m_Rect.gameObject.SetActive(false);
        Instance.m_Menu.Init(array);
    }

    public static void Show()
    {
        Instance.m_Background.gameObject.SetActive(true);
        Instance.m_Rect.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Instance.m_Background.gameObject.SetActive(false);
        Instance.m_Rect.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class MenuItemClass
{
    public string title;
    public System.Action action;
    public MenuItemClass() { }
    public MenuItemClass(string _title, System.Action _action) 
    {
        this.title = _title;
        this.action = _action;
    }
}
