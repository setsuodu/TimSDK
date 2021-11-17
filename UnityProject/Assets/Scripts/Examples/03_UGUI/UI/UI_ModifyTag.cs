using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class UI_ModifyTag : UIWidget
{
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_CommitBtn;

    void Awake()
    {
        m_BackBtn.onClick.AddListener(() => base.Close(true));
        m_CommitBtn.onClick.AddListener(OnCommit);
    }

    void Start()
    {
        
    }

    void OnCommit()
    {
    
    }
}