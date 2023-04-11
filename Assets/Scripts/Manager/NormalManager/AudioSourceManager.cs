using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// 负责控制音乐的播放与停止，以及游戏中各种音效的播放
/// </summary>
public class AudioSourceManager
{
    private List<AudioClip> audioClipList = new List<AudioClip>();
    private AudioSource[] MusicAudioSource = new AudioSource[2]; // 背景音乐播放器（一般只播一首，但可以实现两首音乐的渐入渐出切换）
    private float[] musicVolume = new float[2] { 1, 0 }; // 管理背景音乐当前相对播放音量
    private float[] musicVolumeRate = new float[2] { 1, 1 }; // 管理背景音效的播放音量倍率（与传入的音频剪辑预设有关）
    private AudioSource SeAudioSource; // 音效播放器
    private AudioSource currentAudioSource; // 当前正在播放的背景音乐播放器（主播放器）
    private Dictionary<string, float> startTimeDict = new Dictionary<string, float>(); // 记录音乐剪辑起始点
    private int currentAudioSourceIndex = 0; // 当前音乐播放器数组下标

    private bool playEffectMusic = true;
    private bool playBGMusic = true;
    private bool isStopAllMusic = true;
    private bool isPauseAllMusic = false;

    private AudioClip currentAudioClip; // 当前播放音乐剪辑

    // 构造函数
    public AudioSourceManager()
    {
        // 创建时动态为GameManager添加组件
        MusicAudioSource[0] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[0].loop = true;
        MusicAudioSource[1] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[1].loop = true;
        SeAudioSource = GameManager.Instance.gameObject.AddComponent<AudioSource>();

        currentAudioSourceIndex = 0;
        currentAudioSource = MusicAudioSource[currentAudioSourceIndex]; // 默认主播放器
    }

    public void SetAnotherAudioSourceAsCurrent()
    {
        currentAudioSourceIndex++;
        currentAudioSourceIndex = currentAudioSourceIndex % MusicAudioSource.Length;
        currentAudioSource = MusicAudioSource[currentAudioSourceIndex];
    }

    public static void LoadBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if (info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
        }
    }

    public static IEnumerator AsyncLoadBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if (info != null)
        {
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.AsyncGetAudioClip(info.resPath));
        }
        else
        {
            yield return null;
        }
    }

    public static IEnumerator AsyncLoadEffectMusic(string refenceName)
    {
        SoundsInfo info = SoundsManager.GetSoundsInfo(refenceName);
        if (info != null)
        {
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.AsyncGetAudioClip(info.resPath));
        }
        else
        {
            yield return null;
        }
    }

    public void PlayBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if(info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
            PlayBGMusic(audioClip);
            musicVolumeRate[currentAudioSourceIndex] = info.volume;
        }
    }

    // 播放背景音乐
    public void PlayBGMusic(AudioClip audioClip)
    {
        if (audioClip == null)
            return;

        currentAudioClip = audioClip;

        if (!GameManager.Instance.configManager.mConfig.isPlayBGM)
            return;

        isStopAllMusic = false;
        ResumeAllMusic();

        if (!audioClipList.Contains(audioClip))
            audioClipList.Add(audioClip);

        // 有在播，且正在播的与下一个要播的不是同一首
        // 切换到另一个音乐播放器播放音乐
        if (currentAudioSource.isPlaying && currentAudioSource.clip != audioClip)
        {
            SetAnotherAudioSourceAsCurrent();
            // 注，经过以上方法，此时currentAudioSource引用的对象已发生改变，详情可以查看上面的方法实现
        }

        currentAudioSource.clip = audioClip;
        if (playBGMusic)
        {
            currentAudioSource.time = 0; // 从0开始播放
            currentAudioSource.Play();
        }
        currentAudioSource.time = GetLoopStartTime(audioClip.name); // 设置循环点
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="refenceName"></param>
    public void PlayEffectMusic(string refenceName)
    {
        SoundsInfo info = SoundsManager.GetSoundsInfo(refenceName);
        if(info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
            PlayEffectMusic(audioClip, info.volume * GameManager.Instance.configManager.mConfig.SEVolume);
        }
    }

    // 播放音效
    public void PlayEffectMusic(AudioClip audioClip, float volume)
    {
        if (!playEffectMusic)
            return;
        SeAudioSource.PlayOneShot(audioClip, volume);
    }

    /// <summary>
    /// 停止播放全部音乐
    /// </summary>
    public void StopAllMusic()
    {
        isStopAllMusic = true;
        musicVolume[0] = 0;
        MusicAudioSource[0].Stop();
        musicVolume[1] = 0;
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// 暂停全部音乐
    /// </summary>
    public void PauseAllMusic()
    {
        isPauseAllMusic = true;
        MusicAudioSource[0].Pause();
        MusicAudioSource[1].Pause();
    }

    /// <summary>
    /// 恢复播放全部音乐
    /// </summary>
    public void ResumeAllMusic()
    {
        isPauseAllMusic = false;
        if (!MusicAudioSource[0].isPlaying)
            MusicAudioSource[0].Play();
        if (!MusicAudioSource[1].isPlaying)
            MusicAudioSource[1].Play();
    }

    /// <summary>
    /// 重放当前音乐剪辑
    /// </summary>
    public void ReplayCurrentClip()
    {
        PlayBGMusic(currentAudioClip);
    }

    /// <summary>
    /// 关闭BGM
    /// </summary>
    public void CloseBGMusic()
    {
        MusicAudioSource[0].Stop();
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// 打开BGM
    /// </summary>
    public void OpenBGMusic()
    {
        MusicAudioSource[0].Play();
    }

    public void CloseOrOpenBGMusic()
    {
        playBGMusic = !playBGMusic;
        if (playBGMusic)
        {
            OpenBGMusic();
        }
        else
        {
            CloseBGMusic();
        }
    }

    /// <summary>
    /// 开关音效
    /// </summary>
    public void CloseOrOpenEffectMusic()
    {
        playEffectMusic = !playEffectMusic;
    }

    /// <summary>
    /// 获取BGM的循环起始时间
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private float GetLoopStartTime(string name)
    {
        if (startTimeDict.ContainsKey(name))
        {
            return startTimeDict[name];
        }
        return 0;
    }

    /// <summary>
    /// 每帧更新方法，请把它挂在GameManager上
    /// </summary>
    public void Update()
    {
        ConfigManager.Config config = GameManager.Instance.configManager.mConfig;

        // 渐入渐出效果更新
        if (!isStopAllMusic)
        {
            musicVolume[currentAudioSourceIndex] = Mathf.Min(musicVolume[currentAudioSourceIndex] + 1.0f / 120, 1.0f);
            musicVolume[1 - currentAudioSourceIndex] = Mathf.Max(musicVolume[1 - currentAudioSourceIndex] - 1.0f / 120, 0.0f);
        }
        else
        {
            for (int i = 0; i < musicVolume.Length; i++)
            {
                musicVolume[i] = Mathf.Max(musicVolume[1 - currentAudioSourceIndex] - 1.0f / 120, 0.0f);
            }
        }

        // 调整音量
        MusicAudioSource[currentAudioSourceIndex].volume = musicVolume[currentAudioSourceIndex] * config.BGMVolume * musicVolumeRate[currentAudioSourceIndex];
        MusicAudioSource[1-currentAudioSourceIndex].volume = musicVolume[1-currentAudioSourceIndex] * config.BGMVolume * musicVolumeRate[1-currentAudioSourceIndex];
        // 调整音效音量
        SeAudioSource.volume = config.SEVolume;
    }
}
