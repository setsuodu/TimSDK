using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RESTAPI
{
    public static string FormDataPost() 
    {
        return "";
    }

    public static string JsonPost(string url, byte[] bytes)
    {
        Debug.Log($"{url}");
        string result = string.Empty;

        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = bytes.Length;
        request.Headers.Add("Auther", "user_id");

        using (Stream reqStream = request.GetRequestStream())
        {
            reqStream.Write(bytes, 0, bytes.Length);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using (Stream responseStm = response.GetResponseStream())
        {
            StreamReader redStm = new StreamReader(responseStm, Encoding.UTF8);
            result = redStm.ReadToEnd();
        }
        return result;
    }

    public static string HttpGet(string url, string method = null)
    {
        Debug.Log($"{url}");
        string result = string.Empty;

        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.Method = string.IsNullOrEmpty(method) ? "GET" : method;
        request.ContentType = "application/json";

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using (Stream responseStm = response.GetResponseStream())
        {
            StreamReader redStm = new StreamReader(responseStm, Encoding.UTF8);
            result = redStm.ReadToEnd();
        }
        return result;
    }
}
