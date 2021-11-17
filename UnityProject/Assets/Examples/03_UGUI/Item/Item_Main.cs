using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Main : MonoBehaviour
{
    [SerializeField] private GameObject[] m_Icons;
    public Button m_Button;

    void Awake()
    {
        m_Icons = new GameObject[2]
        {
            transform.GetChild(0).gameObject,
            transform.GetChild(1).gameObject
        };
        m_Button = GetComponent<Button>();
    }

    public void Enable(bool value)
    {
        //On=0, Off=1
        m_Icons[0].SetActive(value);
        m_Icons[1].SetActive(!value);
    }
}
