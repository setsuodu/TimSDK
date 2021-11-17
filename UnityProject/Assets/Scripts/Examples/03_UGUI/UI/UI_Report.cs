using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Report : UIWidget
{
    [SerializeField] private Button m_BackBtn;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
    }
}