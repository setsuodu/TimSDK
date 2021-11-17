using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MaterialUI;
using LitJson;

/// <summary>
/// 封装
/// </summary>
public partial class HttpManager : MonoBehaviour
{
    public static HttpManager Instance;

    public static string token;
    [SerializeField]
    protected const int HTTP_TIME_OUT = 3; //http超时时间
    protected static DialogProgress dialog;

    void Awake()
    {
        Instance = this;
    }

    public static void ShowDialog(string title)
    {
        dialog = DialogManager.ShowProgressCircular(title);
    }

    public static void Hide()
    {
        dialog?.Hide();
        dialog = null;
    }

    private IEnumerator AutoHide(float duration)
    {
        yield return new WaitForSeconds(duration);
        dialog?.Hide();
    }

    public static void HttpGet(string url, System.Action<string> action)
    {
        Instance.StartCoroutine(SendHttpGet(url, action)); //TODO: 多线程
    }

    public static void HttpPost(string url, WWWForm form, System.Action<string> action)
    {
        Instance.StartCoroutine(SendHttpPost(url, form, action)); //TODO: 多线程
    }

    //TODO: 暂时所有文件当做图片，之后做区分tmp
    public static void Download(string url, System.Action<byte[]> action = null, string uuid = null)
    {
        Instance.StartCoroutine(SendDownload(url, action, uuid));
    }

    public static void HttpUpload(string url, WWWForm form, System.Action<string> action)
    {
        Instance.StartCoroutine(SendUpload(url, form, action));
    }

    public static void HttpUploadQueue(string url, string[] pathArray, ContentType content_type, System.Action<string[]> action)
    {
        Instance.StartCoroutine(SendUploadQueue(url, pathArray, content_type, action));
    }

    private static IEnumerator SendHttpGet(string url, System.Action<string> action)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = HTTP_TIME_OUT;
        //while (!www.isDone) yield return null;
        yield return www.SendWebRequest();

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            ToastManager.Show(www.error, 0.5f, MaterialUIManager.UIRoot);
            dialog?.Hide();
            dialog = null;
            yield break;
        }
        
        if (www.responseCode == 200)
        {
            string result = www.downloadHandler.text;
            action?.Invoke(result);
            www.Dispose();
            //dialog?.Hide();
            //dialog = null;
        }
    }

    private static IEnumerator SendHttpPost(string url, WWWForm form, System.Action<string> action)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.timeout = HTTP_TIME_OUT;
        //while (!www.isDone) yield return null;
        yield return www.SendWebRequest();

        //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(postBytes);
        //www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            ToastManager.Show(www.error, 0.5f, MaterialUIManager.UIRoot);
            dialog?.Hide();
            dialog = null;
            yield break;
        }

        if (www.responseCode == 200)
        {
            string result = www.downloadHandler.text;
            action?.Invoke(result);
            www.Dispose();
            //dialog?.Hide();
            //dialog = null;
        }
    }

    private static IEnumerator SendDownload(string url, System.Action<byte[]> action = null, string uuid = null)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
            yield return null;
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError($"url={url}, err={www.error}");
            //TODO: 图片显示为×，读取该图片byte[]返回
            //byte[] err_image = File.ReadAllBytes("");
            //action?.Invoke(err_image);
            yield break;
        }
        byte[] bytes = www.bytes;
        www.Dispose();
        //string fileName = Path.GetFileName(url);
        string fileName = string.IsNullOrEmpty(uuid) ? Path.GetFileName(url) : uuid;
        string filePath = Path.Combine(ConstValue.image_root, fileName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"saved in {filePath}");
        action?.Invoke(bytes);
    }

    private static IEnumerator SendUpload(string url, WWWForm form, System.Action<string> action)
    {
        UnityWebRequest req = UnityWebRequest.Post(url, form);
        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
            Debug.Log(req.error);
        else
            Debug.Log("Uploaded Successfully");

        if (req.responseCode == 200)
        {
            string result = req.downloadHandler.text;
            //Debug.Log(result); //{"code":0,"msg":"OK","data":"http://aud.zd1312.com/null"}
            req.Dispose();

            action?.Invoke(result);
        }
    }

    private static IEnumerator SendUpload(string url, string path, WWWForm form, System.Action<string> action)
    {
        //string url = $"{ConstValue.api_route}/api/upload/article";
        //string path = "C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/Splash.png";
        //string path = "C:/Users/Administrator/AppData/LocalLow/setsuodu/TimSdk/2/FileStorage/Sound/2_123456.mp3";

        UnityWebRequest file = new UnityWebRequest();
        file = UnityWebRequest.Get(path);
        yield return file.SendWebRequest();
        form.AddBinaryData("file", file.downloadHandler.data);

        UnityWebRequest req = UnityWebRequest.Post(url, form);
        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
            Debug.Log(req.error);
        else
            Debug.Log("Uploaded Successfully");

        if (req.responseCode == 200)
        {
            string result = req.downloadHandler.text;
            Debug.Log(result);
            req.Dispose();
        }
    }

    // 多文件上传
    private static IEnumerator SendUploadQueue(string url, string[] pathArray, ContentType content_type, System.Action<string[]> action)
    {
        List<string> tmpArr = new List<string>();

        for (int i = 0; i < pathArray.Length; i++)
        {
            string filePath = pathArray[i];
            WWWForm form = new WWWForm();
            form.AddField("token", token);
            form.AddField("hash", Utils.getFilesMD5Hash(filePath));
            form.AddField("content_type", $"{content_type}");

            //yield return SendUpload(url, filePath, form, action);

            UnityWebRequest file = new UnityWebRequest();
            file = UnityWebRequest.Get(filePath);
            yield return file.SendWebRequest();
            form.AddBinaryData("file", file.downloadHandler.data);

            UnityWebRequest req = UnityWebRequest.Post(url, form);
            yield return req.SendWebRequest();
            if (req.isHttpError || req.isNetworkError)
            {
                Debug.LogError(req.error);
                //yield break; //1张错误，停止整个过程？
            }
            else
            {
                //Debug.Log($"Uploaded Successfully {i}");
                if (req.responseCode == 200)
                {
                    string result = req.downloadHandler.text;
                    req.Dispose();

                    //Debug.Log(result);
                    var obj = JsonMapper.ToObject<HttpCallback>(result);
                    tmpArr.Add(obj.data);
                }
            }
        }

        action?.Invoke(tmpArr.ToArray());
    }
}

