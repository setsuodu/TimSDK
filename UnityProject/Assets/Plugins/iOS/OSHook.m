#import <Foundation/Foundation.h>
#import "UnityAppController.h"
#import <ImSDK/ImSDK.h>
#import "TimCallback.h"

@interface ViewController : NSObject
@property(nonatomic,strong) NSString * gameObjectName;
@end
static ViewController * SharedInstance;
@implementation ViewController
+ (ViewController *)sharedInstance {
    if(SharedInstance == nil)
        SharedInstance = [[ViewController alloc] init];
    return SharedInstance;
}

// 新消息通知 //addMessageListener
- (void)onNewMessage:(NSArray*) msgs {
    NSLog(@"NewMessages: %@", msgs);
    [self parseMessageList:msgs Main:NS_RecvNewMessages];
}

// 网络事件通知 //connListener
- (void)onConnSucc {
    NSLog(@"Connect Succ");
}
- (void)onConnFailed:(int)code err:(NSString*)err {
      // code 错误码：具体参见错误码表
    NSLog(@"Connect Failed: code=%d, err=%@", code, err);
}
- (void)onDisconnect:(int)code err:(NSString*)err {
      // code 错误码：具体参见错误码表
    NSLog(@"Disconnect: code=%d, err=%@", code, err);
}

// 用户状态变更 //userStatusListener
- (void)onForceOffline {
    [self JsonCallback:NS_OnForceOffline Code:0 Data:@"onForceOffline"];
}
- (void)onUserSigExpired {
    [self JsonCallback:NS_OnUserSigExpired Code:0 Data:@"onUserSigExpired"];
}

// 已读回执 //messageReceiptListener //TODO: 把与该用户的C2C会话，显示为已读
- (void)onRecvMessageReceipts:(NSArray<TIMMessageReceipt*>*)receipts {
    for(int i = 0; i < receipts.count; i++) {
        TIMMessageReceipt * recp = receipts[i];
//        NSLog(@"对方已读 %@", recp.conversation.getReceiver);
        NSString * log = [[NSString alloc] initWithFormat:@"对方已读%@", recp.conversation.getReceiver];
        [self NativeCallback:log];
    }
}

//const int APPID = 1400326624;
UITextField* userField = nil;
UITextField* nickField = nil;

- (void)Init:(int)appid {
    // 通讯管理器
    TIMSdkConfig * sdkcfg = [[TIMSdkConfig alloc] init];
    sdkcfg.sdkAppId = appid;
    // 网络事件通知
    sdkcfg.connListener = self;
    sdkcfg.disableLogPrint = YES; //禁止在控制台打印 log
//    sdkcfg.logLevel = TIM_LOG_NONE;
    int result = [[TIMManager sharedInstance] initSdk:sdkcfg];
    [self NativeCallback: [NSString stringWithFormat:@"初始化成功 result=%u", result]];
    
    // 用户状态变更
    TIMUserConfig * usercfg = [[TIMUserConfig alloc] init];
    usercfg.userStatusListener = self;
    [usercfg setEnableReadReceipt:YES]; //开启消息已读回执
    usercfg.messageReceiptListener = self; //设置已读回执监听器
    [[TIMManager sharedInstance] setUserConfig:usercfg];
    
    // 新消息通知
    [[TIMManager sharedInstance] addMessageListener:self];
}

- (void)Login:(const char *)identifier Sig:(const char *)userSig {
    NSString* _identifier = [[NSString alloc] initWithUTF8String:identifier];
    NSString* _userSig = [[NSString alloc] initWithUTF8String:userSig];
    
    TIMLoginParam * login_param = [[TIMLoginParam alloc] init];
    login_param.identifier = _identifier;
    login_param.userSig = _userSig;
    //appidAt3rd App 用户使用 OAuth 授权体系分配的 Appid，在私有帐号情况下，填写与 SDKAppID 一样
    login_param.appidAt3rd = @"123456";
    
    [[TIMManager sharedInstance] login: login_param succ:^(){
        [self JsonCallback:NS_Login Code:0 Data:@"login succ"];
    } fail:^(int code, NSString * err) {
        [self JsonCallback:0 Code:code Data:err];
    }];
}
- (void)Logout {
    [[TIMManager sharedInstance] logout:^() {
        [self JsonCallback:NS_Logout Code:0 Data:@"logout succ"];
    } fail:^(int code, NSString * err) {
        [self JsonCallback:NS_Logout Code:code Data:err];
    }];
}
// 获取当前用户
- (NSString *)GetLoginUser {
    NSString * userid = [[TIMManager sharedInstance] getLoginUser];
//    NSLog(@"当前用户=%@", userid);
    return userid;
}

