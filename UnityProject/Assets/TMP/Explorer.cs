using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Explorer))]
public class ExplorerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        Explorer demo = (Explorer)target;
        if (GUILayout.Button("Open"))
        {
            string path = Application.persistentDataPath;
            System.Diagnostics.Process.Start(path);
        }
        if (GUILayout.Button("Select"))
        {
            string src = Application.persistentDataPath;
            string dst = EditorUtility.OpenFilePanel("Select png", src, "png");
            Debug.Log(dst); //C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png

            byte[] bytes = File.ReadAllBytes(dst);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            demo.t2d = tex;
        }
        if (GUILayout.Button("UploadImage"))
        {
            demo.UploadImage();
        }
    }
}
#endif
public class Explorer : MonoBehaviour
{
    public Texture2D t2d;

    public void UploadImage()
    {
        //string path = "C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png";
        //byte[] bytes = File.ReadAllBytes(path);
        //string url = $"{ConstValue.api_route}/api/article/upload";
        //string hash = Utils.getFilesMD5Hash(path);
        //Debug.Log($"file={bytes.Length} hash={hash}");

        //WWWForm form = new WWWForm();
        //form.AddField("token", HttpManager.token);
        //form.AddField("hash", hash);
        //form.AddField("content_type", "img");

        //StartCoroutine(SendHttpPost(url, form, bytes));

        //StartCoroutine(UploadMultipleFiles());
        StartCoroutine(UploadImages());
    }

    public IEnumerator UploadImages()
    {
        string url = $"{ConstValue.api_route}/api/upload/article";
        string path = "C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png";

        WWWForm form = new WWWForm();
        form.AddField("token", HttpManager.token);
        string hash = Utils.getFilesMD5Hash(path);
        form.AddField("hash", hash);
        form.AddField("content_type", "img");

        UnityWebRequest file = new UnityWebRequest();
        file = UnityWebRequest.Get(path);
        yield return file.SendWebRequest();
        form.AddBinaryData("file", file.downloadHandler.data);

        UnityWebRequest req = UnityWebRequest.Post(url, form);
        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
            Debug.Log(req.error);
        else
            Debug.Log("Uploaded Successfully");

        if (req.responseCode == 200)
        {
            string result = req.downloadHandler.text;
            Debug.Log(result);
            req.Dispose();
        }
    }

    public IEnumerator UploadMultipleFiles()
    {
        string[] path = new string[3];
        path[0] = "D:/File1.txt";
        path[1] = "D:/File2.txt";
        path[2] = "D:/File3.txt";

        UnityWebRequest[] files = new UnityWebRequest[path.Length];
        WWWForm form = new WWWForm();

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = UnityWebRequest.Get(path[i]);
            yield return files[i].SendWebRequest();
            form.AddBinaryData("files[]", files[i].downloadHandler.data, Path.GetFileName(path[i]));
        }

        UnityWebRequest req = UnityWebRequest.Post("http://localhost/Uploader.php", form);
        //UnityWebRequest req = UnityWebRequest.Post("http://localhost/File%20Upload/Uploader.php", form);
        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
            Debug.Log(req.error);
        else
            Debug.Log("Uploaded " + files.Length + " files Successfully");
    }
}