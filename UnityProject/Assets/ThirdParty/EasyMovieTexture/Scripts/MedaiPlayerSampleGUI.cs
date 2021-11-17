using UnityEngine;
using System.Collections;

public class MedaiPlayerSampleGUI : MonoBehaviour
{
	public MediaPlayerCtrl scrMedia;
	public bool m_bFinish = false;

	void Start()
	{
		scrMedia.OnEnd += OnEnd;
	}

	void Update()
	{

	}

#if !UNITY_WEBGL
	void OnGUI()
	{
		if (GUI.Button(new Rect(50, 50, 100, 100), "Load"))
		{
			scrMedia.Load("EasyMovieTexture.mp4");
			m_bFinish = false;
		}

		if (GUI.Button(new Rect(50, 200, 100, 100), "Play"))
		{
			scrMedia.Play();
			m_bFinish = false;
		}

		if (GUI.Button(new Rect(50, 350, 100, 100), "stop"))
		{
			scrMedia.Stop();
		}

		if (GUI.Button(new Rect(50, 500, 100, 100), "pause"))
		{
			scrMedia.Pause();
		}

		if (GUI.Button(new Rect(50, 650, 100, 100), "Unload"))
		{
			scrMedia.UnLoad();
		}

		if (GUI.Button(new Rect(50, 800, 100, 100), "m_bFinish:" + m_bFinish))
		{

		}

		if (GUI.Button(new Rect(200, 50, 100, 100), "SeekTo"))
		{
			scrMedia.SeekTo(10000);
		}

		if (scrMedia.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
		{
			if (GUI.Button(new Rect(200, 200, 100, 100), "pos=" + scrMedia.GetSeekPosition().ToString())) //播放进度
			{
				scrMedia.SetSpeed(2.0f);
			}

			if (GUI.Button(new Rect(200, 350, 100, 100), "dur=" + scrMedia.GetDuration().ToString())) //总时长（帧数）
			{
				scrMedia.SetSpeed(1.0f);
			}

			if (GUI.Button(new Rect(200, 450, 100, 100), "w=" + scrMedia.GetVideoWidth().ToString()))
			{

			}

			if (GUI.Button(new Rect(200, 550, 100, 100), "h=" + scrMedia.GetVideoHeight().ToString()))
			{

			}
		}

		if (GUI.Button(new Rect(200, 650, 150, 100), "per=" + scrMedia.GetCurrentSeekPercent().ToString())) //进度百分比
		{

		}

		if (GUI.Button(new Rect(400, 650, 150, 100), "state=" + scrMedia.GetCurrentState().ToString())) //状态
		{

		}
	}
#endif

	void OnEnd()
	{
		m_bFinish = true;
	}
}