// 文本消息发送
- (void)SendTextElement:(const char *)identifier Text:(const char *)content Type:(int)type {
    NSString* _identifier = [[NSString alloc] initWithUTF8String:identifier];
    NSString* _content = [[NSString alloc] initWithUTF8String:content];
    
    TIMTextElem * text_elem = [[TIMTextElem alloc] init];
    [text_elem setText:_content];
    TIMMessage * msg = [[TIMMessage alloc] init];
    [msg addElem:text_elem];
    
    TIMConversation * con = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_identifier];
    [con sendMessage:msg succ:^() {
        TIMMessageResp * resp = [[TIMMessageResp alloc] init];
        resp.seq = msg.locator.seq;
        resp.rand = msg.locator.rand;
        resp.timestamp = [ViewController longLongFromDate: msg.timestamp];
        resp.isSelf = msg.isSelf;
        resp.isRead = msg.isReaded;
        resp.isPeerReaded = msg.isPeerReaded;
        resp.elemType = NS_Text;
        resp.subType = -1;
        resp.text = _content;
        NSString * json = [resp toJSONString];
        [self JsonCallback:NS_SendTextElement Code:0 Data:json];
    }fail:^(int code, NSString * err) {
        [self JsonCallback:NS_SendTextElement Code:code Data:err];
    }];
}
// 图片消息发送
- (void)SendImageElement:(const char *)identifier Text:(const char *)path Type:(int)type {
    NSString* _identifier = [[NSString alloc] initWithUTF8String:identifier];
    NSString* _path = [[NSString alloc] initWithUTF8String:path];
    
    TIMMessage * msg = [[TIMMessage alloc] init];
    TIMImageElem * image_elem = [[TIMImageElem alloc] init];
//    image_elem.path = @"/xxx/imgPath.jpg";
    image_elem.path = _path;
    
    NSFileManager * fileManager = [NSFileManager defaultManager];
    if(![fileManager fileExistsAtPath:_path]) {
        NSLog(@"ios文件不存在");
    } else {
        NSLog(@"ios文件存在");
    }
    
    [msg addElem:image_elem];
    
    TIMConversation * con = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_identifier];
    [con sendMessage:msg succ:^(){  //成功
        NSLog(@"SendMsg Succ");
    }fail:^(int code, NSString * err) {  //失败
        NSLog(@"SendMsg Failed:%d->%@", code, err);
    }];
}
// 获取所有会话
- (void)GetConversationList {
    NSArray * conversations = [[TIMManager sharedInstance] getConversationList];
    NSLog(@"current session list=%@ count=%lu", [conversations description], (unsigned long)conversations.count);
    
    NSMutableArray<TIMConversationResp *> * array = [[NSMutableArray alloc] init];
    for(int i = 0; i < conversations.count; i++) {
        TIMConversationResp * resp = [[TIMConversationResp alloc] init];
        TIMConversation * con = conversations[i];
        NSLog(@"self identifier=%@ type=%u", con.getReceiver, (int)con.getType);
        resp.peer = con.getReceiver;
        resp.type = (int)con.getType;
        [array addObject:resp];
    }
    NSString * json = [array toJSONString];
    [self JsonCallback:NS_GetConversationList Code:0 Data:json];
}

