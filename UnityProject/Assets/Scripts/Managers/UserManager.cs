using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(UserManager))]
public class UserManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数
        UserManager demo = (UserManager)target;
        if (GUILayout.Button("个人资料"))
        {
            Debug.Log($"Identifier={demo.localPlayer.identifier}, " +
                $"NickName={demo.localPlayer.nickName}, " +
                $"Gender={demo.localPlayer.gender}, " +
                $"Birthday={demo.localPlayer.birthday}, " +
                $"Gender={demo.localPlayer.allowType}, ");
        }
        if (GUILayout.Button("个人设置"))
        {
            Debug.Log(demo.localConfig.ToString());
        }
        if (GUILayout.Button("好友数"))
        {
            Debug.Log(demo.friends.Count);
            foreach (var friend in demo.friends) 
            {
                Debug.Log($"friend id={friend.Value.identifier} amount={friend.Value.amount}");
            }
        }
    }
}
#endif

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;
    public static string UserRoot
    {
        get
        {
            return $"{Application.persistentDataPath}/{Instance.localPlayer.identifier}";
        }
    }

    public TIMUserProfile localPlayer { get; set; }
    public UserConfig localConfig { get; set; }
    public Dictionary<long, TIMUserProfileExt> friends { get; set; } //本地维护好友列表
    public DataService ds;

    void Awake()
    {
        Instance = this;
        localPlayer = new TIMUserProfile();
        friends = new Dictionary<long, TIMUserProfileExt>();
    }

    void OnDestroy()
    {
        ds?.Close();
    }

    // 初始化本地数据库（sdk登录完成后调用）
    public void InitDatabase()
    {
        ds = new DataService($"{localPlayer.identifier}/tempDatabase.db"); //登录成功时已创建目录
    }

    // 个人根目录初始化
    public static void CreateUserRoot()
    {
        //Debug.Log($"个人根目录={UserRoot}, exist={Directory.Exists(UserRoot)}");
        if (!Directory.Exists(UserRoot))
        {
            Directory.CreateDirectory(UserRoot);
            Debug.Log($"<color=green>create folder path={UserRoot}</color>");
        }
    }

    //TODO: Update用户信息也使用全局通知，推送给所有监听的UI
    public UserManager updateUser(TIMUserProfile value) 
    {
        this.localPlayer = value;
        return this;
    }
    public UserManager updateIdentifier(string value)
    {
        localPlayer.identifier = value;
        return this;
    }
    public UserManager updateNick(string value)
    {
        localPlayer.nickName = value;
        return this;
    }
    public UserManager updateGender(int value)
    {
        localPlayer.gender = value;
        return this;
    }
    public UserManager updateBirthday(long value)
    {
        localPlayer.birthday = value;
        return this;
    }
    public UserManager updateAllowType(int value)
    {
        localPlayer.allowType = value;
        return this;
    }
    public UserManager updateFaceUrl(string value)
    {
        localPlayer.faceUrl = value;
        return this;
    }
    public UserManager updateLocation(string value)
    {
        localPlayer.location = value;
        return this;
    }
    public UserManager updateConfig(UserConfig value)
    {
        this.localConfig = value;
        return this;
    }
}
