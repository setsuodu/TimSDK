using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Scrollbar m_Scrollbar;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private Image[] dots;
    [SerializeField] private float mTargetValue;

    public int CurrentDot = 1;
    private bool mNeedMove = false;
    private const float MOVE_SPEED = 1f;
    private const float SMOOTH_TIME = 0.2f;
    private float mMoveSpeed = 0f;
    public float showValue;

    //便于在不同底色上改颜色
    private Color colorOn = new Color(0.5f, 0.5f, 0.5f, 1);
    private Color colorOff = new Color(0.9f, 0.9f, 0.9f, 1);

    void Awake()
    {
        m_ScrollRect.horizontal = false;
        CurrentDot = 1;
    }

    void Update()
    {
        showValue = m_Scrollbar.value;
        if (mNeedMove)
        {
            if (Mathf.Abs(m_Scrollbar.value - mTargetValue) < 0.01f) //停下的条件
            {
                m_Scrollbar.value = mTargetValue;
                mNeedMove = false;
                return;
            }
            m_Scrollbar.value = Mathf.SmoothDamp(m_Scrollbar.value, mTargetValue, ref mMoveSpeed, SMOOTH_TIME);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData data)
    {
        m_ScrollRect.horizontal = true;
        mNeedMove = false;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData data)
    {
        m_ScrollRect.horizontal = false;
        // 判断当前位于哪个区间，设置自动滑动至的位置
        //if (m_Scrollbar.value <= 0.125f)
        //{
        //    mTargetValue = 0;
        //    CurrentDot = 1;
        //}
        //else if (m_Scrollbar.value <= 0.375f)
        //{
        //    mTargetValue = 0.25f;
        //    CurrentDot = 2;
        //}
        //else if (m_Scrollbar.value <= 0.625f)
        //{
        //    mTargetValue = 0.5f;
        //    CurrentDot = 3;
        //}
        //else if (m_Scrollbar.value <= 0.875f)
        //{
        //    mTargetValue = 0.75f;
        //    CurrentDot = 4;
        //}
        //else
        //{
        //    mTargetValue = 1f;
        //    CurrentDot = 5;
        //}
        if (m_Scrollbar.value <= 0.5f)
        {
            mTargetValue = 0;
            CurrentDot = 1;
        }
        else
        {
            mTargetValue = 1f;
            CurrentDot = 2;
        }

        mNeedMove = true;
        mMoveSpeed = 0;

        //轮播点颜色
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = colorOff;
        }
        dots[CurrentDot - 1].color = colorOn;
    }
}