// 获取会话本地消息
NSMutableArray<TIMMessage*> * messages;
- (void)GetLocalMessage:(const char *)peer Count:(int)count Last:(TIMMessage*)lastMsg {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];
    TIMConversation * con = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_peer];
    [con getLocalMessage:count last:lastMsg succ:^(NSArray * msgList) {
        [self parseMessageList:msgList Main:NS_GetLocalMessage];
        for(int i = 0; i < msgList.count; i++) {
            [messages addObject:msgList[i]];
        }
    }fail:^(int code, NSString * err) {
        [self JsonCallback:NS_GetLocalMessage Code:code Data:err];
    }];
}
- (void)GetLocalMessageFirst:(const char *)peer Count:(int)count {
    messages = [[NSMutableArray alloc] init];
    [self GetLocalMessage:peer Count:count Last:NULL];
}
- (void)GetLocalMessageNext:(const char *)peer Count:(int)count {
    TIMMessage * lastMsg = NULL;
    if(messages.count > 0)
        lastMsg = messages[messages.count - 1];
    [self GetLocalMessage:peer Count:count Last:lastMsg];
}
- (void)GetMessage:(const char *)peer Count:(int)count Last:(TIMMessage*)lastMsg {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];
    TIMConversation * conversation = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_peer];
    [conversation getMessage:count last:lastMsg succ:^(NSArray * msgList) {
        [self parseMessageList:msgList Main:NS_GetMessage];
        for(int i = 0; i < msgList.count; i++) {
            [messages addObject:msgList[i]];
        }
    }fail:^(int code, NSString * err) {
        [self JsonCallback:NS_GetMessage Code:code Data:err];
    }];
}
- (void)GetMessageFirst:(const char *)peer Count:(int)count {
    messages = [[NSMutableArray alloc] init];
    [self GetMessage:peer Count:count Last:NULL];
}
- (void)GetMessageNext:(const char *)peer Count:(int)count {
    TIMMessage * lastMsg = NULL;
    if(messages.count > 0)
        lastMsg = messages[messages.count - 1];
    [self GetMessage:peer Count:count Last:lastMsg];
}
// 同步获取会话最后的消息
- (void)GetLastMsg:(const char *)peer {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];

    TIMConversation *conversation = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_peer];
    TIMMessage *lastMsg = [conversation getLastMsg];
    NSArray<TIMMessage *> * array = [[NSArray alloc] initWithObjects:lastMsg, nil];
    [self parseMessageList:array Main:NS_GetLastMsg];
}
// 删除会话
- (BOOL)DeleteConversation:(const char *)peer Local:(BOOL)deleteLocal {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];
    BOOL result = NO;
    if(deleteLocal)
        result = [[TIMManager sharedInstance] deleteConversationAndMessages:TIM_C2C receiver:_peer];
    else
        result = [[TIMManager sharedInstance] deleteConversation:TIM_C2C receiver:_peer];
    return result;
}
- (void)parseMessageList:(NSArray<TIMMessage *> *)msgs Main:(int)main {
    NSLog(@"解析消息 msgId=%u", main);
    
    NSMutableArray<TIMMessageResp *> * respList = [[NSMutableArray alloc] init];
    for (int i = 0; i < msgs.count; i++) {
        TIMMessage * message = msgs[i];
        
        int cnt = [message elemCount];
        for (int i = 0; i < cnt; i++) {
            TIMElem * elem = [message getElem:i];
            if ([elem isKindOfClass:[TIMTextElem class]]) {
                TIMTextElem * text_elem = (TIMTextElem * )elem;
                TIMMessageResp * resp = [[TIMMessageResp alloc] init];
                resp.seq = message.locator.seq;
                resp.rand = message.locator.rand;
                resp.timestamp = [ViewController longLongFromDate: message.timestamp];
                resp.isSelf = message.isSelf;
                resp.isRead = message.isReaded;
                resp.isPeerReaded = message.isPeerReaded;
                resp.elemType = NS_Text;
                resp.subType = -1;
                resp.text = text_elem.text;
                resp.param = @"";
                [respList addObject:resp];
            }
            else if ([elem isKindOfClass:[TIMImageElem class]]) {
                //接收到的图片保存的路径
//                NSString * pic_path = @"/xxx/imgPath.jpg";
//                NSString * imageSavePath = NSHomeDirectory();
                NSString * imageSavePath = [NSHomeDirectory()stringByAppendingPathComponent:@"Documents"];
                
                TIMImageElem * image_elem = (TIMImageElem * )elem;
                //遍历所有图片规格(缩略图、大图、原图)
                NSArray * imgList = [image_elem imageList];
                for (TIMImage * image in imgList) {
                    if(image.size == 0) continue; //过滤空图片
                    NSLog(@"保存图片 uuid=%@ size=%u type=%ld height=%u width=%u", image.uuid, image.size, (long)image.type, image.height, image.width);
                    
                    TIMImageResp * img = [TIMImageResp new];
                    img.type = image.type;
                    img.url = image.url;
                    img.uuid = image.uuid;
                    img.size = image.size;
                    img.height = image.height;
                    img.width = image.width;
                    NSString * jsonParam = [img toJSONString];
                    
//                    NSString * filePath = [[NSString alloc] initWithFormat:@"%@/Documents/%@", imageSavePath, image.uuid];
                    NSString * filePath = [[NSString alloc] initWithFormat:@"%@/%@", imageSavePath, image.uuid];
                    NSLog(@"filePath=%@", filePath);
                    // /var/mobile/Containers/Data/Application/F37565E1-22B3-4FE3-BFA3-2D85A0BC9D52/1_1400326624_test04_b31982f7fa967ae67fcede0be570245b.png
                    
                    TIMMessageResp * resp = [TIMMessageResp new];
                    resp.seq = message.locator.seq;
                    resp.rand = message.locator.rand;
                    resp.timestamp = [ViewController longLongFromDate: message.timestamp];
                    resp.isSelf = message.isSelf;
                    resp.isRead = message.isReaded;
                    resp.isPeerReaded = message.isPeerReaded;
                    resp.elemType = NS_Image;
                    resp.subType = -1;
                    resp.text = filePath;
                    resp.param = jsonParam;
                    [respList addObject:resp];
                    
                    //通过getImage接口下载图片二进制数据 //与消息分离
                    [image getImage:filePath succ:^(){  //接收成功
                        NSLog(@"SUCC: pic store to %@", filePath);
                    }fail:^(int code, NSString * err) {  //接收失败
                        NSLog(@"ERR: code=%d, err=%@", code, err);
                    }];
                }
            }
            else if ([elem isKindOfClass:[TIMSNSSystemElem class]]) {
                NSString * json = @"";
                TIMSNSSystemElem * system_elem = (TIMSNSSystemElem * )elem;
                if(system_elem == NULL) continue;
                switch ([system_elem type]) {
                    case TIM_SNS_SYSTEM_ADD_FRIEND:
                        NSLog(@"SNS.增加好友消息");
                        for (TIMSNSChangeInfo * info in [system_elem users]) {
                            NSLog(@"user %@ become friends", [info identifier]);
                        }
                        break;
                    case TIM_SNS_SYSTEM_DEL_FRIEND:
                        NSLog(@"SNS.删除好友消息");
                        for (TIMSNSChangeInfo * info in [system_elem users]) {
                            NSLog(@"user %@ delete friends", [info identifier]);
                        }
                        break;
                    case TIM_SNS_SYSTEM_ADD_FRIEND_REQ:
                        NSLog(@"SNS.增加好友申请");
                        for (TIMSNSChangeInfo * info in [system_elem users]) {
                            NSLog(@"user %@ request friends: reason=%@", [info identifier], [info wording]);
                        }
                        break;
                    case TIM_SNS_SYSTEM_DEL_FRIEND_REQ:
                        NSLog(@"SNS.删除未决申请");
                        break;
                    case TIM_SNS_SYSTEM_ADD_BLACKLIST:
                        NSLog(@"SNS.黑名单添加");
                        break;
                    case TIM_SNS_SYSTEM_DEL_BLACKLIST:
                        NSLog(@"SNS.黑名单删除");
                        break;
                    case TIM_SNS_SYSTEM_PENDENCY_REPORT:
                        NSLog(@"SNS.未决已读上报");
                        break;
                    case TIM_SNS_SYSTEM_SNS_PROFILE_CHANGE:
                        NSLog(@"SNS.关系链资料变更");
                        break;
                    default:
                        NSLog(@"ignore type");
                        break;
                }
            }
            else {
                NSLog(@"其他类型消息");
            }
        }
    }
    
    NSString * json = [respList toJSONString];
    [self JsonCallback:main Code:0 Data:json];
}
// 测试，打印消息条数
- (void)debugMessages {
    for(int i = 0; i < messages.count; i++) {
        NSString * str = [[NSString alloc] initWithFormat:@"debugMessages: [%u] %@", i, messages[i]];
        [self NativeCallback:str];
    }
}
- (TIMMessage*)getTIMMessage:(TIMMessageLocatorResp*)resp {
    for(int i = 0; i < messages.count; i++) {
        TIMMessage * msg = messages[i];
        if (msg.locator.seq == resp.seq
            && msg.locator.rand == resp.rand
            && [ViewController longLongFromDate: msg.timestamp] == resp.timestamp
            && msg.isSelf == resp.isSelf) {
            return msg;
        }
    }
    return NULL;
}
- (void)printMessage:(const char *)json {
    NSString * _json = [[NSString alloc] initWithUTF8String:json];
    NSError *error;
    TIMMessageLocatorResp * resp = [[TIMMessageLocatorResp alloc] initWithString:_json error:&error];
    TIMMessage * msg = [self getTIMMessage: resp];
    TIMElem * elem = [msg getElem:0];
    if ([elem isKindOfClass:[TIMTextElem class]]) {
        TIMTextElem * text_elem = (TIMTextElem * )elem;
        NSString* str = [[NSString alloc] initWithFormat:@"文本类型：%@", text_elem.text];
        [self NativeCallback:str];
    } else {
        [self NativeCallback:@"其他类型消息"];
    }
}
// 删除本地会话消息
- (BOOL)RemoveMessage:(const char *)json {
    NSString * _json = [[NSString alloc] initWithUTF8String:json];
    NSError *error;
    TIMMessageLocatorResp * resp = [[TIMMessageLocatorResp alloc] initWithString:_json error:&error];
    TIMMessage * msg = [self getTIMMessage: resp];
    
    BOOL result = [msg remove];
    return result;
}
// 撤回消息
- (void)RevokeMessage:(const char *)peer Json:(const char *)json {
    
}

