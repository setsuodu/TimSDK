using SQLite4Unity3d;

public enum TimSdkMessage
{
    Login               = 0,
    Logout              = 1,
    GetSelfProfile      = 2,
    GetUsersProfile     = 3,
    AddFriend           = 4,
    DeleteFriends       = 5,
    DoFriendResponse    = 6,
    CheckFriends        = 7,
    GetPendencyList     = 8,
    GetFriendList       = 9,
    SendTextElement     = 10, //发送(文字/图片/音频)信息
    RecvNewMessages     = 11, //接收(文字/图片/音频/SNS)信息
    ModifySelfProfile   = 12,
    OnForceOffline      = 13, //顶号
    OnUserSigExpired    = 14,
    SetReadMessage      = 15, //对方已读（消息界面/聊天界面接收）
    GetConversationList = 16,
    RevokeMessage       = 17,
    GetLocalMessage     = 18, //本地存储消息 //TODO: 和漫游消息合并
    GetMessage          = 19, //漫游消息
    GetLastMsg          = 20, //最近一条消息
    Download            = 21, //消息文件下载
    Image               = 22, //相册/拍照 返回图片路径
    Sound               = 23, //相册/录制 返回音频路径
    Video               = 24, //相册/拍摄 返回视频路径
}

public enum TIMElemType
{
    Invalid     =(0),
    Text        =(1),
    Image       =(2),
    Sound       =(3),
    Custom      =(4),
    File        =(5),
    GroupTips   =(6),
    Face        =(7),
    Location    =(8),
    GroupSystem =(9),
    SNSTips     =(10),
    ProfileTips =(11),
    Video       =(12),
}
public enum TIMImageType
{
    Original    = (0),
    Thumb       = (1),
    Large       = (2),
}
public enum TIMConversationType 
{
    Invalid     = (0),
    C2C         = (1),
    Group       = (2),
    System      = (3),
}

// 账号登录
public interface ITimUser
{
    string Identifier { get; set; }
    string UserSig { get; set; }
}
public class TimUser : ITimUser
{
    public string Identifier { get; set; }
    public string UserSig { get; set; }
    public TimUser() { }
    public TimUser(string v1, string v2)
    {
        this.Identifier = v1;
        this.UserSig = v2;
    }
}

// TimSdk回调消息
public class TimCallback
{
    public int msg;         // 消息号
    public int code;        // 错误码
    public string data;     // 返回参数
    public TimCallback() { }
    public TimCallback(int _msg, int _code, string _data)
    {
        this.msg = _msg;
        this.code = _code;
        this.data = _data;
    }
    public TimCallback(TimSdkMessage _msg, int _code, string _data)
    {
        this.msg = (int)_msg;
        this.code = _code;
        this.data = _data;
    }
}

public class TIMUserProfileType : TIMUserProfile
{
    public const string TIM_PROFILE_TYPE_KEY_NICK = "Tag_Profile_IM_Nick";
    public const string TIM_PROFILE_TYPE_KEY_FACEURL = "Tag_Profile_IM_Image";
    public const string TIM_PROFILE_TYPE_KEY_ALLOWTYPE = "Tag_Profile_IM_AllowType";
    public const string TIM_PROFILE_TYPE_KEY_GENDER = "Tag_Profile_IM_Gender";
    public const string TIM_PROFILE_TYPE_KEY_BIRTHDAY = "Tag_Profile_IM_BirthDay";
    public const string TIM_PROFILE_TYPE_KEY_LOCATION = "Tag_Profile_IM_Location";
    public const string TIM_PROFILE_TYPE_KEY_LANGUAGE = "Tag_Profile_IM_Language";
    public const string TIM_PROFILE_TYPE_KEY_LEVEL = "Tag_Profile_IM_Level";
    public const string TIM_PROFILE_TYPE_KEY_ROLE = "Tag_Profile_IM_Role";
    public const string TIM_PROFILE_TYPE_KEY_SELFSIGNATURE = "Tag_Profile_IM_SelfSignature";
    public const string TIM_PROFILE_TYPE_KEY_CUSTOM_PREFIX = "Tag_Profile_Custom_";
}
public class TIMUserProfile
{
    [PrimaryKey]
    public string identifier { get; set; }
    public string nickName { get; set; }
    public int gender { get; set; }
    public long birthday { get; set; }
    public int allowType { get; set; }
    public string faceUrl { get; set; }
    public string location { get; set; }
    //public string allowType;
    //public long birthday;
    //public object customInfo;
    //public object customInfoUint;
    //public string faceUrl;
    //public int gender;
    //public string identifier;
    //public int language;
    //public int level;
    //public string location;
    //public string nickName;
    //public int role;
    //public string selfSignature;
}
[System.Serializable]
public class TIMUserProfileExt : TIMUserProfile
{
    public int relation { get; set; } //关系
    public long amount { get; set; } //亲密度
    public string lastMsg { get; set; }
    public long timestamp { get; set; }
    public long unreadMessageNum { get; set; }

