//  Copyright 2017 MaterialUI for Unity http://materialunity.com
//  Please see license file for terms and conditions of use, and more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MaterialUI
{
    [AddComponentMenu("MaterialUI/Dialogs/Bottom List", 1)]
    public class DialogBottomList : MaterialDialog
    {
        [SerializeField]
        private VerticalScrollLayoutElement m_ListScrollLayoutElement;
        public VerticalScrollLayoutElement listScrollLayoutElement
        {
            get { return m_ListScrollLayoutElement; }
            set { m_ListScrollLayoutElement = value; }
        }

        private List<DialogSimpleOption> m_SelectionItems;
        public List<DialogSimpleOption> selectionItems
        {
            get { return m_SelectionItems; }
        }

        private OptionDataList m_OptionDataList;
        public OptionDataList optionDataList
        {
            get { return m_OptionDataList; }
            set { m_OptionDataList = value; }
        }

        [SerializeField]
        private GameObject m_OptionTemplate;

        private Action<int> m_OnItemClick;

		void Start()
		{
			GetComponentInChildren<OverscrollConfig>().Setup();
		}

		public void Initialize(OptionDataList optionDataList, Action<int> onItemClick)
        {
            m_OptionDataList = optionDataList;
            m_SelectionItems = new List<DialogSimpleOption>();

            for (int i = 0; i < m_OptionDataList.options.Count; i++)
            {
                CreateSelectionItem(i);
            }

            float availableHeight = DialogManager.rectTransform.rect.height;
            m_ListScrollLayoutElement.maxHeight = availableHeight - 48f;
            m_OptionTemplate.SetActive(false);

            m_OnItemClick = onItemClick;

            //Initialize();

            float canvasWidth = DialogManager.rectTransform.rect.width; //等宽
            rectTransform.sizeDelta = new Vector2(canvasWidth, rectTransform.sizeDelta.y);

            gameObject.SetActive(false);
        }

        public void AddItem(OptionData data)
        {
            m_OptionDataList.options.Add(data);
            CreateSelectionItem(m_OptionDataList.options.Count - 1);
        }

        public void ClearItems()
        {
            m_OptionDataList.options.Clear();

            for (int i = 0; i < m_SelectionItems.Count; i++)
            {
                Destroy(m_SelectionItems[i].gameObject);
            }

            m_SelectionItems.Clear();
        }

        private void CreateSelectionItem(int i)
        {
            DialogSimpleOption option = Instantiate(m_OptionTemplate).GetComponent<DialogSimpleOption>();
            option.rectTransform.SetParent(m_OptionTemplate.transform.parent);
            option.rectTransform.localScale = Vector3.one;
            option.rectTransform.localEulerAngles = Vector3.zero;
            option.rectTransform.localPosition = new Vector3(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y, 0f);

            OptionData data = m_OptionDataList.options[i];

            Text text = option.gameObject.GetChildByName<Text>("Text");
            text.text = data.text;

            if (data.imageData == null)
            {
                text.rectTransform.sizeDelta = new Vector2(-48f, 0f);
                text.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            }

            option.index = i;
            option.onClickAction += OnItemClick;
            option.gameObject.SetActive(true);

            m_SelectionItems.Add(option);
        }

        public void OnItemClick(int index)
        {
            m_OnItemClick.InvokeIfNotNull(index);
			m_OptionDataList.options[index].onOptionSelected.InvokeIfNotNull();

            Hide();
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Hide();
			}
		}
    }
}