// 未读计数
- (int)GetUnreadMessageNum:(const char *)peer {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];
    TIMConversation * con = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_peer];
    int result = [con getUnReadMessageNum];
    return result;
}
- (void)SetReadMessage:(const char *)peer {
    NSString* _peer = [[NSString alloc] initWithUTF8String:peer];
    TIMConversation * con = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:_peer];
    [con setReadMessage:NULL succ:^{
        [self NativeCallback:@"setReadMessage succ"];
    } fail:^(int code, NSString *msg) {
        NSString * log = [[NSString alloc] initWithFormat:@"setReadMessage failed, code=%u desc=%@", code, msg];
        [self NativeCallback:log];
    }];
}

// 获取自己的资料
- (void)GetSelfProfile:(int)type {
    if(type == 0) { //获取服务器保存的自己的资料
        [[TIMFriendshipManager sharedInstance] getSelfProfile:^(TIMUserProfile *profile) {
            [self GetProfileSucc:profile Type:NS_GetSelfProfile];
        } fail:^(int code, NSString *msg) {
            [self JsonCallback:NS_GetSelfProfile Code:code Data:msg];
        }];
    } else if (type == 1) { //获取本地保存的自己的资料
        TIMUserProfile * profile = [[TIMFriendshipManager sharedInstance] querySelfProfile];
        [self GetProfileSucc:profile Type:NS_GetSelfProfile];
    }
}
// 获取指定用户的资料
- (void)GetUsersProfile:(int)type Json:(const char *)json {
    NSError * error;
    NSString * _json = [[NSString alloc] initWithUTF8String:json];
    NSData * data = [_json dataUsingEncoding:NSUTF8StringEncoding];
    NSArray * users = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];
    
    [[TIMFriendshipManager sharedInstance] getUsersProfile:users forceUpdate:NO succ:^(NSArray * arr) {
        for (TIMUserProfile * profile in arr) {
            NSLog(@"user=%@", profile);
        }
    }fail:^(int code, NSString * err) {
        NSLog(@"GetFriendsProfile fail: code=%d err=%@", code, err);
    }];
}
// 修改自己的资料
- (void)ModifySelfProfile {
    [[TIMFriendshipManager sharedInstance] modifySelfProfile:@{TIMProfileTypeKey_Nick:nickField.text} succ:^() {
        NSLog(@"ModifySelfProfile succ");
    } fail:^(int code, NSString * err) {
        NSLog(@"ModifySelfProfile fail: code=%d err=%@", code, err);
    }];
}
- (void)GetProfileSucc:(TIMUserProfile *)userProfile Type:(int)main {
    TIMUserProfileResp * resp = [[TIMUserProfileResp alloc] init];
    resp.identifier = userProfile.identifier;
    resp.nickName = userProfile.nickname;
    resp.gender = userProfile.gender;
    resp.birthday = userProfile.birthday;
    resp.allowType = userProfile.allowType;
    NSString * json = [resp toJSONString];
    [self JsonCallback:main Code:0 Data:json];
}

