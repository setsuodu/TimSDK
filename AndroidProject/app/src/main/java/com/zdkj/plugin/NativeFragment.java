package com.zdkj.plugin;

import android.app.Fragment;
import android.os.Build;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import org.json.JSONException;
import org.json.JSONObject;

public class NativeFragment extends Fragment {
    private static final String TAG = "NativePlugin";
    private static NativeFragment Instance = null;
    private String gameObjectName;
    public static NativeFragment GetInstance(String gameObject) {
        if(Instance == null) {
            Instance = new NativeFragment();
            Instance.gameObjectName = gameObject;
            UnityPlayer.currentActivity.getFragmentManager().beginTransaction().add(Instance, TAG).commit();
        }
        return Instance;
    }

    // 字符串消息返回
    public void NativeCallback(String log){
        UnityPlayer.UnitySendMessage(gameObjectName,"NativeCallback", log);
    }
    // json消息返回
    public void JsonCallback(int msgId, String param){
        JSONObject obj = new JSONObject();
        try {
            obj.put("msgId", msgId);
            obj.put("param", param);
            System.out.println("JSON=========" + obj.toString());
        } catch (JSONException e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage(gameObjectName,"JsonCallback", obj.toString());
    }

    //<editor-fold desc="本地">

    public void SystemPrint() {
        System.out.println("println test...2");
    }

    // 示例方法一：简单的向Unity回调
    public void HelloWorld() {
        NativeCallback("HelloWorld!");
    }

    // 检查安卓手机厂商
    public String CheckOEM() {
        if (Build.MANUFACTURER.equalsIgnoreCase("xiaomi")){
            return "小米";
        }else if(Build.MANUFACTURER.equalsIgnoreCase("samsung")){
            return "三星";
        }else if(Build.MANUFACTURER.equalsIgnoreCase("huawei")){
            return "华为";
        }else {
            return "其他原生系统手机";
        }
    }

    // json序列化
    public void MakeJson() {

    }

    //</editor-fold>
}
