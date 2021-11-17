using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

/// <summary>
/// 黑名单
/// </summary>
public class UI_Blacks : UIWidget
{
    [SerializeField] private Button m_BackBtn;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
    }
}
