using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstValue
{
    public const string login_route = "http://192.168.2.200:8080/login";
    public const string api_route = "http://192.168.2.200:8080";

    // 用户根目录
    private static string _user_root;
    private static string user_root
    {
        get
        {
            if (string.IsNullOrEmpty(_user_root))
            {
                _user_root = $"{Application.persistentDataPath}/{UserManager.Instance.localPlayer.identifier}";
                if (!Directory.Exists(_user_root))
                    Directory.CreateDirectory(_user_root);
            }
            return _user_root;
        }
    }

    // 文件缓存
    private static string storage_root
    {
        get
        {
            //string path = $"{Application.persistentDataPath}/{UserManager.Instance.localPlayer.identifier}/FileStorage";
            string path = $"{user_root}/FileStorage";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    public static string image_root
    {
        get
        {
            string path = $"{storage_root}/Image";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    public static string sound_root
    {
        get
        {
            string path = $"{storage_root}/Sound";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    public static string video_root
    {
        get
        {
            string path = $"{storage_root}/Video";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    public static string file_root
    {
        get
        {
            string path = $"{storage_root}/File";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    // 数据库
    private static string db_root
    {
        get
        {
            string path = $"{user_root}/Data";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    // 配置
    private static string config_root
    {
        get
        {
            string path = $"{user_root}/Config";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}