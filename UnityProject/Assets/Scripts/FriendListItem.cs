using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Client
{
    public class FriendListItem : MonoBehaviour, IPointerClickHandler
    {
        int mItemDataIndex = -1;
        private TIMUserProfileExt profile;
        public Image mIcon;
        public Text mNameText;
        public Text mAmountText;
        public Button mRelieve;

        public void SetItemData(TIMUserProfileExt itemData,int itemIndex)
        {
            this.profile = itemData;

            mItemDataIndex = itemIndex;
            mNameText.text = profile.nickName;
            mAmountText.text = $"亲密度={itemData.amount}";
            mRelieve.gameObject.SetActive(true);
            FileManager.Download(profile.faceUrl, OnLoadHeadImage);
        }

        void OnLoadHeadImage(byte[] bytes)
        {
            Texture2D t2d = new Texture2D(2, 2);
            t2d.LoadImage(bytes);
            Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            mIcon.sprite = sp;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"goto chat identifier={profile.identifier}");
            Chat();
        }

        void Chat()
        {
            var script = PanelManager.Instance.CreatePanel<UI_Chat>(false, true);
            script.Init(profile);
        }

        void Unfriend()
        {
            Debug.Log("解除关系");
        }
    }
}
