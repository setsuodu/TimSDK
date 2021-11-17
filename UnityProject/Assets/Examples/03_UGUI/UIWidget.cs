using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIWidget : MonoBehaviour
{
    [SerializeField] protected CanvasGroup[] groups; //手动配置
    [SerializeField] protected const float TWEEN_TIME = 0.3f; //页面进出动画时间
    [SerializeField] protected const float HTTP_TIME_OUT = 3f; //http超时时间
    //[SerializeField] protected int retry = 1; //请求失败重试次数

    public virtual void Show()
    {
        transform.DOLocalMoveX(0, TWEEN_TIME);
    }
    public virtual void Hide(System.Action action = null)
    {
        Tweener tw = transform.DOLocalMoveX(360, TWEEN_TIME);
        tw.OnComplete(()=>
        {
            action?.Invoke();
        });
    }

    public virtual void SetAlpha(bool value)
    {
        foreach (var item in groups)
        {
            item.alpha = value ? 1 : 0;
        }
    }
    public virtual void FadeIn()
    {
        foreach (var item in groups)
        {
            item.alpha = 0;
            item.DOFade(1, 0.3f);
            item.interactable = true;
            item.blocksRaycasts = true;
        }
    }
    public virtual void FadeOut()
    {
        foreach (var item in groups)
        {
            item.alpha = 1;
            item.DOFade(0, 0.3f);
            item.interactable = false;
            item.blocksRaycasts = false;
        }
    }

    public virtual void OnSystemMessage(int value)
    {

    }
    public virtual void OnTimSdkMessage(TimCallback obj)
    {
        //Debug.Log($"{(TimMessage)obj.msg} | msg={obj.msg}, code={obj.code}, data={obj.data}");
    }
    public virtual void Close(bool anim = false)
    {
        PanelManager.Instance.CloseLast(anim);
    }
}
