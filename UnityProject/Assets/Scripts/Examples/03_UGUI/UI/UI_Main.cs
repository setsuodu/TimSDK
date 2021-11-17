using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class UI_Main : UIWidget
{
    private int index = -1;
    [SerializeField] private Item_Main[] m_Buttons;
    [SerializeField] private Button m_PublishBtn;

    void Awake()
    {
        m_PublishBtn.onClick.AddListener(OnPublishButtonClick);
    }

    void Start()
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            int idx = i;
            m_Buttons[i].m_Button?.onClick.AddListener(() => OnNavigation(idx)); //必须在Start中，等待子类Awake完成
        }
        OnNavigation(0);
    }

    void OnNavigation(int idx) 
    {
        if (index == idx) 
        {
            //TODO: 限定刷新cd
            return;
        }
        index = idx;
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].Enable(false);
        }
        m_Buttons[index].Enable(true);

        switch (idx) 
        {
            case 0:
                {
                    //Debug.Log("匹配");
                    OnMatch(true);
                }
                break;
            case 1:
                {
                    //Debug.Log("广场");
                    OnSocial(true);
                }
                break;
            case 2:
                {
                    //Debug.Log("消息");
                    OnConversation(true);
                }
                break;
            case 3:
                {
                    //Debug.Log("主页");
                    OnHome(true);
                }
                break;
        }
    }

    void OnMatch(bool value)
    {
        if (value)
        {
            PanelManager.Instance.CloseAll(1);
            PanelManager.Instance.CreatePanel<UI_Match>(false);
        }
    }

    void OnSocial(bool value)
    {
        if (value)
        {
            PanelManager.Instance.CloseAll(1);
            PanelManager.Instance.CreatePanel<UI_Social>(false);
        }
    }

    void OnConversation(bool value)
    {
        if (value)
        {
            PanelManager.Instance.CloseAll(1);
            PanelManager.Instance.CreatePanel<UI_Message>(false);
        }
    }

    void OnHome(bool value)
    {
        if (value)
        {
            PanelManager.Instance.CloseAll(1);
            PanelManager.Instance.CreatePanel<UI_Home>(false);
        }
    }

    void OnPublishButtonClick()
    {
        PanelManager.Instance.CreatePanel<UI_CreateArticle>(false, true);
    }
}
