using UnityEngine;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(loginApiDemo))]
public class loginApiDemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数

        loginApiDemo demo = (loginApiDemo)target;
        if (GUILayout.Button("login"))
        {
            int loginType = 1; //1手机验证码 2手机密码 10微信 11qq 12微博
            //long phone = 13718780428;
            long phone = 18069828910;
            int code = 999000; //验证码或者密码
            demo.login(loginType, phone, code, demo.onLogin);
        }
        if (GUILayout.Button("sendSMS"))
        {
            int requestId = 123456;
            long phone = 13718780428;
            //long phone = 18069828910;
            string key = "123dda3";
            string sign = CryptographyExtensions.GetHashSha1($"{requestId}{phone}{key}");
            Debug.Log(sign);
            //ae1be6d7b2ed41093422d4eb2e3c4c82b469a879
            //ae1be6d7b2ed41093422d4eb2e3c4c82b469a879
            //demo.sendSMS(requestId, phone, sign);
        }
    }
}
#endif
public class loginApiDemo : MonoBehaviour
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginType">1手机验证码 2手机密码 10微信 11qq 12微博</param>
    /// <param name="phone">手机号</param>
    /// <param name="code">验证码或者密码</param>
    public void login(int loginType, long phone, int code, System.Action<string> onLogin)
    {
        string url = $"{ConstValue.login_route}/login?loginType={loginType}&phone={phone}&code={code}";
        HttpManager.HttpGet(url, onLogin);
    }
    public void onLogin(string json)
    {
        //"code": 0,
        //"msg": "OK",
        //"data": "e9fee037d2116b57495d820db1d8b310" //token
        var data = JsonMapper.ToObject<HttpCallback>(json);
        if (data.code == 0)
        {
            HttpManager.token = data.data;
            Debug.Log($"token={HttpManager.token}");
        }
        else
        {
            Debug.LogError($"err code={data.code} msg={data.msg}");
        }
    }

    /// <summary>
    /// 短信验证
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="phone">手机号</param>
    /// <param name="sign"></param>
    public void sendSMS(int requestId, long phone, string sign)
    {
        string url = $"{ConstValue.login_route}/sendSMS?requestId={requestId}&phone={phone}&sign={sign}";
        string result = RESTAPI.HttpGet(url, "POST");
        Debug.Log(result);
        //"code": 0,
        //"msg": "OK",
        //"data": null
    }
}