// 获取所有好友
- (void)GetFriendList {
    [[TIMFriendshipManager sharedInstance] getFriendList:^(NSArray<TIMFriend *> *friends) {
        NSMutableArray<TIMUserProfileResp *> * users = [[NSMutableArray alloc] init];
        for (TIMFriend *friend in friends) {
            TIMUserProfileResp * resp = [[TIMUserProfileResp alloc] init];
            resp.identifier = friend.identifier;
            resp.nickName = [friend profile].nickname;
            resp.gender = [friend profile].gender;
            resp.birthday = [friend profile].birthday;
            resp.allowType = [friend profile].allowType;
            [users addObject:resp];
        }
        NSString * json = [users toJSONString];
        [self JsonCallback:NS_GetFriendList Code:0 Data:json];
    } fail:^(int code, NSString *msg) {
        [self JsonCallback:NS_GetFriendList Code:code Data:msg];
    }];
}
// 添加好友
- (void)AddFriend:(const char *)identifier Word:(const char *)word {
    NSString* _identifier = [[NSString alloc] initWithUTF8String:identifier];
    NSString* _word = [[NSString alloc] initWithUTF8String:word];
    
    TIMFriendRequest *q = [TIMFriendRequest new];
    q.identifier = _identifier; //加好友abc
    q.addWording = _word;
    q.addSource = @"AddSource_Type_iOS";
    q.remark = @"你是abc";
    [[TIMFriendshipManager sharedInstance] addFriend:q succ:^(TIMFriendResult *result) {
        if (result.result_code == 0) {
            NSLog(@"AddFriend succ");
        } else {
            NSLog(@"GetFriendList 异常: code=%ld err=%@", result.result_code, result.result_info);
            switch (result.result_code) {
                case 30010: //自己的好友数已达系统上限。
                    NSLog(@"自己的好友数已达系统上限");
                    break;
                case 30014: //对方的好友数已达系统上限。
                    NSLog(@"对方的好友数已达系统上限");
                    break;
                case 30539: //30539=A加B为好友，B需要验证，A、B之间形成未决关系，标识未决成功。
                    NSLog(@"等待对方验证"); //加好友时有效：等待好友审核同意
                    break;
                default:
                    break;
            }
        }
    } fail:^(int code, NSString *msg) {
        NSLog(@"GetFriendList 失败: code=%d err=%@", code, msg);
    }];
}
// 删除好友
- (void)DeleteFriends:(int)type Json:(const char *)json {
    NSError * error;
    NSString * _json = [[NSString alloc] initWithUTF8String:json];
    NSData * data = [_json dataUsingEncoding:NSUTF8StringEncoding];
    NSArray * users = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];
    
    // TIM_FRIEND_DEL_BOTH 指定删除双向好友
    [[TIMFriendshipManager sharedInstance] deleteFriends:users delType:TIM_FRIEND_DEL_BOTH succ:^(NSArray<TIMFriendResult *> *results) {
        for (TIMFriendResult * res in results) {
            if (res.result_code != TIM_FRIEND_STATUS_SUCC) {
                NSLog(@"deleteFriends failed: user=%@ result_code=%ld", res.identifier, (long)res.result_code); //0=操作成功，31704=删除好友时对方不是好友。
            }
            else {
                NSLog(@"deleteFriends succ: user=%@ result_code=%ld", res.identifier, (long)res.result_code);
            }
        }
    } fail:^(int code, NSString * err) {
        NSLog(@"deleteFriends failed: code=%d err=%@", code, err);
    }];
}
// 同意/拒绝好友申请
- (void)DoFriendResponse:(const char *)identifier Type:(int)type {
    
}
// 校验好友关系
- (void)CheckFriends:(int)type Json:(const char *)json {
    NSError * error;
    NSString * _json = [[NSString alloc] initWithUTF8String:json];
    NSData * data = [_json dataUsingEncoding:NSUTF8StringEncoding];
    NSArray * users = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];
    
    TIMFriendCheckInfo * checkInfo = [[TIMFriendCheckInfo alloc] init];
    checkInfo.users = users;
    checkInfo.checkType = 1; //1=单向好友，2=互为好友
    
    // 1=是好友，0=不是好友
    [[TIMFriendshipManager sharedInstance] checkFriends:checkInfo succ:^(NSArray<TIMCheckFriendResult *> *results) {
        NSMutableArray<TIMCheckFriendResultResp*>* array = [NSMutableArray new];
        for(int i = 0; i < results.count; i++) {
            TIMCheckFriendResultResp * resp = [TIMCheckFriendResultResp new];
            resp.result_code = (int)results[i].result_code;
            resp.identifier = results[i].identifier;
            resp.result_info = results[i].result_info;
            resp.resultType = results[i].resultType;
            [array addObject:resp];
//            NSLog(@"CheckFriends succ code=%u, id=%@, info=%@, resultType=%u", resp.result_code, resp.identifier, resp.result_info, resp.resultType);
        }
        NSString * json = [array toJSONString];
        [self JsonCallback:NS_CheckFriends Code:0 Data:json];
    } fail:^(int code, NSString *msg) {
        [self JsonCallback:NS_CheckFriends Code:code Data:msg];
    }];
}
- (void)GetPendencyList {
    
}

