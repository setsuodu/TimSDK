using System;
using System.IO;
using UnityEngine;

public class MicphoneManager : MonoBehaviour
{
    public static MicphoneManager Instance;

    #region Inspector Variables

    private AudioClip micClip;
    private AudioSource _audioSource;
    private AudioSource audioSource
    {
        get
        {
            if (_audioSource == null)
            {
                _audioSource = gameObject.GetComponent<AudioSource>();
            }
            return _audioSource;
        }
    }

    #endregion

    #region Member Variables

    public static string filePath;
    public int audioLength = 0; //实际录音时长
    public bool IsRecording { get { return Microphone.IsRecording(null); } }
    private const int MIN_LENGTH = 2; //下限2s
    private const int TOTAL_LENGTH = 90; //上限90s
    private const int HEADER_SIZE = 44;
    private int sFrequency = 11025;
    private float loudness;

    #endregion

    #region Unity Methods

    void Awake()
    {
        Instance = this;

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("no microphone found");
        }
        else
        {
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"{Microphone.devices[i]} is ready");
            }
        }
    }

    void Update()
    {
        if (Microphone.devices.Length > 0 && Microphone.IsRecording(null)) //正在录音
        {
            audioLength = Microphone.GetPosition(null) / sFrequency;
            Debug.Log("[正在录音]" + audioLength);
        }
    }

    #endregion

    #region Public Methods

    // 获取音量
    public float GetAveragedVolume()
    {
        float[] data = new float[256];
        float a = 0;
        int position = 0;
        int micPosition = 0;

        micPosition = Microphone.GetPosition(null); //Microphone.devices[0]

        if (micPosition > 256)
            position = micPosition - 256;

        micClip.GetData(data, position);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    // 获取声音数据
    public byte[] GetClipData()
    {
        if (audioSource.clip == null)
        {
            Debug.LogError("GetClipData audio.clip is null");
            return null;
        }

        float[] samples = new float[audioSource.clip.samples];

        audioSource.clip.GetData(samples, 0);

        byte[] outData = new byte[samples.Length * 2];

        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            short temshort = (short)(samples[i] * rescaleFactor);

            byte[] temdata = System.BitConverter.GetBytes(temshort);

            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }

        if (outData == null || outData.Length <= 0)
        {
            Debug.LogError("GetClipData intData is null");
            return null;
        }

        return outData;
    }

    // 重置
    public void Reset()
    {
        Destroy(micClip);
        micClip = null;
        audioLength = 0;
        audioSource.clip = null;
        filePath = string.Empty;
    }

    // 开始录音
    public void StartRecord()
    {
        if (Microphone.devices.Length == 0) return;

        audioLength = TOTAL_LENGTH;
        micClip = Microphone.Start(null, true, audioLength, sFrequency);
        audioSource.clip = micClip;
        audioSource.mute = true;
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
    }

    // 停止录音
    public void StopRecord()
    {
        if (!IsRecording) return; //录音大于上限自动停止，返回。避免pointUp时再执行一次。

        int lastTime = Microphone.GetPosition(null);
        //lastTime = Mathf.CeilToInt((float)lastTime / (float)sFrequency); //导致录音失败
        audioLength = Mathf.CeilToInt((float)lastTime / (float)sFrequency);
        Microphone.End(null); //获取lastTime之后才能停止
        Debug.Log($"录音时长{audioLength}s");

        if (audioLength < MIN_LENGTH)
        {
            Debug.LogError("录音时长不足");
            return;
        }

        float[] samples = new float[audioSource.clip.samples];
        audioSource.clip.GetData(samples, 0);
        float[] ClipSamples = new float[lastTime];
        Array.Copy(samples, ClipSamples, ClipSamples.Length - 1);
        audioSource.clip = AudioClip.Create("audio_trim", ClipSamples.Length, 1, sFrequency, false);
        audioSource.clip.SetData(ClipSamples, 0);

        // 用时间（微秒）命名
        Save(audioSource.clip);
    }

    // 播放录音
    public void PlayRecord()
    {
        if (Microphone.IsRecording(null))
        {
            Debug.LogError("err1");
            return;
        }
        if (audioSource.clip == null)
        {
            Debug.LogError("err2");
            return;
        }
        Debug.Log("play");
        audioSource.mute = false;
        audioSource.loop = false;
        audioSource.Play();
    }

    #endregion

    #region Static Methods

    static bool Save(AudioClip clip)
    {
        //if (!audioName.ToLower().EndsWith(".mp3")) { audioName += ".mp3"; } //确保格式后缀
        long currentTicks = DateTime.Now.Ticks;
        DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;
        string audioName = $"{UserManager.Instance.localPlayer.identifier}_{currentMillis}.mp3";
        Debug.Log($"audioName={audioName}");

        filePath = $"{Application.persistentDataPath}/{UserManager.Instance.localPlayer.identifier}/FileStorage/Sound/{audioName}";
        //string filepath = string.Format("{0}/Cache/Audios/{1}", Application.persistentDataPath, audioName);
        Debug.Log($"filePath={filePath}");

        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (FileStream fileStream = CreateEmpty(filePath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }
        return true; // TODO: return false if there's a failure saving the file  
    }

    static FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header  
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]  

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of  
        //dataSource array because a float converted in Int16 is 2 bytes.  

        int rescaleFactor = 32767; //to convert float to Int16  

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;
        //UInt16 two = 2;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2  
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        fileStream.Close();  
    }

    #endregion
}