/// <summary>
/// 业务
/// </summary>
public partial class HttpManager : MonoBehaviour
{
    //定义一个委托
    public delegate void CompleteDelegate(string info);
    //声明委托对象
    public static CompleteDelegate CallDelegate;
    //委托调用方法，也可以直接使用DelegateTest.Mydelegate()调用委托
    public static void OnHttpComplete(string info)
    {
        CallDelegate?.Invoke(info);
    }

    //TODO: 全部使用POST

    //POST/登录/取得token
    public static void login(int loginType, string phone, string code, Action<string> onLogin)
    {
        //string url = $"{ConstValue.login_route}/login?loginType={loginType}&phone={phone}&code={code}";
        //HttpGet(url, onLogin);
        string url = $"{ConstValue.login_route}/login";
        WWWForm form = new WWWForm();
        form.AddField("loginType", loginType);
        form.AddField("phone", phone);
        form.AddField("code", code);
        HttpPost(url, form, onLogin);
    }

    //POST/获取手机验证码
    public static void sendSMS(int requestId, string phone, Action<string> onSendSMS)
    {
        WWWForm form = new WWWForm();
        form.AddField("requestId", requestId);
        form.AddField("phone", phone);
        string key = "123dda3";
        string sign = CryptographyExtensions.GetHashSha1($"{requestId}{phone}{key}");
        form.AddField("sign", sign);
        string url = $"{ConstValue.login_route}/sendSMS";
        HttpPost(url, form, onSendSMS);
    }

