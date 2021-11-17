using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_GridImage : MonoBehaviour
{
    [SerializeField] private Image m_Thumbnail;
    [SerializeField] private AspectRatioFitter m_AspectRatio;
    [SerializeField] private Button m_DeleteBtn;
    //[SerializeField] private int Index = -1;
    public string filePath;

    void Awake()
    {
        m_DeleteBtn.onClick.AddListener(OnDelete);
    }

    public void Reset()
    {
        m_Thumbnail.sprite = null;
        m_AspectRatio.aspectRatio = 1;
        //Index = -1;
        filePath = string.Empty;
    }

    void OnDelete()
    {
        Debug.Log("删除");
        //Destroy(gameObject);

        //TODO: 回收，List中移除
        //UI_CreateArticle.Instance.m_ImagePathList.RemoveAt(this.Index);
        //Destroy(gameObject);

        UI_CreateArticle.Instance.Despawn(this);
    }

    public void Init(string path)
    {
        this.filePath = path;

        //绘制缩略图tmp
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D t2d = new Texture2D(2, 2);
        t2d.LoadImage(bytes);
        Debug.Log($"{t2d.width}x{t2d.height} bytes={bytes.Length/1000}kb");
        //t2d.Compress(false); //DXT


        //小的控制大的
        float ratio =0;
        if (t2d.width < t2d.height)
        {
            m_AspectRatio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        }
        else
        {
            m_AspectRatio.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        }
        ratio = (float)t2d.width / (float)t2d.height;
        m_Thumbnail.rectTransform.sizeDelta = new Vector2(78, 78);
        m_AspectRatio.aspectRatio = ratio;


        //会裁切而不是压缩
        //int width = t2d.width;
        //int height = t2d.height;
        //if (t2d.width > 256)
        //{
        //    width = 256;
        //    height = Mathf.RoundToInt(width / ratio);
        //}


        Sprite sp = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_Thumbnail.sprite = sp;
        m_Thumbnail.type = Image.Type.Simple;
        m_Thumbnail.preserveAspect = false;
    }
}
