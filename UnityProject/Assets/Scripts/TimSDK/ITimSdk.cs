public interface ITimSdk
{
    #region 初始化
    // 初始化
    void Init(int sdkAppId);
    #endregion

    #region 登录
    // 登录
    void Login(string identifier, string userSig);
    // 登出
    void Logout();
    // 获取当前登录用户
    string GetLoginUser();
    #endregion

    #region 消息收发
    // 文本消息发送
    void SendTextElement(string content, string identifier, int type);
    // 图片消息发送
    void SendImageElement(string fullPath, string identifier, int type);
    // 音频消息发送
    void SendSoundElement(string json, string identifier, int type);
    // 地理位置消息发送
    void SendLocationElement(string json, string identifier, int type);
    // 获取所有会话
    void GetConversationList();
    // 获取会话本地消息（从最近一条开始）
    void GetLocalMessageFirst(string peer, int count);
    // 获取会话本地消息（下一页）
    void GetLocalMessageNext(string peer, int count);
    // 获取会话漫游消息（从最近一条开始）
    void GetMessageFirst(string peer, int count);
    // 获取会话漫游消息（下一页）
    void GetMessageNext(string peer, int count);
    // 删除会话
    bool DeleteConversation(string peer, bool deleteLocal);
    // 同步获取会话最后的消息
    void GetLastMsg(string peer);
    // 删除会话本地消息
    void DeleteLocalMessage(string peer);
    //// 查找本地消息
    //void FindMessages(string peer);
    // 撤回消息
    void RevokeMessage(string peer, string json);
    // 删除消息
    bool RemoveMessage(string json);
    #endregion

    #region 未读计数
    // 获取当前未读消息数量
    long GetUnreadMessageNum(string identifier, int type);
    // 已读上报（单聊）
    void SetReadMessage(string peer);
    #endregion

    #region 群组相关
    #endregion

    #region 用户资料与关系链
    // ************ 用户资料 ************ //
    // 获取自己的资料
    void GetSelfProfile(int type);
    // 获取指定用户的资料（列表）
    void GetUsersProfile(int type, string usersjson);
    // 修改自己的资料（默认字段）
    void ModifySelfNick(string value);
    void ModifySelfGender(int value);
    void ModifySelfBirthday(long value);
    void ModifySelfAllowType(int value);
    void ModifySelfProfile();
    // 修改自己的资料（自定义字段）
    void ModifySelfProfileCustom();
    // ************ 好友关系 ************ //
    // 获取所有好友
    void GetFriendList();
    // 修改好友
    void ModifyFriend();
    // 添加好友
    void AddFriend(string identifier, string word);
    // 删除好友
    void DeleteFriends(int type, string usersjson);
    // 同意/拒绝好友申请
    void DoFriendResponse(string identifier, int type);
    // 校验好友关系
    void CheckFriends(int type, string usersjson);
    // ************ 好友未决 ************ //
    // 获取未决列表
    void GetPendencyList();
    // ************ 黑名单 ************ //
    // ************ 好友分组 ************ //
    // ************ 关系链变更系统通知 ************ //
    // ************ 用户资料变更系统通知 ************ //
    #endregion

    #region 离线推送
    #endregion

    //tmp

    #region 相册方法

    void OpenGallery();

    void OpenCamera();

    void OpenVideo();

    #endregion

    #region 定位方法

    bool CheckGPSIsOpen();

    void OpenGPSSetting();

    #endregion
}
