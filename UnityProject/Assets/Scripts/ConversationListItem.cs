using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SuperScrollView;

namespace Client
{
    public class ConversationListItem : MonoBehaviour, IPointerClickHandler
    {
        //public LoopListView2 mLoopListView;
        int mItemDataIndex = -1;
        TIMUserProfileExt profile;
        public Image mIcon;
        public Text mNameText;
        public Text mAmountText;
        public Text mLastMsgText;
        public Text mTimeText;
        public GameObject mRedDot;
        public Text mUnreadCountText;

        public void Init()
        {

        }

        public void SetItemData(TIMUserProfileExt itemData,int itemIndex)
        {
            this.profile = itemData;

            //mIcon.sprite = ResManager.Get.GetSpriteByName(itemData.mIcon);
            mItemDataIndex = itemIndex;
            mNameText.text = itemData.nickName;
            mAmountText.text = $"{itemData.amount}";
            mLastMsgText.text = itemData.lastMsg;
            //mTimeText.text = Utils.ConvertTimestamp(itemData.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            mTimeText.text = ConvertDateTime(itemData.timestamp);
            mRedDot.SetActive(itemData.unreadMessageNum > 0);
            mUnreadCountText.text = $"{itemData.unreadMessageNum}";
        }

        static string ConvertDateTime(long msgTime) 
        {
            var msgDateTime = Utils.ConvertTimestamp(msgTime);
            TimeSpan span = DateTime.Now - msgDateTime;
            //Debug.Log(span.TotalSeconds);

            if (span.TotalSeconds <= 3600 * 24)
            {
                //24小时之内，时：分
                return $"{msgDateTime.ToString("HH:mm")}";
            }
            else if (span.TotalSeconds > 3600 * 24 && span.TotalSeconds <= 3600 * 48)
            {
                //48小时之内，昨天
                return "昨天";
            }
            else if (span.TotalSeconds > 3600 * 48 && (msgDateTime.Year >= DateTime.Now.Year))
            {
                //48小时之外，月日
                return $"{msgDateTime.Month}/{msgDateTime.Day}";
            }
            else
            {
                //自然年去年，年月日
                return $"{msgDateTime.Year}/{msgDateTime.Month}/{msgDateTime.Day}";
                //return $"{msgDateTime.ToString("yyyy-MM-dd HH:mm:ss")}\n{span.TotalSeconds}";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"goto chat identifier={profile.identifier}");
        }
    }
}