    //POST/绑定手机
    public static void binding(string phone, string code, Action<string> onBinding)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("phone", phone);
        form.AddField("code", code);
        string url = $"{ConstValue.login_route}/phone/binding";
        HttpPost(url, form, onBinding);
    }



    //POST/使用userSig进行sdk登录
    public static void getSig(Action<string> onGetSig)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        string url = $"{ConstValue.api_route}/api/user/sig";
        HttpPost(url, form, onGetSig);
    }

    //POST/获取个人资料
    public static void myinfo(Action<string> onMyInfo)
    {
        //string url = $"{ConstValue.api_route}/api/user/myinfo?token={token}";
        //HttpGet(url, onMyInfo);
        //"_id":2, updateTime":1585299769011, "coin":0,"diamond":0,"exp":0,"flower":0,"flowerBalance":0,"role":0,"visitor":0,"nickname":"用户4","avatar":"http://avatar.zd1312.com/def/women_320_320.png","sex":0,"birthday":0,"signalType":0,"signal":null,"poseId":0,"phone":0,"vip":0,"wardrobeCnt":0,"imInit":1,"province":null,"city":null,"cpId":null}
        string url = $"{ConstValue.api_route}/api/user/myinfo";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        HttpPost(url, form, onMyInfo);
    }

    //POST/获取指定用户信息
    public static void info(int uid, Action<string> onInfo)
    {
        //string url = $"{ConstValue.api_route}/api/user/info?token={token}&uid={uid}";
        //HttpGet(url, onInfo);
        string url = $"{ConstValue.api_route}/api/user/info";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("uid", uid);
        HttpPost(url, form, onInfo);
    }

    //POST/请求好友列表
    public static void myFriends(int skip, int limit, Action<string> onMyFriends)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        string url = $"{ConstValue.api_route}/api/user/myFriends";
        HttpPost(url, form, onMyFriends);
    }

    //GET/加好友请求
    public static void toFriend(string rid, Action<string> onToFriend)
    {
        Debug.Log($"加好友请求 rid={rid}");
        string url = $"{ConstValue.api_route}/api/relationship/toFriend?token={token}&rid={rid}";
        HttpGet(url, onToFriend);
    }

    //POST/上传聊天记录
    public static void chatLogs(string chatLogs, Action<string> onChatLogs)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("chatLogs", chatLogs);
        string url = $"{ConstValue.api_route}/api/chat/logs";
        HttpPost(url, form, onChatLogs);
    }

    //获取所有可选标签在user/info中

    //POST/获取个人标签
    public static void userTags(int type, int skip, int limit, Action<string> onUserTags)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("type", type); //1:性格 2:兴趣
        form.AddField("skip", skip);
        form.AddField("limit", limit); //20
        string url = $"{ConstValue.api_route}/api/user/tags";
        HttpPost(url, form, onUserTags);
    }

    //POST/修改个人标签
    public static void setTag(string tags, Action<string> onSetTag)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("ids", tags); //1,2,3
        string url = $"{ConstValue.api_route}/api/user/setTag";
        HttpPost(url, form, onSetTag);
    }

    //POST/两两之间的关系
    public static void relationUser(string rid, Action<string> onRelationUser)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("rid", rid); //对方uid
        string url = $"{ConstValue.api_route}/api/user/relationUser";
        HttpPost(url, form, onRelationUser);
    }

    //POST/修改用户资料（昵称）
    public static void userUpdate(string nickname, Action<string> onUserUpdate)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("nickname", nickname);
        string url = $"{ConstValue.api_route}/api/user/update";
        HttpPost(url, form, onUserUpdate);
    }

    //POST/拉取个人配置（登录成功后请求）
    public static void userConfig(Action<string> onUserConfig)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        string url = $"{ConstValue.api_route}/api/user/config";
        HttpPost(url, form, onUserConfig);
    }

    //POST/个人通知设置
    public static void setNotice(int show, int recv, Action<string> onSetNotice)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("noticeShow", show); //通知显示      1:显示详情 2:只显示通知
        form.AddField("noticeRecv", recv); //接受消息提醒  1:全部     2:社区及聊天  3:聊天
        string url = $"{ConstValue.api_route}/api/user/setNotice";
        HttpPost(url, form, onSetNotice);
    }

    //POST/个人隐私设置
    public static void setPrivacy(int shield, int matched, int chatWho, Action<string> onSetPrivacy)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("shieldList", shield);        //1:屏蔽通讯录  2:不屏蔽
        form.AddField("onlineMatched", matched);    //1:在线可匹配  2:在线不可匹配
        form.AddField("chatWho", chatWho);          //1:全部        2:好友  3:cp
        string url = $"{ConstValue.api_route}/api/user/setPrivacy";
        HttpPost(url, form, onSetPrivacy);
    }

    //POST/申请Cp关系
    public static void applyCp(string rid, Action<string> onApplyCp)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("rid", rid);
        string url = $"{ConstValue.api_route}/api/relationship/applyCp";
        HttpPost(url, form, onApplyCp);
    }

    //POST/同意Cp申请
    public static void replyCp(string id, int status, Action<string> onReplyCp)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("id", id); //111_222
        form.AddField("status", status); //1 同意 其他不同意
        string url = $"{ConstValue.api_route}/api/relationship/replyCp";
        HttpPost(url, form, onReplyCp);
    }

    #region 社区

    //POST/推荐文章
    public static void discover(int skip, int limit, int type, Action<string> onDiscover)
    {
        //http://localhost:8082/api/article/discover
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&skip=0
        //&limit=20
        //&type=0
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("type", type);
        string url = $"{ConstValue.api_route}/api/article/discover";
        HttpPost(url, form, onDiscover);
    }

    //POST/附近文章
    public static void nearby(int skip, int limit, int type, double lat, double lng, Action<string> onNearby)
    {
        //http://localhost:8082/api/article/nearby
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&skip=0
        //&limit=20
        //&type=0
        //&lat=20.21
        //&lng=120.1101
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("type", type);
        form.AddField("lat", lat.ToString());
        form.AddField("lng", lng.ToString());
        string url = $"{ConstValue.api_route}/api/article/nearby";
        HttpPost(url, form, onNearby);
    }

    //POST/圈子文章
    public static void group(int skip, int limit, int type, int group, Action<string> onGroup)
    {
        //http://localhost:8082/api/article/group
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&skip=0
        //&limit=20
        //&type=0
        //&group=1
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("type", type);
        form.AddField("group", group);
        string url = $"{ConstValue.api_route}/api/article/group";
        HttpPost(url, form, onGroup);
    }

    //POST/发布评论
    public static void commentReply(int aid, int cid, string content, string pics, Action<string> onCommentReply)
    {
        //http://localhost:8082/api/article/comment/reply
        //?token=b3e51b941ceac50f10ac7c0c315bdea7fead092fedae1c91
        //&aid=4
        //&cid=1
        //&content=哈dfd哈嗒
        //&pics=
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("aid", aid);
        form.AddField("cid", cid); //楼的_id //回复贴主填0
        form.AddField("content", content);
        form.AddField("pics", pics);
        string url = $"{ConstValue.api_route}/api/article/comment/reply";
        HttpPost(url, form, onCommentReply);
    }

    //POST/文章评论列表
    public static void commentList(int skip, int limit, int aid, Action<string> onCommentList)
    {
        //http://localhost:8082/api/article/comment/list?
        //token=3ff72c8032cf7bd474432be615f078cda10e48c0d688e104
        //&skip=0
        //&limit=20
        //&aid=30 //文章id
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("aid", aid);
        string url = $"{ConstValue.api_route}/api/article/comment/list";
        HttpPost(url, form, onCommentList);
    }

    //POST/发布文章pub
    public class ArticlePub
    {
        public int type;        //TYPE_IMG = 1;// 图片 TYPE_VIDEO = 2;// 视频  TYPE_AUDIO = 3;// 音频 TYPE_TXT = 4;// 纯文TYPE_MODEL = 10;// 模型 TYPE_SIGN_IN = 100 签到
        public string content;  //文章内容
        public string cover;    //封面 //http://dmimg.5054399.com/allimg/pkm/pk/22.jpg
        public string pics;     //图集 //http://dmimg.5054399.com/allimg/pkm/pk/22.jpg,http://dmimg.5054399.com/allimg/pkm/pk/22.jpg
        public string dataUrl;  //视频等地址
        public string topics;   //话题
        public int pri;         //可见类型
        public double lat;      //纬度
        public double lng;      //经度
        public string addr;     //地址
        public int top;         //1:置顶 0:不置顶
        public int anonymous;   //1:匿名发布 0:不匿名

        public ArticlePub()
        {
            type = (int)TYPE.TYPE_TXT;
            content = string.Empty;
            cover = string.Empty;
            pics = string.Empty;
            dataUrl = string.Empty;
            topics = string.Empty;
            pri = (int)Pri.PRI_ALL;
            lat = 0;
            lng = 0;
            addr = string.Empty;
            top = 0;
            anonymous = 0;
        }
        //文章类型
        public enum TYPE
        {
            TYPE_IMG = 1, // 图片
            TYPE_VIDEO = 2, // 视频
            TYPE_AUDIO = 3, // 音频
            TYPE_TXT = 4, // 纯文
            TYPE_MODEL = 10, // 模型
            TYPE_SIGN_IN = 100 // 签到
        }
        //文章可见类型
        public enum Pri
        {
            //public static int PRI_ALL = 100;// 全部可见
            //public static int PRI_NOT_FRIEND = 4;// 好友不可见
            //public static int PRI_CP = 3;// 伴侣可见
            //public static int PRI_ZONE = 2;// 空间可见
            //public static int PRI_MYSELF = 1;// 自己可见
            PRI_MYSELF      = 1,    //自己可见
            PRI_ZONE        = 2,    //空间可见
            PRI_CP          = 3,    //伴侣可见
            PRI_NOT_FRIEND  = 4,    //好友不可见（仅陌生人可见）
            PRI_ALL         = 100,  //全部可见
        }

        public override string ToString()
        {
            return $"ArticlePub: " +
                $"type={type} " +
                $"content={content} " +
                $"cover={cover} " +
                $"pics={pics} " +
                $"dataUrl={dataUrl} " +
                $"topics={topics} " +
                $"pri={pri} " +
                $"lat={lat} " +
                $"lng={lng} " +
                $"addr={addr} " +
                $"top={top} " +
                $"anonymous={anonymous}";
        }
    }
    public static void pub(ArticlePub article, Action<string> onGroup)
    {
        //http://localhost:8082/api/article/pub
        //?token=51b19cf624e5162abcd8b8f441d9b065df3283742ae6f412
        //&type=1
        //&content=test
        //&cover=http://dmimg.5054399.com/allimg/pkm/pk/22.jpg
        //&pics=http://dmimg.5054399.com/allimg/pkm/pk/22.jpg,http://dmimg.5054399.com/allimg/pkm/pk/22.jpg
        //&dataUrl
        //&topics=最萌宝宝,可爱
        //&pri
        //&lat=20.1&lng=120.12
        //&addr=杭州
        //&top=0
        //&anonymous=0
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("type", article.type);
        form.AddField("content", article.content);
        form.AddField("cover", article.cover);
        form.AddField("pics", article.pics);
        form.AddField("dataUrl", article.dataUrl);
        form.AddField("topics", article.topics);
        form.AddField("pri", article.pri);
        form.AddField("lat", article.lat.ToString());
        form.AddField("lng", article.lng.ToString());
        form.AddField("addr", article.addr);
        form.AddField("top", article.top);
        form.AddField("anonymous", article.anonymous);
        string url = $"{ConstValue.api_route}/api/article/pub";
        HttpPost(url, form, onGroup);
    }

    //POST/删除文章
    public static void del(int id, Action<string> onGroup)
    {
        //http://localhost:8082/api/article/del
        //?token=f05443c9acfa209537184dfcc2473ab15b2e8bb42343ba38
        //&id=45
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("id", id);
        string url = $"{ConstValue.api_route}/api/article/del";
        HttpPost(url, form, onGroup);
    }

    //POST/文章话题
    public static void articleTopic(int skip, int limit, int topicId, Action<string> onArticleTopic)
    {
        //http://localhost:8082/api/article/topic
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&skip=0
        //&limit=20
        //&topicId=1
        string url = $"{ConstValue.api_route}/api/article/topic";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("topicId", topicId);
        HttpPost(url, form, onArticleTopic);
    }

    //POST/点赞文章
    public static void praise(int aid, Action<string> onPraise)
    {
        //http://localhost:8082/api/article/praise
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&aid=25
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("aid", aid); //文章id
        string url = $"{ConstValue.api_route}/api/article/praise";
        HttpPost(url, form, onPraise);
    }

    //POST/取消点赞文章
    public static void unPraise(int aid, Action<string> onUnPraise)
    {
        //http://localhost:8082/api/article/unPraise
        //?token=b3e51b941ceac50f10ac7c0c315bdea7fead092fedae1c91
        //&aid=4
        string url = $"{ConstValue.api_route}/api/article/unPraise";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("aid", aid); //文章id
        HttpPost(url, form, onUnPraise);
    }

    //POST/评论点赞
    public static void commentPraise(int cid, Action<string> onCommentPraise)
    {
        //http://localhost:8082/api/article/comment/praise
        //?token=b3e51b941ceac50f10ac7c0c315bdea7fead092fedae1c91
        //&cid=1
        string url = $"{ConstValue.api_route}/api/article/comment/praise";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("cid", cid); //楼的_id
        HttpPost(url, form, onCommentPraise);
    }

    //POST/评论点赞取消
    public static void commentUnPraise(int cid, Action<string> onCommentUnPraise)
    {
        //http://localhost:8082/api/article/comment/unPraise
        //?token=b3e51b941ceac50f10ac7c0c315bdea7fead092fedae1c91
        //&cid=1
        string url = $"{ConstValue.api_route}/api/article/comment/unPraise";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("cid", cid);
        HttpPost(url, form, onCommentUnPraise);
    }

    //POST/上传文件 //1图片，2视频，4纯文字
    public enum ContentType
    {
        img     = 1, //图片
        video   = 2, //视频
        audio   = 3, //音频
        txt     = 4, //纯文字
    }
    public static void upload(string filePath, byte[] data, ContentType content_type, Action<string> onUpload)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";

        string fileName = Path.GetFileName(filePath);
        Debug.Log($"fileName={fileName}");

        ///*
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("hash", Utils.getFilesMD5Hash(filePath));
        form.AddField("content_type", $"{content_type}");
        form.AddBinaryData("file", data);
        //form.headers.Add("", "");
        HttpUpload(url, form, onUpload);
        //*/
    }

    //POST/附近POI
    public static void nearAddr(double lat, double lng, Action<string> onNearAddr)
    {
        //http://localhost:8082/api/lbs/nearAddr
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&lat=39.984154
        //&lng=116.307490
        string url = $"{ConstValue.api_route}/api/lbs/nearAddr";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("lat", lat.ToString());
        form.AddField("lng", lng.ToString());
        HttpPost(url, form, onNearAddr);
    }

    //POST/lbs搜索
    public static void lbsSearch(string keyword, string region, int size, int index, Action<string> onLbsSearch)
    {
        //http://localhost:8082/api/lbs/search
        //?token=
        //&keyword=西湖
        //&region=杭州
        //&size=10
        //&index=1
        string url = $"{ConstValue.api_route}/api/lbs/search";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("keyword", keyword); //关键词
        form.AddField("region", region); //我的城市
        form.AddField("size", size);
        form.AddField("index", index);
        HttpPost(url, form, onLbsSearch);
    }

    //POST/话题列表
    public static void topicList(int skip, int limit, string name, Action<string> onTopicList)
    {
        //http://localhost:8082/api/topic/list
        //?token=ace71a62d99c56534747cea40f38287e119879a7a8d836bc
        //&skip=0
        //&limit=20
        //&name=清
        string url = $"{ConstValue.api_route}/api/topic/list";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        form.AddField("name", name); //关键词搜索
        HttpPost(url, form, onTopicList);
    }

    //POST/创建话题
    public static void topicCreate(string name, Action<string> onTopicCreate)
    {
        if (name.Length > 10)
        {
            Debug.LogError("话题长度超上限");
            return;
        }
        //http://192.168.2.200:8080/api/topic/create
        //?token=16ef06c1a19cfa7035976c2ec907a87c
        //&name=青花瓶
        string url = $"{ConstValue.api_route}/api/topic/create";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("name", name); //话题 //限10字
        HttpPost(url, form, onTopicCreate);
    }

    //POST/表白墙首页
    public static void showLoveTop(Action<string> onShowLoveTop)
    {
        //http://localhost:8082/api/showLove/top?token=
        string url = $"{ConstValue.api_route}/api/showLove/top";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        HttpPost(url, form, onShowLoveTop);
    }

    //POST/表白墙列表
    public static void showLoveList(int skip, int limit, Action<string> onShowLoveList)
    {
        //http://localhost:8082/api/showLove/list
        //?token=
        //&skip=
        //&limit
        string url = $"{ConstValue.api_route}/api/showLove/list";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("skip", skip);
        form.AddField("limit", limit);
        HttpPost(url, form, onShowLoveList);
    }

    //POST/表白墙表白
    public static void showLoveShout(long recvUid, string content, int count, Action<string> onShowLoveShout)
    {
        //http://localhost:8082/api/showLove/shout
        //?token=
        //&recvUid=
        //&content
        //&count=
        string url = $"{ConstValue.api_route}/api/showLove/list";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("recvUid", recvUid.ToString()); //表白接受方
        form.AddField("content", content); //表白内容
        form.AddField("count", count); //表白次数
        HttpPost(url, form, onShowLoveShout);
    }

    #endregion
}

