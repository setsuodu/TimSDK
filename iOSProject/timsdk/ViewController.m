#import "ViewController.h"
#import <ImSDK/ImSDK.h>

@interface ViewController ()
@end
@implementation ViewController

// 新消息通知
- (void)onNewMessage:(NSArray*) msgs {
    NSLog(@"NewMessages: %@", msgs);
}

// 网络事件通知
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

// 用户状态变更
- (void)onForceOffline {
    NSLog(@"==>>force offline!!!");
}
- (void)onUserSigExpired {
    NSLog(@"==>>userSig expired!!!");
}

const int APPID = 1400326624;
UITextField* selfField = nil;
UITextField* userField = nil;
UITextField* nickField = nil;
UITextField* chatField = nil;

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    
    // 分割线
    [self createTableView:65];
    [self initBtn]; //60
    // 分割线
    [self createTableView:145];
    selfField = [self createTextField:20:100];
    selfField.text = @"test01";
    [self loginBtn]; //140
    [self logoutBtn]; //140
    [self getLoginUserBtn]; //140
    // 分割线
    [self createTableView:225];
    [self getSelfProfileBtn]; //220
    [self getUsersProfileBtn]; //220
    nickField = [self createTextField:20:220];
    [self modifySelfProfileBtn];
    // 分割线
    [self createTableView:345];
    [self getFriendListBtn]; //260
    userField = [self createTextField:20:300];
    userField.text = @"test02";
    [self addFriendBtn]; //300
    [self deleteFriendsBtn]; //300
    [self checkFriendsBtn]; //340
    // 分割线
    [self createTableView:385];
    chatField = [self createTextField:20:380];
    [self sendTextElementBtn]; //340
}

- (UIButton *)createButton:(CGFloat)x:(CGFloat)y title:(NSString*)title {
    int width = 100;
    int width2 = (int)title.length * 20;
    if(width2 > width) width = width2;
    UIButton *button = [[UIButton alloc] initWithFrame:CGRectMake(x, y, width, 30)];
    [button setBackgroundColor:[UIColor lightGrayColor]];
    [button setTitle:title forState:UIControlStateNormal];
    [button setTitleColor:[UIColor redColor] forState:UIControlStateHighlighted];
    return button;
}
// 分割线
- (UITableView *)createTableView:(CGFloat)y {
    UITableView *separator = [[UITableView alloc] init];
    int screenWidth = [[UIScreen mainScreen] bounds].size.width;
    separator.frame = CGRectMake(0, y, screenWidth, 30);
    separator.separatorStyle = UITableViewCellSeparatorStyleSingleLine;
    separator.separatorColor = UIColor.greenColor;
    separator.rowHeight = 30;
    [self.view addSubview:separator];
    return separator;
}
- (UITextField *)createTextField:(CGFloat)x:(CGFloat)y {
    UITextField *textField = [[UITextField alloc] init];
    textField.frame = CGRectMake(x, y, 100, 30);
    textField.autocapitalizationType = UITextAutocapitalizationTypeNone; //设置首字母小写
    textField.placeholder = [[NSString alloc] initWithFormat:@"input Field"];
    textField.borderStyle = UITextBorderStyleRoundedRect;
    textField.layer.cornerRadius = 15.0;
    //.h中添加UITextField委托(ViewController : UIViewController,UITextFieldDelegate,并在.m中实现相关方法
    //[textField becomeFirstResponder];//设置第一相应对象，会相应appDelegate中添加的UITextFieldDelegate委托
    [self.view addSubview:textField];
    return textField;
}

