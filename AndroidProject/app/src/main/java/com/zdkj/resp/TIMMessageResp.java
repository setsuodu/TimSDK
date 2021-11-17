package com.zdkj.resp;

import com.tencent.imsdk.TIMMessage;

// IM SDK 中的消息需要通过{seq, rand, timestamp, isSelf} 四元组来唯一确定一条具体的消息，我们把这个四元组称为消息的查找参数。
// 通过 TIMMessage 中的 getMessageLocator 接口可以从当前消息中获取到当前消息的查找参数。
public class TIMMessageResp {
    public long seq;
    public long rand;
    public long timestamp;
    public boolean isSelf;
    public boolean isRead;
    public boolean isPeerReaded;
    public int elemType;
    public int subType;
    public String text; //文本消息内容，图片消息路径
    public String param; //备注信息,Json格式

    public TIMMessageResp setMessage(TIMMessage msg) {
        this.seq = msg.getSeq();
        this.rand = msg.getRand();
        this.timestamp = msg.timestamp();
        this.isSelf = msg.isSelf();
        this.isRead = msg.isRead();
        this.isPeerReaded = msg.isPeerReaded();
        return this;
    }
    public TIMMessageResp setType(int main, int sub) {
        this.elemType = main;
        this.subType = sub;
        return this;
    }
    public TIMMessageResp setText(String value) {
        this.text = value;
        return this;
    }
    public TIMMessageResp setParam(String value) {
        this.param = value;
        return this;
    }
}
