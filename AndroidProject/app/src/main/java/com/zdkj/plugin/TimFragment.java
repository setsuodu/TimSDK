package com.zdkj.plugin;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.location.LocationManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.provider.Settings;
import android.util.Log;
import android.widget.Toast;

import com.tencent.imsdk.TIMFaceElem;
import com.tencent.imsdk.TIMManager;
import com.tencent.imsdk.TIMCallBack;
import com.tencent.imsdk.TIMConnListener;
import com.tencent.imsdk.TIMConversation;
import com.tencent.imsdk.TIMConversationType;
import com.tencent.imsdk.TIMElem;
import com.tencent.imsdk.TIMElemType;
import com.tencent.imsdk.TIMSNSSystemElem;
import com.tencent.imsdk.TIMSNSSystemType;
import com.tencent.imsdk.TIMSnapshot;
import com.tencent.imsdk.TIMTextElem;
import com.tencent.imsdk.TIMImage;
import com.tencent.imsdk.TIMImageElem;
import com.tencent.imsdk.TIMSoundElem;
import com.tencent.imsdk.TIMLocationElem;
import com.tencent.imsdk.TIMGroupTipsElem;
import com.tencent.imsdk.TIMFriendshipManager;
import com.tencent.imsdk.TIMGroupEventListener;
import com.tencent.imsdk.TIMLogLevel;
import com.tencent.imsdk.TIMMessage;
import com.tencent.imsdk.TIMMessageListener;
import com.tencent.imsdk.TIMRefreshListener;
import com.tencent.imsdk.TIMSdkConfig;
import com.tencent.imsdk.TIMUserConfig;
import com.tencent.imsdk.TIMUserProfile;
import com.tencent.imsdk.TIMUserStatusListener;
import com.tencent.imsdk.TIMValueCallBack;
import com.tencent.imsdk.TIMVideo;
import com.tencent.imsdk.TIMVideoElem;
import com.tencent.imsdk.ext.message.TIMMessageReceipt;
import com.tencent.imsdk.ext.message.TIMMessageReceiptListener;
import com.tencent.imsdk.ext.message.TIMMessageRevokedListener;
import com.tencent.imsdk.friendship.TIMCheckFriendResult;
import com.tencent.imsdk.friendship.TIMFriend;
import com.tencent.imsdk.friendship.TIMFriendCheckInfo;
import com.tencent.imsdk.friendship.TIMFriendPendencyInfo;
import com.tencent.imsdk.friendship.TIMFriendPendencyRequest;
import com.tencent.imsdk.friendship.TIMFriendPendencyResponse;
import com.tencent.imsdk.friendship.TIMFriendRequest;
import com.tencent.imsdk.friendship.TIMFriendResponse;
import com.tencent.imsdk.friendship.TIMFriendResult;
import com.tencent.imsdk.friendship.TIMPendencyType;
import com.tencent.imsdk.session.SessionWrapper;
import com.tencent.imsdk.ext.message.TIMMessageLocator;

import com.alibaba.fastjson.JSONObject;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import com.zdkj.resp.*;
import static com.zdkj.plugin.Utils.*;

