using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Item_Menu : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button[] buttons;
    private MenuItemClass[] items;

    public void Init(MenuItemClass[] array)
    {
        items = array;
        for (int i = 0; i < items.Length; i++) 
        {
            int index = i;
            buttons[i].onClick.AddListener(() => { DoAction(index); });
            buttons[i].GetComponentInChildren<Text>().text = items[i].title;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("exit");
        LongPressManager.Hide();
    }

    void DoAction(int index)
    {
        items[index].action?.Invoke();
        LongPressManager.Hide();
    }
}