// 获取系统版本
- (NSString *)version {
    return [NSString stringWithFormat:@"IOS %.2f",[[[UIDevice currentDevice] systemVersion] floatValue]];
}
- (void)getVersion {
    NSString * res = [self version];
    NSLog(@"version=%@", res); //IOS 12.20
}
// 回调
- (void)NativeCallback:(NSString*)data {
    NSLog(@"[OC] gameObjectName=%@", self.gameObjectName);
    const char * _gameObjectName = [self.gameObjectName UTF8String];
    const char * _msg = [data UTF8String];
    UnitySendMessage(_gameObjectName, "NativeCallback", _msg);
}
- (void)JsonCallback:(int)msg Code:(int)code Data:(NSString*)data {
    const char * _gameObjectName = [self.gameObjectName UTF8String];
    
    TimCallback * callback = [[TimCallback alloc] init];
    callback.msg = msg;
    callback.code = code;
    callback.data = data;
    NSString * json = [callback toJSONString];
    const char * _json = [json UTF8String];
    char* copyJson = cStringCopy(_json);
    
    UnitySendMessage(_gameObjectName, "JsonCallback", copyJson);
}

char * cStringCopy(const char* string) {
    if(string == NULL) return NULL;
    char * res = (char *) malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}
//NSDate -> long long:
+ (long long)longLongFromDate:(NSDate*)date{
    return [date timeIntervalSince1970] * 1000;
}
//long long -> NSDate:
+ (NSDate*)dateFromLongLong:(long long)msSince1970{
    return [NSDate dateWithTimeIntervalSince1970:msSince1970 / 1000];
}
@end

