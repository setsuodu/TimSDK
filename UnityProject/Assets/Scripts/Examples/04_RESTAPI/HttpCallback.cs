//Http基础解析
public class HttpCallback
{
    public int code { get; set; }
    public string msg { get; set; }
    public string data { get; set; }
}
public class HttpCallback<T> where T : class
{
	public int code { get; set; }
	public string msg { get; set; }
	public T data { get; set; }
}

//TODO: 和服务器校对类型
public class MyInfoCallback
{
	public long coin { get; set; }
	public long diamond { get; set; }
	public string city { get; set; }
	public string province { get; set; }
	public string signal { get; set; }
	public int vip { get; set; }
	public int avatarId { get; set; }
	public long exp { get; set; }
	public long uid { get; set; }
	public string nickname { get; set; }
	public byte sex { get; set; } //1 男 -1 女
	public string no { get; set; }
	public string avatar { get; set; }
	//旧接口
	//public long updateTime { get; set; }
	//public long flower { get; set; }
	//public int flowerBalance { get; set; }
	//public int role { get; set; }
	//public long visitor { get; set; }
	//public int birthday { get; set; }
	//public int signalType { get; set; }
	//public int poseId { get; set; }
	//public int phone { get; set; }
	//public int wardrobeCnt { get; set; }
	//public int imInit { get; set; }
	//public string cpId { get; set; }
}

public class InfoCallback
{
	public int avatarId { get; set; }
	public int vip { get; set; }
	public string signal { get; set; }
	public string province { get; set; }
	public string city { get; set; }
	public long exp { get; set; }
	public string avatar { get; set; }
	public string no { get; set; }
	public int sex { get; set; } //0=? //1 男 -1 女
	public string nickname { get; set; }
	public int uid { get; set; }

	public override string ToString()
	{
		return $"UserConfig: " +
			$"avatarId={avatarId} " +
			$"vip={vip} " +
			$"signal={signal} " +
			$"province={province} " +
			$"city={city} " +
			$"exp={exp} " +
			$"avatar={avatar} " +
			$"no={no} " +
			$"sex={sex} " +
			$"nickname={nickname} " +
			$"uid={uid}";
	}
}

public class MyFriendsCallback
{
	public int total { get; set; }
	public int size { get; set; }
	public string url { get; set; }
	public MyFriendsItem[] list { get; set; }
	public int totalPage { get; set; }
	public int curPage { get; set; }
}
public class MyFriendsItem//ItemData
{
	public int r { get; set; } //关系
	public string no { get; set; } //对方用户号 //_id进制转换后的缩小显示值
	public string avatar { get; set; } //头像
	public int uid { get; set; } //对方完整uid
	public string nickname { get; set; }
	public long amount { get; set; } //亲密度，陌生人没有字段返回
}
/*
public static int CP = 5;// 好友发起人
public static int FRIEND = 3;// 好友
public static int APPLY = 1;// 好友发起人
public static int NULL = 0;// 陌生人
public static int BLACK_PULL = -2;// 拉黑
public static int BLACK = -3;// 黑名单
*/

//聊天记录上传
public class ChatLogs 
{
	public long sendUid;
	public long recvUid;
	public long seq;
	public long timestamp;
}

//用户配置
public class UserConfig
{
	public long identifier { get; set; }
	public long updateTime { get; set; }
	public int noticeShow { get; set; }		//通知显示消息详情	//1:显示详情	2:只显示通知
	public int noticeRecv { get; set; }		//接收消息设置		//1:全部		2:社区及聊天		3:聊天
	public int shieldList { get; set; }		//屏蔽通讯录		//1:屏蔽通讯录	2:不屏蔽
	public int onlineMatched { get; set; }  //在线时允许被匹配	//1:在线可匹配	2:在线不可匹配
	public int chatWho { get; set; }        //谁可以发消息给我	//1:全部		2:好友				3:自闭

