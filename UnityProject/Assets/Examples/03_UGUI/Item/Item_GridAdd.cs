using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_GridAdd : MonoBehaviour
{
    [SerializeField] private Button m_AddBtn;

    void Awake()
    {
        m_AddBtn.onClick.AddListener(OnAddBtnClick);
    }

    void OnAddBtnClick()
    {
        Debug.Log("添加");
    }
}
