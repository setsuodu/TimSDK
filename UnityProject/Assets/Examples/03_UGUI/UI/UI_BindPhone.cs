using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using MaterialUI;

public class UI_BindPhone : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private InputField m_PhoneInput;
    [SerializeField] private InputField m_CodeInput;
    [SerializeField] private Button m_GetCodeBtn;
    [SerializeField] private Button m_CommitBtn;
    [SerializeField] private Text m_ColdDownText;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));

        m_GetCodeBtn.onClick.AddListener(CmdGetCode);
        m_CommitBtn.onClick.AddListener(CmdCommit);
    }

    void CmdGetCode()
    {
        //TODO: 非空，长度/格式校验
        if (string.IsNullOrEmpty(m_PhoneInput.text)) 
        {
            ToastManager.Show("手机号为空", 0.5f, MaterialUIManager.UIRoot);
            return;
        }
        if (!Utils.CheckPhoneNumber(m_PhoneInput.text))
        {
            ToastManager.Show("手机号格式错误", 0.5f, MaterialUIManager.UIRoot);
            return;
        }

        HttpManager.sendSMS(123456, m_PhoneInput.text, onSendSMS); //获取验证码，60s?内有效
    }

    void CmdCommit()
    {
        HttpManager.binding(m_PhoneInput.text, m_CodeInput.text, onBinding);
    }

    //短信验证码倒计时器
    IEnumerator NextCodeTime()
    {
        int timer = 60;
        m_GetCodeBtn.interactable = false;

        while (timer >= 0)
        {
            m_ColdDownText.text = $"{timer}s";
            yield return new WaitForSeconds(1); //暂停1秒
            timer--;
        }

        //Debug.Log("倒计时结束");
        m_ColdDownText.text = "获取验证码";
        m_GetCodeBtn.interactable = true;
    }

    #region HTTP回调

    void onSendSMS(string json)
    {
        Debug.Log($"sms={json}"); //{"code":0,"msg":"OK","data":null}

        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            ToastManager.Show("验证码获取成功", 0.5f, MaterialUIManager.UIRoot);

            StartCoroutine(NextCodeTime());
        }
        else
        {
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    void onBinding(string json)
    {
        Debug.Log($"binding={json}");

        var obj = JsonMapper.ToObject<HttpCallback>(json);
        if (obj.code == 0)
        {
            ToastManager.Show("手机绑定成功", 0.5f, MaterialUIManager.UIRoot);
        }
        else
        {
            ToastManager.Show(obj.msg, 0.5f, MaterialUIManager.UIRoot);
        }
    }

    #endregion
}