- (void)initBtn {
    UIButton *button = [self createButton:20:60 title:@"初始化"];
    [button addTarget:self action:@selector(Init) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)Init {
    // 通讯管理器
    TIMSdkConfig * sdkcfg = [[TIMSdkConfig alloc] init];
    sdkcfg.sdkAppId = APPID;
    // 网络事件通知
    sdkcfg.connListener = self;
    sdkcfg.disableLogPrint = YES; //禁止在控制台打印 log
//    sdkcfg.logLevel = TIM_LOG_NONE;
    NSLog(@"TIMSdkConfig=%u", sdkcfg.sdkAppId);
    [[TIMManager sharedInstance] initSdk:sdkcfg];

    // 用户状态变更
    TIMUserConfig * usercfg = [[TIMUserConfig alloc] init];
    usercfg.userStatusListener = self;
    [[TIMManager sharedInstance] setUserConfig:usercfg];
    
    // 新消息通知
    [[TIMManager sharedInstance] addMessageListener:self];
}

- (void)loginBtn {
    UIButton *button = [self createButton:20:140 title:@"登录"];
    [button addTarget:self action:@selector(Login) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)Login {
    NSString* identifier = selfField.text;
    
    TIMLoginParam * login_param = [[TIMLoginParam alloc] init];
    login_param.identifier = identifier;
    login_param.userSig = @"eJwtzEELgjAYxvHvsnPI6zadCV0C7dJBcFHX0Klv2hJd04y*e6Ien98D-y*R59SxqiMhoQ6Q3bIxV9pggQsb1Rtwt6fP63vbYk5ClwMw6vuUr48aW*zU7J7nUQBY1eBzsYBxIeiebRUs53AnomtcNw*h46ScBvlxbTpeXpmVxXQ7vrXVFW*S7DQE0YH8-mKwMu0_"; //@"usersig";
    //appidAt3rd App 用户使用 OAuth 授权体系分配的 Appid，在私有帐号情况下，填写与 SDKAppID 一样
    login_param.appidAt3rd = @"123456";

    NSLog(@"login_param => %@", (login_param != nil)? @"YES":@"NO");
//    NSLog(@"=> %@, %@, %@", login_param.identifier, login_param.userSig, login_param.appidAt3rd);
    
    [[TIMManager sharedInstance] login: login_param succ:^(){
        NSLog(@"Login Succ");
    } fail:^(int code, NSString * err) {
        NSLog(@"Login Failed: %d->%@", code, err);
    }];
}

- (void)logoutBtn {
    UIButton *button = [self createButton:140:140 title:@"登出"];
    [button addTarget:self action:@selector(Logout) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)Logout {
    [[TIMManager sharedInstance] logout:^() {
        NSLog(@"logout succ");
    } fail:^(int code, NSString * err) {
        NSLog(@"logout fail: code=%d err=%@", code, err);
    }];
}

// 获取当前用户
- (void)getLoginUserBtn {
    UIButton *button = [self createButton:260:140 title:@"获取当前用户"];
    [button addTarget:self action:@selector(getLoginUser) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)getLoginUser {
    NSString * loginUser = [[TIMManager sharedInstance] getLoginUser];
    NSLog(@"当前用户 = %@", loginUser);
}

// 获取自己的资料
- (void)getSelfProfileBtn {
    UIButton *button = [self createButton:20:180 title:@"获取自己的资料"];
    [button addTarget:self action:@selector(GetSelfProfile) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)GetSelfProfile {
    NSString* identifier = selfField.text;
    
    NSArray<NSString *> * users = [[NSArray alloc] initWithObjects:identifier, nil];
    [[TIMFriendshipManager sharedInstance] getUsersProfile:users forceUpdate:NO succ:^(NSArray<TIMUserProfile *> *profiles) {
        NSLog(@"GetSelfProfile succ, %@", profiles);
    } fail:^(int code, NSString *msg) {
        NSLog(@"GetSelfProfile fail: code=%d err=%@", code, msg);
    }];
}
// 获取指定用户的资料
- (void)getUsersProfileBtn {
    UIButton *button = [self createButton:180:180 title:@"获取别人的资料"];
    [button addTarget:self action:@selector(GetUsersProfile) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)GetUsersProfile {
    NSString* identifier = userField.text;
    
    NSMutableArray * arr = [[NSMutableArray alloc] init];
    [arr addObject:identifier];
//    [arr addObject:@"test03"];
    [[TIMFriendshipManager sharedInstance] getUsersProfile:arr forceUpdate:NO succ:^(NSArray * arr) {
        for (TIMUserProfile * profile in arr) {
            NSLog(@"user=%@", profile);
        }
    }fail:^(int code, NSString * err) {
        NSLog(@"GetFriendsProfile fail: code=%d err=%@", code, err);
    }];
}
// 修改自己的资料
- (void)modifySelfProfileBtn {
    UIButton *button = [self createButton:140:220 title:@"修改昵称"];
    [button addTarget:self action:@selector(ModifySelfProfile) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)ModifySelfProfile {
    [[TIMFriendshipManager sharedInstance] modifySelfProfile:@{TIMProfileTypeKey_Nick:nickField.text} succ:^() {
        NSLog(@"ModifySelfProfile succ");
    } fail:^(int code, NSString * err) {
        NSLog(@"ModifySelfProfile fail: code=%d err=%@", code, err);
    }];
}

// 获取所有好友
- (void)getFriendListBtn {
    UIButton *button = [self createButton:20:260 title:@"获取所有好友"];
    [button addTarget:self action:@selector(GetFriendList) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)GetFriendList {
    [[TIMFriendshipManager sharedInstance] getFriendList:^(NSArray<TIMFriend *> *friends) {
        NSMutableString *msg = [NSMutableString new];
        [msg appendString:@"好友列表: "];
        for (TIMFriend *friend in friends) {
            [msg appendFormat:@"[%@,%@,%llu,%@,%@,%@]", friend.identifier, friend.remark, friend.addTime, friend.addSource, friend.addWording, friend.groups];
        }
        NSLog(@"GetFriendList succ = [%lu]%@",(unsigned long)friends.count, msg);
    } fail:^(int code, NSString *msg) {
        NSLog(@"GetFriendList 失败: code=%d err=%@", code, msg);
    }];
}
// 添加好友
- (void)addFriendBtn {
    UIButton *button = [self createButton:140:300 title:@"添加好友"];
    [button addTarget:self action:@selector(AddFriend) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)AddFriend {
    NSString* identifier = userField.text;
    
    TIMFriendRequest *q = [TIMFriendRequest new];
    q.identifier = identifier; // 加好友 abc
    q.addWording = @"求通过";
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
                    NSLog(@"等待对方验证");
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
- (void)deleteFriendsBtn {
    UIButton *button = [self createButton:260:300 title:@"删除好友"];
    [button addTarget:self action:@selector(DeleteFriends) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)DeleteFriends {
    NSString* identifier = userField.text;
    
    NSMutableArray * del_users = [[NSMutableArray alloc] init];
    [del_users addObject:identifier];
    // TIM_FRIEND_DEL_BOTH 指定删除双向好友
    [[TIMFriendshipManager sharedInstance] deleteFriends:del_users delType:TIM_FRIEND_DEL_BOTH succ:^(NSArray<TIMFriendResult *> *results) {
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
// 校验好友关系
- (void)checkFriendsBtn {
    UIButton *button = [self createButton:20:340 title:@"校验好友"];
    [button addTarget:self action:@selector(CheckFriends) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)CheckFriends {
    NSString* identifier = userField.text;
    
    NSArray<NSString *> * users = [[NSArray alloc] initWithObjects:identifier, nil];
    TIMFriendCheckInfo * checkInfo = [[TIMFriendCheckInfo alloc] init];
    checkInfo.users = users;
    checkInfo.checkType = 1; //1=单向好友，2=互为好友
    
    // 1=是好友，0=不是好友
    [[TIMFriendshipManager sharedInstance] checkFriends:checkInfo succ:^(NSArray<TIMCheckFriendResult *> *results) {
        NSLog(@"CheckFriends succ = %ld,%@,%@,%ld。",
              (long)results[0].result_code,
              results[0].identifier,
              results[0].result_info,
              (long)results[0].resultType);
    } fail:^(int code, NSString *msg) {
        NSLog(@"CheckFriends fail: code=%d err=%@", code, msg);
    }];
}

// 文本消息发送
- (void)sendTextElementBtn {
    UIButton *button = [self createButton:260:380 title:@"发送"];
    [button addTarget:self action:@selector(SendTextElement) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:button];
}
- (void)SendTextElement {
    NSString* identifier = userField.text;
    
    // 获取对方 identifier 为『iOS-001』的单聊会话：
    TIMConversation * c2c_conversation = [[TIMManager sharedInstance] getConversation:TIM_C2C receiver:identifier];
    // 获取群组 ID 为『TGID1JYSZEAEQ』的群聊会话示例：
//    TIMConversation * grp_conversation = [[TIMManager sharedInstance] getConversation:TIM_GROUP receiver:@"TGID1JYSZEAEQ"];

    TIMTextElem * text_elem = [[TIMTextElem alloc] init];
    [text_elem setText:chatField.text];
    
    TIMMessage * msg = [[TIMMessage alloc] init];
    [msg addElem:text_elem];
    
    [c2c_conversation sendMessage:msg succ:^(){
        NSLog(@"SendMsg Succ");
    }fail:^(int code, NSString * err) {
        NSLog(@"SendMsg Failed:%d->%@", code, err);
    }];
}

// 获取系统版本
- (NSString *)version {
    return [NSString stringWithFormat:@"IOS %.2f",[[[UIDevice currentDevice] systemVersion] floatValue]];
}
- (void)getVersion {
    NSString * res = [self version];
    NSLog(@"version = %@", res); //IOS 12.20
}

@end