/// <summary>
/// 搭搭
/// </summary>
public partial class HttpManager
{
    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }

    public static HttpWebResponse MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters)
    {
        string userAgent = "zdmz";
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;
        byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);
        return PostForm(postUrl, userAgent, contentType, formData);
    }

    private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    {
        Stream formDataStream = new MemoryStream();
        bool needsCLRF = false;

        foreach (var param in postParameters)
        {
            if (needsCLRF)
                formDataStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, Encoding.UTF8.GetByteCount("\r\n"));

            needsCLRF = true;

            if (param.Value is FileParameter)
            {
                FileParameter fileToUpload = (FileParameter)param.Value;

                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                    boundary,
                    param.Key,
                    fileToUpload.FileName ?? param.Key,
                    fileToUpload.ContentType ?? "application/octet-stream");

                formDataStream.Write(Encoding.UTF8.GetBytes(header), 0, Encoding.UTF8.GetByteCount(header));
                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
            }
            else
            {
                string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    boundary,
                    param.Key,
                    param.Value);
                formDataStream.Write(Encoding.UTF8.GetBytes(postData), 0, Encoding.UTF8.GetByteCount(postData));
            }
        }

        // Add the end of the request.  Start with a newline
        string footer = "\r\n--" + boundary + "--\r\n";
        formDataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));

        // Dump the Stream into a byte[]
        formDataStream.Position = 0;
        byte[] formData = new byte[formDataStream.Length];
        formDataStream.Read(formData, 0, formData.Length);
        formDataStream.Close();

        return formData;
    }

    private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
        request.Method = "POST";
        request.ContentType = contentType;
        request.UserAgent = userAgent;
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(formData, 0, formData.Length);
            requestStream.Close();
        }
        return request.GetResponse() as HttpWebResponse;
    }

    // 携带Header上传文件
    private static void UploadWithHead(string url, Dictionary<string, object> postParameters, Action<string> action = null)
    {
        HttpWebResponse response = MultipartFormDataPost(url, postParameters);
        using (Stream responseStream = response.GetResponseStream())
        {
            StreamReader redStm = new StreamReader(responseStream, Encoding.UTF8);
            string result = redStm.ReadToEnd();
            //Debug.Log(result);
            action?.Invoke(result);
        }
    }
    // 单图上传
    public static void UploadFile(string filePath, ContentType contentType, Action<string> action = null)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";
        byte[] data = File.ReadAllBytes(filePath);
        string fileName = Path.GetFileName(filePath);

        Dictionary<string, object> postParameters = new Dictionary<string, object>();
        postParameters.Add("token", token);
        postParameters.Add("hash", Utils.getFilesMD5Hash(filePath));
        postParameters.Add("content_type", $"{contentType}");
        postParameters.Add("file", new FileParameter(data, fileName, "multipart/file"));

        UploadWithHead(url, postParameters, action);
    }
}