	public UserConfig Update(UserConfig value) 
	{
		this.identifier = value.identifier;
		this.updateTime = value.updateTime;
		this.noticeShow = value.noticeShow;
		this.noticeRecv = value.noticeRecv;
		this.shieldList = value.shieldList;
		this.onlineMatched = value.onlineMatched;
		this.chatWho = value.chatWho;
		return this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(System.Object _value) 
	{
		var value = (UserConfig)_value;
		if (this.identifier == value.identifier
			&& this.updateTime == value.updateTime
			&& this.noticeShow == value.noticeShow
			&& this.noticeRecv == value.noticeRecv
			&& this.shieldList == value.shieldList
			&& this.onlineMatched == value.onlineMatched
			&& this.chatWho == value.chatWho)
			return true;
		return false;
	}

	public override string ToString()
	{
		return $"UserConfig: " +
			$"identifier={identifier} " +
			$"updateTime={updateTime} " +
			$"noticeShow={noticeShow} " +
			$"noticeRecv={noticeRecv} " +
			$"shieldList={shieldList} " +
			$"onlineMatched={onlineMatched} " +
			$"chatWho={chatWho}";
	}
}

//社区首页文章列表返回
public class Articles
{
	//{"total":0,"size":20,"url":null,"list":[],"curPage":1,"totalPage":0}
	public int total { get; set; }
	public int size { get; set; }
	public string url { get; set; }
	public Article[] list { get; set; } //TODO: subclass
	public int curPage { get; set; }
	public int totalPage { get; set; }
}
public class Article//ItemData
{
	//{"praised":false,"user":{"uid":2,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","no":"4","sex":1,"nickname":"圣诞w@@#^sss"},"id":2,"type":1,"content":"啦啦啦啦",
	//"status":3,"anonymous":0,"topics":null,"top":0,"dataUrl":"","commentCnt":0,"cover":"","publishTime":1586831183613,"pics":null,"essence":1,"pri":100,"addr":""},
	//{"praised":false,"user":{"uid":2,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","no":"4","sex":1,"nickname":"圣诞w@@#^sss"},"id":1,"type":1,"content":"hello world","status":3,"anonymous":0,"topics":null,"top":0,"dataUrl":"","commentCnt":0,"cover":"","publishTime":1586763183447,"pics":null,"essence":1,"pri":100,"addr":""}
	public bool praised { get; set; }		//是否赞过
	public ArticleUser user { get; set; }	//贴主
	public int id { get; set; }				//文章id
	public int type { get; set; }			//文字贴,图片贴,视频贴
	public string content { get; set; }		//文章内容
	public int status { get; set; }			//<0已删除,1:未审核,2:已审核
	public int anonymous { get; set; }		//匿名
	public string topics { get; set; }		//话题
	public int top { get; set; }			//置顶
	public string dataUrl { get; set; }		//视频等连接
	public int commentCnt { get; set; }		//评论数
	public int praiseCnt { get; set; }      //点赞数
	public string cover { get; set; }		//封面
	public long publishTime { get; set; }	//发布时间
	public string[] pics { get; set; }		//图片
	public int essence { get; set; }		//加精
	public int pri { get; set; }			//可见类型
	public string addr { get; set; }        //发布位置

	public override string ToString()
	{
		return $"Article: " +
			$"praised={praised} " +
			$"id={id} " +
			$"type={type} " +
			$"content={content} " +
			$"praiseCnt={praiseCnt}";
	}
}
public class ArticleUser
{
	//{"uid":2,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","no":"4","sex":1,"nickname":"圣诞w@@#^sss"}
	public long uid { get; set; }
	public string avatar { get; set; }
	public string no { get; set; }
	public int sex { get; set; }
	public string nickname { get; set; }

	public override string ToString()
	{
		return $"ArticleUser: " +
			$"uid={uid} " +
			$"avatar={avatar} " +
			$"no={no} " +
			$"sex={sex} " +
			$"nickname={nickname}";
	}
}

//表白墙首页数据
public class ShowLoveTop
{
	//{"code":0,"msg":"OK","data":
	//{"sendUser":{"nickname":"圣诞w@@#^sss","sex":1,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","uid":2,"no":"4"},
	//"content":"","count":1,"reviUser":null,"createTime":1587031778034}}

