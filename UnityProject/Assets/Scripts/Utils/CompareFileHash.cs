using System;
using System.Security.Cryptography;
using UnityEngine;

public class CompareFileHash
{
    public string file1, file2;

    public static bool IsTheSameFile(string _file1, string _file2)
    {
        string path = Application.dataPath + "/Cache/";
        _file1 = path + _file1;
        _file2 = path + _file2;

        //计算第一个文件的哈希值
        var hash = System.Security.Cryptography.HashAlgorithm.Create();
        var stream_1 = new System.IO.FileStream(_file1, System.IO.FileMode.Open);
        byte[] hashByte_1 = hash.ComputeHash(stream_1);
        stream_1.Close();

        //计算第二个文件的哈希值
        var stream_2 = new System.IO.FileStream(_file2, System.IO.FileMode.Open);
        byte[] hashByte_2 = hash.ComputeHash(stream_2);
        stream_2.Close();

        Debug.Log("[file1]" + BitConverter.ToString(hashByte_1) + "\n[file2]" + BitConverter.ToString(hashByte_2));

        //比较两个哈希值
        if (BitConverter.ToString(hashByte_1) == BitConverter.ToString(hashByte_2))
        {
            Debug.Log("两个文件相等");
            return true;
        }
        else
        {
            Debug.Log("两个文件不等");
            return false;
        }
    }

    public static string FileToHash(byte[] _bt)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] result = md5.ComputeHash(_bt);
        string t2 = BitConverter.ToString(result, 4, 8);
        t2 = t2.Replace("-", "");
        t2 = t2.ToLower();
        return t2;
    }
}
