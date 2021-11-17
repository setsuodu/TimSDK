using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

namespace Client
{
    public class ArticleListItemWall : MonoBehaviour
    {
        public ArticleItemData mItemData;
        public int mItemDataIndex = -1;

        [SerializeField] private Button mWallBtn;
        [SerializeField] private Image mHeadImage;
        [SerializeField] private Text mNickText;
        [SerializeField] private Text mDescText;
        [SerializeField] private Text mCountText;
        [SerializeField] private Image m_GenderImage;
        [SerializeField] private Sprite[] genderSprites;
        [SerializeField] private Text mContentText;

        void Awake()
        {
            mWallBtn.onClick.AddListener(OnWallBtnClick);
        }

        void Start()
        {
            HttpManager.showLoveTop(onShowLoveTop);
        }

        public void SetItemData(ArticleItemData itemData, int itemIndex)
        {
            mItemData = itemData;
            mItemDataIndex = itemIndex;
            //这里itemData是null，通过http请求获得的
            //Debug.Log($"itemData={itemData.ToString()}");
        }

        void OnWallBtnClick()
        {
            PanelManager.Instance.CreatePanel<UI_Wall>(false, true);
        }

        void onShowLoveTop(string json)
        {
            //Debug.Log($"表白墙: {json}");
            //{"code":0,"msg":"OK","data":{"sendUser":{"nickname":"圣诞w@@#^sss","sex":1,"avatar":"http://avatar.zd1312.com/def/women_320_320.png","uid":2,"no":"4"},"content":"","count":1,"reviUser":null,"createTime":1587031778034}}

            var obj = JsonMapper.ToObject<HttpCallback<ShowLoveTop>>(json);
            if (obj.code == 0)
            {
                mNickText.text = obj.data.sendUser.nickname;
                mDescText.text = "//TODO:??";
                m_GenderImage.sprite = genderSprites[(obj.data.sendUser.sex == 1) ? 0 : 1];
                mCountText.text = $"X{obj.data.count}";
                mContentText.text = obj.data.content;
            }
        }
    }
}
