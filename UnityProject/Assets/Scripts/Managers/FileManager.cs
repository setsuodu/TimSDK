using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(FileManager))]
public class FileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        DrawDefaultInspector(); //显示默认所有参数
        FileManager demo = (FileManager)target;
        if (GUILayout.Button("Download"))
        {
            FileManager.Download("http://avatar.zd1312.com/def/women_320_320.png", demo.LoadT2d);
        }
        if (GUILayout.Button("cache"))
        {
            demo.cacheCount();
        }
    }
}
#endif

public class FileManager : MonoBehaviour
{
    public static FileManager Instance;

    void Awake()
    {
        Instance = this;
    }

    //下载 //聊天图片才有uuid
    public static void Download(string url, Action<byte[]> action, string uuid = null)
    {
        //http://avatar.zd1312.com/def/women_320_320.png

        // 没有uuid就截取
        // 有就用uuid保存
        //Debug.Log(url);
        string fileName = string.IsNullOrEmpty(uuid) ? Path.GetFileName(url) : uuid;
        string filePath = Path.Combine(ConstValue.image_root, fileName);
        //Debug.Log($"Download fileName={fileName}");
        if (File.Exists(filePath))
        {
            Load(filePath, action);
        }
        else
        {
            HttpManager.Download(url, action, uuid); //下载完保存到本地
            //Debug.Log("通过网络加载");
        }
    }

    //本地
    public static void Load(string path, Action<byte[]> action) 
    {
        string fileName = Path.GetFileName(path);
        if (cache.ContainsKey(fileName))
        {
            Get(fileName, action);
        }
        else
        {
            var bytes = File.ReadAllBytes(path);
            action?.Invoke(bytes);
            cache.Add(fileName, bytes);
            //Debug.Log("通过本地加载");
        }
    }

    //内存 //TODO: 只有ListView中的图片需要三级缓存，避免上下拖拉卡顿
    //在具体的View中实现：
    //UI_Message/UI_FriendList/UI_Visitors/UI_Chat
    private static Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
    public static void Get(string fileName, Action<byte[]> action) 
    {
        byte[] bytes = null;
        bool result = cache.TryGetValue(fileName, out bytes);
        if (result)
            action?.Invoke(bytes);
        //Debug.Log("通过内存加载");
    }

    public static void OnLoadImage(byte[] bytes, Image image)
    {
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        image.sprite = sp;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
    }

    public static void OnLoadRawImage(byte[] bytes, RawImage image, System.Action action = null)
    {
        Texture2D t2d = new Texture2D(2, 2);
        //Debug.Log(bytes.Length);
        t2d.LoadImage(bytes);
        image.texture = t2d;
        action?.Invoke();
    }

    #region 测试
    public Texture2D t2d;
    public void LoadT2d(byte[] bytes)
    {
        t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
    }
    public void cacheCount() 
    {
        Debug.Log($"cache={cache.Count}");
    }
    #endregion
}
