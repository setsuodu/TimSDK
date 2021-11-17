using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class ImageHelper : MonoBehaviour
{
    private void OnGetPhotoData(byte[] data, int width, int height)
    {
        Debug.Log($"{width}x{height}");

        Texture2D headAvatar = new Texture2D(width, height);
        headAvatar.LoadImage(data);
        headAvatar.Apply();
        headAvatar = MakeHeadAvatar(headAvatar);
        //SharedData.Self.HeadAvatar = headAvatar;
        //image_head_avatar.texture = headAvatar;
        //image_head_avatar.gameObject.SetActive(true);
        //UploadHeadAvatar(headAvatar, OnUploadSuccess, OnUploadError);
    }

    const string HeadAvatarUrl = "/user/avatar/upload";

    public const int HeadAvatarSize = 512;

    public static Texture2D MakeHeadAvatar(Texture2D source)
    {
        if (source.width == HeadAvatarSize && source.height == HeadAvatarSize) return source;
        return TextureScale.Bilinear(source, HeadAvatarSize, HeadAvatarSize);
    }

    public static void UploadHeadAvatar(Dictionary<string, object> postParameters, Action<object> success = null, Action<string> error = null)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";

        //byte[] data = image.EncodeToJPG();
        //Dictionary<string, object> postParameters = new Dictionary<string, object>();
        //postParameters.Add("file", new FileParameter(data, "head_avatar.jpg", "multipart/file"));
        //string hash = CompareFileHash.FileToHash(data);
        //postParameters.Add("hash", hash);


        HttpWebResponse webResponse = HttpManager.MultipartFormDataPost(url, postParameters);


        if (webResponse != null)
        {
            var responseStream = webResponse.GetResponseStream();
            if (responseStream != null)
            {
                StreamReader responseReader = new StreamReader(responseStream);
                string httpResult = responseReader.ReadToEnd();
                Debug.Log($"result={httpResult}");
                //UploadResult result = JsonMapper.ToObject<UploadResult>(httpResult);
                //if (result.code == 0)
                //    success?.Invoke(result.data.ToString());
                //else
                //    error?.Invoke(result.msg);
            }
            webResponse.Close();
        }
        else 
        {
            Debug.LogError($"webResponse is null");
        }
    }
}
