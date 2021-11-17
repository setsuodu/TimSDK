#import <JSONModel/JSONModel.h>

@interface TimCallback : JSONModel
@property(nonatomic,assign) uint32_t msg;
@property(nonatomic,assign) uint32_t code;
@property(nonatomic,strong) NSString * data;
@end
@implementation TimCallback
@end

@implementation NSArray (JSONModelExtensions)
- (NSString*)toJSONString {
    NSMutableArray * jsonObjects = [NSMutableArray new];
    for(id obj in self)
        [jsonObjects addObject:[obj toJSONString]];
    return [NSString stringWithFormat:@"[%@]", [jsonObjects componentsJoinedByString:@","]];
}
@end

typedef NS_ENUM(NSInteger, TimMessage) {
    NS_Login               = 0,
    NS_Logout              = 1,
    NS_GetSelfProfile      = 2,
    NS_GetUsersProfile     = 3,
    NS_AddFriend           = 4,
    NS_DeleteFriends       = 5,
    NS_DoFriendResponse    = 6,
    NS_CheckFriends        = 7,
    NS_GetPendencyList     = 8,
    NS_GetFriendList       = 9,
    NS_SendTextElement     = 10, //发送(文字/图片/音频)信息
    NS_RecvNewMessages     = 11, //接收(文字/图片/音频/SNS)信息
    NS_ModifySelfProfile   = 12,
    NS_OnForceOffline      = 13, //顶号
    NS_OnUserSigExpired    = 14,
    NS_RecvSNSTips         = 15, //合并到 RecvNewMessages
    NS_GetConversationList = 16,
    NS_RevokeMessage       = 17,
    NS_GetLocalMessage     = 18, //本地存储消息 //TODO: 和漫游消息合并
    NS_GetMessage          = 19, //漫游消息
    NS_GetLastMsg          = 20, //最近一条消息
};
typedef NS_ENUM(NSInteger, TIMElemType) {
    NS_Invalid             = 0,
    NS_Text                = 1,
    NS_Image               = 2,
    NS_Sound               = 3,
    NS_Custom              = 4,
    NS_File                = 5,
    NS_GroupTips           = 6,
    NS_Face                = 7,
    NS_Location            = 8,
    NS_GroupSystem         = 9,
    NS_SNSTips             = 10,
    NS_ProfileTips         = 11,
    NS_Video               = 12,
};

@interface TIMUserProfileResp : JSONModel
@property(nonatomic,strong) NSString * identifier; //长度不定strong
@property(nonatomic,strong) NSString * nickName;
@property(nonatomic,assign) uint32_t gender;
@property(nonatomic,assign) uint64_t birthday;
@property(nonatomic,assign) uint32_t allowType;
@end
@implementation TIMUserProfileResp
@end

@interface TIMConversationResp : JSONModel
@property(nonatomic,strong) NSString * peer;
@property(nonatomic,assign) uint32_t type; //Invalid=0, C2C=1, Group=2, System=3
@end
@implementation TIMConversationResp
@end

@interface TIMMessageResp : JSONModel
@property(nonatomic,assign) uint64_t seq;
@property(nonatomic,assign) uint64_t rand;
@property(nonatomic,assign) uint64_t timestamp;
@property(nonatomic,assign) BOOL isSelf;
@property(nonatomic,assign) BOOL isRead;
@property(nonatomic,assign) BOOL isPeerReaded;
@property(nonatomic,assign) int32_t elemType;
@property(nonatomic,assign) int32_t subType;
@property(nonatomic,strong) NSString * text; //文本消息内容，图片消息路径
@property(nonatomic,strong) NSString * param; //备注信息，Json格式
@end
@implementation TIMMessageResp
@end

@interface TIMMessageLocatorResp : JSONModel
@property(nonatomic,assign) uint64_t seq;
@property(nonatomic,assign) uint64_t rand;
@property(nonatomic,assign) uint64_t timestamp;
@property(nonatomic,assign) BOOL isSelf;
@end
@implementation TIMMessageLocatorResp
@end

@interface TIMCheckFriendResultResp : JSONModel
@property(nonatomic,strong) NSString* identifier;
@property(nonatomic,assign) int32_t result_code;
@property(nonatomic,strong) NSString *result_info;
@property(nonatomic,assign) int32_t resultType;
@end
@implementation TIMCheckFriendResultResp
@end

@interface TIMImageResp : JSONModel
@property(nonatomic,assign) int32_t type;
@property(nonatomic,strong) NSString* url;
@property(nonatomic,strong) NSString* uuid;
@property(nonatomic,assign) int64_t size;
@property(nonatomic,assign) int64_t height;
@property(nonatomic,assign) int64_t width;
@end
@implementation TIMImageResp
@end
