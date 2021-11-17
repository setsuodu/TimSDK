using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

/// <summary>
/// 表白墙
/// </summary>
public class UI_Wall : UIWidget
{
    [SerializeField] private Button m_BackBtn;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
    }

    public void Refresh()
    {

    }
}
