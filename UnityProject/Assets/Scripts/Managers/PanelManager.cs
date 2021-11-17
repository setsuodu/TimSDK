using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PanelManager))]
public class PanelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        PanelManager demo = (PanelManager)target;
        if (GUILayout.Button("Show"))
        {
            demo.Pool.Peek().Show();
        }
        if (GUILayout.Button("Hide"))
        {
            demo.Pool.Peek().Hide();
        }
    }
}
#endif
public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    public Transform Parent;
    private Stack<UIWidget> pool;
#if UNITY_EDITOR
    public Stack<UIWidget> Pool { get { return pool; } }
#endif

    void Awake()
    {
        Instance = this;

        Parent = Parent ? Parent : GameObject.Find("Canvas").transform;
        pool = new Stack<UIWidget>();

        Application.targetFrameRate = 60;
    }

    void OnEnable()
    {
        SystemEventManager.StartListening(SystemEventName.BackPressed, OnSystemMessage);
    }

    void OnDisable()
    {
        SystemEventManager.StopListening(SystemEventName.BackPressed, OnSystemMessage);
    }

    void Start()
    {
        CreatePanel<UI_Login>();
    }

    //#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SystemEventManager.TriggerEvent(SystemEventName.BackPressed);
        }
    }
    //#endif

    public void OnSystemMessage(int value)
    {
        switch (value)
        {
            case (int)SystemEventName.BackPressed:
                BackToClose(true);
                break;
        }
    }

    public T CreatePanel<T>(bool pop = true, bool anim = false) where T : UIWidget
    {
        if (pop)
            CloseLast();
        
        string fileName = $"prefabs/ui/{typeof(T).ToString().ToLower()}";
        GameObject asset = Resources.Load<GameObject>(fileName);
        GameObject go = Instantiate(asset, Parent);

        go.name = typeof(T).ToString();
        go.layer = LayerMask.NameToLayer("Default");
        go.transform.SetParent(Parent);
        go.transform.localPosition = anim ? new Vector3(360, 0, 0) : Vector3.zero;
        go.transform.localScale = Vector3.one;

        T script = go.GetComponent<T>();
        if (anim)
            script.Show();
        pool.Push(script);

        //func?.Invoke();
        return script;
    }

    public void CloseLast(bool anim = false)
    {
        if (pool == null || pool.Count == 0) return;
        var obj = pool.Pop();
        if (obj == null) return;
        if (!anim) 
        {
            Destroy(obj.gameObject);
            return; 
        }
        obj.Hide(()=>
        {
            Destroy(obj.gameObject);
        });
    }

    public void BackToClose(bool anim = false)
    {
        if (pool == null || pool.Count == 2) return;
        var obj = pool.Pop();
        if (obj == null) return;
        if (!anim)
        {
            Destroy(obj.gameObject);
            return;
        }
        obj.Hide(() =>
        {
            Destroy(obj.gameObject);
        });
    }

    public void CloseAll(int left = 0)
    {
        for (int i = pool.Count - 1; i >= left; i--)
        {
            var obj = pool.Pop();
            if (obj == null) return;
            Destroy(obj.gameObject);
        }
        //Debug.Log($"CloseAll: {pool.Count}");
    }

    public UIWidget GetUI<T>() where T : UIWidget
    {
        return null;
    }
}
