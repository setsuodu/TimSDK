package com.zdkj.resp;

import com.tencent.imsdk.TIMImage;

public class TIMImageResp {
    public int type; //Original(0), Thumb(1), Large(2)
    public String url;
    public String uuid;
    public long size;
    public long height;
    public long width;

    public TIMImageResp setImage(TIMImage img) {
        this.type = img.getType().value();
        this.url = img.getUrl();
        this.uuid = img.getUuid();
        this.size = img.getSize();
        this.height = img.getHeight();
        this.width = img.getWidth();
        return this;
    }
}
