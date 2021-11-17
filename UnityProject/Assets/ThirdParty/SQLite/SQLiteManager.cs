using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SQLiteManager : MonoBehaviour
{
    public DataService ds;

    void Start()
    {
        ds = new DataService($"2/tempDatabase.db"); //登录成功时已创建目录
    }

    [ContextMenu("_Insert")]
    public void _Insert()
    {
        TIMUserProfileExt ext = new TIMUserProfileExt();
        ext.identifier = "3";
        ext.nickName = "用户3";
        ext.gender = 1;
        ext.faceUrl = "http://";
        ext.relation = 3;
        ds.InsertProfile(ext);
    }

    [ContextMenu("_Delete")]
    public void _Delete()
    {
        ds.DeleteProfile("3");
    }

    [ContextMenu("_Update")]
    public void _Update()
    {
        ds.UpdateProfile("3", "Lala");
    }

    [ContextMenu("_Query")]
    public void _Query()
    {
        var ext = ds.QueryProfile("2");
        Debug.Log(ext.nickName);
    }
}