#ifdef __cplusplus
extern "C" {
#endif
    
    // 实例化
    void GetInstance(const char* gameObjectName) {
        NSLog(@"[extern] GetInstance gameObjectName=%s", gameObjectName);
        NSString* _gameObjectName = [[NSString alloc] initWithUTF8String:gameObjectName];
        ViewController * controller = [ViewController sharedInstance];
        controller.gameObjectName = _gameObjectName;
    }
    
    // 初始化
    void Init(int sdkAppId) {
        [[ViewController sharedInstance] Init:sdkAppId];
    }
    
    // 登录
    void Login(const char* identifier, const char* userSig) {
        [[ViewController sharedInstance] Login:identifier Sig:userSig];
    }
    void Logout() {
        [[ViewController sharedInstance] Logout];
    }
    char* GetLoginUser() {
        NSString * identifier = [[ViewController sharedInstance] GetLoginUser];
        const char * str = [identifier UTF8String];

        char* copyStr = cStringCopy(str);
        return copyStr;
    }
    
    // 消息收发
    void SendTextElement(const char* content, const char* identifier, int type) {
        NSLog(@"[extern] SendTextElement  content=%s identifier=%s type=%u", content, identifier, type);
        [[ViewController sharedInstance] SendTextElement:identifier Text:content Type:type];
    }
    void SendImageElement(const char* path, const char* identifier, int type) {
        NSLog(@"[extern] SendImageElement  path=%s identifier=%s type=%u", path, identifier, type);
        [[ViewController sharedInstance] SendImageElement:identifier Text:path Type:type];
    }
    void GetConversationList() {
        NSLog(@"[extern] GetConversationList");
        [[ViewController sharedInstance] GetConversationList];
    }
    void GetLocalMessageFirst(const char* peer, int count) {
        NSLog(@"[extern] GetLocalMessageFirst peer=%s", peer);
        [[ViewController sharedInstance] GetLocalMessageFirst:peer Count:count];
    }
    void GetLocalMessageNext(const char* peer, int count) {
        NSLog(@"[extern] GetLocalMessageNext peer=%s", peer);
        [[ViewController sharedInstance] GetLocalMessageNext:peer Count:count];
    }
    void GetMessageFirst(const char* peer, int count) {
        NSLog(@"[extern] GetMessageFirst peer=%s count=%u", peer, count);
        [[ViewController sharedInstance] GetMessageFirst:peer Count:count];
    }
    void GetMessageNext(const char* peer, int count) {
        NSLog(@"[extern] GetMessageNext peer=%s count=%u", peer, count);
        [[ViewController sharedInstance] GetMessageNext:peer Count:count];
    }
    void GetLastMsg(const char* peer) {
        NSLog(@"[extern] GetLastMsg peer=%s", peer);
        [[ViewController sharedInstance] GetLastMsg:peer];
    }
    void debugMessages() {
        NSLog(@"[extern] debugMessages");
        [[ViewController sharedInstance] debugMessages];
    }
    void printMessage(const char* json) {
        NSLog(@"[extern] printMessage json=%s", json);
        [[ViewController sharedInstance] printMessage:json];
    }
    BOOL DeleteConversation(const char * peer, BOOL deleteLocal) {
        NSLog(@"[extern] DeleteConversation peer=%s deleteLocal=%i", peer, deleteLocal);
        
        BOOL result = [[ViewController sharedInstance] DeleteConversation:peer Local:deleteLocal];
        return result;
    }
    BOOL RemoveMessage(const char* json) {
        NSLog(@"[extern] RemoveMessage json=%s", json);
        BOOL result = [[ViewController sharedInstance] RemoveMessage:json];
        
        return result;
    }
    void RevokeMessage(const char * peer, const char * json) {
        NSLog(@"[extern] RevokeMessage json=%s", json);
        [[ViewController sharedInstance] RevokeMessage:peer Json:json];
    }
    
    // 未读计数
    int GetUnreadMessageNum(const char* peer, int type) {
        NSLog(@"[extern] GetUnreadMessageNum peer=%s", peer);
        int num = [[ViewController sharedInstance] GetUnreadMessageNum:peer];
        return num;
    }
    // 已读上报（单聊）
    void SetReadMessage(const char * peer) {
        NSLog(@"[extern] SetReadMessage peer=%s", peer);
        [[ViewController sharedInstance] SetReadMessage:peer];
    }
    
    // 群组相关
    
    // 用户资料与关系链
    void GetSelfProfile(int type) {
        NSLog(@"[extern] GetSelfProfile");
        [[ViewController sharedInstance] GetSelfProfile: type];
    }
    void GetUsersProfile(int type, const char * usersjson) {
        NSLog(@"[extern] GetUsersProfile type=%u json=%s", type, usersjson);
        [[ViewController sharedInstance] GetUsersProfile: type Json:usersjson];
    }
    void GetFriendList() {
        NSLog(@"[extern] GetFriendList");
        [[ViewController sharedInstance] GetFriendList];
    }
    void AddFriend(const char * identifier, const char * word) {
        NSLog(@"[extern] AddFriend");
        [[ViewController sharedInstance] AddFriend:identifier Word:word];
    }
    void DeleteFriends(int type, const char * usersjson) {
        NSLog(@"[extern] DeleteFriends type=%u json=%s", type, usersjson);
        [[ViewController sharedInstance] DeleteFriends:type Json:usersjson];
    }
    void DoFriendResponse() {
        NSLog(@"[extern] DoFriendResponse");
//        [[ViewController sharedInstance] DoFriendResponse];
    }
    void CheckFriends(int type, const char * usersjson) {
        NSLog(@"[extern] CheckFriends type=%u json=%s" ,type, usersjson);
        [[ViewController sharedInstance] CheckFriends:type Json:usersjson];
    }
    void GetPendencyList() {
        NSLog(@"[extern] GetPendencyList");
        [[ViewController sharedInstance] GetPendencyList];
    }
    
#ifdef __cplusplus
}
#endif
