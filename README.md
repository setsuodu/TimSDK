# ImSDK

- [ ] 初始化
void Init(int sdkAppId);

- [ ] 登录
void Login(string identifier, string userSig);
- [ ] 登出
void Logout();
- [ ] 获取当前登录用户
string GetLoginUser();

- [ ] 文本消息发送
void SendTextElement(string content, string identifier, int type);
- [ ] 图片消息发送
void SendImageElement();

- [ ] 获取当前未读消息数量
long GetUnreadMessageNum(string identifier, int type);

// ************ 用户资料 ************ //
// 获取自己的资料
void GetSelfProfile(int type);
// 获取指定用户的资料（列表）
void GetUsersProfile(int type, string[] users);
// 修改自己的资料（默认字段）
void ModifySelfNick(string value);
void ModifySelfGender(int value);
void ModifySelfBirthday(long value);
void ModifySelfAllowType(string value);
void ModifySelfProfile();
// 修改自己的资料（自定义字段）
void ModifySelfProfileCustom();
// ************ 好友关系 ************ //
- [ ] 获取所有好友
void GetFriendList();
- [ ] 修改好友
void ModifyFriend();
- [ ] 添加好友
void AddFriend(string identifier, string word);
- [ ] 删除好友
void DeleteFriends(string identifier);
- [ ] 同意/拒绝好友申请
void DoFriendResponse();
- [ ] 校验好友关系
void CheckFriends();
// ************ 好友未决 ************ //
- [ ] 获取未决列表
void GetPendencyList();
// ************ 黑名单 ************ //
// ************ 好友分组 ************ //
// ************ 关系链变更系统通知 ************ //
// ************ 用户资料变更系统通知 ************ //
