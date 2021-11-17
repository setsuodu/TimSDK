using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AndroidNative : MonoBehaviour
{
    private const string className = "com.zdkj.plugin.NativeFragment";
    private AndroidJavaClass jc = null;
    private AndroidJavaObject jo = null;

    [Header("Native")]
    [SerializeField] private Button m_SystemPrintBtn;
    [SerializeField] private Button m_HelloWorldBtn;
    [SerializeField] private Button m_OnCheckOEMBtn;

    void Awake()
    {
        jc = new AndroidJavaClass(className);
        jo = jc.CallStatic<AndroidJavaObject>("GetInstance", gameObject.name);

        // ************ Native ************ //
        m_SystemPrintBtn.onClick.AddListener(SystemPrint);
        m_HelloWorldBtn.onClick.AddListener(HelloWorld);
        m_OnCheckOEMBtn.onClick.AddListener(OnCheckOEM);
    }

    void OnDestroy()
    {
        jc.Dispose();
        jc = null;
        jo.Dispose();
        jo = null;

        // ************ Native ************ //
        m_SystemPrintBtn.onClick.RemoveListener(SystemPrint);
        m_HelloWorldBtn.onClick.RemoveListener(HelloWorld);
        m_OnCheckOEMBtn.onClick.RemoveListener(OnCheckOEM);
    }

    #region 本地
    public void NativeCallback(string msg)
    {
        Debug.Log($"[NativeCallback] {msg}");
    }

    public void SystemPrint()
    {
        jo.Call("SystemPrint");
    }

    public void HelloWorld()
    {
        jo.Call("HelloWorld");
    }

    //检查安卓手机厂商
    public void OnCheckOEM()
    {
        string oem = jo.Call<string>("CheckOEM");
        Debug.Log(oem);
    }
    #endregion
}