	//{"sendUser":{"nickname":"圣诞w@@#^sss","sex":1,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","uid":2,"no":"4"},
	//"content":"",
	//"count":1,
	//"reviUser":null,
	//"createTime":1587031778034}
	public ShowLoveUser sendUser { get; set; }
	public string content { get; set; }
	public int count { get; set; }
	public string reviUser { get; set; }
	public long createTime { get; set; }
}
public class ShowLoveUser
{
	//{"nickname":"圣诞w@@#^sss",
	//"sex":1,
	//"avatar":"http://avatar.zd1312.com/def/women_320_320.png",
	//"uid":2,
	//"no":"4"}
	public string nickname { get; set; }
	public int sex { get; set; }
	public string avatar { get; set; }
	public long uid { get; set; }
	public string no { get; set; }
}

//POI数据
public class POI
{
	//{"id":"695671858669776040",
	//"title":"中兴立交桥",
	//"address":"浙江省杭州市滨江区",
	//"category":"基础设施:道路附属:桥",
	//"location":{"lat":30.194752,"lng":120.187042},
	//"ad_info":{"adcode":"330108","province":"","city":"","district":""},
	//"_distance":3,
	//"_dir_desc":"东南"}
	public string id { get; set; }
	public string title { get; set; }
	public string address { get; set; }
	public string category { get; set; }
	public Location location { get; set; }
	public ADInfo ad_info { get; set; }
	public int _distance { get; set; }
	public string _dir_desc { get; set; }
}
public class Location
{
	public double lat { get; set; }
	public double lng { get; set; }
}
public class ADInfo
{
	public string adcode { get; set; }
	public string province { get; set; }
	public string city { get; set; }
	public string district { get; set; }
}

//发布文章成功回调
public class ArticlePub
{
	//{"_id":1,
	//"updateTime":1586763183464,
	//"type":1,
	//"content":"hello world",
	//"cover":"http://aud.zd1312.com/arts/4/A9BE77FA03C2B5C8B57CBB83EA6C6787.dat",
	//"pics":null,
	//"dataUrl":"",
	//"userId":2,
	//"status":3,
	//"essence":1,
	//"audited":1,
	//"adminId":0,
	//"sort":0.0,
	//"readCnt":0,
	//"praiseCnt":0,
	//"commentCnt":0,
	//"shareCnt":0,
	//"createTime":1586763183464,
	//"publishTime":1586763183447,
	//"topics":null,
	//"pri":0,
	//"addr":"",
	//"picFormat":null,
	//"top":0,
	//"anonymous":0,
	//"loc":{"type":"Point","coordinates":[0.0,0.0]},
	//"group":0}
	public int _id { get; set; }
	public long updateTime { get; set; }
	public int type { get; set; }
	public string content { get; set; }
	public string cover { get; set; }
	public string[] pics { get; set; }
	public string dataUrl { get; set; }
	public long userId { get; set; }
	public int status { get; set; }
	public int essence { get; set; }
	public int audited { get; set; }
	public int adminId { get; set; }
	public double sort { get; set; }
	public int readCnt { get; set; }
	public int praiseCnt { get; set; }
	public int commentCnt { get; set; }
	public int shareCnt { get; set; }
	public long createTime { get; set; }
	public long publishTime { get; set; }
	public string topics { get; set; }
	public int pri { get; set; }
	public string addr { get; set; }
	public string picFormat { get; set; }
	public int top { get; set; }
	public int anonymous { get; set; }
	public ArticlePubLoc loc { get; set; }
	public int group { get; set; }
}
public class ArticlePubLoc
{
	//{"type":"Point","coordinates":[0.0,0.0]}
	public string type { get; set; }
	public double[] coordinates { get; set; }
}

// 话题列表
public class ArticleTopicList
{
	//{"total":1,"size":20,"url":null,"list":[{"_id":"青花瓶","count":0,"sort":0.0,"createTime":1587107768621,"status":1,"updateTime":1587107768621}],"totalPage":1,"curPage":1}
	public int total { get; set; }
	public int size { get; set; }
	public string url { get; set; }
	public ArticleTopic[] list { get; set; }
	public int totalPage { get; set; }
	public int curPage { get; set; }
}
public class ArticleTopic
{
	//{"_id":"青花瓶","count":0,"sort":0.0,"createTime":1587107768621,"status":1,"updateTime":1587107768621}
	public string _id { get; set; }
	public int count { get; set; }
	public double sort { get; set; }
	public long createTime { get; set; }
	public int status { get; set; }
	public long updateTime { get; set; }
}

// 发表评论
public class ArticleCommentReply
{
	//"_id":1,
	//"updateTime":1587451392078,
	//"aid":13,
	//"type":1,
	//"content":"abc",
	//"userId":2,
	//"status":3,
	//"adminId":0,
	//"praiseCnt":0,
	//"createTime":1587451392078,
	//"baseId":0,
	//"cid":0,
	//"atUsers":null,
	//"pics":null,
	//"replyCnt":0
	public long _id { get; set; }			//评论唯一id
	public long updateTime { get; set; }	//发布时间？
	public int aid { get; set; }			//文章id
	public int type { get; set; }			//默认1，文字
	public string content { get; set; }		//评论内容
	public long userId { get; set; }		//评论者？
	public int status { get; set; }			//评论状态
	public long adminId { get; set; } 
	public int praiseCnt { get; set; }		//点赞数
	public long createTime { get; set; }	//发布时间？
	public int baseId { get; set; }			//服务器优化用的字段
	public long cid { get; set; }			//接楼id
	public long[] atUsers { get; set; }		//@（没有）
	public string pics { get; set; }		//图片（没有）
	public int replyCnt { get; set; }		//回复数
}

// 评论列表
public class CommentList
{
	public int total { get; set; }		//总评论数
	public int size { get; set; }		//单页评论数
	public string url { get; set; }		//web端用的字段
	public Comment[] list { get; set; } //评论楼层
	public int curPage { get; set; }	//当前页
	public int totalPage { get; set; }	//总页数
}
// 评论列表元素
public class Comment
{
	//"replies":null,
	//"content":"abc",
	//"total":0,
	//"nickname":"圣诞w@@#^sss",
	//"uid":2,
	//"createTime":1587451392078,
	//"baseId":0,
	//"praised":false,
	//"avatarUrl":"http://avatar.zd1312.com/def/women_320_320.png",
	//"commentId":1,
	//"praiseCount":0,
	//"replyCid":0
	public string[] replies { get; set; }	//NULL
	public string content { get; set; }		//评论内容
	public int total { get; set; }			//
	public string nickname { get; set; }	//评论者昵称
	public long uid { get; set; }            //评论者
	public long createTime { get; set; }    //发布时间
	public int baseId { get; set; }			//服务器优化用的字段
	public bool praised { get; set; }		//我是否点赞
	public string avatarUrl { get; set; }	//评论者头像
	public int commentId { get; set; }		
	public int praiseCount { get; set; }	//点赞数
	public long replyCid { get; set; }		//该评论回复的对象
}