using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UploadTest : MonoBehaviour
{
    private string url = "http://localhost/Upload.php";
    //private string url = "http://localhost/Uploader.php";
    public Texture2D t2d;
    public Texture2D dst;

    [ContextMenu("Upload")]
    public void Upload()
    {
        StartCoroutine(OnUpload());
    }
    public IEnumerator OnUpload()
    {
        byte[] bytes = t2d.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes);

        WWW www = new WWW(url, form);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            yield break;
        }
        Debug.Log("ok");
        Debug.Log(www.text);
    }


    [ContextMenu("ChooseImage")]
    public void ChooseImage()
    {
#if UNITY_EDITOR
        string srcPath = "C:/Users/Administrator/Pictures";
        string dstPath = EditorUtility.OpenFilePanel("Select png", srcPath, "*.*");
        HttpManager.UploadFile(dstPath, HttpManager.ContentType.img);
#endif
    }
    [ContextMenu("ChooseAudio")]
    public void ChooseAudio()
    {
#if UNITY_EDITOR
        string srcPath = "C:/Users/Administrator/Pictures";
        string dstPath = EditorUtility.OpenFilePanel("Select png", srcPath, "*.*");
        HttpManager.UploadFile(dstPath, HttpManager.ContentType.audio, onUploadFile);
#endif
    }
    void onUploadFile(string json)
    {

    }


    // 图片缩放
    [ContextMenu("Compress")]
    public void ChooseCompress()
    {
#if UNITY_EDITOR
        string srcPath = "C:/Users/Administrator/Pictures";
        string dstPath = EditorUtility.OpenFilePanel("Select png", srcPath, "*.*");
        Compress(dstPath);
#endif
    }
    public void Compress(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(data);
        string ext = Path.GetExtension(filePath);
        Debug.Log($"origin={t2d.width}x{t2d.height}, ext={ext}, length={data.Length}");

        byte[] dstData = null;
        dst = TextureScale.Bilinear(t2d, t2d.width / 2, t2d.height / 2);
        if (ext.Equals(".png"))
        {
            dstData = dst.EncodeToPNG();
        }
        else if (ext.Equals(".jpg"))
        {
            dstData = dst.EncodeToJPG();
        }
        else 
        {
            Debug.LogError("未知格式");
        }
        Debug.Log($"output={dst.width}x{dst.height}, length={dstData.Length}");
    }
}
