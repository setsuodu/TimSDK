/*
 * Copyright (c) 2015 Kyungmin Bang
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;
using AOT;

public class PluginMsgHandler : MonoBehaviour
{
    private static PluginMsgHandler _instance;

    private int _curReceiverIndex;
    private Dictionary<int, PluginMsgReceiver> _receiverDict;

    private const string DEFAULT_NAME = "NativeEditPluginHandler";

    private bool isEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
			return false;
#endif
        }
    }

    private bool isStandalone
    {
        get
        {
#if UNITY_STANDALONE
            return true;
#else
			return false;
#endif
        }
    }

    public static PluginMsgHandler GetInstanceForReceiver(PluginMsgReceiver receiver)
    {
        if(_instance == null)
        {
            var handlerObject = new GameObject(DEFAULT_NAME);
            _instance = handlerObject.AddComponent<PluginMsgHandler>();
        }

        return _instance;
    }

    private void Awake()
    {
        _receiverDict = new Dictionary<int, PluginMsgReceiver>();
        InitializeHandler();
    }

    private void OnDestroy()
    {
        FinalizeHandler();
        _instance = null;
    }

    public int RegisterAndGetReceiverId(PluginMsgReceiver receiver)
    {
        if(receiver == null)
            throw new ArgumentNullException(
                "MonoNativeEditBox: Receiver cannot be null while RegisterAndGetReceiverId!");

        var index = _curReceiverIndex;
        _curReceiverIndex++;

        _receiverDict[index] = receiver;
        return index;
    }

    public void RemoveReceiver(int nReceiverId)
    {
        _receiverDict.Remove(nReceiverId);
        if(_receiverDict.Count == 0) Destroy(_instance.gameObject);
    }

    public PluginMsgReceiver GetReceiver(int nSenderId)
    {
        return _receiverDict[nSenderId];
    }

    private void OnMsgFromPlugin(string jsonPluginMsg)
    {
        if(jsonPluginMsg == null) return;
        var jsonMsg = new JsonObject(jsonPluginMsg);
        var nSenderId = jsonMsg.GetInt("senderId");

        // In some cases the receiver might be already removed, for example if a button is pressed
        // that will destroy the receiver while the input field is focused an end editing message
        // will be sent from the plugin after the receiver is already destroyed on Unity side.
        if(_receiverDict.ContainsKey(nSenderId))
        {
            var receiver = GetReceiver(nSenderId);
            receiver.OnPluginMsgDirect(jsonMsg);
        }
    }

#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _iOS_InitPluginMsgHandler(string unityName);
	[DllImport ("__Internal")]
	private static extern string _iOS_SendUnityMsgToPlugin(int nSenderId, string strMsg);
	[DllImport ("__Internal")]
	private static extern void _iOS_ClosePluginMsgHandler();	

	public void InitializeHandler()
	{		
		if (!isEditor)
			_iOS_InitPluginMsgHandler(this.name);
	}
	
	public void FinalizeHandler()
	{
		if (!isEditor)
			_iOS_ClosePluginMsgHandler();
		
	}

#elif UNITY_ANDROID
	private static AndroidJavaClass smAndroid;
	public void InitializeHandler()
	{	
		if (isEditor) return;

		// Reinitialization was made possible on Android to be able to use as a workaround in an issue where the
		// NativeEditBox text would be hidden after using Unity's Handheld.PlayFullScreenMovie().
		if (smAndroid == null)
			smAndroid = new AndroidJavaClass("com.bkmin.android.NativeEditPlugin");
		smAndroid.CallStatic("InitPluginMsgHandler", name);
	}
	
	public void FinalizeHandler()
	{	
		if (!isEditor)
			smAndroid.CallStatic("ClosePluginMsgHandler");
	}

#else
    public void InitializeHandler()
    {
    }

    public void FinalizeHandler()
    {
    }

#endif


    public JsonObject SendMsgToPlugin(int nSenderId, JsonObject jsonMsg)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return new JsonObject();
#else
        jsonMsg["senderId"] = nSenderId;
        string strJson = jsonMsg.Serialize();

        string strRet = "";
        #if UNITY_IPHONE
        strRet = _iOS_SendUnityMsgToPlugin(nSenderId, strJson);
        #elif UNITY_ANDROID 
        strRet = smAndroid.CallStatic<string>("SendUnityMsgToPlugin", nSenderId, strJson);
        #endif

        JsonObject jsonRet = new JsonObject(strRet);
        return jsonRet;
#endif
    }
}