package com.zdkj.plugin;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.res.Resources;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class NativeActivity extends UnityPlayerActivity {
    public static Activity mActivity;

    @Override
    protected void onCreate(Bundle bundle){
        super.onCreate(bundle);
        mActivity = UnityPlayer.currentActivity;
    }

    // our native method, which will be called from Unity3D
    public void PrintString(final Context ctx, final String message) {
        //create / show an android toast, with message and short duration.
        new Handler(Looper.getMainLooper()).post(new Runnable() {
            @Override
            public void run() {
                Toast.makeText(ctx, message, Toast.LENGTH_SHORT).show();
            }
        });
    }

    public static int showSimpleAlert(final String msg){
        final Activity mActivity = UnityPlayer.currentActivity;
        mActivity.runOnUiThread(new Runnable(){
            public void run()
            {
                new AlertDialog.Builder(mActivity).setTitle("")
                        .setMessage(msg)
                        .setPositiveButton("OK", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int which) {

                            }
                        })
                        .setIcon(android.R.drawable.ic_dialog_alert)
                        .show();
            }
        });
        return 1;
    }

    @Override
    public void onBackPressed() {
        super.onBackPressed();
        UnityPlayer.UnitySendMessage("Activity","NativeCallback","onBackPressed!");
    }

    public void Test1() {
        System.out.println("println haha...");
        UnityPlayer.UnitySendMessage("Activity","NativeCallback","Haha TestActivity!");
    }

    // 获取顶部status bar 高度
    public int getStatusBarHeight() {
        Resources resources = mActivity.getResources();
        int resourceId = resources.getIdentifier("status_bar_height", "dimen","android");
        int height = resources.getDimensionPixelSize(resourceId);
        Log.v("dbw", "Status height:" + height);
        return height;
    }

    // 获取底部 navigation bar 高度
    public int getNavigationBarHeight() {
        Resources resources = mActivity.getResources();
        int resourceId = resources.getIdentifier("navigation_bar_height","dimen", "android");
        int height = resources.getDimensionPixelSize(resourceId);
        Log.v("dbw", "Navi height:" + height);
        return height;
    }
}