    public override string ToString()
    {
        return $"TIMUserProfile: identifier={identifier}, nickName={nickName}";
    }
}
[System.Serializable]
public class ConversationItemData
{
    public long identifier;
    public string nickName;
    public string faceUrl;
    public long amount; //亲密度
    public string lastMsg;
    public long timestamp;
    public long unreadMessageNum;
}

// addFriend返回
public class TIMFriendResult
{
    public string Identifier;
    public int ResultCode;
    public string ResultInfo;
}

// 加好友隐私选项
public enum TIMFriendAllowType
{
    TIM_FRIEND_INVALID      = 0,
    TIM_FRIEND_ALLOW_ANY    = 1,
    TIM_FRIEND_DENY_ANY     = 2,
    TIM_FRIEND_NEED_CONFIRM = 3,
}

// 会话
public class TIMConversation
{
    public string peer;
    public int type; //Invalid(0), C2C(1), Group(2),System(3);
}

// 系统SNS消息
public class TIMSNSSystemType
{
    public const int INVALID = 0;
    public const int TIM_SNS_SYSTEM_ADD_FRIEND = 1;
    public const int TIM_SNS_SYSTEM_DEL_FRIEND = 2;
    public const int TIM_SNS_SYSTEM_ADD_FRIEND_REQ = 3;
    public const int TIM_SNS_SYSTEM_DEL_FRIEND_REQ = 4;
    public const int TIM_SNS_SYSTEM_ADD_BLACKLIST = 5;
    public const int TIM_SNS_SYSTEM_DEL_BLACKLIST = 6;
    public const int TIM_SNS_SYSTEM_PENDENCY_REPORT = 7;
    public const int TIM_SNS_SYSTEM_SNS_PROFILE_CHANGE = 8;
    public const int TIM_SNS_SYSTEM_ADD_RECOMMEND = 9;
    public const int TIM_SNS_SYSTEM_DEL_RECOMMEND = 10;
    public const int TIM_SNS_SYSTEM_ADD_DECIDE = 11;
    public const int TIM_SNS_SYSTEM_DEL_DECIDE = 12;
    public const int TIM_SNS_SYSTEM_RECOMMEND_REPORT = 13;
    public const int TIM_SNS_SYSTEM_DECIDE_REPORT = 14;
    public TIMSNSSystemType() { }
}
public class TIMFriendPendencyInfo
{
    public string fromUser;
    public string addSource;
    public string fromUserNickName;
    public string addWording;
}

// getPendencyList返回
public class TIMFriendPendencyResponse 
{

}
[System.Serializable]
public class TIMFriendPendencyItem
{
    public string identifier;
    public long addTime;
    public string addSource;
    public string addWording;
    public string nickname;
    public int type;
}