import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class TimFragment extends UnityPlayerActivity {
    public static Activity currentActivity;
    private static final String TAG = "TimPlugin";
    private static String gameObjectName;
    public static void GetInstance(String _gameObjectName) {
        gameObjectName = _gameObjectName;
        System.out.println("GetInstance gameObjectName=" + gameObjectName);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        currentActivity = TimFragment.this;
        System.out.println("UnityPlayerActivity onCreate: currentActivity=" + currentActivity.getClass());
    }

//    @SuppressLint("SetTextI18n")
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent result) {
        super.onActivityResult(requestCode, resultCode, result);
        System.out.println("onActivityResult: requestCode=" + requestCode + " resultCode=" + resultCode);

        if (resultCode == RESULT_OK) {
            switch (requestCode) {
                case REQUEST_GALLERY: {
//                    Log.e(TAG, "onActivityResult: REQUEST_GALLERY: uri=" + result.getData()); //返回的是Uri
                    String path = toPath(result);
                    Log.e(TAG, "onActivityResult: REQUEST_GALLERY: path=" + path);
//                    NativeCallback(path);
                    JsonCallback(TimMessageType.TimMessage.Image.value(),0,path);
                    break;
                } //相册
                case REQUEST_Camera: {
//                    Log.e(TAG, "onActivityResult: REQUEST_Camera: 拍照完成:" + imageUri);
                    String path = getRealFilePath(this, imageUri);
                    Log.e(TAG, "onActivityResult: REQUEST_Camera: path=" + path);
                    NativeCallback(path);
                    break;
                } //拍照
                case REQUEST_CROP: {
                    String path = toPath(result);
                    Log.e(TAG, "onActivityResult: REQUEST_CROP: path=" + path);
                    NativeCallback(path);
                    break;
                } //裁剪
                case REQUEST_VIDEO: {
                    String path = toPath(result);
                    Log.e(TAG, "onActivityResult: REQUEST_VIDEO: path=" + path);
//                    NativeCallback(path);
                    JsonCallback(TimMessageType.TimMessage.Video.value(),0,path);
                    break;
                } //选择视频
            }
        }
    }

    // 字符串消息返回
    public void NativeCallback(String log){
        String result = TAG + ": " + log;
        UnityPlayer.UnitySendMessage(gameObjectName,"NativeCallback", result);
    }
    // json消息返回
    public void JsonCallback(int msg, int code, String data){
        TimCallback resp = new TimCallback();
        resp.msg = msg;
        resp.code = code;
        resp.data = data;
        String json = JSONObject.toJSONString(resp,true);
        UnityPlayer.UnitySendMessage(gameObjectName,"JsonCallback", json);
    }

    //<editor-fold desc="初始化">

    // 初始化 IM SDK
    public void init(int sdkAppId) {
        //初始化 IM SDK 基本配置
        //判断是否是在主线程
        if (SessionWrapper.isMainProcess(UnityPlayer.currentActivity)) {
            TIMSdkConfig config = new TIMSdkConfig(sdkAppId)
                    .enableLogPrint(true)
                    .setLogLevel(TIMLogLevel.DEBUG)
                    .setLogPath(Environment.getExternalStorageDirectory().getPath() + "/justfortest/");

            //初始化 SDK
            TIMManager.getInstance().init(UnityPlayer.currentActivity, config);
        }

        NativeCallback("初始化成功：" + TIMManager.getInstance().isInited());

        // 网络事件通知 & 用户状态变更
        setUserConfig();
        // 新消息通知
        addMessageListener();
    }

    // 用户配置
    private void setUserConfig() {
        //基本用户配置
        TIMUserConfig userConfig = new TIMUserConfig()
                //设置用户状态变更事件监听器
                .setUserStatusListener(new TIMUserStatusListener() {
                    @Override
                    public void onForceOffline() {
                        //被其他终端踢下线
//                        Log.i(TAG, "onForceOffline");
                        JsonCallback(TimMessageType.TimMessage.OnForceOffline.value(),0,"onForceOffline");
                    }

                    @Override
                    public void onUserSigExpired() {
                        //用户签名过期了，需要刷新 userSig 重新登录 IM SDK
//                        Log.i(TAG, "onUserSigExpired");
                        JsonCallback(TimMessageType.TimMessage.OnUserSigExpired.value(),0,"onUserSigExpired");
                    }
                })
                //设置连接状态事件监听器
                .setConnectionListener(new TIMConnListener() {
                    @Override
                    public void onConnected() {
                        Log.i(TAG, "onConnected");
                    }

                    @Override
                    public void onDisconnected(int code, String desc) {
                        Log.i(TAG, "onDisconnected");
                    }

                    @Override
                    public void onWifiNeedAuth(String name) {
                        Log.i(TAG, "onWifiNeedAuth");
                    }
                })
                //设置群组事件监听器
                .setGroupEventListener(new TIMGroupEventListener() {
                    @Override
                    public void onGroupTipsEvent(TIMGroupTipsElem elem) {
                        Log.i(TAG, "onGroupTipsEvent, type: " + elem.getTipsType());
                    }
                })
                //设置会话刷新监听器
                .setRefreshListener(new TIMRefreshListener() {
                    @Override
                    public void onRefresh() {
                        Log.i(TAG, "onRefresh");
                        // 获取所有会话
                        getConversationList();
                    }

                    @Override
                    public void onRefreshConversation(List<TIMConversation> conversations) {
                        Log.i(TAG, "onRefreshConversation, conversation size: " + conversations.size());
                    }
                });

        //禁用本地所有存储
//        userConfig.disableStorage();
        //开启消息已读回执
        userConfig.enableReadReceipt(true);
        //设置已读回执监听器
        userConfig.setMessageReceiptListener(new TIMMessageReceiptListener() {
            @Override
            public void onRecvReceipt(List<TIMMessageReceipt> list) {
                for(TIMMessageReceipt receipt : list) {
//                    NativeCallback("对方已读 peer=" + receipt.getConversation().getPeer());
                    JsonCallback(TimMessageType.TimMessage.SetReadMessage.value(),0,receipt.getConversation().getPeer());
                }
            }
        });
        userConfig.setMessageRevokedListener(new TIMMessageRevokedListener() {
            @Override
            public void onMessageRevoked(TIMMessageLocator timMessageLocator) {
                NativeCallback("消息撤回成功 locator=" + timMessageLocator.getSeq());
            }
        });

        //将用户配置与通讯管理器进行绑定
        TIMManager.getInstance().setUserConfig(userConfig);
    }

    // 注册新消息通知
    private TIMMessageListener onNewMessages;
    private void addMessageListener() {
        onNewMessages = new TIMMessageListener() {//消息监听器
            @Override
            public boolean onNewMessages(List<TIMMessage> msgs) {//收到新消息
                //消息的内容解析请参考消息收发文档中的消息解析说明
                parseMessageList(msgs, TimMessageType.TimMessage.RecvNewMessages.value());
                return true; //返回true将终止回调链，不再调用下一个新消息监听器
            }
        };
        //设置消息监听器，收到新消息时，通过此监听器回调
        TIMManager.getInstance().addMessageListener(onNewMessages);
    }
    // 移除新消息通知
    private void removeMessageListener() {
        TIMManager.getInstance().removeMessageListener(onNewMessages);
    }
    //</editor-fold>

    //<editor-fold desc="登录">

    // 登录
    public void login(String identifier, String userSig) {
        //70402 //参数非法，请检查必填字段是否填充，或者字段的填充是否满足协议要求。//appid填错

        // identifier 为用户名，userSig 为用户登录凭证
        TIMManager.getInstance().login(identifier, userSig, new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
                JsonCallback(TimMessageType.TimMessage.Login.value(),code,desc);
            }
            @Override
            public void onSuccess() {
                JsonCallback(TimMessageType.TimMessage.Login.value(),0,"login succ");
            }
        });
    }

    // 登出
    public void logout() {
        TIMManager.getInstance().logout(new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
                JsonCallback(TimMessageType.TimMessage.Logout.value(),0,desc);
            }
            @Override
            public void onSuccess() {
                JsonCallback(TimMessageType.TimMessage.Logout.value(),0,"logout succ");
            }
        });
    }

    // 获取当前登录用户
    public String getLoginUser() {
        // 可以获取当前用户名，也可以通过这个方法判断是否已经登录。
        String userName = TIMManager.getInstance().getLoginUser();
        return userName;
    }
    //</editor-fold>

    //<editor-fold desc="消息收发">

    //<editor-fold desc="消息发送">

    // 文本消息发送
    // peer：对方账号或群号
    // type：0=无效的，1=单聊，2=群聊，3=系统
    public void sendTextElement(final String content, String peer, int type) {
        TIMMessage msg = new TIMMessage();
        TIMTextElem elem = new TIMTextElem();
        elem.setText(content);

        //将elem添加到消息
        if(msg.addElement(elem) != 0) {
            JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),-1,"addElement failed");
            return;
        }

        TIMConversation conversation = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        conversation.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),code,desc);
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
                TIMMessageResp resp = new TIMMessageResp();
                resp.setMessage(msg)
                        .setType(TIMElemType.Text.value(), -1)
                        .setText(content);
                String json = JSONObject.toJSONString(resp,true);
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),0,json);
            }
        });
    }

    // 图片消息发送
    public void sendImageElement(String fullPath, String peer, int type) {
//        NativeCallback("sendImageElement fullPath=" + fullPath);
        // /storage/emulated/0/Android/data/com.xxx.xxx/files/
//        final String filePath = "/storage/emulated/0/Pictures/1.jpg";
        final String filePath = fullPath;
        NativeCallback("图片消息发送: " + filePath);

        TIMMessage msg = new TIMMessage();
        TIMImageElem elem = new TIMImageElem();
        elem.setPath(filePath);
        elem.setLevel(1); //0: 原图发送  1: 高压缩率图发送(图片较小，默认值)   2:高清图发送(图片较大)

        //将 elem 添加到消息
        if(msg.addElement(elem) != 0) {
            JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),-1,"addElement failed");
            return;
        }

        //发送消息
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        con.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),code,desc);
                //7007 errmsg: File check failed //文件打开失败，请检查文件是否存在，或者已被独占打开，引起 SDK 打开失败。
                //7006 errmsg: File check failed //空文件，要求文件大小不是0字节，如果上传图片、语音、视频或文件，请检查文件是否正确生成。
                //20003 errmsg: Invalid sender or receiver identifier! //请检查用户帐号（UserID）是否已在腾讯云导入，当 UserID 无效或 UserID 未导入腾讯即时通信 IM 时，会返回此错误码。
                //6006 //文件传输鉴权失败，请重试。
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
                TIMMessageResp resp = new TIMMessageResp();
                resp.setMessage(msg)
                        .setType(TIMElemType.Image.value(), -1)
                        .setText(filePath);
                String json = JSONObject.toJSONString(resp,true);
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),0,json);
            }
        });
    }

    // 表情消息发送
    public void sendFaceElement(String fullPath, String peer, int type) {
        //添加表情
        TIMFaceElem elem = new TIMFaceElem();
//        elem.setData(sampleByteArray); //自定义 byte[]
        elem.setIndex(10);   //自定义表情索引

        //将 elem 添加到消息
        TIMMessage msg = new TIMMessage();
        if(msg.addElement(elem) != 0) {
            Log.d(TAG, "addElement failed");
            return;
        }

        //发送消息
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        con.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
                Log.d(TAG, "send message failed. code: " + code + " errmsg: " + desc);
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
                Log.e(TAG, "SendMsg ok");
            }
        });
    }

    // 语音消息发送
    public void sendSoundElement(String json, String peer, int type) {
        TIMSoundElemResp recv = JSONObject.parseObject(json, TIMSoundElemResp.class);

        //添加语音
        final TIMSoundElem elem = new TIMSoundElem();
        elem.setPath(recv.path); //填写语音文件路径
        elem.setDuration(recv.duration);  //填写语音时长

        //将 elem 添加到消息
        TIMMessage msg = new TIMMessage();
        if(msg.addElement(elem) != 0) {
            Log.d(TAG, "addElement failed");
            return;
        }
        //发送消息
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        con.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
//                NativeCallback("send message failed. code: " + code + " errmsg: " + desc);
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),code,desc);
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
//                NativeCallback("SendMsg ok");
                TIMMessageResp resp = new TIMMessageResp();
                resp.setMessage(msg)
                        .setType(TIMElemType.Sound.value(), -1)
                        .setText(elem.getPath());
                String json = JSONObject.toJSONString(resp,true);
                JsonCallback(TimMessageType.TimMessage.SendTextElement.value(),0,json);
            }
        });
    }

    // 地理位置消息发送
    public void sendLocationElement(String json, String peer, int type) {
        TIMLocationElemResp resp = JSONObject.parseObject(json, TIMLocationElemResp.class);

        //添加位置信息
        TIMLocationElem elem = new TIMLocationElem();
        elem.setLatitude(resp.latitude);   //设置纬度
        elem.setLongitude(resp.longitude);   //设置经度
        elem.setDesc(resp.desc);

        //将elem添加到消息
        TIMMessage msg = new TIMMessage();
        if(msg.addElement(elem) != 0) {
            Log.d(TAG, "addElement failed");
            return;
        }
        //发送消息
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        con.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
                NativeCallback("send message failed. code: " + code + " errmsg: " + desc);
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
                NativeCallback("SendMsg ok");
            }
        });
    }

    // 小文件消息发送

    // 自定义消息发送

    // 短视频消息发送
    public void sendVideoElement(String json, String peer, int type) {
        TIMVideo video = new TIMVideo();
//        video.setDuaration(duration / 1000); //设置视频时长
        video.setType("mp4"); // 设置视频文件类型

        TIMSnapshot snapshot = new TIMSnapshot();
//        snapshot.setWidth(width); // 设置视频快照图宽度
//        snapshot.setHeight(height); // 设置视频快照图高度

        //构造一个短视频对象
        TIMVideoElem elem = new TIMVideoElem();
        elem.setSnapshot(snapshot);
        elem.setVideo(video);
//        elem.setSnapshotPath(imgPath);
//        elem.setVideoPath(videoPath);

        //将 elem 添加到消息
        TIMMessage msg = new TIMMessage();
        if(msg.addElement(elem) != 0) {
            Log.d(TAG, "addElement failed");
            return;
        }

        //发送消息
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        con.sendMessage(msg, new TIMValueCallBack<TIMMessage>() {//发送消息回调
            @Override
            public void onError(int code, String desc) {//发送消息失败
                Log.d(TAG, "send message failed. code: " + code + " errmsg: " + desc);
            }
            @Override
            public void onSuccess(TIMMessage msg) {//发送消息成功
                Log.e(TAG, "SendMsg ok");
            }
        });
    }

    //</editor-fold>

    //<editor-fold desc="接收消息">

    private String imageSavePath = "/storage/emulated/0/Android/data/com.setsuodu.timsdk/files/"; //TODO: 使用API获取
    // msgId: 新消息/最后一条消息/漫游本地消息
    // onNewMessages/getLastMsg/getMessage/getLocalMessage/抛出消息的解析
    private void parseMessageList(List<TIMMessage> msgs, int mainId) {
        NativeCallback("解析消息 msgId=[" + mainId + "], count=" + msgs.size());
        List<TIMMessageResp> respList = new ArrayList<>();

        for (int i = 0; i < msgs.toArray().length; i++) {
            TIMMessage msg = msgs.get(i);
            if(msg == null) {
                NativeCallback("msg为空");
                continue;
            }
            for(int t = 0; t < msg.getElementCount(); ++t) {
                TIMElem elem = msg.getElement(t);
                TIMElemType elemType = elem.getType(); //获取当前元素的类型
//                NativeCallback("elem type=" + elemType.name());

                // 解析流程
                switch (elemType) {
                    case Text: {
                        TIMTextElem e = (TIMTextElem) elem;
                        TIMMessageResp resp = new TIMMessageResp();
                        resp.setMessage(msg)
                                .setType(elemType.value(), -1)
                                .setText(e.getText());
                        respList.add(resp);
                        break;
                    } //处理文本消息
                    case Image: {
                        TIMImageElem e = (TIMImageElem) elem;
                        for(TIMImage image : e.getImageList()) {
                            if(image.getSize() == 0) continue; //过滤空图片

                            TIMImageResp img = new TIMImageResp();
                            img.setImage(image);
                            String jsonParam = JSONObject.toJSONString(img,true);

                            final String filePath = imageSavePath + "/" + image.getUuid();
                            TIMMessageResp resp = new TIMMessageResp();
                            resp.setMessage(msg)
                                    .setType(elemType.value(), -1)
                                    .setText(filePath)
                                    .setParam(jsonParam);
                            respList.add(resp);

                            // 下载的数据需要由开发者缓存，IM SDK 每次调用 getImage 都会从服务端重新下载数据。
                            // 建议通过图片的 uuid 作为 key 进行图片文件的存储。
                            /*
                            //通过 getImage 接口下载图片二进制数据 //与消息分离
                            image.getImage(filePath, new TIMCallBack() {
                                @Override
                                public void onError(int code, String desc) {//获取图片失败
                                    NativeCallback("下载图片失败");
                                }
                                @Override
                                public void onSuccess() {//成功，参数为图片数据
                                    NativeCallback("下载图片成功 path=" + filePath);
                                }
                            });*/
                        }
                        break;
                    } //处理图片消息
                    case Sound: {
                        TIMSoundElem e = (TIMSoundElem) elem;
                        final TIMSoundElemResp clip = new TIMSoundElemResp();
                        clip.uuid = e.getUuid();
                        clip.path = "";
                        clip.duration = e.getDuration(); //时长占位符
                        String jsonParam = JSONObject.toJSONString(clip,true);

                        final String filePath = imageSavePath + "/" + e.getUuid();
                        TIMMessageResp resp = new TIMMessageResp();
                        resp.setMessage(msg)
                                .setType(TIMElemType.Sound.value(), -1)
                                .setText(filePath) //保存地址
                                .setParam(jsonParam); //文件参数
                        respList.add(resp);

                        e.getUrl(new TIMValueCallBack<String>() {
                            @Override
                            public void onError(int i, String s) {
                                JsonCallback(TimMessageType.TimMessage.Download.value(), i, s);
                            }
                            @Override
                            public void onSuccess(String s) {
                                TIMDownloadResp data = new TIMDownloadResp();
                                data.elemType = TIMElemType.Sound.value();
                                data.uuid = clip.uuid;
                                data.url = s;
                                String json = JSONObject.toJSONString(data,true);
                                JsonCallback(TimMessageType.TimMessage.Download.value(), 0, json);
                            }
                        });
//                        e.getSoundToFile(filePath, new TIMCallBack() {
//                            @Override
//                            public void onError(int i, String s) {
//                                JsonCallback(TimMessageType.TimMessage.Download.value(), TIMElemType.Sound.value(), filePath);
//                            }
//                            @Override
//                            public void onSuccess() {
//                                JsonCallback(TimMessageType.TimMessage.Download.value(), TIMElemType.Sound.value(), filePath);
//                            }
//                        });

                        break;
                    } //处理音频消息
                    case Custom: {
                        break;
                    } //自定义消息
                    case File: {
                        break;
                    } //文件消息
                    case Location: {
                        TIMLocationElem e = (TIMLocationElem) elem;
                        TIMLocationElemResp data = new TIMLocationElemResp();
                        data.desc = e.getDesc();
                        data.latitude = e.getLatitude();
                        data.longitude = e.getLongitude();
                        String json = JSONObject.toJSONString(data,true);

                        TIMMessageResp resp = new TIMMessageResp();
                        resp.setMessage(msg)
                                .setType(elemType.value(), -1)
                                .setText(json);
                        respList.add(resp);
                        break;
                    } //地理位置
                    case SNSTips: {
                        String json = "";
                        TIMSNSSystemElem systemElem = elem instanceof TIMSNSSystemElem ? ((TIMSNSSystemElem) elem) : null;
                        if(systemElem == null) continue;
                        switch (systemElem.getSubType()) {
                            case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND: {
                                NativeCallback("SNS.增加好友消息" + systemElem.getRequestAddFriendUserList().size());
                                json = JSONObject.toJSONString(systemElem.getRequestAddFriendUserList(),true);
                                break;
                            }
                            case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND: {
                                NativeCallback("SNS.删除好友消息" + systemElem.getDelRequestAddFriendUserList().size());
                                json = JSONObject.toJSONString(systemElem.getDelRequestAddFriendUserList(),true);
                                break;
                            }
                            case TIMSNSSystemType.TIM_SNS_SYSTEM_ADD_FRIEND_REQ: {
                                NativeCallback("SNS.增加好友申请" + systemElem.getFriendAddPendencyList().size());
                                json = JSONObject.toJSONString(systemElem.getFriendAddPendencyList(),true);
                                break;
                            }
                            case TIMSNSSystemType.TIM_SNS_SYSTEM_DEL_FRIEND_REQ: {
                                NativeCallback("SNS.删除好友申请" + systemElem.getDelFriendAddPendencyList().size());
                                json = JSONObject.toJSONString(systemElem.getDelFriendAddPendencyList(),true);
                                break;
                            }
                            default: {
                                NativeCallback("SNS.其它系统消息：" + systemElem.getSubType());
                                json = "其它系统消息";
                                break;
                            }
                        }
                        TIMMessageResp resp = new TIMMessageResp();
                        resp.setMessage(msg)
                                .setType(elemType.value(), systemElem.getSubType()) //SNSTips,
                                .setText(json);
                        respList.add(resp);
                        break;
                    } //关系链变更系统通知类型
                    case Video: {
                        break;
                    } //短视频消息
                    default: {
                        break;
                    } //...处理更多消息
                }
            }
        }

        String json = JSONObject.toJSONString(respList,true);
        JsonCallback(mainId,0,json);
    }
    //</editor-fold>

    //<editor-fold desc="消息属性">

    private TIMMessage getTIMMessage(TIMMessageLocatorResp resp) {
        for(TIMMessage msg : messages) {
            if(msg.getSeq() == resp.seq
                    && msg.getRand() == resp.rand
                    && msg.timestamp() == resp.timestamp
                    && msg.isSelf() == resp.isSelf) {
                return msg;
            }
        }
        return null;
    }

    //TODO: 测试函数
    public void printMessage(String json) {
        NativeCallback("printMessage=" + json);

        TIMMessageLocatorResp resp = JSONObject.parseObject(json, TIMMessageLocatorResp.class);
        TIMMessage msg = getTIMMessage(resp);

        TIMElem elem = msg.getElement(0);
        TIMElemType elemType = elem.getType();
        if(elemType == TIMElemType.Text) {
            TIMTextElem e = (TIMTextElem) elem;
            NativeCallback("文本类型：" + e.getText());
        } else {
            NativeCallback("其它类型：" + msg.getElement(0).getType());
        }
    }
    public void JsonArray(String json) {
        List<String> list = JSONObject.parseArray(json, String.class);
//        NativeCallback("list cout=" + list.size());
//        for(String item : list) {
//            NativeCallback("item=" + item);
//        }
    }

    // 删除消息
    // 传入序列化的 TIMMessageLocatorResp 属性
    public boolean removeMessage(String json) {
        NativeCallback("removeMessage json=" + json);
        TIMMessageLocatorResp resp = JSONObject.parseObject(json, TIMMessageLocatorResp.class);
        TIMMessage msg = getTIMMessage(resp);
        return msg.remove();
    }

    //</editor-fold>

    //<editor-fold desc="会话操作">

    // 获取所有会话
    // SDK 会在内部不断更新会话列表，每次更新后都会通过 TIMRefreshListener.onRefresh 回调，
    // 请在 onRefresh 之后再调用 getConversationList 更新会话列表。
    public void getConversationList() {
        List<TIMConversation> list = TIMManager.getInstance().getConversationList();
//        NativeCallback("getConversationList count=" + list.size());

        List<TIMConversationResp> respList = new ArrayList<>();
        for(TIMConversation con : list) {
            TIMConversationResp resp = new TIMConversationResp();
            resp.peer = con.getPeer();
            resp.type = con.getType().value();
            respList.add(resp);
        }
        String json = JSONObject.toJSONString(respList,true);
        JsonCallback(TimMessageType.TimMessage.GetConversationList.value(), 0, json);
    }

    // 获取会话本地消息
    private void getLocalMessage(String peer, int count, TIMMessage lastMsg) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C, peer);//获取会话扩展实例
        con.getLocalMessage(count, lastMsg, new TIMValueCallBack<List<TIMMessage>>() {//获取此会话的消息
            @Override
            public void onError(int code, String desc) {//获取消息失败
                JsonCallback(TimMessageType.TimMessage.GetLocalMessage.value(), code, desc);
            }
            @Override
            public void onSuccess(List<TIMMessage> msgs) {//获取消息成功
                parseMessageList(msgs, TimMessageType.TimMessage.GetLocalMessage.value());
                for(TIMMessage msg : msgs) {
                    messages.add(msg);
                }
            }
        });
    }
    public void getLocalMessageFirst(String peer, int count) {
        TIMMessage lastMsg = null;
        messages = new ArrayList<>();
        getLocalMessage(peer, count, lastMsg);
    }
    public void getLocalMessageNext(String peer, int count) {
        TIMMessage lastMsg = null;
        if(messages.size() > 0)
            lastMsg = messages.get(messages.size() - 1);
        getLocalMessage(peer, count, lastMsg);
    }

    // 获取会话漫游消息
    private List<TIMMessage> messages = new ArrayList<>(); //翻页标记
    private void getMessage(String peer, int count, TIMMessage lastMsg) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C, peer);
        con.getMessage(count, lastMsg, new TIMValueCallBack<List<TIMMessage>>() {//回调接口
            @Override
            public void onError(int code, String desc) {//获取消息失败
                JsonCallback(TimMessageType.TimMessage.GetMessage.value(), code, desc);
            }
            @Override
            public void onSuccess(List<TIMMessage> msgs) {//获取消息成功
                parseMessageList(msgs, TimMessageType.TimMessage.GetMessage.value());
                for(TIMMessage msg : msgs) {
                    messages.add(msg);
                }
            }
        });
    }
    public void getMessageFirst(String peer, int count) {
        TIMMessage lastMsg = null;
        messages = new ArrayList<>();
        getMessage(peer, count, lastMsg);
    }
    public void getMessageNext(String peer, int count) {
        TIMMessage lastMsg = null;
        if(messages.size() > 0)
            lastMsg = messages.get(messages.size() - 1);
        getMessage(peer, count, lastMsg);
    }
    // 测试，打印消息条数
    public void debugMessages() {
        for(int i = 0; i < messages.size(); i++) {
            NativeCallback("debugMessages: [" + i + "/" + messages.size() + "]" + messages.get(i).toString());
        }
    }

    // 删除会话
    // 删除本地消息的情况下，C2C 会话将无法获取到删除会话前的历史消息。
    // 删除本地消息的情况下，群组会话通过 getMessage 仍然会拉取到漫游消息，
    // 所以存在删除消息成功，但是拉取消息的时候仍然获取到删除会话前的历史消息的情况，取决于是否重新从漫游拉回到本地。
    // 如果不需要拉取漫游，可以通过 getLocalMessage 获取消息，
    // 或者只通过 getMessage 拉取指定条数（如未读条数数量）的消息。
    public boolean deleteConversation(String peer, boolean deleteLocal) {
        boolean result = false;
        if(deleteLocal) {
            //删除会话缓存并同时删除该会话相关的本地消息
            result = TIMManager.getInstance().deleteConversationAndLocalMsgs(TIMConversationType.C2C, peer);
        } else {
            //删除会话缓存
            result = TIMManager.getInstance().deleteConversation(TIMConversationType.C2C, peer);
        }
        return result;
    }

    // 同步获取会话最后的消息
    public void getLastMsg(String peer) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C, peer);
        TIMMessage lastMsg = con.getLastMsg();

        List<TIMMessage> lst = new ArrayList<>();
        lst.add(lastMsg);
        parseMessageList(lst, TimMessageType.TimMessage.GetLastMsg.value());
    }

    // 删除会话本地消息
    public void deleteLocalMessage(String peer) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C, peer);
        con.deleteLocalMessage(new TIMCallBack() {
            @Override
            public void onError(int i, String s) {
                NativeCallback("删除成功 code=" + i + " desc=" + s);
            }
            @Override
            public void onSuccess() {
                NativeCallback("删除成功");
            }
        });
    }

    // 查找本地消息
    // IM SDK 提供了根据提供参数查找相应消息的功能，只能精准查找，暂时不支持模糊查找。
    // 开发者可以通过调用 TIMConversation 中的 findMessages 方法进行消息查找。
    public void findMessages(String peer, long seq, long rand, long timestamp, boolean isself) {}

    // 撤回消息
    public void revokeMessage(String peer, String json) {
        NativeCallback("revokeMessage json=" + json);

        TIMMessageLocatorResp resp = JSONObject.parseObject(json, TIMMessageLocatorResp.class);
        TIMMessage msg = getTIMMessage(resp);

        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C, peer);
        con.revokeMessage(msg, new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
                NativeCallback("revokeMessage failed. code: " + code + " errmsg: " + desc);
            }
            @Override
            public void onSuccess() {
                NativeCallback("撤回成功");
            }
        });
    }

    //</editor-fold>

    //<editor-fold desc="系统消息">
    //</editor-fold>

    //<editor-fold desc="设置后台消息通知栏提醒">

    //新消息可以显示在顶部通知栏，通知中心或锁屏上
    private void setNotification () {
//        NotificationManager mNotificationManager = (NotificationManager) context.getSystemService(context.NOTIFICATION_SERVICE);
//        NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(context);
//        Intent notificationIntent = new Intent(context, getActivity());
//        notificationIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP| Intent.FLAG_ACTIVITY_SINGLE_TOP);
//        PendingIntent intent = PendingIntent.getActivity(context, 0, notificationIntent, 0);
//        mBuilder.setContentTitle(senderStr)//设置通知栏标题
//                .setContentText(contentStr)
//                .setContentIntent(intent) //设置通知栏单击意图
//                .setNumber(++pushNum) //设置通知集合的数量
//                .setTicker(senderStr+":"+contentStr) //通知首次出现在通知栏，带上升动画效果的
//                .setWhen(System.currentTimeMillis())//通知产生的时间，会在通知信息里显示，一般是系统获取到的时间
//                .setDefaults(Notification.DEFAULT_ALL)//向通知添加声音、闪灯和振动效果的最简单、最一致的方式是使用当前的用户默认设置，使用 defaults 属性，可以组合
//                .setSmallIcon(R.drawable.ic_launcher);//设置通知小 ICON
//        Notification notify = mBuilder.build();
//        notify.flags |= Notification.FLAG_AUTO_CANCEL;
//        mNotificationManager.notify(pushId, notify);
    }
    //</editor-fold>

    //</editor-fold>

    //<editor-fold desc="未读计数">

    // 未读消息

    // 获取当前未读消息数量
    public long getUnreadMessageNum(String peer, int type) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.values()[type], peer);
        long num = con.getUnreadMessageNum();
        Log.d(TAG, "unread msg num: " + num);
        return num;
    }

    // 已读上报（单聊）
    public void setReadMessage(String peer) {
        TIMConversation con = TIMManager.getInstance().getConversation(TIMConversationType.C2C,peer);
        //将此会话的所有消息标记为已读
        con.setReadMessage(null, new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
                NativeCallback("setReadMessage failed, code: " + code + "|desc: " + desc);
            }
            @Override
            public void onSuccess() {
                NativeCallback("setReadMessage succ");
            }
        });
    }

    // 多终端已读上报同步
    public void onRefreshConversation() {}

    // 禁用自动上报
    public void disableAutoReport() {}
    //</editor-fold>

    //<editor-fold desc="群组相关">
    //</editor-fold>

    //<editor-fold desc="用户资料与关系链">

    // 获取自己的资料
    // 0=云端，1=本地
    public void getSelfProfile(int type) {
        switch (type) {
            case 0:
                //获取服务器保存的自己的资料
                TIMFriendshipManager.getInstance().getSelfProfile(new TIMValueCallBack<TIMUserProfile>(){
                    @Override
                    public void onError(int code, String desc){
                        JsonCallback(TimMessageType.TimMessage.GetSelfProfile.value(), code, desc);
                    }
                    @Override
                    public void onSuccess(TIMUserProfile result){
                        getProfileSucc(result, TimMessageType.TimMessage.GetSelfProfile);
                    }
                });
                break;
            case 1:
                //获取本地保存的自己的资料
                TIMUserProfile selfProfile = TIMFriendshipManager.getInstance().querySelfProfile();
                getProfileSucc(selfProfile, TimMessageType.TimMessage.GetSelfProfile);
                break;
        }
    }
    // 获取指定用户的资料
    // 0=云端，1=本地
    public void getUsersProfile(int type, String usersjson) {
        List<String> userArray = JSONObject.parseArray(usersjson, String.class);
        //待获取用户资料的用户列表
        List<String> users = new ArrayList<String>();
        for(String user : userArray){
            users.add(user);
        }

        switch (type) {
            case 0: //云端刷新
                TIMFriendshipManager.getInstance().getUsersProfile(users, true, new TIMValueCallBack<List<TIMUserProfile>>(){
                    @Override
                    public void onError(int code, String desc){
                        JsonCallback(TimMessageType.TimMessage.GetUsersProfile.value(), code, desc);
                    }
                    @Override
                    public void onSuccess(List<TIMUserProfile> result){
                        for(TIMUserProfile res : result){
                            getProfileSucc(res, TimMessageType.TimMessage.GetUsersProfile);
                        }
                    }
                });
                break;
            case 1: //本地缓存
                TIMUserProfile userProfile = TIMFriendshipManager.getInstance().queryUserProfile("sample_user_1");
                getProfileSucc(userProfile, TimMessageType.TimMessage.GetUsersProfile);
                break;
        }
    }
    private void getProfileSucc(TIMUserProfile userProfile, TimMessageType.TimMessage msg) {
        TIMUserProfileResp resp = new TIMUserProfileResp();
        resp.identifier = userProfile.getIdentifier();
        resp.nickName = userProfile.getNickName();
        resp.gender = userProfile.getGender();
        resp.birthday = userProfile.getBirthday();
        resp.allowType = TimMessageType.TIMFriendAllowType(userProfile.getAllowType());
        String json = JSONObject.toJSONString(resp,true);
        JsonCallback(msg.value(),0,json);
    }

    // 修改自己的昵称
    public void modifySelfNick(String nickName) {
        HashMap<String, Object> profileMap = new HashMap<>();
        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_NICK, nickName);
        modifySelfProfile(profileMap);
    }
    // 修改自己的性别
    public void modifySelfGender(int gender) {
        HashMap<String, Object> profileMap = new HashMap<>();
//        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_GENDER, TIMFriendGenderType.GENDER_MALE);
        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_GENDER, gender);
        modifySelfProfile(profileMap);
    }
    // 修改自己的性别
    public void modifySelfBirthday(long date) {
        HashMap<String, Object> profileMap = new HashMap<>();
        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_BIRTHDAY, date);
        modifySelfProfile(profileMap);
    }
    // 修改自己的隐私
    public void modifySelfAllowType(String allowType) {
        HashMap<String, Object> profileMap = new HashMap<>();
        //TIMProfileAddPermission //用户加好友的选项。
        //0=未知，1=允许任何人添加好友，2=拒绝任何人添加好友，3=添加好友需要验证
//        List<String> allowTypes = new ArrayList<>();
//        allowTypes.add(TIMFriendAllowType.TIM_FRIEND_INVALID);
//        allowTypes.add(TIMFriendAllowType.TIM_FRIEND_ALLOW_ANY);
//        allowTypes.add(TIMFriendAllowType.TIM_FRIEND_DENY_ANY);
//        allowTypes.add(TIMFriendAllowType.TIM_FRIEND_NEED_CONFIRM);
//        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_ALLOWTYPE, 3);
        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_ALLOWTYPE, allowType);
        modifySelfProfile(profileMap);
    }
    private void modifySelfProfile(HashMap<String, Object> profileMap) {
        TIMFriendshipManager.getInstance().modifySelfProfile(profileMap, new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
//                Log.e(TAG, "modifySelfProfile failed: " + code + " desc" + desc);
                JsonCallback(TimMessageType.TimMessage.ModifySelfProfile.value(), code, desc);
            }

            @Override
            public void onSuccess() {
//                Log.e(TAG, "modifySelfProfile success");
                JsonCallback(TimMessageType.TimMessage.ModifySelfProfile.value(), 0, "modifySelfProfile success");
            }
        });
    }
    // 支持泛型修改
    public void modifySelfProfile(String key, Object value) {
        NativeCallback("key:" + key + ", value:" + value);
        HashMap<String, Object> profileMap = new HashMap<>();
        profileMap.put(key, value);
        modifySelfProfile(profileMap);
    }
    // 修改自己的资料（自定义字段）
    public void modifySelfProfileCustom() {
        HashMap<String, Object> profileMap = new HashMap<>();
        profileMap.put(TIMUserProfile.TIM_PROFILE_TYPE_KEY_CUSTOM_PREFIX + "Blood", 1);
        TIMFriendshipManager.getInstance().modifySelfProfile(profileMap, new TIMCallBack() {
            @Override
            public void onError(int code, String desc) {
                Log.e(TAG, "modifySelfProfile failed: " + code + " desc" + desc);
            }
            @Override
            public void onSuccess() {
                Log.e(TAG, "modifySelfProfile success");
            }
        });
    }

    // 获取所有好友
    public void getFriendList() {
        TIMFriendshipManager.getInstance().getFriendList(new TIMValueCallBack<List<TIMFriend>>() {
            @Override
            public void onError(int code, String desc) {
                JsonCallback(TimMessageType.TimMessage.GetFriendList.value(), code, desc);
            }
            @Override
            public void onSuccess(List<TIMFriend> timFriends) {
                List<TIMUserProfileResp> friendList = new ArrayList<>();
                for (TIMFriend timFriend : timFriends){
                    TIMUserProfileResp friend = new TIMUserProfileResp();
                    friend.identifier = timFriend.getTimUserProfile().getIdentifier();
                    friend.nickName = timFriend.getTimUserProfile().getNickName();
                    friend.gender = timFriend.getTimUserProfile().getGender();
                    friend.birthday = timFriend.getTimUserProfile().getBirthday();
//                    friend.allowType = timFriend.getTimUserProfile().getAllowType();
                    friend.allowType = TimMessageType.TIMFriendAllowType(timFriend.getTimUserProfile().getAllowType());
                    friendList.add(friend);
                }
                String json = JSONObject.toJSONString(friendList,true);
                JsonCallback(TimMessageType.TimMessage.GetFriendList.value(),0,json);
            }
        });
    }

    // 修改好友
    public void modifyFriend() {}

    // 添加好友
    public void addFriend(String identifier, String word) {
        TIMFriendRequest timFriendRequest = new TIMFriendRequest(identifier);
        timFriendRequest.setAddWording(word);
        timFriendRequest.setAddSource("android");
        TIMFriendshipManager.getInstance().addFriend(timFriendRequest, new TIMValueCallBack<TIMFriendResult>() {
            @Override
            public void onError(int i, String s) {
//                QLog.e(TAG, "addFriend err code = " + i + ", desc = " + s);
                JsonCallback(TimMessageType.TimMessage.AddFriend.value(), i, s);
            }
            @Override
            public void onSuccess(TIMFriendResult timFriendResult) {
//                QLog.i(TAG, "addFriend success result = " + timFriendResult.toString());
                String json = JSONObject.toJSONString(timFriendResult,true);
                System.out.println("序列化：" + json);
                JsonCallback(TimMessageType.TimMessage.AddFriend.value(),0,json);
                //identifier=test02,resultCode=30516,resultInfo=Err_SNS_FriendAdd_Friend_Allow_Type_Forbid
            }
        });
    }

    // 删除好友 //1=单向，2=双向
    public void deleteFriends(int type, String usersjson) {
        List<String> userArray = JSONObject.parseArray(usersjson, String.class);
        List<String> identifiers = new ArrayList<>();
        for(String user : userArray){
            identifiers.add(user);
        }

        //TIMDelFriendType.TIM_FRIEND_DEL_SINGLE
        TIMFriendshipManager.getInstance().deleteFriends(identifiers, type, new TIMValueCallBack<List<TIMFriendResult>>() {
            @Override
            public void onError(int i, String s) {
//                QLog.e(TAG, "deleteFriends err code = " + i + ", desc = " + s);
                JsonCallback(TimMessageType.TimMessage.DeleteFriends.value(), i, s);
            }

            @Override
            public void onSuccess(List<TIMFriendResult> timUserProfiles) {
//                QLog.i(TAG, "deleteFriends success");
                for(int i = 0; i < timUserProfiles.size(); i++) {
                    NativeCallback("解除关系.成功："
                            + timUserProfiles.get(i).getIdentifier() + ", " //对方id
                            + timUserProfiles.get(i).getResultCode() + ", " //0=操作成功，31704=删除好友时对方不是好友。
                            + timUserProfiles.get(i).getResultInfo()); //OK/Err_SNS_FriendDelete_Not_Friend
                }
            }
        });
    }

    // 同意/拒绝好友申请 //0=同意，1=同意并添加，2=拒绝
    public void doFriendResponse(String identifier, int type) {
        TIMFriendResponse response = new TIMFriendResponse();
        response.setIdentifier(identifier);
        response.setResponseType(type);
        TIMFriendshipManager.getInstance().doResponse(response, new TIMValueCallBack<TIMFriendResult>(){
            @Override
            public void onError(int i, String s) {
                JsonCallback(TimMessageType.TimMessage.DoFriendResponse.value(), i, s);
            }
            @Override
            public void onSuccess(TIMFriendResult cb) {
                //0=操作成功
                //30614=响应好友申请时有效：对方没有申请过好友, Err_SNS_FriendResponse_Pendency_Not_Exist
                //30010=加好友、响应好友时有效：自己的好友数已达系统上限
                //30014=加好友、响应好友时有效：对方的好友数已达系统上限
                String json = JSONObject.toJSONString(cb,true);
                JsonCallback(TimMessageType.TimMessage.DoFriendResponse.value(), 0, json);
            }
        });
    }

    // 校验好友关系 //1=单向好友，2=互为好友
    public void checkFriends(int type, String usersjson) {
        List<String> userArray = JSONObject.parseArray(usersjson, String.class);
        List<String> identifiers = new ArrayList<>();
        for(String user : userArray){
            identifiers.add(user);
        }

        TIMFriendCheckInfo timCheckFriendRequest = new TIMFriendCheckInfo();
        timCheckFriendRequest.setUsers(identifiers);
//        timCheckFriendRequest.setCheckType(TIMFriendCheckType.TIM_FRIEND_CHECK_TYPE_BIDIRECTION);
        timCheckFriendRequest.setCheckType(type);
        TIMFriendshipManager.getInstance().checkFriends(timCheckFriendRequest, new TIMValueCallBack<List<TIMCheckFriendResult>>() {
            @Override
            public void onError(int i, String s) {
                JsonCallback(TimMessageType.TimMessage.CheckFriends.value(), i, s);
            }
            @Override
            public void onSuccess(List<TIMCheckFriendResult> checkFriendResult) {
                for(int i = 0; i < checkFriendResult.size(); i++) {
                    NativeCallback("校验关系.成功："
                            + checkFriendResult.get(i).getResultCode() + ", " //0
                            + checkFriendResult.get(i).getIdentifier() + ", " //对方id
                            + checkFriendResult.get(i).getResultInfo() + ", " //空
                            + checkFriendResult.get(i).getResultType()); //好友关系类型
                            // 0=不是好友，1=对方在我的好友列表中，2=我在对方的好友列表中，3=互为好友
                }
                String json = JSONObject.toJSONString(checkFriendResult,true);
                JsonCallback(TimMessageType.TimMessage.CheckFriends.value(), 0, json);
            }
        });
    }

    // 获取未决列表
    public void getPendencyList() {
        //0=操作成功，30001=请求参数错误，请根据错误描述检查请求是否正确
        TIMFriendPendencyRequest timFriendPendencyRequest = new TIMFriendPendencyRequest();
        timFriendPendencyRequest.setSeq(0);
        timFriendPendencyRequest.setTimestamp(0);
        timFriendPendencyRequest.setNumPerPage(10);
        timFriendPendencyRequest.setTimPendencyGetType(TIMPendencyType.TIM_PENDENCY_BOTH);
        TIMFriendshipManager.getInstance().getPendencyList(timFriendPendencyRequest, new TIMValueCallBack<TIMFriendPendencyResponse>() {
            @Override
            public void onError(int i, String s) {
                JsonCallback(TimMessageType.TimMessage.GetPendencyList.value(), i, s);
            }
            @Override
            public void onSuccess(TIMFriendPendencyResponse cb) {
                System.out.println("未决数：" + cb.getItems().size());
                String json = JSONObject.toJSONString(cb.getItems(),true);
                JsonCallback(TimMessageType.TimMessage.GetPendencyList.value(),0,json);
            }
        });
    }

    // 未决删除

    // 未决已读上报

    //</editor-fold>

    //<editor-fold desc="离线推送">
    //</editor-fold>

    //

    //<editor-fold desc="相册方法">
    private Uri imageUri = null;
    private static final int REQUEST_PERMISSION = 100;
    private static final int REQUEST_GALLERY    = 101;
    private static final int REQUEST_Camera     = 102;
    private static final int REQUEST_CROP       = 103;
    private static final int REQUEST_VIDEO      = 104;

    /**
     * 本地相册
     */
    public void openGallery() {
        Intent intent = new Intent(Intent.ACTION_PICK);
        intent.setDataAndType(MediaStore.Images.Media.INTERNAL_CONTENT_URI, "image/*");
        startActivityForResult(intent, REQUEST_GALLERY);
    }

    /**
     * 拍照
     */
    public void openCamera() {
        Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        imageUri = getImageUri();
        intent.putExtra(MediaStore.EXTRA_OUTPUT, imageUri);
        startActivityForResult(intent, REQUEST_Camera);
    }
    public Uri getImageUri() {
        File file = new File(Environment.getExternalStorageDirectory(), "/temp/" + System.currentTimeMillis() + ".jpg");
        if (!file.getParentFile().exists()) {
            file.getParentFile().mkdirs();
        }
        String path = file.getPath();
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.N) {
            imageUri = Uri.fromFile(file);
        } else {
            //兼容android7.0 使用共享文件的形式
            ContentValues contentValues = new ContentValues(1);
            contentValues.put(MediaStore.Images.Media.DATA, path);
            imageUri = this.getApplication().getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, contentValues);
        }
        return imageUri;
    }

    /**
     * 选择视频
     */
    public void openVideo() {
        //Intent.ACTION_GET_CONTENT获取的是所有本地图片
        //Intent.ACTION_PICK获取的是相册中的图片
        Intent intent = new Intent(Intent.ACTION_PICK);
//        Intent intent = new Intent(Intent.ACTION_PICK, android.provider.MediaStore.Video.Media.EXTERNAL_CONTENT_URI);

        //intent.setType("image/*"); //选择图片
        // intent.setType("audio/*"); //选择音频
        intent.setType("video/*"); //选择视频 （mp4 3gp 是android支持的视频格式）
        // intent.setType("video/*;image/*");//同时选择视频和图片

        /* 取得相片后返回本画面 */
        startActivityForResult(intent, REQUEST_VIDEO);
    }

    //</editor-fold>

    // GPS
    public boolean checkGPSIsOpen() {
        boolean isOpen;
        LocationManager locationManager = (LocationManager) currentActivity.getSystemService(Context.LOCATION_SERVICE);
        isOpen = locationManager.isProviderEnabled(android.location.LocationManager.GPS_PROVIDER);
        return isOpen;
    }
    public void openGPSSetting() {
        //1、提示用户打开定位服务；2、跳转到设置界面
        Toast.makeText(currentActivity, "无法定位，请打开定位服务", Toast.LENGTH_SHORT).show();
        Intent i = new Intent();
        i.setAction(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
        startActivity(i);
    }

    //拷贝String到剪贴板
    public void onClickCopy(String str) {
        //获取剪贴板管理器：
        ClipboardManager cm = (ClipboardManager) currentActivity.getSystemService(Context.CLIPBOARD_SERVICE);
        // 创建普通字符型ClipData
        ClipData mClipData = ClipData.newPlainText("Label", str); //Label是任意文字标签
        // 将ClipData内容放到系统剪贴板里。
        cm.setPrimaryClip(mClipData);

        NativeCallback("onClickCopy: " + cm.toString());
    }
    //粘贴
    public String onClickPaste() {
        ClipboardManager cm = (ClipboardManager) currentActivity.getSystemService(Context.CLIPBOARD_SERVICE);
        ClipData clipData = cm.getPrimaryClip();
        ClipData.Item item = clipData.getItemAt(0); //这里获取第一条，也可以用遍历获取任意条
        CharSequence charSequence = item.coerceToText(currentActivity.getApplicationContext());

        String result = charSequence.toString();
//        NativeCallback("onClickPaste: " + result);
        return result;
    }
}
