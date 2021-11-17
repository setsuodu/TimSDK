using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class ImageViewerListItem : MonoBehaviour
    {
        ImageViewerItemData mItemData;
        int mItemDataIndex = -1;
        public string url;

        public RawImage mImage;
        public AspectRatioFitter mAspect;
        public GameObject mContentRootObj;

        public void Init()
        {
            //只执行一次的逻辑
        }

        public void SetItemData(ImageViewerItemData itemData, int itemIndex)
        {
            this.mItemData = itemData;
            this.mItemDataIndex = itemIndex;
            this.url = itemData.mImageUrl;

            FileManager.Download(mItemData.mImageUrl, (byte[] bytes) => FileManager.OnLoadRawImage(bytes, mImage, ()=>
            {
                //Debug.Log($"{mImage.texture.width}*{mImage.texture.height}");
                if (mAspect != null)
                {
                    mAspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                    float ratio = (float)mImage.texture.width / (float)mImage.texture.height;
                    mAspect.aspectRatio = ratio;
                }
            }));
        }
    }
}