/// <summary>
/// Task请求
/// </summary>
public partial class HttpManager
{
    // 返回值是string的Task //tmp测试
    public static async Task<string> PostAsync(string url)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.Accept = "application/json";

        try
        {
            var response = (HttpWebResponse)await request.GetResponseAsync();
            if (response == null || response.StatusCode != HttpStatusCode.OK)
                return string.Empty;

            string data;
            using (var responseStream = response.GetResponseStream())
            {
                using (var postStreamReader = new StreamReader(responseStream))
                {
                    data = await postStreamReader.ReadToEndAsync();
                    postStreamReader.Close();
                }
                responseStream.Close();
            }
            return data ?? string.Empty;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"err={ex}");
            return string.Empty;
        }
    }


    // 单文件上传业务
    public static async void UploadTask(string filePath, ContentType content_type, Action<string> action = null)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";
        byte[] data = File.ReadAllBytes(filePath);
        string fileName = Path.GetFileName(filePath);

        Dictionary<string, object> postParameters = new Dictionary<string, object>();
        postParameters.Add("token", token);
        postParameters.Add("hash", Utils.getFilesMD5Hash(filePath));
        postParameters.Add("content_type", $"{content_type}");
        postParameters.Add("file", new FileParameter(data, fileName, "multipart/file"));

        HttpWebResponse response = (HttpWebResponse)await FormDataPostAsync(url, postParameters);
        using (Stream responseStream = response.GetResponseStream())
        {
            StreamReader redStm = new StreamReader(responseStream, Encoding.UTF8);
            string result = redStm.ReadToEnd();
            action?.Invoke(result);
        }
    }
    // 多图上传业务
    public static async void UploadTasks(string[] files, ContentType content_type, Action<string[]> action = null)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";

        List<string> tmpArr = new List<string>();

        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];

            byte[] data = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("token", token);
            postParameters.Add("hash", Utils.getFilesMD5Hash(filePath));
            postParameters.Add("content_type", $"{content_type}");
            postParameters.Add("file", new FileParameter(data, fileName, "multipart/file"));

            HttpWebResponse response = (HttpWebResponse)await FormDataPostAsync(url, postParameters);
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader redStm = new StreamReader(responseStream, Encoding.UTF8);
                string result = redStm.ReadToEnd();
                //action?.Invoke(result);
                //Debug.Log(result);
                var obj = JsonMapper.ToObject<HttpCallback>(result);
                tmpArr.Add(obj.data);
            }
        }

        action?.Invoke(tmpArr.ToArray());
    }
    // 视频及封面上传
    public static async void UploadVideo(string coverPath, string videoPath, Action<string[]> action = null)
    {
        string url = $"{ConstValue.api_route}/api/upload/article";

        List<string> tmpArr = new List<string>();
        {
            Debug.Log("上传封面");

            byte[] data = File.ReadAllBytes(coverPath);
            string fileName = Path.GetFileName(coverPath);

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("token", token);
            postParameters.Add("hash", Utils.getFilesMD5Hash(coverPath));
            postParameters.Add("content_type", $"{ContentType.img}");
            postParameters.Add("file", new FileParameter(data, fileName, "multipart/file"));

            HttpWebResponse response = (HttpWebResponse)await FormDataPostAsync(url, postParameters);
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader redStm = new StreamReader(responseStream, Encoding.UTF8);
                string result = redStm.ReadToEnd();
                //tmpArr.Add(result);
                var obj = JsonMapper.ToObject<HttpCallback>(result);
                tmpArr.Add(obj.data);
            }
        }
        {
            Debug.Log("上传视频");

            byte[] data = File.ReadAllBytes(videoPath);
            string fileName = Path.GetFileName(videoPath);

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("token", token);
            postParameters.Add("hash", Utils.getFilesMD5Hash(videoPath));
            postParameters.Add("content_type", $"{ContentType.video}");
            postParameters.Add("file", new FileParameter(data, fileName, "multipart/file"));

            HttpWebResponse response = (HttpWebResponse)await FormDataPostAsync(url, postParameters);
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader redStm = new StreamReader(responseStream, Encoding.UTF8);
                string result = redStm.ReadToEnd();
                //tmpArr.Add(result);
                var obj = JsonMapper.ToObject<HttpCallback>(result);
                tmpArr.Add(obj.data);
            }
        }
        action?.Invoke(tmpArr.ToArray());
    }

    private static async Task<WebResponse> FormDataPostAsync(string postUrl, Dictionary<string, object> postParameters)
    {
        string userAgent = "zdmz";
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;
        byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
        request.Method = "POST";
        request.ContentType = contentType;
        request.UserAgent = userAgent;
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(formData, 0, formData.Length);
            requestStream.Close();
        }
        var response = await request.GetResponseAsync();
        return response;
    }
}