// doFriendResponse返回
public class TIMFriendStatus
{
    public const int TIM_FRIEND_STATUS_UNKNOWN = -1;
    public const int TIM_FRIEND_STATUS_SUCC = 0;
    public const int TIM_FRIEND_PARAM_INVALID = 30001;
    public const int TIM_ADD_FRIEND_STATUS_SELF_FRIEND_FULL = 30010;
    public const int TIM_UPDATE_FRIEND_GROUP_STATUS_MAX_GROUPS_EXCEED = 30011;
    public const int TIM_ADD_FRIEND_STATUS_THEIR_FRIEND_FULL = 30014;
    public const int TIM_ADD_FRIEND_STATUS_IN_SELF_BLACK_LIST = 30515;
    public const int TIM_ADD_FRIEND_STATUS_FRIEND_SIDE_FORBID_ADD = 30516;
    public const int TIM_ADD_FRIEND_STATUS_IN_OTHER_SIDE_BLACK_LIST = 30525;
    public const int TIM_ADD_FRIEND_STATUS_PENDING = 30539;
    public const int TIM_DEL_FRIEND_STATUS_NO_FRIEND = 31704;
    public const int TIM_RESPONSE_FRIEND_STATUS_NO_REQ = 30614;
    public const int TIM_ADD_BLACKLIST_FRIEND_STATUS_IN_BLACK_LIST = 31307;
    public const int TIM_DEL_BLACKLIST_FRIEND_STATUS_NOT_IN_BLACK_LIST = 31503;
    public const int TIM_ADD_FRIEND_GROUP_STATUS_GET_SDKAPPID_FAILED = 32207;
    public const int TIM_ADD_FRIEND_GROUP_STATUS_NOT_FRIEND = 32216;
    public const int TIM_UPDATE_FRIEND_GROUP_STATUS_GET_SDKAPPID_FAILED = 32511;
    public const int TIM_UPDATE_FRIEND_GROUP_STATUS_ADD_NOT_FRIEND = 32518;
    public const int TIM_UPDATE_FRIEND_GROUP_STATUS_ADD_ALREADY_IN_GROUP = 32519;
    public const int TIM_UPDATE_FRIEND_GROUP_STATUS_DEL_NOT_IN_GROUP = 32520;
}

// 本地/漫游消息返回
public class TIMMessageResp
{
    public long seq;
    public long rand;
    public long timestamp;
    public bool isSelf;
    public bool isRead;
    public bool isPeerReaded;
    public int elemType; //TIMElemType
    public int subType; //SNS子消息，-1代表没有子消息
    public string text; //文本消息内容，图片消息路径
    public string param; //备注信息（Json，根据用途解析）

    public TIMMessageResp setMessage(long _seq, long _rand, long _timestamp, bool _isself, bool _isread, bool _ispeerreaded)
    {
        this.seq = _seq;
        this.rand = _rand;
        this.timestamp = _timestamp;
        this.isSelf = _isself;
        this.isRead = _isread;
        this.isPeerReaded = _ispeerreaded;
        return this;
    }
    public TIMMessageResp setType(int main, int sub)
    {
        this.elemType = main;
        this.subType = sub;
        return this;
    }
    public TIMMessageResp setText(string value)
    {
        this.text = value;
        return this;
    }
    public TIMMessageResp setParam(string value)
    {
        this.param = value;
        return this;
    }
}

// checkFriends返回
public class TIMCheckFriendResult
{
    public string identifier;
    public int resultCode;
    public string resultInfo;
    public int resultType;
}

public class TIMImageResp
{
    public int type; //Original(0), Thumb(1), Large(2)
    public string url;
    public string uuid;
    public long size;
    public long height;
    public long width;
}

// 查询消息四要素
public class TIMMessageLocatorResp
{
    public long seq;
    public long rand;
    public long timestamp;
    public bool isSelf;
}

// 语音消息
public class TIMSoundElemResp
{
    public string uuid;
    public string path;
    public long duration;
}

// 地理位置
public class TIMLocationElemResp
{
    public string desc;
    public double longitude;
    public double latitude;
}

// 消息文件下载
public class TIMDownloadResp
{
    public int elemType;
    public string uuid;
    public string url;
}
