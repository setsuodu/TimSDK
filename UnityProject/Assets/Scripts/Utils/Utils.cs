using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

public class Utils
{
    public static DateTime ConvertTimestamp(long timestamp)
    {
        DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
#if UNITY_IOS
        DateTime newDateTime = converted.AddSeconds(timestamp / 1000);
#else
        DateTime newDateTime = converted.AddSeconds(timestamp);
#endif
        return newDateTime.ToLocalTime();
    }

    public static long ToTimestamp(DateTime value)
    {
        TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
        return (long)span.TotalSeconds;
    }

    // Load image from mobile
    public static void LoadFileBytes(string fileName, ref Texture2D t2d)
    {
        byte[] byteArray = File.ReadAllBytes(Application.persistentDataPath + "/" + fileName);
        //Debug.Log("读取成功 data=" + byteArray.Length);
        Texture2D texture = new Texture2D(t2d.width, t2d.height); //out无法获取到参数t2d的值
        texture.LoadImage(byteArray);
        t2d = texture;
    }

    // 保存图片到本地
    public static void SaveTextureToFile(Texture2D texture, string filePath)
    {
        //Debug.Log(filePath);
        if (File.Exists(filePath)) return;
        byte[] bytes = texture.EncodeToPNG();
        var file = File.Open(filePath, FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
        binary.Close();
    }

    /// <summary>
    /// 手机号格式验证
    /// </summary>
    /// <param name="number">手机号码</param>
    /// <returns></returns>
    public static bool CheckPhoneNumber(string number)
    {
        //判断条件：首位数为1，第二位为3/5/8，总计11位
        return System.Text.RegularExpressions.Regex.IsMatch(number, @"^[1]+[3,5,8]+\d{9}");
    }

    /// <summary>
    /// C#判断字符串中包含某个字符的个数
    /// </summary>
    /// <param name="value">定义字符串</param>
    /// <returns></returns>
    public static int IncludeCharCount(string str, string key)
    {
        //获取@字符出现的次数
        str = "humakesdkj@idsk@";
        key = "@";

        int count = Regex.Matches(str, key).Count;
        Debug.Log($"count={count}");
        return count;
    }

    /// <summary>
    /// 字符串长度（中文占2个字符）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int GetLength(string str)
    {
        if (str.Length == 0)
            return 0;
        System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
        int tempLen = 0;
        byte[] s = ascii.GetBytes(str);
        for (int i = 0; i < s.Length; i++)
        {
            if ((int)s[i] == 63)
            {
                tempLen += 2;
            }
            else
            {
                tempLen += 1;
            }
        }
        return tempLen;
    }

    /// <summary>
    /// 年代标签（80后/90后/00后）
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static string Generation(DateTime birthday)
    {
        //birthday = new System.DateTime(1992, 12, 12);
        int gen = birthday.Year % 100;
        Debug.Log($"年代={gen}");
        string result = $"{gen / 10}0后";
        return result;
    }

    /// <summary>
    /// 获取文件hash值
    /// </summary>
    /// <param name="file">路径</param>
    /// <returns></returns>
    public static string getFilesMD5Hash(string file)
    {
        //MD5 hash provider for computing the hash of the file
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        //open the file
        FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);

        //calculate the files hash
        md5.ComputeHash(stream);

        //close our stream
        stream.Close();

        //byte array of files hash
        byte[] hash = md5.Hash;

        //string builder to hold the results
        StringBuilder sb = new StringBuilder();

        //loop through each byte in the byte array
        foreach (byte b in hash)
        {
            //format each byte into the proper value and append
            //current value to return value
            sb.Append(string.Format("{0:X2}", b));
        }

        //return the MD5 hash of the file
        return sb.ToString();
    }

    // 动态权限申请
    public static void RequestUserPermission(string permission)
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
        {
            UnityEngine.Android.Permission.RequestUserPermission(permission);
        }
        else
        {
            //Debug.Log($"{permission}已授权");
        }
#elif UNITY_IOS
#endif
    }

    // 图片水平翻转
    public static Texture2D FlipHorizontal(Texture2D t2d)
    {
        Texture2D flipTexture = new Texture2D(t2d.width, t2d.height); 
        for (int i = 0; i < t2d.width; i++)
        {
            flipTexture.SetPixels(i, 0, 1, t2d.height, t2d.GetPixels(t2d.width - i - 1, 0, 1, t2d.height));
        }
        flipTexture.Apply();
        return flipTexture;
    }

    // 图片垂直翻转
    public static Texture2D FlipVertical(Texture2D t2d)
    {
        Texture2D flipTexture = new Texture2D(t2d.width, t2d.height);
        for (int i = 0; i < t2d.height; i++)
        {
            flipTexture.SetPixels(0, i, t2d.width, 1, t2d.GetPixels(0, t2d.height - i - 1, t2d.width, 1));
        }
        flipTexture.Apply();
        return flipTexture;
    }
}

/// <summary>
/// 星座计算
/// </summary>
public class DateParser
{
    private readonly DateTime _birthday;

    private static string[] _constellationNames =
    {
        "白羊座", "金牛座", "双子座",
        "巨蟹座", "狮子座", "处女座",
        "天秤座", "天蝎座", "射手座",
        "摩羯座", "水瓶座", "双鱼座"
    };

    public DateParser(DateTime birthday)
    {
        _birthday = birthday;
    }

    public string Constellation
    {
        get
        {
            int index = 0;
            int y = _birthday.Year;
            int m = _birthday.Month;
            int d = _birthday.Day;
            y = m * 100 + d;
            if (((y >= 321) && (y <= 419))) { index = 0; }
            else if ((y >= 420) && (y <= 520)) { index = 1; }
            else if ((y >= 521) && (y <= 620)) { index = 2; }
            else if ((y >= 621) && (y <= 722)) { index = 3; }
            else if ((y >= 723) && (y <= 822)) { index = 4; }
            else if ((y >= 823) && (y <= 922)) { index = 5; }
            else if ((y >= 923) && (y <= 1022)) { index = 6; }
            else if ((y >= 1023) && (y <= 1121)) { index = 7; }
            else if ((y >= 1122) && (y <= 1221)) { index = 8; }
            else if ((y >= 1222) || (y <= 119)) { index = 9; }
            else if ((y >= 120) && (y <= 218)) { index = 10; }
            else if ((y >= 219) && (y <= 320)) { index = 11; }
            else { index = 0; }

            return _constellationNames[index];
        }
    }
}