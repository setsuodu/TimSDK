using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SuperScrollView;

public class DebugTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public LoopListView2 mLoopListView;

    [SerializeField] private Scrollbar m_Scrollbar;
    [SerializeField] private ScrollRect m_ScrollRect;

    public float mTargetValue;
    public int CurrentDot = 1;
    private bool mNeedMove = false;
    //private const float MOVE_SPEED = 1f;
    //private const float SMOOTH_TIME = 0.2f;
    private float mMoveSpeed = 0f;
    public float showValue;

    void Start()
    {
        
    }

    [ContextMenu("Print")]
    void Print()
    {
        Debug.Log(mLoopListView.ScrollRect.horizontalScrollbar.numberOfSteps);
        //Debug.Log(mLoopListView.ScrollRect.verticalScrollbar.numberOfSteps);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData data)
    {
        //Debug.Log("OnPointerDown");

        //m_ScrollRect.horizontal = true;
        mNeedMove = false;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData data)
    {
        //m_ScrollRect.horizontal = false;

        // 2页
        if (m_Scrollbar.value <= 0.5f)
        {
            mTargetValue = 0;
            CurrentDot = 1;

            Debug.Log("<= 0.5f");
        }
        else
        {
            mTargetValue = 1f;
            CurrentDot = 2;

            Debug.Log("> 0.5f");
        }

        mNeedMove = true;
        mMoveSpeed = 0;

        ////轮播点颜色
        //for (int i = 0; i < dots.Length; i++)
        //{
        //    dots[i].color = colorOff;
        //}
        //dots[CurrentDot - 1].color = colorOn;
    }
}
