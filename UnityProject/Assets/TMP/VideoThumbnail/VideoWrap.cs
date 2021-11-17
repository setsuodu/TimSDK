using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoWrap : MonoBehaviour
{
    RawImage m_RawImage;
    AspectRatioFitter m_Aspect;
    VideoPlayer videoPlayer;
    public Texture2D preview;

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        m_Aspect = GetComponent<AspectRatioFitter>();
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private IEnumerator Start()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
        yield return new WaitUntil(()=> videoPlayer.isPrepared && videoPlayer.frame == 2);
        Debug.Log($"播放: {videoPlayer.isPlaying}");

        m_RawImage.texture = videoPlayer.texture;
        Debug.Log($"{videoPlayer.texture.width}x{videoPlayer.texture.height}"); //1920x1080
        m_Aspect.aspectRatio = (float)videoPlayer.texture.width / (float)videoPlayer.texture.height;

        int width = videoPlayer.texture.width;
        int height = videoPlayer.texture.height;

        Texture mainTexture = videoPlayer.texture;
        preview = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 24);
        Graphics.Blit(mainTexture, renderTexture);
        RenderTexture.active = renderTexture;
        preview.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        preview.Apply();

        yield return new WaitUntil(() => videoPlayer.isPrepared && videoPlayer.frame == 3);
        videoPlayer.Pause();
        RenderTexture.active = null;

        preview = TextureScale.Bilinear(preview, preview.width / 2, preview.height / 2); //缩略图480宽
    }
}
